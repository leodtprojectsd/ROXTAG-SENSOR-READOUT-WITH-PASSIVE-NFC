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
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TLogger.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageView
    {
        readonly List<ToolbarItem> _secondaryToolbarItems;

        public MainPageView ()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                // Add iOS refresh first.
                ToolbarItems.Insert(0, new ToolbarItem(null, "reset_color.png", OnRefresh, ToolbarItemOrder.Primary));
                _secondaryToolbarItems = ToolbarItems.Where(x => x.Order == ToolbarItemOrder.Secondary).ToList();
                ToolbarItems.Add(new ToolbarItem(null, "ellipsisv.png", ToggleSecondaryToolbarItems, ToolbarItemOrder.Primary));
                ToggleSecondaryToolbarItems();
            }
            else if (Device.RuntimePlatform == Device.macOS)
            {
                _secondaryToolbarItems = ToolbarItems.Where(x => x.Order == ToolbarItemOrder.Secondary).ToList();
                ToolbarItems.Add(new ToolbarItem(null, "ellipsisv.png", ToggleSecondaryToolbarItems, ToolbarItemOrder.Primary));
                ToggleSecondaryToolbarItems();
            }

            var osVersion = Convert.ToInt32(DeviceInfo.VersionString.Split('.')[0]);
            if (!(Device.RuntimePlatform == Device.iOS && osVersion < 13))
            {
                // Configuration page is not supported on iOS < 13.
                Children.Insert(2, new ConfigurationPageView());
            }

            new ViewModels.MainViewModel();
            App.MainPageView = this;
        }


        async void OnRefresh()
        {
            await App.MsgLib.IosRefreshCmdAsync();
        }

        async void OnAppSettings(object sender, EventArgs e)
        {
            await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(new Popups.AppSettingsPopup());
            if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.macOS)
                ToggleSecondaryToolbarItems();
        }

        void ToggleSecondaryToolbarItems()
        {
            var secondaryToolbarItems = ToolbarItems.Where(x => x.Order == ToolbarItemOrder.Secondary).ToList();

            if (secondaryToolbarItems.Count > 0)
                foreach (var item in secondaryToolbarItems)
                {
                    ToolbarItems.Remove(item);
                }
            else
            {
                foreach (var item in _secondaryToolbarItems)
                {
                    ToolbarItems.Add(item);
                }
            }
        }
    }
}
