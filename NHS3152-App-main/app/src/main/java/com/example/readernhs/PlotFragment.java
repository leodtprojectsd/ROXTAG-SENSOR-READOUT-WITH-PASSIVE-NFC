package com.example.readernhs;

import android.graphics.Color;
import android.os.Bundle;

import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;


import com.github.mikephil.charting.charts.BarChart;
import com.github.mikephil.charting.data.BarData;
import com.github.mikephil.charting.data.BarDataSet;
import com.github.mikephil.charting.data.BarEntry;
import com.github.mikephil.charting.utils.ColorTemplate;

import java.util.ArrayList;



/**
 * A simple {@link Fragment} subclass.
 * Use the {@link PlotFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class PlotFragment extends Fragment {
    private static final String TAG = MainActivity.class.getSimpleName();
    private static final float rInfinity = -1;
    BarChart barChart ;
    BarData barData;
    SharedModel shareModel;
    ArrayList<MessageFormat> messageList;

    public PlotFragment() {
        // Required empty public constructor
    }

    /**
     * Use this factory method to create a new instance of
     * this fragment using the provided parameters.
     *
     * @return A new instance of fragment PlotFragment.
     */

    public static PlotFragment newInstance() {

        return new PlotFragment();
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        messageList = new ArrayList<>();




        shareModel = new ViewModelProvider(requireActivity()).get(SharedModel.class);
       messageList=  shareModel.getMessage();

        String nameChart = "";
        ArrayList<BarEntry> resistance  = new ArrayList<>();
        for( int i = 0 ; i < messageList.size(); i ++) {

            switch (shareModel.getStat()) {
                case 0: {

                    nameChart = "Resistance";
                    if (messageList.get(i).getR() == Double.POSITIVE_INFINITY)
                        resistance.add(new BarEntry(i, rInfinity));
                    else {
                        float log= (float) Math.log(messageList.get(i).getR());
                        resistance.add(new BarEntry(i, log));
                    }
                    break;
                }

                case 1: {
                    nameChart = "V1";
                        resistance.add(new BarEntry(i, (float) messageList.get(i).getV1()));
                    break;
                }

                case 2: {

                    nameChart = "V2";

                    resistance.add(new BarEntry(i, (float) messageList.get(i).getV2()));
                    break;
                }

                case 3: {
                    nameChart = "VDS";

                    resistance.add(new BarEntry(i, (float) messageList.get(i).getVds()));
                    break;
                }
            }



        }




        BarDataSet barDataSet = new BarDataSet(resistance,nameChart);
        barDataSet.setColors(ColorTemplate.MATERIAL_COLORS);
        barDataSet.setValueTextColor(Color.BLACK);
        barDataSet.setValueTextSize(16f);


        barData = new BarData(barDataSet);



        //TextView da = findView
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        View v = inflater.inflate(R.layout.fragment_plot, container, false);
        barChart = v.findViewById(R.id.barChart);
        barChart.setFitBars(true);
        barChart.setData(barData);
        barChart.getDescription().setText("Bar Chart");
        barChart.animateY(2000);
        return v;


    }

}