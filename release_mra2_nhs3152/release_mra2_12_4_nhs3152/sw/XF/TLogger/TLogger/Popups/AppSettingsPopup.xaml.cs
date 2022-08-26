/*
 * Copyright 2018-2019 NXP
 * This software is owned or controlled by NXP and may only be used strictly
 * in accordance with the applicable license terms.  By expressly accepting
 * such terms or by downloading, installing, activating and/or otherwise using
 * the software, you are agreeing that you have read, and that you agree to
 * comply with and are bound by, such license terms.  If you do not agree to
 * be bound by the applicable license terms, then you may not retain, install,
 * activate or otherwise use the software.
 */

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TLogger.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppSettingsPopup : PopupPage
    {
        public AppSettingsPopup()
        {
            InitializeComponent();

            celsiusFahrenheitSelect.IsToggled = App.AppSettingsService.TemperatureUnit ==
                Services.AppSettingsService.ETemperatureUnit.Fahrenheit ? true : false;
            isHistory.IsToggled = App.AppSettingsService.IsHistory;
            isLogging.IsToggled = App.AppSettingsService.AreDebugLogs;
        }

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        async void OnOkButton(object sender, EventArgs e)
        {
            App.AppSettingsService.TemperatureUnit = celsiusFahrenheitSelect.IsToggled ?
                Services.AppSettingsService.ETemperatureUnit.Fahrenheit :
                Services.AppSettingsService.ETemperatureUnit.Celsius;
            App.AppSettingsService.IsHistory = isHistory.IsToggled;
            App.AppSettingsService.AreDebugLogs = isLogging.IsToggled;

            // Debug log.
            if (App.AppSettingsService.AreDebugLogs)
            {
                await Task.Run(async () =>
                {
                    await App.DatabaseService.DBase.InsertAsync(new Services.DatabaseService.DebugLogsAppSettingsTable
                    {
                        Id = 0,
                        Timestamp = DateTime.Now,
                        DebugLogsEnabled = App.AppSettingsService.AreDebugLogs,
                        TemperatureUnit = App.AppSettingsService.TemperatureUnit,
                        DateFormat = App.AppSettingsService.DateFormat,
                        TimeFormat = App.AppSettingsService.TimeFormat,
                        IsHistory = App.AppSettingsService.IsHistory,
                    });
                });
            }

            await PopupNavigation.Instance.PopAsync(true);
        }

        async void OnCancelButton(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync(true);
        }


        async void OnClearHistory(object sender, EventArgs e)
        {
            bool isYes = false;
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                await PopupNavigation.Instance.PushAsync(
                    new ConfirmPopup(
                        ImageSource.FromResource("TLogger.Images.ok.png"), "Confirmation", "Are you sure?", tcs));

                await tcs.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        if (antecedent.Result)
                        {
                            isYes = true;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, ex.Message);
            }

            if (isYes)
            {
                // Clear history.
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.TagsConfigTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.TagsConfigTable>();
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.TagsStatusTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.TagsStatusTable>();
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.TagsTemperatureDataTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.TagsTemperatureDataTable>();

                // Clear current data models.
                App.MsgLib.ResetModels(true);

                await PopupNavigation.Instance.PushAsync(new OkPopup(ImageSource.FromResource("TLogger.Images.ok.png"),
                    string.Empty, "History has been cleared"));
            }

            await Task.CompletedTask;

        }

        async void OnClearLogs(object sender, EventArgs e)
        {
            bool isYes = false;
            try
            {
                var tcs = new TaskCompletionSource<bool>();
                await PopupNavigation.Instance.PushAsync(
                    new ConfirmPopup(
                        ImageSource.FromResource("TLogger.Images.ok.png"), "Confirmation", "Are you sure?", tcs));

                await tcs.Task.ContinueWith(antecedent =>
                {
                    if (antecedent.IsCompleted)
                    {
                        if (antecedent.Result)
                        {
                            isYes = true;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, ex.Message);
            }

            if (isYes)
            {
                // Clear internal logs.
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.DebugLogsAppSettingsTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.DebugLogsAppSettingsTable>();
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.DebugLogsTagsDataResetTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.DebugLogsTagsDataResetTable>();
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.DebugLogsNdefAccessTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.DebugLogsNdefAccessTable>();
                await App.DatabaseService.DBase.DropTableAsync<Services.DatabaseService.DebugLogsExceptionsTable>();
                await App.DatabaseService.DBase.CreateTableAsync<Services.DatabaseService.DebugLogsExceptionsTable>();

                await PopupNavigation.Instance.PushAsync(new OkPopup(ImageSource.FromResource("TLogger.Images.ok.png"),
                    string.Empty, "Logs have been cleared"));
            }
        }

    }
}
