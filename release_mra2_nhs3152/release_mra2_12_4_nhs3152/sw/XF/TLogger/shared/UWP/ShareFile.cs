/*
 * Copyright 2020 NXP
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
using System.Threading.Tasks;
using Monitor.UWP;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;

[assembly: Xamarin.Forms.Dependency(typeof(ShareFile))]
namespace Monitor.UWP
{
    public class ShareFile : Interfaces.IShareFile
    {
        private string[] _filePaths;
        private string _title;
        private string _message;
        private readonly DataTransferManager _dtm;

        public ShareFile()
        {
            _dtm = DataTransferManager.GetForCurrentView();
            _dtm.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(OnDataRequested);
        }


        async public Task ShareAsync(string[] filePaths, string title, string message)
        {
            _filePaths = filePaths;
            _title = title;
            _message = message;

            DataTransferManager.ShowShareUI();

            await Task.CompletedTask;
        }

        private async void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest req = e.Request;

            req.Data.Properties.Title =
                string.IsNullOrEmpty(_title) ? Windows.ApplicationModel.Package.Current.DisplayName : _title;
            req.Data.SetText(_message);
            DataRequestDeferral deferral = req.GetDeferral();

            try
            {
                List<IStorageItem> storageItems = new List<IStorageItem>();
                foreach (string f in _filePaths)
                {
                    var attachment = await StorageFile.GetFileFromPathAsync(f);
                    storageItems.Add(attachment);

                }
                req.Data.SetStorageItems(storageItems);
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}
