package com.example.readernhs;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;

import com.google.android.material.bottomnavigation.BottomNavigationView;

import org.jetbrains.annotations.NotNull;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;

public class DashBoard extends AppCompatActivity {

    private static final String TAG = MainActivity.class.getSimpleName();
    Context context;
    RecyclerView recyclerFiles, recyclerView; // two recycler view, one horizontal for files and the other one verical for the records
    FilesAdapter adapter ; // adapter for RecyclerView
    ArrayList<String> nameFiles;
    ArrayList<MessageFormat> messageList;
    List<String> singleRecord ;
    RecordAdapter adapter2;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        try
        {
            this.getSupportActionBar().hide();
        }
        catch (NullPointerException e){
            Log.i(TAG, "Error ");
        }


        setContentView(R.layout.activity_dash_board);
        recyclerFiles= findViewById(R.id.filesReader);
        recyclerView= findViewById(R.id.listNFC);


        BottomNavigationView bottomNavigationView = findViewById(R.id.bottom_navigation);


        bottomNavigationView.setSelectedItemId(R.id.export);
        bottomNavigationView.setOnNavigationItemSelectedListener(new BottomNavigationView.OnNavigationItemSelectedListener() {
            @Override
            public boolean onNavigationItemSelected(@NonNull MenuItem item) {
                switch (item.getItemId()){
                    case R.id.export:
                        return true;
                    case R.id.home:
                        startActivity(new Intent(getApplicationContext(),MainActivity.class));
                        overridePendingTransition(0,0);
                        return true;
                    case R.id.stats:
                        startActivity(new Intent(getApplicationContext(),Stats.class));
                        overridePendingTransition(0,0);
                        return true;
                }
                return false;
            }
        });
        nameFiles = new ArrayList<>();
        singleRecord = new ArrayList<>();
        messageList = new ArrayList<>();
        readNameFile();

        context = this;


        // adpater to visualize files lines
        adapter2= new DashBoard.RecordAdapter(messageList);
        recyclerView.setAdapter(adapter2);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));


        // adapter to visualize file
        adapter= new DashBoard.FilesAdapter(nameFiles);
        recyclerFiles.setAdapter(adapter);
        LinearLayoutManager layoutManager
                = new LinearLayoutManager(this, LinearLayoutManager.HORIZONTAL, false);
        recyclerFiles.setLayoutManager(layoutManager);

    }


/* Adapter for recycler view */

    public class FilesAdapter extends
            RecyclerView.Adapter<DashBoard.FilesAdapter.ViewHolder> {

        @NonNull
        @NotNull
        @Override
        public DashBoard.FilesAdapter.ViewHolder onCreateViewHolder(@NonNull @NotNull ViewGroup parent, int viewType) {
            Context context = parent.getContext();
            LayoutInflater inflater = LayoutInflater.from(context);
            View recordView = inflater.inflate(R.layout.records, parent , false );

            return new ViewHolder(recordView);
        }

        @Override
        public void onBindViewHolder(@NonNull @NotNull DashBoard.FilesAdapter.ViewHolder holder, int position) {
            //set the name file
            TextView text = holder.nameFile;
            text.setText(nameofFiles.get(position));
            // if name or imageFile pressed launch readFile
            holder.nameFile.setOnClickListener(
                    v -> {
                        String fileName = String.valueOf( holder.nameFile.getText());
                        readFile(fileName);
                    });

            holder.imageButton.setOnClickListener(
                    v -> {
                        String fileName = String.valueOf( holder.nameFile.getText());
                        readFile(fileName);
                    });
        }


        @Override
        public int getItemCount() {
            return nameofFiles.size();
        }

        // Provide a direct reference to each of the views within a data item
        // Used to cache the views within the item layout for fast access
        public class ViewHolder extends RecyclerView.ViewHolder {
            // Your holder should contain a member variable
            // for any view that will be set as you render a row
            public TextView nameFile;
            public ImageButton imageButton;

            // We also create a constructor that accepts the entire item row
            // and does the view lookups to find each subview
            public ViewHolder(View itemView) {
                // Stores the itemView in a public final member variable that can be used
                // to access the context from any ViewHolder instance.
                super(itemView);

                nameFile = itemView.findViewById(R.id.nameFile);
                imageButton = itemView.findViewById(R.id.imageButton2);

            }
        }

        private final ArrayList<String> nameofFiles;

        public FilesAdapter( ArrayList<String> nameFiles){

            nameofFiles = nameFiles;
        }


    }


    // read file name in the predefinited directory
    public void readNameFile(){
        // folder is always the same
        File folder = new File(Environment.getExternalStorageDirectory()+"/Android/data/com.example.readernhs/files/TestDirectory");
        // return the list of file
        File[] listOfFiles = folder.listFiles();
        if(listOfFiles!=null){

            for (File listOfFile : listOfFiles) {
                if (listOfFile.isFile()) {
                    //add in nameList all name files
                    String name = listOfFile.getName();
                    nameFiles.add(name);
                }
            }

    }

    }


    //read the lines from the clicked file and creates a array with the lines of the file
    public void readFile(String fileName){
        singleRecord.clear();
        try {
            // Same directory
            File myObj = new File(Environment.getExternalStorageDirectory()+"/Android/data/com.example.readernhs/files/TestDirectory/"+fileName);
            Scanner myReader = new Scanner(myObj);
            // read the file passed and read the lines
            while (myReader.hasNextLine()) {
                String data = myReader.nextLine();
                // in the case the file doesn't start with "id"
                if(!data.contains("id") ) {
                    // add in the list
                    singleRecord.add(data);
                }

            }
            //close the Scanner reader
            myReader.close();
        } catch (FileNotFoundException e) {
            System.out.println("An error occurred.");
            e.printStackTrace();
        }
        createMessageFormat();
    }

    // split the line and create a istance and add to ArrayList
    public void createMessageFormat(){
        // flush recycler view
        clear();
        for(int i = 0 ; i < singleRecord.size() ; i ++){
            //substring the text
            String[] parts = singleRecord.get(i).split(",");
            String data = parts[0];
            String codVersion = parts[1];

            double r = Double.parseDouble(parts[2]);
            double v1 = Double.parseDouble(parts[3]);
            double v2 = Double.parseDouble(parts[4]);
            double vds = Double.parseDouble(parts[5]);
            int ipA = Integer.parseInt(parts[6]);
            int nativeV = Integer.parseInt(parts[7]);
            // create a istance and add in the list
            MessageFormat m1 = new MessageFormat(data,codVersion,r ,v1,v2,vds,ipA,nativeV);
            messageList.add(m1);
        }
        //update the changes
        adapter2.notifyDataSetChanged();

    }

    // Adapter for visualize Recycler View for records
    public class RecordAdapter extends
            RecyclerView.Adapter<DashBoard.RecordAdapter.ViewHolder> {

        @NonNull
        @NotNull
        @Override
        public DashBoard.RecordAdapter.ViewHolder onCreateViewHolder(@NonNull @NotNull ViewGroup parent, int viewType) {
            Context context = parent.getContext();
            LayoutInflater inflater = LayoutInflater.from(context);
            View recordView = inflater.inflate(R.layout.rownocheck, parent , false );

            return new ViewHolder(recordView);
        }

        @Override
        public void onBindViewHolder(@NonNull @NotNull DashBoard.RecordAdapter.ViewHolder holder, int position) {

            TextView text = holder.data;
            String dataOnTable = messageList.get(position).getDate();
            int index =  dataOnTable.indexOf("2021");
            String rString = dataOnTable.substring(index+4).trim();
            text.setText(rString);
            TextView codText = holder.codversion;
            codText.setText(messageList.get(position).getCodVersion());
            String stringV1= Double.toString(messageList.get(position).getV1());
            holder.v1.setText(stringV1);
            String stringR= Double.toString(messageList.get(position).getR());
            holder.r.setText(stringR);
            String stringV2= Double.toString(messageList.get(position).getV2());
            holder.v2.setText(stringV2);
            String stringVds= Double.toString(messageList.get(position).getVds());
            holder.vds.setText(stringVds);
            holder.ipa.setText(String.valueOf(messageList.get(position).getIpa()));
            holder.nativeV.setText(String.valueOf(messageList.get(position).getNative()));

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

            public TextView data, r,codversion,v1,v2,vds,ipa, nativeV;


            // We also create a constructor that accepts the entire item row
            // and does the view lookups to find each subview
            public ViewHolder(View itemView) {
                // Stores the itemView in a public final member variable that can be used
                // to access the context from any ViewHolder instance.
                super(itemView);

                data = itemView.findViewById(R.id.dataTable);
                codversion = itemView.findViewById(R.id.codTable);
                r = itemView.findViewById(R.id.rTable);
                v1 = itemView.findViewById(R.id.v1Table);
                v2 = itemView.findViewById(R.id.v2Table);
                vds = itemView.findViewById(R.id.vdsTable);
                ipa = itemView.findViewById(R.id.ipaTable);
                nativeV = itemView.findViewById(R.id.nativeTable);


            }
        }

        private final ArrayList <MessageFormat> messageFormat;

        public RecordAdapter( ArrayList<MessageFormat> messageF){

            messageFormat=messageF;
        }


    }

    // for flush recycler view
    public void clear() {
        int size = messageList.size();
        if (size > 0) {
            messageList.subList(0, size).clear();

            adapter2.notifyItemRangeRemoved(0, size);
        }
    }




}