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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace TLogger.Views
{
	public class TemperatureHistoryPageViewCS : CarouselPage
	{
        const string STATUS_LABEL_MESSAGE_NOT_SUPPORTED =
            "Firmware not supported. Please upgrade and try again";
        const string STATUS_LABEL_MESSAGE_DEVICE_DISABLED =
            "Invalid NDEF message";
        const string STATUS_LABEL_MESSAGE_DEVICE_PRISTINE_STATE =
            "In Pristine State";
        const string STATUS_LABEL_MESSAGE_DEVICE_RECORDING =
            "The tag is configured and is logging.";
        const string STATUS_LABEL_MESSAGE_DEVICE_STOPPED_BATTERY_DIED =
            "Stopped as battery died";
        const string STATUS_LABEL_MESSAGE_DEVICE_STOPPED_STORAGE_FULL =
            "Stopped as storage is full";
        const string STATUS_LABEL_MESSAGE_DEVICE_STOPPED_DEMO_EXPIRED =
            "Stopped as configured running time has expired";
        const string STATUS_LABEL_MESSAGE_DEVICE_UNSUPPORTED =
            "Unsupported tag, please tap an NTAG SmartSensor.";
        const string STATUS_APP_MSG_EVENT_EXPIRED =
            "Logging was stopped after the configured running time.";
        const string STATUS_APP_MSG_EVENT_FULL =
            "Logging has stopped because no more free space is available to store samples.";
        const string STATUS_APP_MSG_EVENT_BOD =
            "Battery is (nearly) depleted.";
        const string STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_LOW =
            "At least one temperature value was lower than the valid minimum value.";
        const string STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_HIGH =
            "At least one temperature value was higher than the valid maximum value.";
        const string STATUS_APP_MSG_EVENT_STOPPED =
            "The tag is configured and has been logging. Now it has stopped logging.";
        const string STATUS_APP_MSG_EVENT_LOGGING =
            "The tag is configured and is logging. At least one sample is available.";
        const string STATUS_APP_MSG_EVENT_STARTING =
            "The tag is configured and will make a first measurement after the configured delay.";
        const string STATUS_APP_MSG_EVENT_CONFIGURED =
            "The tag is configured, but requires an external trigger to start measuring.";
        const string STATUS_APP_MSG_EVENT_PRISTINE =
            "The tag is not yet configured and contains no data.";
        readonly Services.DatabaseService.TagsStatusTable _status;

        ContentPage _temperatureStatusPage;
        ContentPage _temperatureChartPage;
        ContentPage _temperatureGraphPage;

        public TemperatureHistoryPageViewCS(Services.DatabaseService.TagsStatusTable status)
		{
            _status = status;
            try
            {
                NavigationPage.SetHasNavigationBar(this, true);
                if(Device.RuntimePlatform == Device.macOS)
                    // MacOS issue:
                    // https://github.com/xamarin/Xamarin.Forms/issues/5434
                    // Navigation has been disabled on MacOS till this issue is resolved.
                    NavigationPage.SetHasBackButton(this, false);
                else
                    NavigationPage.SetHasBackButton(this, true);


                Title = "HISTORY";

                ShowStatusPage();
                ShowChartPage();
                ShowGraphPage();
            }
            catch (Exception)
            {
            }
        }

        void ShowStatusPage()
        {
            try
            {
                (var statusIcon, var statusText) = PrepareStatus();

                var statusPage = new ContentPage();
                _temperatureStatusPage = statusPage;
                statusPage.BackgroundColor = Color.White;

                var statusGrid = new Grid();
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(34, GridUnitType.Star) });
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Star) });
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(28, GridUnitType.Star) });
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(13, GridUnitType.Star) });
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
                statusGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                var statusImage = new Image
                {
                    Source = statusIcon,
                    Aspect = Aspect.AspectFit,
                };

                var statusLabel = new Label
                {
                    Text = statusText,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(10, 0),
                    TextColor = Color.FromHex("#8BAED8"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                };

                var ndefTextLabel = new Label
                {
                    Text = PrepareNdefText(),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#8BAED8"),
                    //FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                };
                var ndefTextStack = new StackLayout { };

                var mimeTypeLabel = new Label
                {
                    Text = _status.NdefMimeType,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#E8B410"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var versionLabel = new Label
                {
                    Text = 
                        $"SW:{_status.Version.FwVersion}  " +
                        $"API:{_status.Version.ApiVersion}  " +
                        $"APP:{AppInfo.VersionString}",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#E8B410"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var mimeTypeAndVersionStack = new StackLayout
                {
                    VerticalOptions = LayoutOptions.End,
                };

                var page1Image = new Image
                {
                    Source = ImageSource.FromResource("TLogger.Images.page1.png"),
                    Aspect = Aspect.AspectFit,
                };

                Device.BeginInvokeOnMainThread(() =>
                {
                    statusGrid.Children.Add(statusImage, 0, 1);

                    statusLabel.FontAttributes = FontAttributes.Bold;
                    statusLabel.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    statusGrid.Children.Add(statusLabel, 0, 2);

                    ndefTextLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    ndefTextStack.Children.Add(ndefTextLabel);
                    statusGrid.Children.Add(ndefTextStack, 0, 3);

                    mimeTypeLabel.FontAttributes = FontAttributes.Bold;
                    mimeTypeLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    mimeTypeAndVersionStack.Children.Add(mimeTypeLabel);
                    versionLabel.FontAttributes = FontAttributes.Bold;
                    versionLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    mimeTypeAndVersionStack.Children.Add(versionLabel);
                    statusGrid.Children.Add(mimeTypeAndVersionStack, 0, 4);

                    statusGrid.Children.Add(page1Image, 0, 5);

                    // MacOS carousel page does not support swiping, adding it here.
                    if (Device.RuntimePlatform == Device./*UWP*/macOS)
                    {
                        // MacOS issue:
                        // https://github.com/xamarin/Xamarin.Forms/issues/5434
                        // Navigation has been disabled on MacOS till this issue is resolved.
                        // Left arrow is used as back button.
                        // Status page.
                        var statusLeftGestureRecognizer = new TapGestureRecognizer();
                        statusLeftGestureRecognizer.Tapped += (s, ev) =>
                        {
                            //CurrentPage = _temperatureStatusPage;
                            Navigation.PopAsync();
                        };
                        var statusLeftImage = new Image
                        {
                            Source = "arrow_left.png",
                            Aspect = Aspect.AspectFit,
                        };
                        statusLeftImage.GestureRecognizers.Add(statusLeftGestureRecognizer);

                        var statusRightGestureRecognizer = new TapGestureRecognizer();
                        statusRightGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = _temperatureChartPage;
                        };
                        var statusRightImage = new Image
                        {
                            Source = "arrow_right.png",
                            Aspect = Aspect.AspectFit,
                        };
                        statusRightImage.GestureRecognizers.Add(statusRightGestureRecognizer);
                        var statusSwipeGrid = new Grid();
                        statusSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                        statusSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Star) });
                        statusSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                        statusSwipeGrid.Children.Add(statusLeftImage, 0, 0);
                        statusSwipeGrid.Children.Add(statusRightImage, 2, 0);
                        statusGrid.Children.Add(statusSwipeGrid, 0, 5);

                    }

                    statusPage.Content = statusGrid;
                    Children.Add(statusPage);
                });
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }


        }

        void ShowChartPage()
        {
            try
            {
                var chartPage = new ContentPage
                {
                    BackgroundColor = Color.White
                };
                _temperatureChartPage = chartPage;

                Color textColor = Color.FromHex("#E8B410");
                string statusText = GetSimplifiedStatus();
                string minLimitText = string.Empty;
                string maxLimitText = string.Empty;
                bool isCelsius =
                    App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;
                string temperatureUnit = isCelsius ? "°C" : "°F";
                var configMin = _status.ConfigTable.TemperatureConfig.ConfiguredMin;
                var configMax = _status.ConfigTable.TemperatureConfig.ConfiguredMax;
                if (!isCelsius)
                {
                    configMin = configMin * 9 / 5 + 32;
                    configMax = configMax * 9 / 5 + 32;
                }
                minLimitText = $"{configMin} {temperatureUnit}";
                maxLimitText = $"{configMax} {temperatureUnit}";
                if (statusText == "OK")
                    textColor = Color.FromHex("#E8B410");
                else
                    textColor = Color.FromHex("#AD4749");


                var chartGrid = new Grid();
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35, GridUnitType.Star) });
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20, GridUnitType.Star) });
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(42, GridUnitType.Star) });
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
                chartGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                var label1 = new Label
                {
                    Text = "NFC ID",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frameLabel1 = new Label
                {
                    Text = Regex.Replace(_status.ConfigTable.TagId, ".{2}", "$0:").TrimEnd(new char[] { ':' }),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frame1 = new Frame
                {
                    Margin = new Thickness(15, 0),
                    BackgroundColor = Color.FromHex("#F3FACF"),
                    BorderColor = Color.FromHex("#E8B410"),
                    HasShadow = false,
                    Padding = new Thickness(5, 10),
                };
                var label2 = new Label
                {
                    Text = "Number of measurements",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frameLabel2 = new Label
                {
                    Text = $"{_status.MeasurementStatus.NumberOfMeasurements}",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frame2 = new Frame
                {
                    Margin = new Thickness(15, 0),
                    BackgroundColor = Color.FromHex("#F3FACF"),
                    BorderColor = Color.FromHex("#E8B410"),
                    HasShadow = false,
                    Padding = new Thickness(5, 10),
                };
                var label3 = new Label
                {
                    Text = "Configuration time",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frameLabel3 = new Label
                {
                    Text = Helpers.RtcAccuracyHelper.CorrectedLocalTimestamp(
                        _status.MeasurementStatus.ConfigTime, _status.MeasurementStatus.ConfigTime, _status.Drift).
                        ToString(App.AppSettingsService.DateTimeFormat),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frame3 = new Frame
                {
                    Margin = new Thickness(15, 0),
                    BackgroundColor = Color.FromHex("#F3FACF"),
                    BorderColor = Color.FromHex("#E8B410"),
                    HasShadow = false,
                    Padding = new Thickness(5, 10),
                };
                var label4 = new Label
                {
                    Text = "Logging for",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frameLabel4 = new Label
                {
                    Text = Helpers.TimeSpanHelper.ShortReadableTimeSpan(
                            Helpers.RtcAccuracyHelper.CorrectedTimeSpan(
                                _status.MeasurementStatus.RunningDuration), 2),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                };
                var frame4 = new Frame
                {
                    Margin = new Thickness(15, 0),
                    BackgroundColor = Color.FromHex("#F3FACF"),
                    BorderColor = Color.FromHex("#E8B410"),
                    HasShadow = false,
                    Padding = new Thickness(5, 10),
                };

                var chartWeb = new WebView()
                {
                    Source = PrepareChart(),
                };

                var grid1 = new Grid
                {
                    RowSpacing = 0,
                };
                var stack1 = new StackLayout { };

                var label211 = new Label
                {
                    Text = $"{GetNumRetrieved()}",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = textColor,
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 16,
                };
                var label212 = new Label
                {
                    Text = "values have been retrieved.",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 14,
                };
                var stack21 = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var label221 = new Label
                {
                    Text = "Status of the measured values:",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 14,
                };
                var label222 = new Label
                {
                    Text = statusText,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = textColor,
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 16,
                };
                var stack22 = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var label231 = new Label
                {
                    Text = "Measurement interval: every",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 14,
                };
                var label232 = new Label
                {
                    Text = Helpers.TimeSpanHelper.ReadableTimeSpan(_status.ConfigTable.MeasurementConfig.Interval, 2),
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = textColor,
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 16,
                };
                var stack23 = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var label241 = new Label
                {
                    Text = "Minimum valid value:",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 14,
                };
                var label242 = new Label
                {
                    Text = minLimitText,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = textColor,
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 16,
                };
                var stack24 = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var label251 = new Label
                {
                    Text = "Maximum valid value:",
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = Color.FromHex("#7A7D30"),
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 14,
                };
                var label252 = new Label
                {
                    Text = maxLimitText,
                    HorizontalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalOptions = LayoutOptions.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    TextColor = textColor,
                    //FontAttributes = FontAttributes.Bold,
                    //FontSize = 16,
                };
                var stack25 = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center,
                };
                var stack2 = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                };

                var page2Image = new Image
                {
                    Source = ImageSource.FromResource("TLogger.Images.page2.png"),
                    Aspect = Aspect.AspectFit,
                };

                Device.BeginInvokeOnMainThread(() =>
                {
                    label1.FontAttributes = FontAttributes.Bold;
                    label1.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    grid1.Children.Add(label1, 0, 0);
                    frameLabel1.FontAttributes = FontAttributes.Bold;
                    frameLabel1.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    frame1.Content = frameLabel1;
                    grid1.Children.Add(frame1, 0, 1);

                    label2.FontAttributes = FontAttributes.Bold;
                    label2.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    grid1.Children.Add(label2, 1, 0);
                    frameLabel2.FontAttributes = FontAttributes.Bold;
                    frameLabel2.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    frame2.Content = frameLabel2;
                    grid1.Children.Add(frame2, 1, 1);

                    label3.FontAttributes = FontAttributes.Bold;
                    label3.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    grid1.Children.Add(label3, 0, 2);
                    frameLabel3.FontAttributes = FontAttributes.Bold;
                    frameLabel3.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    frame3.Content = frameLabel3;
                    grid1.Children.Add(frame3, 0, 3);

                    label4.FontAttributes = FontAttributes.Bold;
                    label4.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    grid1.Children.Add(label4, 1, 2);
                    frameLabel4.FontAttributes = FontAttributes.Bold;
                    frameLabel4.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
                    frame4.Content = frameLabel4;
                    grid1.Children.Add(frame4, 1, 3);

                    stack1.Children.Add(grid1);
                    chartGrid.Children.Add(stack1, 0, 0);

                    chartGrid.Children.Add(chartWeb, 0, 1);

                    label211.FontAttributes = FontAttributes.Bold;
                    label211.FontSize = 16;
                    stack21.Children.Add(label211);
                    label212.FontAttributes = FontAttributes.Bold;
                    label212.FontSize = 14;
                    stack21.Children.Add(label212);

                    label221.FontAttributes = FontAttributes.Bold;
                    label221.FontSize = 14;
                    stack22.Children.Add(label221);
                    label222.FontAttributes = FontAttributes.Bold;
                    label222.FontSize = 16;
                    stack22.Children.Add(label222);

                    label231.FontAttributes = FontAttributes.Bold;
                    label231.FontSize = 14;
                    stack23.Children.Add(label231);
                    label232.FontAttributes = FontAttributes.Bold;
                    label232.FontSize = 16;
                    stack23.Children.Add(label232);

                    label241.FontAttributes = FontAttributes.Bold;
                    label241.FontSize = 14;
                    stack24.Children.Add(label241);
                    label242.FontAttributes = FontAttributes.Bold;
                    label242.FontSize = 16;
                    stack24.Children.Add(label242);

                    label251.FontAttributes = FontAttributes.Bold;
                    label251.FontSize = 14;
                    stack25.Children.Add(label251);
                    label252.FontAttributes = FontAttributes.Bold;
                    label252.FontSize = 16;
                    stack25.Children.Add(label252);

                    stack2.Children.Add(stack21);
                    stack2.Children.Add(stack22);
                    stack2.Children.Add(stack23);
                    stack2.Children.Add(stack24);
                    stack2.Children.Add(stack25);
                    chartGrid.Children.Add(stack2, 0, 2);

                    chartGrid.Children.Add(page2Image, 0, 3);

                    // MacOS carousel page does not support swiping, adding it here.
                    if (Device.RuntimePlatform == Device./*UWP*/macOS)
                    {
                        // Chart page.
                        var chartLeftGestureRecognizer = new TapGestureRecognizer();
                        chartLeftGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = _temperatureStatusPage;
                        };
                        var chartLeftImage = new Image
                        {
                            Source = "arrow_left.png",
                            Aspect = Aspect.AspectFit,
                        };
                        chartLeftImage.GestureRecognizers.Add(chartLeftGestureRecognizer);
                        var chartRightGestureRecognizer = new TapGestureRecognizer();
                        chartRightGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = _temperatureGraphPage;
                        };
                        var chartRightImage = new Image
                        {
                            Source = "arrow_right.png",
                            Aspect = Aspect.AspectFit,
                        };
                        chartRightImage.GestureRecognizers.Add(chartRightGestureRecognizer);
                        var chartSwipeGrid = new Grid();
                        chartSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                        chartSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Star) });
                        chartSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                        chartSwipeGrid.Children.Add(chartLeftImage, 0, 0);
                        chartSwipeGrid.Children.Add(chartRightImage, 2, 0);
                        chartGrid.Children.Add(chartSwipeGrid, 0, 3);
                    }


                    chartPage.Content = chartGrid;
                    Children.Add(chartPage);
                });
            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }

        }

        void ShowGraphPage()
        {
            try
            {
                var retrieved = GetNumRetrieved();
                var infoText = $"{retrieved} values in " +
                    Helpers.TimeSpanHelper.ShortReadableTimeSpan(
                        Helpers.RtcAccuracyHelper.CorrectedTimeSpan(
                            _status.MeasurementStatus.RunningDuration), 2) +
                            (retrieved < _status.MeasurementStatus.NumberOfMeasurements ?
                                " - not all data have been read" : "");

                var graphPage = new ContentPage();
                _temperatureGraphPage = graphPage;
                graphPage.BackgroundColor = Color.White;

                var graphGrid = new Grid();
                graphGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80, GridUnitType.Star) });
                graphGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5, GridUnitType.Star) });
                graphGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(12, GridUnitType.Star) });
                graphGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
                graphGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                var graphWeb = new WebView
                {
                    Source = PrepareGraph()
                };

                var graphLabel = new Label
                {
                    Text = infoText,
                    HorizontalOptions = LayoutOptions.Center,
                };

                var graphButton = new Button
                {
                    Text = "EXPORT TEMPERATURE VALUES",
                    BackgroundColor = Color.FromHex("#E8B410"),
                    TextColor = Color.White,
                    Margin = new Thickness(20, 5),
                };
                graphButton.Clicked += OnButtonClickAsync;

                var graphImage = new Image
                {
                    Source = ImageSource.FromResource("TLogger.Images.page3.png"),
                    Aspect = Aspect.AspectFit,
                };

                Device.BeginInvokeOnMainThread(() =>
                {
                    graphLabel.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));


                    graphGrid.Children.Add(graphWeb, 0, 0);
                    graphGrid.Children.Add(graphLabel, 0, 1);
                    graphGrid.Children.Add(graphButton, 0, 2);
                    graphGrid.Children.Add(graphImage, 0, 3);

                    // MacOS carousel page does not support swiping, adding it here.
                    if (Device.RuntimePlatform == Device./*UWP*/macOS)
                    {
                        // Graph page.
                        var graphLeftGestureRecognizer = new TapGestureRecognizer();
                        graphLeftGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = _temperatureChartPage;
                        };
                        var graphLeftImage = new Image
                        {
                            Source = "arrow_left.png",
                            Aspect = Aspect.AspectFit,
                        };
                        graphLeftImage.GestureRecognizers.Add(graphLeftGestureRecognizer);

                        var graphSwipeGrid = new Grid();
                        graphSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                        graphSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Star) });
                        graphSwipeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                        graphSwipeGrid.Children.Add(graphLeftImage, 0, 0);
                        graphGrid.Children.Add(graphSwipeGrid, 0, 3);
                    }


                    graphPage.Content = graphGrid;

                    Children.Add(graphPage);
                });

            }
            catch (Exception ex)
            {
                var x = ex.Message;
            }
        }

        (ImageSource, string) PrepareStatus()
        {
            ImageSource icon = null;
            string text = string.Empty;

            var status = _status.MeasurementStatus.Measurement;
            switch (status)
            {
                case Msg.Models.MeasurementStatusModel.Measurement.Reset:
                case Msg.Models.MeasurementStatusModel.Measurement.Unknown:
                    icon = ImageSource.FromResource("TLogger.Images.UNKNOWN_STATE.png");
                    text = STATUS_LABEL_MESSAGE_NOT_SUPPORTED;
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.NotConfigured:
                    icon = ImageSource.FromResource("TLogger.Images.PRISTINE_STATE.png");
                    text = STATUS_APP_MSG_EVENT_PRISTINE;
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Starting:
                    icon = ImageSource.FromResource("TLogger.Images.STARTING_STATE.png");
                    text = STATUS_APP_MSG_EVENT_STARTING;
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Configured:
                    icon = ImageSource.FromResource("TLogger.Images.CONFIGURED_STATE.png");
                    text = STATUS_APP_MSG_EVENT_CONFIGURED;
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Logging:
                    if (_status.TemperatureStatus.Temperature ==
                        Msg.Models.TemperatureStatusModel.Temperature.Low)
                    {
                        icon = ImageSource.FromResource("TLogger.Images.TEMP_TOO_LOW_STATE.png");
                        text = STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_LOW;
                    }
                    else if (_status.TemperatureStatus.Temperature ==
                        Msg.Models.TemperatureStatusModel.Temperature.High)
                    {
                        icon = ImageSource.FromResource("TLogger.Images.TEMP_TOO_LOW_STATE.png");
                        text = STATUS_APP_MSG_EVENT_TEMPERATURE_TOO_HIGH;
                    }
                    else
                    {
                        icon = ImageSource.FromResource("TLogger.Images.LOGGING_STATE.png");
                        text = STATUS_APP_MSG_EVENT_LOGGING;
                    }
                    break;
                case Msg.Models.MeasurementStatusModel.Measurement.Stopped:
                    // Check failure states.
                    switch (_status.MeasurementStatus.Failure)
                    {
                        case Msg.Models.MeasurementStatusModel.Failure.NoFailure:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_STATE.png");
                            text = STATUS_APP_MSG_EVENT_STOPPED;
                            break;
                        case Msg.Models.MeasurementStatusModel.Failure.Bod:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_BROWN_OUT.png");
                            text = STATUS_APP_MSG_EVENT_BOD;
                            break;
                        case Msg.Models.MeasurementStatusModel.Failure.Full:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_DISK_FULL.png");
                            text = STATUS_APP_MSG_EVENT_FULL;
                            break;
                        case Msg.Models.MeasurementStatusModel.Failure.Expired:
                            icon = ImageSource.FromResource("TLogger.Images.STOPPED_EXPIRED.png");
                            text = STATUS_APP_MSG_EVENT_EXPIRED;
                            break;
                    }
                    break;
            }

            return (icon, text);
        }

        string PrepareNdefText()
        {
            var jsonNdefText = JsonConvert.DeserializeObject<string[]>(_status.JsonNdefText);

            var ndefText = string.Empty;

            foreach (var text in jsonNdefText)
            {
                var t = text;
                if (App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Fahrenheit)
                {
                    // Use regex to replace C into F.
                    t = Regex.Replace(t, @"[+-]*\d{1,3}\.\dC", m =>
                    {
                        var c = Convert.ToSingle(m.Value.Replace("C", ""), Helpers.GlobalHelper.FormatProvider);
                        var f = c * 9 / 5 + 32;
                        var s = f.ToString("0.0", Helpers.GlobalHelper.FormatProvider) + "F";
                        return s;
                    });
                }

                ndefText += t;
                ndefText += Environment.NewLine;
            }

            return ndefText;
        }

        int GetNumRetrieved()
        {
            int retrieved = 0;
            foreach (var range in _status.TemperatureDataRanges)
            {
                var diff = range.End - range.Begin;
                retrieved += diff + (diff == 0 ? 0 : 1);
            }

            return retrieved;
        }

        string GetSimplifiedStatus()
        {
            if (_status.TemperatureStatus.Temperature ==
                Msg.Models.TemperatureStatusModel.Temperature.High) return "NOT OK";
            else if (_status.TemperatureStatus.Temperature ==
                Msg.Models.TemperatureStatusModel.Temperature.Low) return "NOT OK";
            else if (_status.MeasurementStatus.Failure == Msg.Models.MeasurementStatusModel.Failure.Bod)
                return "OK";
            else if (_status.MeasurementStatus.Failure == Msg.Models.MeasurementStatusModel.Failure.Full)
                return "OK";
            else if (_status.MeasurementStatus.Failure ==
                Msg.Models.MeasurementStatusModel.Failure.Expired) return "OK";

            if (_status.MeasurementStatus.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Stopped) return "OK";
            else if (_status.MeasurementStatus.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Logging) return "OK";
            else if (_status.MeasurementStatus.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Starting) return "OK";
            else if (_status.MeasurementStatus.Measurement ==
                Msg.Models.MeasurementStatusModel.Measurement.Configured) return "NOT OK";

            return "NOT OK";
        }

        HtmlWebViewSource PrepareChart()
        {
            (float minLimit, float maxLimit, string colorRanges, string pointers, string trendPoints) =
                GetTemperatureParams();

            var minLimitStr = minLimit.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var maxLimitStr = maxLimit.ToString("0.#", Helpers.GlobalHelper.FormatProvider);

            string htmlContent =
                $@"
                    <html>
                        <head>
                            <script type='text/javascript' src='fusioncharts.js'></script>
                            <meta name='viewport' content='width=device-width,initial-scale=1,maximum-scale=1'/>
                        </head>
                        <body>
                            <div id='container'>Preparing graph...</div>
                            <script type='text/javascript'>
                            new FusionCharts({{
                                type: 'hlineargauge',
                                renderAt: 'container',
                                width: '100%',
                                height: '100%',
                                dataFormat: 'json',
                                dataSource: {{
                                    chart: {{
                                        theme: 'fint',
                                        bgColor: '#ffffff',
                                        showBorder: '0',
                                        lowerLimit: {minLimitStr},
                                        upperLimit: {maxLimitStr},
                                        chartBottomMargin: 0,
                                        chartTopMargin: 0,
                                        valueFontSize: 12,
                                        valueFontBold: '0',
                                        gaugeFillMix: '{{light-10}},{{light-70}},{{dark-10}}',
                                        gaugeFillRatio: '40,20,40',
                                        showTickMarks: '1',
                                        showTickValues: '1',
                                        pointerOnTop: '1',
                                        valueAbovePointer: '0',
                                        majorTMNumber: 3,
                                        placeValuesInside: '1',
                                    }},
                                    colorRange: {{
                                        color: {colorRanges},
                                    }},
                                    pointers: {{
                                        pointer: {pointers},
                                    }},
                                    trendPoints: {{
                                        point: {trendPoints},
                                    }},
                                }}
                            }}).render();
                            </script>
                        </body>
                    </html>
                    ";

            var baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();
            if (Device.RuntimePlatform == Device.UWP)
            {
                baseUrl += "tlogger_chart.html";
            }

            return new HtmlWebViewSource
            {
                    Html = htmlContent,
                    BaseUrl = baseUrl,
            };
        }

        class ColorRange
        {
            public float minValue { get; set; }
            public float maxValue { get; set; }
            public string label { get; set; }
            public string code { get; set; }
        }

        class Pointer
        {
            public float value { get; set; }
            public string displayValue { get; set; }
        }

        class TrendPoint
        {
            public float startValue { get; set; }
            public float endValue { get; set; }
            public string displayValue { get; set; }
            public int alpha { get; set; }
            public string color { get; set; }
            public string dashed { get; set; }  // bool in fusion charts
            public string showOnTop { get; set; }   // bool in fusion charts
            public string useMarker { get; set; }   // bool in fusion charts
            public string markerColor { get; set; }
            public int markerRadius { get; set; }
        }

        const int MarkerRadius = 15;

        (float minLimit, float maxLimit, string colorRanges, string pointers, string trendPoints)
            GetTemperatureParams()
        {
            const float MinLimit = -40.0f;
            const float MaxLimit = 85.0f;

            float minLimit = MinLimit;
            float maxLimit = MaxLimit;
            var colorRangeList = new List<ColorRange>();
            var pointerList = new List<Pointer>();
            var trendPointList = new List<TrendPoint>();

            bool isCelsius =
                App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;

            if (_status.TemperatureStatus.Temperature ==
                    Msg.Models.TemperatureStatusModel.Temperature.Reset ||
                _status.TemperatureStatus.Temperature ==
                    Msg.Models.TemperatureStatusModel.Temperature.Unknown)
            {
                if (!isCelsius)
                {
                    minLimit = minLimit * 9 / 5 + 32;
                    maxLimit = maxLimit * 9 / 5 + 32;
                }

                // Not configured.
                colorRangeList.Add(new ColorRange
                {
                    minValue = minLimit,
                    maxValue = maxLimit,
                    label = "Not configured",
                    code = "#babec4",
                });
            }
            else
            {
                // Configured.
                var configMin = (float)_status.ConfigTable.TemperatureConfig.ConfiguredMin;
                var configMax = (float)_status.ConfigTable.TemperatureConfig.ConfiguredMax;
                var recMin = (float)_status.TemperatureStatus.RecordedMin;
                var recMax = (float)_status.TemperatureStatus.RecordedMax;
                minLimit = configMin;
                maxLimit = configMax;
                if (_status.TemperatureStatus.IsRecorded)
                {
                    if (recMin < configMin) minLimit = recMin;
                    if (recMax > configMax) maxLimit = recMax;
                }
                minLimit -= 5.0f;
                maxLimit += 5.0f;
                if (minLimit < MinLimit) minLimit = MinLimit;
                if (maxLimit > MaxLimit) maxLimit = MaxLimit;

                if (!isCelsius)
                {
                    minLimit = minLimit * 9 / 5 + 32;
                    maxLimit = maxLimit * 9 / 5 + 32;
                    recMin = recMin * 9 / 5 + 32;
                    recMax = recMax * 9 / 5 + 32;
                    configMin = configMin * 9 / 5 + 32;
                    configMax = configMax * 9 / 5 + 32;
                }

                colorRangeList.AddRange(new List<ColorRange>
                {
                    new ColorRange
                    {
                        minValue = minLimit,
                        maxValue = configMin,
                        label = "Low",
                        ////code = "#D9E2F1",
                        code =  "#ff6666",
                    },
                    new ColorRange
                    {
                        minValue = configMin,
                        maxValue = configMax,
                        label = "Acceptable",
                        code = "#70db70",
                    },
                    new ColorRange
                    {
                        minValue = configMax,
                        maxValue = maxLimit,
                        label =  "High",
                        code = "#ff6666",
                    },
                });

                pointerList.AddRange(new List<Pointer>
                {
                    new Pointer
                    {
                        value = configMin,
                        displayValue = configMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                    },
                    new Pointer
                    {
                        value = configMax,
                        displayValue = configMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                    },
                });

                if (_status.TemperatureStatus.IsRecorded)
                {
                    trendPointList.AddRange(new List<TrendPoint>
                    {
                        new TrendPoint
                        {
                            startValue = recMin,
                            endValue = recMin,
                            displayValue = recMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                            alpha = 100,
                            color = "#264d00",
                            dashed = "1",
                            showOnTop = "0",
                            useMarker = "1",
                            markerColor = "#ffffff",
                            markerRadius = MarkerRadius,
                        },
                        new TrendPoint
                        {
                            startValue = recMax,
                            endValue = recMax,
                            displayValue = recMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider),
                            alpha = 100,
                            color = "#264d00",
                            dashed = "1",
                            showOnTop = "0",
                            useMarker = "1",
                            markerColor = "#ffffff",
                            markerRadius = MarkerRadius,
                        },
                        new TrendPoint
                        {
                            startValue = recMin,
                            endValue = recMax,
                            displayValue = " ",
                            alpha = 50,
                            color = "#264d00",
                            dashed = "0",
                            showOnTop = "0",
                            useMarker = "1",
                            markerColor = "#ffffff",
                            markerRadius = MarkerRadius,
                        },
                    });
                }

            }

            string colorRanges = JsonConvert.SerializeObject(colorRangeList);
            string pointers = JsonConvert.SerializeObject(pointerList);
            string trendPoints = JsonConvert.SerializeObject(trendPointList);
            return (minLimit, maxLimit, colorRanges, pointers, trendPoints);
        }


        HtmlWebViewSource PrepareGraph()
        {
            var schema = GetSchema();
            (string data, string temperatureUnit, float temperatureMin, float temperatureMax, float temperatureLimitMin,
                float temperatureLimitMax) = GetData();

            var temperatureMinStr = temperatureMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var temperatureMaxStr = temperatureMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var temperatureLimitMinStr = temperatureLimitMin.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
            var temperatureLimitMaxStr = temperatureLimitMax.ToString("0.#", Helpers.GlobalHelper.FormatProvider);

            string htmlContent =
                $@"
                <html>
                    <head>
                        <script type='text/javascript' src='fusioncharts.js'></script>
                        <meta name='viewport' content='width=device-width,initial-scale=1,maximum-scale=1'/>
                    </head>
                    <body>
                        <div id='container'>Preparing graphs...</div>
                        <script type='text/javascript'>
                        var schema = {schema};
                        var data = {data};
                        var events = [{{ }}];
                        var dataTable = new FusionCharts.DataStore().createDataTable(data, schema);
                        new FusionCharts({{
                            type: 'timeseries',
                            width: '100%',
                            height: '100%',
                            renderAt: 'container',
                            dataSource: {{
                                caption: {{
                                    text: 'Recorded Temperature Values'
                                }},
                                data: dataTable,
                                xaxis: [{{
                                    plot: 'Time',
                                    timeMarker: events
                                }}],
                                yAxis: [{{
                                    plot: [{{
                                        value: 'Temperature',
                                        type: 'area'
                                    }}],
                                    format: {{
                                        suffix: '{temperatureUnit}'
                                    }},
                                    title: '',
                                    min: {temperatureMinStr},
                                    max: {temperatureMaxStr},
                                    referenceLine: [{{
                                        value: {temperatureLimitMinStr}
                                    }}, {{
                                        value: {temperatureLimitMaxStr}
                                    }}]
                                }}],
                            }}
                        }}).render();
                        </script>
                    </body>
                </html>
                ";

            var baseUrl = DependencyService.Get<Interfaces.IBaseUrl>().Get();
            if (Device.RuntimePlatform == Device.UWP)
            {
                baseUrl += "tlogger_graph.html";
            }

            return new HtmlWebViewSource
            {
                Html = htmlContent,
                BaseUrl = baseUrl,
            };
        }

        class Schema
        {
            public string name { get; set; }
            public string type { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string format { get; set; }
        };

        string GetSchema()
        {
            string timestampFormatJs = "%Y-%m-%d %H:%M:%S";

            var schemaList = new List<Schema>();
            schemaList.AddRange(new List<Schema> {
                new Schema
                {
                    name = "SeqNo",
                    type = "number",
                },
                new Schema
                {
                    name = "Time",  // Do NOT use Timestamp as name, this causes problems!!!
                    type = "date",
                    format = $"{timestampFormatJs}",
                },
                new Schema
                {
                    name = "Temperature",
                    type = "number",
                }
             });

            var schema = JsonConvert.SerializeObject(schemaList);
            return schema;
        }

        class Data
        {
            public int seqNo { get; set; }
            public DateTime timestamp { get; set; }
            public float temperature { get; set; }
            public float humidity { get; set; }
        }

        (string data, string temperatureUnit, float temperatureMin, float temperatureMax, float temperatureLimitMin,
            float temperatureLimitMax)
            GetData()
        {
            var config = _status.ConfigTable;

            var temperatureData = config.TemperatureDataTable.Data;
            var startTime = Helpers.RtcAccuracyHelper.CorrectedLocalTimestamp(
                _status.MeasurementStatus.StartTime);
            var interval = config.MeasurementConfig.Interval;
            float temperatureValue;
            string timestampFormatCs = "yyyy-MM-dd HH:mm:ss";

            var dataBuilder = new StringBuilder();

            bool isCelsius =
                App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;
            string temperatureUnit = isCelsius ? "°C" : "°F";

            var temperatureMin = Math.Min(_status.TemperatureStatus.RecordedMin,
                config.TemperatureConfig.ConfiguredMin);
            var temperatureMax = Math.Max(_status.TemperatureStatus.RecordedMax,
                config.TemperatureConfig.ConfiguredMax);
            var temperatureLimitMin = (float)config.TemperatureConfig.ConfiguredMin;
            var temperatureLimitMax = (float)config.TemperatureConfig.ConfiguredMax;
            if (!isCelsius)
            {
                temperatureMin = temperatureMin * 9 / 5 + 32;
                temperatureMax = temperatureMax * 9 / 5 + 32;
                temperatureLimitMin = temperatureLimitMin * 9 / 5 + 32;
                temperatureLimitMax = temperatureLimitMax * 9 / 5 + 32;
            }

            for (int i = 0; i < temperatureData.Length; i++)
            {
                var entry = new Data
                {
                    seqNo = i + 1,
                    timestamp = startTime + TimeSpan.FromSeconds(interval.TotalSeconds * i),
                };

                temperatureValue = temperatureData[i];
                if (temperatureValue == Helpers.GlobalHelper.NotInitializedData)
                    temperatureValue = 0;
                else
                {
                    if (temperatureValue == Helpers.GlobalHelper.TemperaturePlaceholder)
                    {
                        // Place holder value. It is reported when the measurement is done while NFC field 
                        // is present. Use neighbour value.
                        int range = Math.Max(i, temperatureData.Length - 1 - i);
                        for (int j = 1; j <= range; j++)
                        {
                            if (i + j < temperatureData.Length)
                            {
                                if (temperatureData[i + j] != Helpers.GlobalHelper.TemperaturePlaceholder)
                                {
                                    temperatureValue = temperatureData[i + j];
                                    break;
                                }
                            }
                            if (i - j >= 0)
                            {
                                if (temperatureData[i - j] != Helpers.GlobalHelper.TemperaturePlaceholder)
                                {
                                    temperatureValue = temperatureData[i - j];
                                    break;
                                }
                            }
                        }
                    }
                    if (!isCelsius)
                        // Fahrenheit.
                        temperatureValue = temperatureValue * 9 / 5 + 32;
                }
                entry.temperature = temperatureValue;

                var temperatureStr = entry.temperature.ToString("0.#", Helpers.GlobalHelper.FormatProvider);
                dataBuilder.Append($"[{entry.seqNo},'{entry.timestamp.ToString(timestampFormatCs)}'," +
                    $"{temperatureStr}],");
            }

            var data = "[" + dataBuilder.ToString() + "]";
            return (data, temperatureUnit, temperatureMin, temperatureMax, temperatureLimitMin, temperatureLimitMax);
        }

        async void OnButtonClickAsync(object sender, EventArgs e)
        {
            // Prepare CSV file and share.
            var now = DateTime.Now;
            var sharePath = Path.Combine(FileSystem.CacheDirectory, "share");
            Directory.CreateDirectory(sharePath);
            var filePath = Path.Combine(
                sharePath,
                "NHS3100TemperatureData_" + now.ToString("yyyy.MM.dd_HH.mm.ss") + ".csv");

            var csv = new StringBuilder();

            csv.AppendLine("NFC ID: " + _status.ConfigTable.TagId);
            csv.AppendLine($"DRIFT: " + _status.Drift.ToString("0.0", Helpers.GlobalHelper.FormatProvider));
            csv.AppendLine("#, epoch, corrected, UTC, temperature(C)");

            // Temperature data.
            var temperatureData = _status.ConfigTable.TemperatureDataTable.Data;
            var startTime = _status.MeasurementStatus.StartTime;
            var interval = _status.ConfigTable.MeasurementConfig.Interval;
            float temperatureValue;
            string timestampFormat = "yyyy-MM-dd HH:mm:ss";

            bool isCelsius =
                App.AppSettingsService.TemperatureUnit == Services.AppSettingsService.ETemperatureUnit.Celsius;

            bool isPlaceholder = false;

            for (int i = 0; i < temperatureData.Length; i++)
            {
                var timestamp = startTime + TimeSpan.FromSeconds(interval.TotalSeconds * i);
                var correctedTimestamp = Helpers.RtcAccuracyHelper.CorrectedTimestamp(
                    _status.ConfigTable.TagConfigTimestamp, timestamp, _status.Drift);
                var epoch = (long)(timestamp - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                var correctedEpoch = (long)(correctedTimestamp - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).
                    TotalSeconds;
                var temperatureString = string.Empty;

                temperatureValue = temperatureData[i];
                if (temperatureValue == Helpers.GlobalHelper.TemperaturePlaceholder)
                {
                    isPlaceholder = true;
                }
                else
                {
                    if (!isCelsius)
                    {
                        temperatureValue = temperatureValue * 9 / 5 + 32;
                    }
                    temperatureString = temperatureValue.ToString("0.0", Helpers.GlobalHelper.FormatProvider);
                    csv.AppendLine(
                        $"{i + 1}, " +
                        $"{epoch}, " +
                        $"{correctedEpoch}, " +
                        $"{correctedTimestamp.ToString(timestampFormat)}, " +
                        temperatureString
                    );
                }
            }

            if (isPlaceholder)
                csv.Append("Temperature measurements were skipped when an NFC field was present");

            File.WriteAllText(filePath, csv.ToString());
            await DependencyService.Get<Interfaces.IShareFile>().ShareAsync(new string[]
            {
                filePath
            });
        }

    }
}
