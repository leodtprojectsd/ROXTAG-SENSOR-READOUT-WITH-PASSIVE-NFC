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

using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TLogger.ViewModels
{
    class FeedbackViewModel : ObservableObject
    {
        public FeedbackViewModel()
        {
            TitleText = "FEEDBACK";

            SupportCommand = new Command(async () => await OnSupportCommandAsync());
            BizDevCommand = new Command(async () => await OnBizDevCommandAsync());
        }

        string _titleText;
        public string TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        public Command SupportCommand { get; }
        async Task OnSupportCommandAsync()
        {
            try
            {
                _ = Launcher.OpenAsync(new Uri("mailto:nhs-support@nxp.com"));
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, ex.Message);
            }

            await Task.CompletedTask;
        }

        public Command BizDevCommand { get; }
        async Task OnBizDevCommandAsync()
        {
            try
            {
                _ = Launcher.OpenAsync(new Uri("mailto:nhs-info@nxp.com"));
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, ex.Message);
            }

            await Task.CompletedTask;
        }
    }
}
