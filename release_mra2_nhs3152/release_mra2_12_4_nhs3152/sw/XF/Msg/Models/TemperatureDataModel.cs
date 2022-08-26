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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Msg.Models
{
    public class TemperatureDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        DataIn _dataIn = new DataIn()
        {
            Data = new float[] { },
        };

        public DataIn TemperatureDataIn
        {
            get => _dataIn;
            set
            {
                if (value.Data.Length != 0)
                {
                    _dataIn = value;
Debug.WriteLine("################## TEMPERATURE ####");
Debug.WriteLine($"#### DATAIN: Begin:{_dataIn.Offset} End:{_dataIn.Offset + _dataIn.Data.Length - 1} ####");
                    ////Debug.WriteLine($"#### DATAIN: offset:{_dataIn.Offset} length:{_dataIn.Data.Length} ####");

                    TemperatureData = new DataInToOut(_dataIn, _dataOut).DataOut;
                }
            }
        }

        DataOut _dataOut = new DataOut
        {
            RangeList = new List<Range>(),
            Data = new float[] { },
        };
        public DataOut TemperatureData
        {
            get => _dataOut;
            set
            {
                _dataOut = value;
                OnPropertyChanged();
            }
        }

        public void Reset(DataIn dataIn = null, DataOut dataOut = null, bool isInvokePropertyChange = false)
        {
            ////var backup = _dataOut;

            _dataIn = dataIn?? new DataIn()
            {
                Data = new float[] { },
            };
            _dataOut = dataOut?? new DataOut
            {
                RangeList = new List<Range>(),
                Data = new float[] { },
            };

            ////if (isInvokePropertyChange)
                ////OnPropertyChanged(nameof(Reset));

        }
    }
}
