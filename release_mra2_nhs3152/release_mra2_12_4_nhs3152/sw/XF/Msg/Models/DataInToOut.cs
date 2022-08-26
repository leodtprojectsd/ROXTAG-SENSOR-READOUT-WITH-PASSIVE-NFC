/*
 * Copyright 2019 NXP
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

namespace Msg.Models
{
    public class DataIn
    {
        public int Offset { get; set; }
        public float[] Data { get; set; }
    }

    public class Range
    {
        public int Begin { get; set; }
        public int End { get; set; }
    }

    public class DataOut
    {
        public List<Range> RangeList { get; set; }
        public int NumData { get; set; }
        public float[] Data { get; set; }
    }


    public class DataInToOut
    {
        enum State
        {
            AtBegin,
            AtEnd,
            Done,
        }

        public DataOut DataOut { get; private set; }

        public DataInToOut(DataIn dataIn, DataOut current)
        {
            State state = State.AtBegin;

            var dataOut = current;
            if (dataIn.Offset + dataIn.Data.Length > dataOut.Data.Length)
            {
                var data =  Enumerable.Repeat(
                    Helpers.GlobalHelper.NotInitializedData, dataIn.Offset + dataIn.Data.Length).ToArray();
                dataOut.Data.CopyTo(data, 0);
                dataOut.Data = data;
            }

            var ri = new Range  // rangein
            {
                Begin = dataIn.Offset,
                End = dataIn.Offset + dataIn.Data.Length - 1,
            };

            var rn = new Range();   // rangenew

            List<Range> newList = new List<Range>();

            int count = dataOut.RangeList.Count;
            if (count == 0)
            {
                newList.Add(ri);
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var ro = dataOut.RangeList[i];  // range original
                    switch (state)
                    {
                        case State.AtBegin:
                            if (ri.Begin < ro.Begin && ri.End < ro.Begin - 1)
                            {
                                // Add input one and original.
                                newList.Add(ri);
                                newList.Add(ro);
                                state = State.Done;
                            }
                            else if (ri.Begin > ro.End + 1)
                            {
                                // Add original till new one hits.
                                newList.Add(ro);

                                if (i == count - 1)
                                {
                                    // Last one.
                                    newList.Add(ri);
                                    state = State.Done;
                                }
                            }
                            else
                            {
                                // Hit. Set begin.
                                rn.Begin = Math.Min(ri.Begin, ro.Begin);

                                if (ri.End <= ro.End || i == count - 1)
                                {
                                    // Covered by original or last one.
                                    rn.End = Math.Max(ri.End, ro.End);
                                    newList.Add(rn);
                                    state = State.Done;
                                }
                                else
                                {
                                    // Get next one to detect end.
                                    state = State.AtEnd;
                                }
                            }
                            break;

                        case State.AtEnd:
                            if (ri.End < ro.Begin - 1)
                            {
                                // No hit with original, or last one.
                                rn.End = ri.End;
                                newList.Add(rn);
                                newList.Add(ro);
                                state = State.Done;
                            }
                            else if (ri.End <= ro.End || i == count - 1)
                            {
                                // Hit original or last one.
                                rn.End = Math.Max(ri.End, ro.End);
                                newList.Add(rn);
                                state = State.Done;
                            }
                            else
                            {
                                // Get next one to detect end.
                            }
                            break;

                        case State.Done:
                            newList.Add(ro);
                            break;
                    }
                }
            }

            dataOut.RangeList = newList;
            for (int i = 0; i < dataOut.RangeList.Count; i++)
                System.Diagnostics.Debug.WriteLine($"#### DATAOUT #### ({newList[i].Begin},{newList[i].End})");

            dataOut.NumData = 0;
            for (int i = 0; i < dataOut.RangeList.Count; i++)
                dataOut.NumData += dataOut.RangeList[i].End - dataOut.RangeList[i].Begin + 1;
            System.Diagnostics.Debug.WriteLine($"#### DATAOUT #### NumData:{dataOut.NumData}");

            dataIn.Data.CopyTo(dataOut.Data, dataIn.Offset);

            DataOut = dataOut;
        }
    }
}
