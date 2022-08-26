package com.example.readernhs;




import android.util.Log;

import java.text.DateFormat;



public class MessageFormat {

    static final double MAX = Double.POSITIVE_INFINITY;
    String date ;
    String codVersion;
    double r, v1, v2, vds ;
    int  ipA;
    int nativeValue;
    boolean selected;

    public MessageFormat(String message) {

        long yourmilliseconds = System.currentTimeMillis();
        date =  DateFormat.getDateTimeInstance().format(yourmilliseconds);



        if((message.contains("V:"))&&( message.contains("R:"))){
            int index =  message.lastIndexOf("V:");
            int index2 =  message.indexOf("R:");
            codVersion = message.substring(index+2, index2).trim();
        }

        if((message.contains("R:"))&&( message.contains("adc_1[V]:"))){
            int index =  message.lastIndexOf("R:");
            int index2 =  message.indexOf("adc_1[V]:");
            String rString = message.substring(index+2, index2).trim();
            if( rString.equals("inf"))
            {
                r= MAX;
            }
            else {
                r = Double.parseDouble(message.substring(index + 2, index2).trim());

            }
        }
        if((message.contains("adc_1[V]:"))&&( message.contains("adc_4[V]:"))){
            int index =  message.lastIndexOf("adc_1[V]:");
            int index2 =  message.indexOf("adc_4[V]:");
            v1 = Double.parseDouble(message.substring(index+9, index2).trim());
        }
        if((message.contains("adc_4[V]:"))&&( message.contains("VDS[V]:"))){
            int index =  message.indexOf("adc_4[V]:");
            int index2 =  message.indexOf("VDS[V]:");
            v2 = Double.parseDouble(message.substring(index+9, index2).trim());
        }

        if((message.contains("VDS[V]:"))&&( message.contains("I[pA]:"))){
            int index =  message.indexOf("VDS[V]:");
            int index2 =  message.indexOf("I[pA]:");
            vds = Double.parseDouble(message.substring(index+7, index2).trim());
        }
        if((message.contains("I[pA]:"))&&( message.contains("I native"))){
            int index =  message.indexOf("I[pA]:");
            int index2 =  message.indexOf("I native");

            if(!message.substring(index+6, index2).trim().isEmpty())
                ipA = Integer.parseInt(message.substring(index+6, index2).trim());
            else ipA = 0 ;
        }
        if((message.contains("native:")&&( message.contains("£")))){
            int index =  message.indexOf("native:");
            int index2 =  message.indexOf("£");
            if(message.substring(index+7, index2).trim().isEmpty())
                nativeValue = 0 ;
                        else {
                            try {
                                nativeValue = Integer.parseInt(message.substring(index + 7, index2).trim());
                            }catch(Exception e){
                                nativeValue = -1;
                            }
            }
        }

        this.selected = false ;

    }

    public MessageFormat() {

    }

    public MessageFormat(String codVersion,double r,  double v1 , double v2 , double vds, int ipA, int nativeValue) {
        long yourmilliseconds = System.currentTimeMillis();
        date =  DateFormat.getDateTimeInstance().format(yourmilliseconds);
        this.codVersion = codVersion;
        this.r = r  ;
        this.v1 = v1 ;
        this.v2= v2;
        this.vds = vds;
        this.ipA = ipA ;
        this.nativeValue = nativeValue;
        this.selected = false;
    }
    public MessageFormat(String date, String codVersion,double r ,  double v1 , double v2 , double vds, int ipA, int nativeValue) {
        this.date = date;
        this.codVersion =codVersion;
        this.r = r ;
        this.v1 = v1 ;
        this.v2= v2;
        this.vds = vds;
        this.ipA = ipA ;
        this.nativeValue = nativeValue;
        this.selected = false;
    }
    public String getDate(){ return date; }
    public String getCodVersion(){ return codVersion; }
    public double getR() { return r ; }
    public double getV1(){
        return v1;
    }
    public double getV2(){ return v2; }
    public double getVds(){
        return vds;
    }

    public int getIpa(){
        return ipA;
    }

    public int getNative(){
        return nativeValue;
    }

    public boolean getSelected(){
        return selected;
    }
    public void setSelected(boolean selected){
        this.selected= selected;
    }

    @Override
    public String toString() {
        return "MessageFormat{" +
                "date='" + date + '\'' +
                ", codVersion='" + codVersion + '\'' +
                ", r=" + r +
                ", v1=" + v1 +
                ", v2=" + v2 +
                ", vds=" + vds +
                ", ipA=" + ipA +
                ", nativeValue=" + nativeValue +
                ", selected=" + selected +
                '}';
    }
}
