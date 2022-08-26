package com.example.readernhs;


import androidx.lifecycle.ViewModel;

import java.util.ArrayList;


public class SharedModel extends ViewModel {
    private ArrayList<MessageFormat> messageList =new ArrayList<MessageFormat>();
    private int selectStats = 0;


    public void setMessages(ArrayList<MessageFormat> list) {
        this.messageList= list;
    }

    public ArrayList<MessageFormat> getMessage() {
        return messageList ;
    }


    public void setStats(int stat){
        this.selectStats = stat;
    }

    public int getStat(){
        return  selectStats;
    }


}