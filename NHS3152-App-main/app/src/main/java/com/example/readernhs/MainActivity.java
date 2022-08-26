package com.example.readernhs;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.drawable.Drawable;
import android.media.MediaScannerConnection;
import android.nfc.FormatException;
import android.nfc.NdefMessage;
import android.nfc.NfcAdapter;
import android.nfc.Tag;
import android.nfc.tech.Ndef;
import android.os.Bundle;
import android.os.Environment;
import android.os.Parcelable;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.material.bottomnavigation.BottomNavigationView;

import org.jetbrains.annotations.NotNull;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;

public class MainActivity extends AppCompatActivity {


    private static final String TAG = MainActivity.class.getSimpleName();
    int startIndex = 0 ; // indexs for export the last recording
    int endIndex = 0;
    int countMessage = 0; // counter of the numbers of messages to be exported
    PendingIntent pendingIntent; // for handle NFC intent
    IntentFilter[] writingTagFilters;
    NfcAdapter nfcAdapter; // default adapter for NFC
    Tag myTag;
    Context context;
    Thread thread;
    Ndef ndef; // manage NFC communication

    int timeMessage = 3000; // default time for continuos reading NDEF message

    ArrayList<MessageFormat> messageList; // list of records

    // UI elements

    TextView nfc_content;
    Button ActiveButton,buttonExport,btn_clear;
    ImageButton buttonExportAll;
    RecordAdapter adapter;
    RecyclerView recyclerView;
    EditText et_timeLec;



    boolean writeMode;

    String exportText;
    // For stop reading
    boolean stopReading = true;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        //for hide the title bar
        try
        {
            this.getSupportActionBar().hide();
        }
        catch (NullPointerException e){
            Log.e(TAG, "Error with bottom bar ");
        }



        // initialize Ui
        setContentView(R.layout.activity_main);
        nfc_content=findViewById(R.id.visualResult);
        buttonExport = findViewById(R.id.buttonExport);
        ActiveButton= findViewById(R.id.buttonStart);
        buttonExportAll = findViewById(R.id.buttonExportAll);
        btn_clear = findViewById(R.id.btn_clear);
        et_timeLec = findViewById(R.id.et_timeLec);




        context=this;
        messageList = new ArrayList<>();
        // in case ExternalStorage isn't writable
        if(!isExternalStorageWritable()){
            buttonExport.setEnabled(false);
        }
        // if device hasn't NFC, close Application
        nfcAdapter = NfcAdapter.getDefaultAdapter(this);

        if(nfcAdapter==null){
            Toast.makeText(this, "This Device does not support NFC", Toast.LENGTH_SHORT).show();
            finish();
        }


        recyclerView= findViewById(R.id.listNFC);
        // define adpater to visualize result
        adapter= new RecordAdapter(messageList);
        recyclerView.setAdapter(adapter);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));

        // bottom Bar Menu'
        BottomNavigationView bottomNavigationView = findViewById(R.id.bottom_navigation);

        //select the layout
        bottomNavigationView.setSelectedItemId(R.id.home);
        //set listener on the buttons
        bottomNavigationView.setOnNavigationItemSelectedListener(item -> {
            switch (item.getItemId()){
                // in case one button pressed, start an activity and set a transition
                case R.id.export:
                startActivity(new Intent(getApplicationContext(),DashBoard.class));
                overridePendingTransition(0,0);
                return true;
                case R.id.home:
                return true;
                case R.id.stats:
                    startActivity(new Intent(getApplicationContext(),Stats.class));
                    overridePendingTransition(0,0);
                    return true;
            }
            return false;
        });

        // if a new Intent detected and it equals to NFC's Intent start this method
        readfromIntent(getIntent());
        //pendingIntent and writingTagFilters necessary parameters in the enableForegroundDispatch method
        pendingIntent= PendingIntent.getActivity(this, 0, new Intent(this, getClass()).addFlags(Intent.FLAG_ACTIVITY_SINGLE_TOP), 0 );
        IntentFilter tagDetected = new IntentFilter(NfcAdapter.ACTION_TAG_DISCOVERED);
        tagDetected.addCategory(Intent.CATEGORY_DEFAULT);
        writingTagFilters= new IntentFilter[]{tagDetected};

        //save selected records
        checkButtonClick();
        // select record from the last recording
        checkExportAll();

        // clear the list and the screen
        btn_clear.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                messageList.clear();
                adapter.notifyDataSetChanged();
            }
        });

    }


    /* On buttonExport pressed , save on a csv file all items selected  */
    private void checkButtonClick() {

        if(isExternalStorageWritable()) {
            Button myButton = findViewById(R.id.buttonExport);
            // if button pressed
            myButton.setOnClickListener(v -> {


                // save in a new list all new messages
                ArrayList<MessageFormat> countryList = messageList;
                ArrayList<MessageFormat> messageToExport = new ArrayList<>();

                for (int i = 0; i < countryList.size(); i++) {
                    // if message was selected add in the new list
                    if (messageList.get(i).getSelected()) {
                        messageToExport.add(messageList.get(i));
                    }
                }
                // reset checkbox selection
                for( int i = 0 ; i < messageList.size() ; i ++){
                    messageList.get(i).setSelected(false);
                }
                // make a string text ( csv format )
                exportText = makeText(messageToExport);
                // create a file and save the text created previously
                save();

                adapter.notifyDataSetChanged();

            });
        }
        else{
            Log.e("State ", "No Writable");
        }
    }


    /* Select all items on the last rec and save it */
    private void checkExportAll() {
        // if ExternalStorage is writable
        if(isExternalStorageWritable()) {
            // initialize button
           LinearLayout myButton = findViewById(R.id.linearLayout);
           // if button pressed
            myButton.setOnClickListener(v -> {

                // set the checkbox of the last reading to checked
                for (int i = startIndex; i < endIndex; i++) {
                    // if the recors isn't already selected, select it
                    if(!messageList.get(i).getSelected()){
                        messageList.get(i).setSelected(true);

                    }


                }
                // update Recylcler View
                adapter.notifyDataSetChanged();

            });
        }
        else{
            Log.e("State ", "No Writable");
        }
    }



    //for save file in External Storage
    public void save(){
        // check if the new is avaible, otherwise increment the number compenent of the name's file
        String nameFile = "test1.csv";
        // every file will be save in this directory
        File fileText = new File(getExternalFilesDir("TestDirectory"), nameFile);
        int increase=1;
        while(fileText.exists()){

            increase++;
            nameFile = "test" + increase+ ".csv";
            fileText = new File(getExternalFilesDir("TestDirectory"), nameFile);
        }
        // initialize FileOutputStream
        FileOutputStream fos = null;
        try {
            fos = new FileOutputStream(fileText);
            byte[] mybytes = exportText.getBytes();
            //write file
            fos.write(mybytes);
            //make usb Visible
            fixUsbVisibleFile(this, fileText);
            // Show Toast
            Toast.makeText(this, "Saved to "+getExternalFilesDir(nameFile) + "/"+nameFile, Toast.LENGTH_LONG ).show();
        }catch (FileNotFoundException e) {
            System.out.println("FilenotFound");
            e.printStackTrace();

        } catch (IOException e) {
            e.printStackTrace();
        }finally {
            if(fos!= null){
                try {
                    fos.close();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }

    }

    //For create file and make visible
    private static void fixUsbVisibleFile(Context context, File file) {
        MediaScannerConnection.scanFile(context,
                new String[]{file.toString()},
                null, null);
    }



    // this method create the string text for create and export .csv files from the arrayList's record
    public String makeText(ArrayList<MessageFormat> listRecords ){
        StringBuilder sb = new StringBuilder();
        sb.append("id");
        sb.append(',');
        sb.append("CodVersion");
        sb.append(',');
        sb.append("r");
        sb.append(',');
        sb.append("v1");
        sb.append(',');
        sb.append("v2");
        sb.append(',');
        sb.append("vds");
        sb.append(',');
        sb.append("ipA");
        sb.append(',');
        sb.append("nativeValue");
        sb.append('\n');

        for (MessageFormat rowData : listRecords) {
            sb.append(rowData.getDate());
            sb.append(',');
            sb.append(rowData.getCodVersion());
            sb.append(',');
            sb.append(rowData.getR());
            sb.append(',');
            sb.append(rowData.getV1());
            sb.append(',');
            sb.append(rowData.getV2());
            sb.append(',');
            sb.append(rowData.getVds());
            sb.append(',');
            sb.append(rowData.getIpa());
            sb.append(',');
            sb.append(rowData.getNative());
            sb.append("\n");
        }

        return sb.toString();
    }



    /* If Intent equals to a NFC discovered, read the message and update UI*/
    private void readfromIntent(Intent intent){
        String action = intent.getAction();
        // check if the the intent discovered is equals to new TAG discovered
        if ( NfcAdapter.ACTION_TAG_DISCOVERED.equals(action)||
                NfcAdapter.ACTION_TECH_DISCOVERED.equals(action) ||
                NfcAdapter.ACTION_NDEF_DISCOVERED.equals(action)){

            // get raw message
            Parcelable[] rawMsgs= intent.getParcelableArrayExtra(NfcAdapter.EXTRA_NDEF_MESSAGES);
            NdefMessage[] msgs= null;
            // if the raw message isn't null, cast message in NDEF
            if(rawMsgs!= null)
            {

                msgs= new NdefMessage[rawMsgs.length];
                for ( int i = 0 ; i < rawMsgs.length; i++){
                    msgs[i] = (NdefMessage) rawMsgs[i];
                }
            }
            //convert message
            buildTagViews(msgs);
            //update UI
            updateStatus();

        }
    }

    // Starts thread to read multiple message, change text and color for the "activity bar"
    public void startLooping(View view) {

        stopReading = true;
        // when button "Start reading" pressed, set disabled this button
        ActiveButton.setEnabled(false);
        nfc_content.setText("Reading...");
        nfc_content.setBackground(ContextCompat.getDrawable(context, R.color.reading));

        if(et_timeLec.getText().toString() != null  ) {
            if(!et_timeLec.getText().toString().equals("")) {
                //  Log.i(TAG, "Il valore di et_" + et_timeLec.getText().toString());
                timeMessage = Integer.parseInt(et_timeLec.getText().toString());
            }
        }

        Executor mSingleThreadExecutor = Executors.newSingleThreadExecutor();
        // run thread
        mSingleThreadExecutor.execute(runnable);

    }
    // when reading is stopped, change text and color of the "Activity bar"
    public void stopLooping(View v) {
        stopReading = false;
        // in case the button is disabled, set the button enabled
        if(!ActiveButton.isEnabled())
                ActiveButton.setEnabled(true);

        nfc_content.setText("Tag gone");
        nfc_content.setBackground(ContextCompat.getDrawable(context, R.color.error));

        // if the EditText value is null, use defalut value otherwise get the value


    }



    /*This is a runnable method to continuos reading NFC tag  */
    private final Runnable runnable = new Runnable() {
        @Override
        public void run() {
            // When start reading save the initial number of messages
            startIndex = messageList.size();
            try {
                // while device is connected to Tag and stop button not pressed exe this method
                while (!ndef.isConnected()&&  stopReading) {
                    try {
                        // wait "timeMessage" milliseconds
                        Thread.sleep(timeMessage);
                        if(!ndef.isConnected()) // try to connect
                        ndef.connect();



                        //get the message
                        NdefMessage msg = ndef.getNdefMessage();
                        if (msg != null) {
                            //convert message and save in the list
                            String message = new String(msg.getRecords()[0].getPayload());
                            MessageFormat messageFormat = new MessageFormat(message);
                            messageList.add(messageFormat);
                            countMessage++; // add new mwssage
                        }
                    } /*
                    When tag go too far from the device this exception will be thrown

                    */catch (IOException e) {
                        e.printStackTrace();
                        Log.e("IOEXCEPTION", "error");


                        break;
                    } catch (FormatException e) {
                        e.printStackTrace();
                        Log.e("FORMATEX", "error");
                        break;
                    } finally {
                        try {
                            // when method is finish close ndef
                            ndef.close();

                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    }
                }
            } catch (InterruptedException e) {
                e.printStackTrace();
            }/*
            If tag is not close to device throw this expection
            */
            catch(NullPointerException e){
                e.printStackTrace();
                Log.e(TAG, "No connection");

            }


            // Update the UI with progress
            runOnUiThread(() -> updateStatus());

        }
    };
    /*
    Update UI ListView, Thread in Android can't do operation on UI elements, so when thread is finishing
    update the list and the "Activity Bar"
     */
        public void updateStatus(){
            // check if connection is closed
            if(ndef!=null) {
                if (!ndef.isConnected()) {

                    nfc_content.setText("Tag gone");
                    nfc_content.setBackground(ContextCompat.getDrawable(context, R.color.error));
                }
            }

            // if the communication is closed but the Activity Bar's text is equals to "Reading.." change this
            if(nfc_content.getText().toString().equals("Reading...")){
                nfc_content.setText("Tag gone");
                nfc_content.setBackground(ContextCompat.getDrawable(context, R.color.error));
            }
        // set enabled the button
        if(!ActiveButton.isEnabled()) {
            ActiveButton.setEnabled(true);
        }

        endIndex = messageList.size();

            // take the data from arrayList ---> messageList and show it
            adapter.notifyDataSetChanged();


    }
    // Adapter to visualize RecyclerView
    public class RecordAdapter extends
            RecyclerView.Adapter<RecordAdapter.ViewHolder> {

        @NonNull
        @NotNull
        @Override
        public ViewHolder onCreateViewHolder(@NonNull @NotNull ViewGroup parent, int viewType) {
            Context context = parent.getContext();
            LayoutInflater inflater = LayoutInflater.from(context);
            View recordView = inflater.inflate(R.layout.row2, parent , false );

            return new ViewHolder(recordView);
        }

        @Override
        public void onBindViewHolder(@NonNull @NotNull MainActivity.RecordAdapter.ViewHolder holder, int position) {
            // deifne all UI with holder

            TextView text = holder.myTitle;
                    text.setText(messageList.get(position).getDate());
            TextView codText = holder.codversion;

            codText.setText(messageList.get(position).getCodVersion());

            String stringR= Double.toString(messageList.get(position).getR());
            holder.r.setText(stringR);

            String stringV1= Double.toString(messageList.get(position).getV1());
            holder.v1.setText(stringV1);
            String stringV2= Double.toString(messageList.get(position).getV2());
            holder.v2.setText(stringV2);
            String stringVds= Double.toString(messageList.get(position).getVds());
            holder.vds.setText(stringVds);
            holder.ipa.setText(String.valueOf(messageList.get(position).getIpa()));
            holder.nativeV.setText(String.valueOf(messageList.get(position).getNative()));
            // in case checkbox pressed, message's attribute 'Selected' is setting to true
            holder.name.setOnClickListener(
                    v -> {
                        CheckBox cb = (CheckBox) v;

                        messageList.get(position).setSelected(cb.isChecked());
                    });

            holder.name.setChecked(messageList.get(position).getSelected());
        }

        @Override
        public int getItemCount() {
            return messageList.size();
        }

        // Provide a direct reference to each of the views within a data item
        // Used to cache the views within the item layout for fast access
        public class ViewHolder extends RecyclerView.ViewHolder {
            // Your holder should contain a member variable
            // for any view that will be set as you render a row
            public TextView myTitle;
            public TextView r,codversion,v1,v2,vds,ipa, nativeV;
            public CheckBox name;

            // We also create a constructor that accepts the entire item row
            // and does the view lookups to find each subview
            public ViewHolder(View itemView) {
                // Stores the itemView in a public final member variable that can be used
                // to access the context from any ViewHolder instance.
                super(itemView);

                myTitle = itemView.findViewById(R.id.title);
                codversion = itemView.findViewById(R.id.codTable);
                r = itemView.findViewById(R.id.rTable);
                v1 = itemView.findViewById(R.id.v1Table);
                v2 = itemView.findViewById(R.id.v2Table);
                vds = itemView.findViewById(R.id.vdsTable);
                ipa = itemView.findViewById(R.id.ipaTable);
                nativeV = itemView.findViewById(R.id.nativeTable);
                name = itemView.findViewById(R.id.checkBox1);
            }
        }

        private final ArrayList <MessageFormat> messageFormat;

        public RecordAdapter( ArrayList<MessageFormat> messageF){

            messageFormat=messageF;
        }


    }







    /* Read Ndef record and message, then Print the message
    * This is for reading the single message  */
    private void buildTagViews(NdefMessage[] msgs){
        // if message is null return
        if(msgs==null || msgs.length== 0 ) return;

        String text ="";
        // get from the first message the type of message( payload , textEnconding ... )
        byte[] payload= msgs[0].getRecords()[0].getPayload();
        String textEncoding = ((payload[0] & 128)== 0) ? "UTF-8":"UTF-16";
        int languageCodeLenght = payload[0] & 51;
        try{
            text = new String ( payload, languageCodeLenght+1 , payload.length- languageCodeLenght-1 , textEncoding);
        }catch(UnsupportedEncodingException e){
            Log.e("UnsupportedEnconding", e.toString());
        }
        // set the counter
        countMessage++;
        // add in ArrayList of new record the new message
        MessageFormat messageFormat = new MessageFormat(text);
        messageList.add(messageFormat);
    }




    /*
    When a NFC tag has discovered , take the tag
     */
    @Override
    protected void onNewIntent(Intent intent) {
        //override old methods
        super.onNewIntent(intent);
        setIntent(intent);
        // Read, convert and save message
        readfromIntent(intent);
        //Update Ui elements
        nfc_content.setText("Phone is connected, press start");
        // chaange UI color
        nfc_content.setBackground(ContextCompat.getDrawable(context, R.color.azzuro));
        // if the new Intent is equals to NFC Intent
        if(NfcAdapter.ACTION_TAG_DISCOVERED.equals(intent.getAction())){
            // save the NXP's TAG
            myTag= intent.getParcelableExtra(NfcAdapter.EXTRA_TAG);
            // Return an instance of Ndef for the given tag.
             ndef = Ndef.get(myTag);
             /*

             After 4000 milliseconds , if nothing happends update "Activity Bar"
              */
           thread = new Thread() {

                @Override
                public void run() {
                    try {
                            // wait 4000 millis
                            Thread.sleep(4000);

                            runOnUiThread(() -> {

                                    // if TextView's text is equals to the defalut message update it
                                    if(nfc_content.getText()=="Phone is connected, press start") {
                                        nfc_content.setText("Tag may be gone!\nTry Again");
                                        nfc_content.setBackground(ContextCompat.getDrawable(context, R.color.error));

                                    }

                            });


                    } catch (InterruptedException e) {
                        Log.e(TAG, "Error ");
                    }
                }
            };

            thread.start();
        }
    }

    // this one is for save datas in the same session , whitout onSave... but whit DataHolder
    static class DataHolder {
        ArrayList<MessageFormat> records = new ArrayList<>();

        private DataHolder() {}

        static DataHolder getInstance() {
            if( instance == null ) {
                instance = new DataHolder();
            }
            return instance;
        }

        private static DataHolder instance;
    }



    @Override
    protected void onPause() {
        // when app is on Pause
        super.onPause();
        writeModeOff(); // enable Foreground dispatcher
        // set DataHolder for visualize if app is resume
        DataHolder.getInstance().records = messageList;
    }

    @Override
    protected void onResume() {
        super.onResume();
        writeModeOn();
        // get the Data saved previously and update the View
        messageList = DataHolder.getInstance().records;
        adapter.notifyDataSetChanged();
    }
    //Enable foreground dispatch to the given Activity.
    private void writeModeOn(){
        writeMode= true;
        nfcAdapter.enableForegroundDispatch(this, pendingIntent, writingTagFilters, null);
    }
   // Disable foreground dispatch to the given Activity.
    private void writeModeOff(){
        writeMode= false;
        nfcAdapter.disableForegroundDispatch(this);
    }

    // check if the external storage is Writable
    public boolean isExternalStorageWritable(){
        return Environment.MEDIA_MOUNTED.equals(Environment.getExternalStorageState());
    }
}