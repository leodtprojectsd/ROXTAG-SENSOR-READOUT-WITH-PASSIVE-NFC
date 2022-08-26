/*
 * Copyright 2019-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using Helpers;
using Msg;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TLogger.Services
{
    public class DatabaseService
    {
        private Msg.Lib _msgLib;

        // SQLite supports the following types.
        //      Boolean, Byte, UInt16, SByte, Int16, Int32, UInt32, Int64,
        //      Single, Double, Decimal,
        //      String,
        //      DateTime,
        //      Enum,
        //      byte[],
        //      Guid
        // We convert unsupported types into JSON string using TextBlob using SQLiteNetExtension.

        private readonly SQLiteAsyncConnection _db;
        private static SemaphoreSlim _sem = new SemaphoreSlim(1);
        public EventHandler<EventArgs> DBaseUpdateEvent;
        public DatabaseService()
        {
            _msgLib = App.MsgLib;

            var fileName = "TLogger.db3";
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            _db = new SQLiteAsyncConnection(dbPath);

            _db.CreateTableAsync<DebugLogsAppSettingsTable>().Wait();
            _db.CreateTableAsync<DebugLogsTagsDataResetTable>().Wait();
            _db.CreateTableAsync<DebugLogsNdefAccessTable>().Wait();
            _db.CreateTableAsync<DebugLogsExceptionsTable>().Wait();
            _db.CreateTableAsync<TagsConfigTable>().Wait();
            _db.CreateTableAsync<TagsStatusTable>().Wait();
            _db.CreateTableAsync<TagsTemperatureDataTable>().Wait();

            _msgLib.NdefAccessEvent += OnNdefAccess;
            ExceptionLogHelper.ExceptionLogEvent += OnExceptionLogEvent;
            _msgLib.DataProcessEvent += OnDataProcess;
            _msgLib.SetConfigEvent += OnSetConfig;
        }


        public SQLiteAsyncConnection DBase { get => _db; }

        private async void OnNdefAccess(object sender, Lib.NdefAccessEventArgs e)
        {
            if (App.AppSettingsService.AreDebugLogs)
            {
                try
                {
                    await _sem.WaitAsync();
                    await _db.InsertAsync(new DebugLogsNdefAccessTable
                    {
                        NfcId = e.NfcId,
                        Timestamp = e.Timestamp,
                        NdefAccess = (ENdefAccess)e.NdefAccess,
                        Data = e.Data,
                    });
                }
                catch (Exception ex)
                {
                    var tagId = _msgLib.TagModel.TagId;
                    ExceptionLogHelper.Log(tagId, Helpers.ExceptionLogHelper.GetCurrentMethod() +
                        " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + tagId + " - " +
                        ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
                finally
                {
                    _sem.Release();
                }
            }
        }

        private async void OnExceptionLogEvent(object sender, ExceptionLogEventArgs e)
        {
            if (App.AppSettingsService.AreDebugLogs)
            {
                try
                {
                    await _sem.WaitAsync();
                    await _db.InsertAsync(new DebugLogsExceptionsTable
                    {
                        TagId = e.TagId,
                        Timestamp = e.Timestamp,
                        Description = e.Description,
                    });
                    _ = await _db.GetAllWithChildrenAsync<DebugLogsExceptionsTable>(null, true);
                }
                catch (Exception ex)
                {
                    var tagId = _msgLib.TagModel.TagId;
                    ExceptionLogHelper.Log(tagId, Helpers.ExceptionLogHelper.GetCurrentMethod() +
                        " - " + ex.Message);
                    Debug.WriteLine("Caught exception:" + tagId + " - " +
                        ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                }
                finally
                {
                    _sem.Release();
                }
            }
        }

        private async void OnDataProcess(object sender, Lib.DataProcessEventArgs e)
        {
            var tagId = _msgLib.TagModel.TagId;

            try
            {
                await _sem.WaitAsync();

                // SQLite saves DateTime in ISO-8601 (Kind=Unspecify) format.
                // So keep all members the same except Kind.
                var tagConfigTimestamp = DateTime.SpecifyKind(_msgLib.TagModel.TagConfigTimestamp, DateTimeKind.Unspecified);


                if (e.Op == Msg.Lib.DataProcessOp.RefreshDataFromCache)
                {
                    // Check if tag(NfcId) and config(ConfigDate) data already exist in DB.
                    // We need to use synchronous version here as this code will will update the models from
                    // database and at the same time tag is updating the models. So there will be a race
                    // condition on async version.
                    var queryList = Task.Run(async () =>
                    {
                        return await _db.GetAllWithChildrenAsync<TagsConfigTable>(
                            i => i.TagId == tagId && i.TagConfigTimestamp == tagConfigTimestamp, true);
                    }).Result;

                    // Check if there is cached data.
                    if (queryList.Count == 1)
                    {
                        Debug.WriteLine($"&&&&&&&&&&&&&&&&&& Cached data found");

                        var config = queryList[0];

                        // Init all models from cache.
                        if (config.StatusTable.Count != 0)
                        {
                            var status = config.StatusTable[config.StatusTable.Count - 1];
                            _msgLib.NfcIdModel.Reset(status.NfcId);
                            _msgLib.VersionModel.Reset(status.Version);
                            _msgLib.MeasurementStatusModel.Reset(status.MeasurementStatus);
                            _msgLib.TemperatureStatusModel.Reset(status.TemperatureStatus);

                            var temperatureData = new Msg.Models.DataOut();
                            temperatureData.RangeList = status.TemperatureDataRanges;
                            for (int i = 0; i < temperatureData.RangeList.Count; i++)
                                temperatureData.NumData += temperatureData.RangeList[i].End - temperatureData.RangeList[i].Begin + 1;
                            temperatureData.Data = config.TemperatureDataTable.Data;
                            _msgLib.TemperatureDataModel.Reset(null, temperatureData);
                        }

                        _msgLib.MeasurementConfigModel.Reset(config.MeasurementConfig);
                        _msgLib.TemperatureConfigModel.Reset(config.TemperatureConfig);
                    }
                }
                else if (e.Op == Msg.Lib.DataProcessOp.End)
                {
                    // Check if tag(NfcId) and config(ConfigDate) data already exist in DB.
                    var queryList = await _db.GetAllWithChildrenAsync<TagsConfigTable>(
                        i => i.TagId == tagId && i.TagConfigTimestamp == tagConfigTimestamp, true);

                    var status = new TagsStatusTable
                    {
                        Timestamp = DateTime.Now,
                        NfcId = _msgLib.NfcIdModel.NfcId,
                        Version = _msgLib.VersionModel.Version,
                        MeasurementStatus = _msgLib.MeasurementStatusModel.Status,
                        TemperatureStatus = _msgLib.TemperatureStatusModel.Status,
                        TemperatureDataRanges = _msgLib.TemperatureDataModel.TemperatureData.RangeList,
                        NdefMimeType = _msgLib.NdefMimeType.NdefMimeType,
                        JsonNdefText = JsonConvert.SerializeObject(_msgLib.NdefText.NdefText),
                        Drift = RtcAccuracyHelper.GetDrift(),
                    };

                    var temperatureData = new TagsTemperatureDataTable
                    {
                        Data = _msgLib.TemperatureDataModel.TemperatureData.Data,
                    };

                    if (queryList.Count == 0)
                    {
                        // No entry in DB.
                        // This means the tag has been configured by another phone
                        // or DB has been deleted.
                        // Create new data tables and config entry with current status and data as child.
                        var config = new TagsConfigTable
                        {
                            TagConfigTimestamp = DateTime.SpecifyKind(
                                _msgLib.TagModel.TagConfigTimestamp,
                                DateTimeKind.Unspecified),
                            TagId = tagId,
                            Timestamp = DateTime.Now,
                            MeasurementConfig = _msgLib.MeasurementConfigModel.Config,
                            TemperatureConfig = _msgLib.TemperatureConfigModel.Config,
                            StatusTable = new List<TagsStatusTable>
                        {
                            status,
                        },
                            TemperatureDataTable = temperatureData,
                        };
                        await _db.InsertWithChildrenAsync(config, true);
                    }
                    else if (queryList.Count == 1)
                    {
                        // Already an entry in DB.
                        // Add the new status. 
                        var config = queryList[0];

                        config.StatusTable.Add(status);
                        config.TemperatureDataTable = temperatureData;

                        await _db.InsertWithChildrenAsync(status, true);
                        await _db.InsertWithChildrenAsync(temperatureData, true);
                        await _db.InsertOrReplaceWithChildrenAsync(config, false);
                    }
                    else
                    {
                        //// THIS IS ERROR, shouldnt be happening
                    }

                    DBaseUpdateEvent?.Invoke(this, new EventArgs());

                    var c = await _db.GetAllWithChildrenAsync<TagsConfigTable>(null, true);
                    var s = await _db.GetAllWithChildrenAsync<TagsStatusTable>(null, true);
                    var t = await _db.GetAllWithChildrenAsync<TagsTemperatureDataTable>(null, true);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogHelper.Log(tagId, Helpers.ExceptionLogHelper.GetCurrentMethod() +
                    " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + tagId + " - " +
                    ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                _sem.Release();
            }
        }

        private async void OnSetConfig(object sender, Lib.SetConfigEventArgs e)
        {
            var tagId = _msgLib.TagModel.TagId;

            try
            {
                var temperatureData = new TagsTemperatureDataTable
                {
                    Data = new float[] { },
                };
                await _db.InsertWithChildrenAsync(temperatureData, true);

                var config = new TagsConfigTable
                {
                    TagId = tagId,
                    TagConfigTimestamp = //e.MeasurementConfig.ConfigTime,
                        DateTime.SpecifyKind(
                        _msgLib.TagModel.TagConfigTimestamp,
                        DateTimeKind.Unspecified),
                    Timestamp = DateTime.Now,
                    StatusTable = new List<TagsStatusTable>
                    {

                    },
                    MeasurementConfig = e.MeasurementConfig,
                    TemperatureConfig = e.TemperatureConfig,
                };
                await _db.InsertWithChildrenAsync(config, true);

                var c = await _db.GetAllWithChildrenAsync<TagsConfigTable>(null, true);
                var s = await _db.GetAllWithChildrenAsync<TagsStatusTable>(null, true);
                var t = await _db.GetAllWithChildrenAsync<TagsTemperatureDataTable>(null, true);

            }
            catch (Exception ex)
            {
                ExceptionLogHelper.Log(tagId, Helpers.ExceptionLogHelper.GetCurrentMethod() +
                    " - " + ex.Message);
                Debug.WriteLine("Caught exception:" + tagId + " - " +
                    ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
            finally
            {
                _sem.Release();
            }
        }

        public class DebugLogsAppSettingsTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            public DateTime Timestamp { get; set; }
            public bool DebugLogsEnabled { get; set; }
            public AppSettingsService.ETemperatureUnit TemperatureUnit { get; set; }
            public string DateFormat { get; set; }
            public string TimeFormat { get; set; }
            public bool IsHistory { get; set; }
        }

        public class DebugLogsTagsDataResetTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            public DateTime Timestamp { get; set; }
        }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public enum ENdefAccess
        {
            [EnumMember(Value = "Read")]
            Read,

            [EnumMember(Value = "Write")]
            Write,
        }

        public class DebugLogsNdefAccessTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            [JsonConverter(typeof(Helpers.JsonConverterHelper.ByteArrayHexConverter))]
            public byte[] NfcId { get; set; }

            public DateTime Timestamp { get; set; }
            public ENdefAccess NdefAccess { get; set; }
            public byte[] Data { get; set; }
        }

        public class DebugLogsExceptionsTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            public DateTime Timestamp { get; set; }
            public string TagId { get; set; }
            public string Description { get; set; }
        }

        public class TagsStatusTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            [ForeignKey(typeof(TagsConfigTable))]
            [JsonIgnore]
            public int ConfigId { get; set; }

            public DateTime Timestamp { get; set; }

            [JsonConverter(typeof(Helpers.JsonConverterHelper.ByteArrayHexConverter))]
            public byte[] NfcId { get; set; }

            [TextBlob("VersionBlob")]
            public Msg.Models.VersionModel.CVersion Version { get; set; }

            [TextBlob("MeasurementStatusBlob")]
            public Msg.Models.MeasurementStatusModel.CStatus MeasurementStatus { get; set; }

            [TextBlob("TemperatureStatusBlob")]
            public Msg.Models.TemperatureStatusModel.CStatus TemperatureStatus { get; set; }

            public string NdefMimeType { get; set; }

            // In JSON format.
            public string JsonNdefText { get; set; }

            [TextBlob("TemperatureDataRangesBlob")]
            public List<Msg.Models.Range> TemperatureDataRanges { get; set; }

            public double Drift { get; set; }

            [ManyToOne]
            public TagsConfigTable ConfigTable { get; set; }

            // Serialized data blobs.
            [JsonIgnore]
            public string VersionBlob { get; set; }
            [JsonIgnore]
            public string MeasurementStatusBlob { get; set; }
            [JsonIgnore]
            public string TemperatureStatusBlob { get; set; }
            [JsonIgnore]
            public string TemperatureDataRangesBlob { get; set; }
        }

        public class TagsTemperatureDataTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            [ForeignKey(typeof(TagsConfigTable))]
            [JsonIgnore]
            public int ConfigId { get; set; }

            [TextBlob("DataBlob")]
            public float[] Data { get; set; }

            [OneToOne]
            public TagsConfigTable ConfigTable { get; set; }

            // Serialized data blobs.
            [JsonIgnore]
            public string DataBlob { get; set; }
        }


        public class TagsConfigTable
        {
            [PrimaryKey, AutoIncrement]
            [JsonIgnore]
            public int Id { get; set; }

            public string TagId { get; set; }
            public DateTime TagConfigTimestamp { get; set; }

            public DateTime Timestamp { get; set; }

            [TextBlob("MeasurementConfigBlob")]
            public Msg.Models.MeasurementConfigModel.CConfig MeasurementConfig { get; set; }

            [TextBlob("TemperatureConfigBlob")]
            public Msg.Models.TemperatureConfigModel.CConfig TemperatureConfig { get; set; }


            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<TagsStatusTable> StatusTable { get; set; }


            [OneToOne(CascadeOperations = CascadeOperation.All)]
            public TagsTemperatureDataTable TemperatureDataTable { get; set; }


            // Serialized data blobs.
            [JsonIgnore]
            public string MeasurementConfigBlob { get; set; }
            [JsonIgnore]
            public string TemperatureConfigBlob { get; set; }
        }
    }
}
