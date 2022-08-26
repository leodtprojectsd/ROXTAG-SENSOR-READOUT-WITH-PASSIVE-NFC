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

using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TLogger.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TemperaturePageView : CarouselPage
    {
        private readonly ViewModels.TemperatureStatusViewModel _viewModelTemperatureStatus;
        private readonly ViewModels.TemperatureChartViewModel _viewModelTemperatureChart;
        private readonly ViewModels.TemperatureGraphViewModel _viewModelTemperatureGraph;
        private bool _isNfcIdValid;

        public TemperaturePageView()
        {
            InitializeComponent();

            _viewModelTemperatureStatus = new ViewModels.TemperatureStatusViewModel();
            temperatureStatusPage.BindingContext = _viewModelTemperatureStatus;
            _viewModelTemperatureStatus.NfcIdValidEvent += OnNfcIdValid;

            _viewModelTemperatureChart = new ViewModels.TemperatureChartViewModel();
            temperatureChartPage.BindingContext = _viewModelTemperatureChart;

            _viewModelTemperatureGraph = new ViewModels.TemperatureGraphViewModel();
            temperatureGraphPage.BindingContext = _viewModelTemperatureGraph;
        }

        private void OnNfcIdValid(object sender, EventArgs e)
        {
            if (!_isNfcIdValid)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    // MacOS carousel page does not support swiping, adding it here.
                    if (Device.RuntimePlatform == Device./*UWP*/macOS)
                    {
                        // Status page.
                        var statusRightGestureRecognizer = new TapGestureRecognizer();
                        statusRightGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = temperatureChartPage;
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
                        statusSwipeGrid.Children.Add(statusRightImage, 2, 0);
                        statusGrid.Children.Add(statusSwipeGrid, 0, 5);

                        // Chart page.
                        var chartLeftGestureRecognizer = new TapGestureRecognizer();
                        chartLeftGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = temperatureStatusPage;
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
                            CurrentPage = temperatureGraphPage;
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

                        // Graph page.
                        var graphLeftGestureRecognizer = new TapGestureRecognizer();
                        graphLeftGestureRecognizer.Tapped += (s, ev) =>
                        {
                            CurrentPage = temperatureChartPage;
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

                    Children.Add(temperatureChartPage);
                    Children.Add(temperatureGraphPage);
                    _isNfcIdValid = true;
                });
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Show chart and graph pages only when we have a valid NFC id.
            if (!_isNfcIdValid)
            {
                Children.Remove(temperatureGraphPage);
                Children.Remove(temperatureChartPage);
            }
        }
    }
}
