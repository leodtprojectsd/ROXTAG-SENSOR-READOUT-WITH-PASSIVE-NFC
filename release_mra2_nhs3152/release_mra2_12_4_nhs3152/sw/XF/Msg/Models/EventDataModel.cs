/*
 * Copyright 2018-2020 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Msg.Models
{
    public class EventDataModel : INotifyPropertyChanged
    {
        public enum EventId
        {
            NoEvent,
            MeasurementPristine,
            MeasurementConfigured,
            MeasurementStarting,
            MeasurementLogging,
            MeasurementStopped,
            TemperatureTooHigh,
            TemperatureTooLow,
            BatteryBod,
            StorageFull,
            DemoExpired,
            I2cError,
            SpiError,
            AccelerometerShock,
            AccelerometerShake,
            AccelerometerVibration,
            AccelerometerTilt,
            AccelerometerShockConfigured,
            AccelerometerShakeConfigured,
            AccelerometerVibrationConfigured,
            AccelerometerTiltConfigured,
            HumidityConfigured,
            HumidityTooHigh,
            HumidityTooLow,
            TemperatureOk,
            HumidityOk,
        }

        public enum EventRow
        {
            Humidity,
            Temperature,
            Accelerometer,
            Measurement,
        }

        const string OkBackgroundColor = "#F0F0F0";
        const string HighBackgroundColor = "#FFD0D0";
        const string LowBackgroundColor = "#D9E2F1";
        const string AlarmBackgroundColor = "#FFD0D0";



        public class Event : IEquatable<Event>
        {
            public EventId Id { get; set; }
            public DateTime Timestamp { get; set; }
            public string Description { get; set; }

            public bool Equals(Event other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id) && Timestamp.Equals(other.Timestamp) && Description.Equals(other.Description);
            }

            // If Equals() returns true for a pair of objects  
            // then GetHashCode() must return the same value for these objects. 
            public override int GetHashCode()
            {
                int hashId = Id.GetHashCode();
                int hashTimestamp = Timestamp.GetHashCode();
                int hashDescription = Description == null ? 0 : Description.GetHashCode();
                return hashId ^ hashTimestamp ^ hashDescription;
            }
        }

        public class DataIn
        {
            public Event[] Data { get; set; }
        }

        public class DataOut
        {
            public Event[] Data { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        DataIn _dataIn = new DataIn()
        {
            Data = new Event[] { },
        };


        public DataIn EventDataIn
        {
            get => _dataIn;
            set
            {
                if (value.Data.Length != 0)
                {
                    _dataIn = value;
                    Debug.WriteLine($"DataIn: {_dataIn.Data.Length} events");
                    //foreach (Event ev in value.Data)
                    //{
                    //    System.Diagnostics.Debug.WriteLine($@"
                    //        Id: {ev.Id}
                    //        Timestamp: {ev.Timestamp}
                    //        Description: {ev.Description}
                    //    ");
                    //}
                    EventData = ProcessDataInToOut();
                }
            }
        }

        public EventDataModel()
        {
        }

        DataOut _dataOut = new DataOut
        {
            Data = new Event[] { },
        };
        public DataOut EventData
        {
            get => _dataOut;
            set
            {
                _dataOut = value;
                OnPropertyChanged();
                Debug.WriteLine("################## EVENT ####");
                Debug.WriteLine($"#### DATAOUT: NumEvents:{_dataOut.Data.Length} ####");
            }
        }

        public void Reset(DataIn dataIn = null, DataOut dataOut = null, bool isInvokePropertyChange = false)
        {
////            var backup = _dataOut;

            _dataIn = dataIn ?? new DataIn()
            {
                Data = new Event[] { },
            };
            _dataOut = dataOut ?? new DataOut
            {
                Data = new Event[] { },
            };

            if (isInvokePropertyChange)
                OnPropertyChanged(nameof(Reset));
        }


        DataOut ProcessDataInToOut()
        {
            var dataOut = new List<Event>(_dataOut.Data);

            // Add new events.
            dataOut.AddRange(new List<Event>(_dataIn.Data));

            // Remove duplicates. 
            var newDataOut = dataOut.Distinct().ToList();

            // Sort by time.
            var newDataOutSorted = new ObservableCollection<Event>(newDataOut.OrderBy(i => i.Timestamp)).ToList();
            return new DataOut { Data = newDataOutSorted.ToArray() };

        }

        public class EventParams
        {
            public EventRow Row { get; set; }
            public string Color { get; set; }
            public string BackgroundColor { get; set; }
            public string Image { get; set; }
            public string Description { get; set; }
        }

        public readonly EventParams[] EventParamsArray = new EventParams[]
        {
            new EventParams
            {
                // index 0 is not valid
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "pristine.png",
                Description = "Pristine",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "configured.png",
                Description = "Configured",
            },
            new EventParams
            {

                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "starting.png",
                Description = "Starting",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "monitoring.png",
                Description = "Monitoring",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "stop.png",
                Description = "Stopped",
            },
            new EventParams
            {
                Row = EventRow.Temperature,
                Color = "#a40044",
                BackgroundColor = HighBackgroundColor,
                Image = "temp high.png",
                Description = "Temperature too high",
            },
            new EventParams
            {
                Row = EventRow.Temperature,
                Color = "#a40044",
                BackgroundColor = LowBackgroundColor,
                Image = "temp low.png",
                Description = "Temperature too low",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#003883",
                BackgroundColor = AlarmBackgroundColor,
                Image = "battery empty.png",
                Description = "Battery BOD",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#003883",
                BackgroundColor = AlarmBackgroundColor,
                Image = "storage full.png",
                Description = "Storage full",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#003883",
                BackgroundColor = AlarmBackgroundColor,
                Image = "",
                Description = "Expired",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#003883",
                BackgroundColor = AlarmBackgroundColor,
                Image = "",
                Description = "I2C error",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#003883",
                BackgroundColor = AlarmBackgroundColor,
                Image = "",
                Description = "SPI error",
            },
            new EventParams
            {
                Row = EventRow.Accelerometer,
                Color = "#7bb1db",
                BackgroundColor = AlarmBackgroundColor,
                Image = "acc shock.png",
                Description = "Shock",
            },
            new EventParams
            {
                Row = EventRow.Accelerometer,
                Color = "#7bb1db",
                BackgroundColor = AlarmBackgroundColor,
                Image = "",
                Description = "Shake",
            },
            new EventParams
            {
                Row = EventRow.Accelerometer,
                Color = "#7bb1db",
                BackgroundColor = AlarmBackgroundColor,
                Image = "acc vibration.png",
                Description = "Vibration",
            },
            new EventParams
            {
                Row = EventRow.Accelerometer,
                Color = "#f9b500",
                BackgroundColor = AlarmBackgroundColor,
                Image = "acc tilting.png",
                Description = "Tilting",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "",
                Description = "Shock configured",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "",
                Description = "Shake configured",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "",
                Description = "Vibration configured",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "",
                Description = "Tilting configured",
            },
            new EventParams
            {
                Row = EventRow.Measurement,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "",
                Description = "Humidity configured",
            },
            new EventParams
            {
                Row = EventRow.Humidity,
                Color = "#d54e12",
                BackgroundColor = HighBackgroundColor,
                Image = "humid high.png",
                Description = "Humidity too high",
            },
            new EventParams
            {
                Row = EventRow.Humidity,
                Color = "#d54e12",
                BackgroundColor = LowBackgroundColor,
                Image = "humid low.png",
                Description = "Humidity too low",
            },
            new EventParams
            {
                Row = EventRow.Temperature,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "temp ok.png",
                Description = "Temperature OK",
            },
            new EventParams
            {
                Row = EventRow.Humidity,
                Color = "#c9d200",
                BackgroundColor = OkBackgroundColor,
                Image = "humid ok.png",
                Description = "Humidity OK",
            },
        };
    }
}
