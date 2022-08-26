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

using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace TLogger.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DataProcessPopup : PopupPage
    {
		public DataProcessPopup()
		{
			InitializeComponent();
		}

        protected override bool OnBackgroundClicked()
        {
            return false;
        }

        public void SetDataRetrieval(int current, int total)
        {
            DataProcessLabel.Text = $"Retrieved {current} measurements of {total}";
            DataProcessProgressBar.Progress = (double)current / (double)total;
        }

        async public Task TerminateAsync()
        {
            try
            {
                await PopupNavigation.Instance.PopAsync(true);
            }
            catch (Exception ex)
            {
                Helpers.ExceptionLogHelper.Log(App.MsgLib.TagModel.TagId, Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Caught exception:" + App.MsgLib.TagModel.TagId + " - " + Helpers.ExceptionLogHelper.GetCurrentMethod() + " - " + ex.Message);
            }
        }
    }
}
