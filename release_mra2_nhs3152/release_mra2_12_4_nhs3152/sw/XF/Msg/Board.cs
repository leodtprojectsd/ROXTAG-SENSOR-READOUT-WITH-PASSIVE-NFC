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

namespace Msg
{
    public class Board
    {
        public enum EBoardType
        {
            Unknown,
            SensorBoard,
            SensorButton,
            TLogger,
            TAdherence,
        }

        static public bool CanMeasureTemperature { get; private set; }
        static public bool CanMeasureAcceleration { get; private set; }
        static public bool CanMeasureHumidity { get; private set; }

        public bool IsSensorBoard
        {
            get { return Board.CanMeasureHumidity; }
            set { Board.CanMeasureHumidity = value; }
        }
        public bool IsSensorButton
        {
            get { return !Board.CanMeasureHumidity; }
            set { Board.CanMeasureHumidity = !value; }
        }

        private static EBoardType _boardType = EBoardType.Unknown;
        static public EBoardType BoardType
        {
            get => _boardType;
            set
            {
                _boardType = value;

                switch (value)
                {
                    case EBoardType.SensorBoard:
                        CanMeasureTemperature = true;
                        CanMeasureAcceleration = true;
                        CanMeasureHumidity = true;
                        break;

                    case EBoardType.SensorButton:
                        CanMeasureTemperature = true;
                        CanMeasureAcceleration = true;
                        CanMeasureHumidity = false;
                        break;

                    case EBoardType.TLogger:
                        CanMeasureTemperature = true;
                        CanMeasureAcceleration = false;
                        CanMeasureHumidity = false;
                        break;

                    case EBoardType.TAdherence:
                    default:
                        CanMeasureTemperature = false;
                        CanMeasureAcceleration = false;
                        CanMeasureHumidity = false;
                        break;
                }
            }
        }

        public static Board Current = new Board();
    }
}
