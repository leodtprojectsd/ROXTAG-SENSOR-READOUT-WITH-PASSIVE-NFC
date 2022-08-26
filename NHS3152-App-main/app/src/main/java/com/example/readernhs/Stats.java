package com.example.readernhs;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.lifecycle.ViewModelProvider;
import androidx.lifecycle.ViewModelProviders;
import androidx.lifecycle.ViewModelStore;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.os.Environment;
import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.Spinner;
import android.widget.TextView;

import com.google.android.material.bottomnavigation.BottomNavigationView;

import org.jetbrains.annotations.NotNull;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;

public class Stats extends AppCompatActivity {

    private static final String TAG = MainActivity.class.getSimpleName();
    Context context;
    RecyclerView recyclerFiles;
    FilesAdapter adapter ;
    // this one contain every namefile in the folder
    ArrayList<String> nameFiles;

    Button newPlot;
    // the single record from a file
    List<String> singleRecord ;
    ArrayList<MessageFormat> messageList;


    String[] nameFileToPlot;

    ArrayList<MessageFormat> messageToPlot ;

    SharedModel viewModel = new SharedModel();


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        try
        {
            this.getSupportActionBar().hide();
        }
        catch (NullPointerException e){}


        setContentView(R.layout.activity_stats);
        BottomNavigationView bottomNavigationView = findViewById(R.id.bottom_navigation);


        viewModel  = new ViewModelProvider(this).get(SharedModel.class);

        bottomNavigationView.setSelectedItemId(R.id.stats);
        bottomNavigationView.setOnNavigationItemSelectedListener(new BottomNavigationView.OnNavigationItemSelectedListener() {
            @Override
            public boolean onNavigationItemSelected(@NonNull MenuItem item) {
                switch (item.getItemId()){
                    case R.id.export:
                        startActivity(new Intent(getApplicationContext(),DashBoard.class));
                        overridePendingTransition(0,0);
                        return true;
                    case R.id.home:
                        startActivity(new Intent(getApplicationContext(),MainActivity.class));
                        overridePendingTransition(0,0);
                        return true;
                    case R.id.stats:

                        return true;

                }
                return false;
            }
        });

        Spinner dropdown = findViewById(R.id.spinner);
        //create a list of items for the spinner.
        String[] items = new String[]{"R", "V1", "V2", "VDS"};
    //create an adapter to describe how the items are displayed
        ArrayAdapter<String> adapter2 = new ArrayAdapter<>(this, android.R.layout.simple_spinner_dropdown_item, items);
        //set the spinners adapter to the previously created one.
        dropdown.setAdapter(adapter2);

        dropdown.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
                viewModel.setStats(i);
            }

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {

            }
        });
        messageList = new ArrayList<>();
        singleRecord = new ArrayList<>();
        nameFiles = new ArrayList<>();
        messageToPlot = new ArrayList<>();
        readNameFile(); // the same in the DashBoard Activity

        nameFileToPlot = new String[nameFiles.size()];

        recyclerFiles= findViewById(R.id.filesReader);
        recyclerFiles.addOnItemTouchListener(
                new RecyclerItemClickListener(context, recyclerFiles ,new RecyclerItemClickListener.OnItemClickListener() {
                    @Override public void onItemClick(View view, int position) {
                        // if view element isn't already selected , add in the String array the name of the file
                        if(!view.isSelected()){
                            view.setSelected(true);
                            nameFileToPlot[position] = nameFiles.get(position);
                        }

                        else {
                            // if is already selected, change the file name to a empty name
                            view.setSelected(false);
                            nameFileToPlot[position] = "";
                        }

                    }

                    @Override public void onLongItemClick(View view, int position) {

                        // do whatever
                    }
                })
        );




        context = this;
        adapter = new FilesAdapter(nameFiles);
        recyclerFiles.setAdapter(adapter);
        LinearLayoutManager layoutManager
                = new LinearLayoutManager(this, LinearLayoutManager.HORIZONTAL, false);
        recyclerFiles.setLayoutManager(layoutManager);


        // fragment for chart
        getSupportFragmentManager().beginTransaction()
                .setReorderingAllowed(true)
                .add(R.id.flFragment, PlotFragment.class, null)
                .commit();

        // shared model for pass data to fragment

        newPlot = findViewById(R.id.buttonStart);
        // if the button clicked, plot the graph
        newPlot.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {

                messageList.clear();


                for ( int i = 0 ; i < nameFileToPlot.length; i ++){
                    // if the name isn't equals to null or empty String read and create the plot
                    if(nameFileToPlot[i]!=null && nameFileToPlot[i] !="") {

                        readFile(nameFileToPlot[i]);
                    }
                }
                // pass date to Fragment
                viewModel.setMessages(messageList);

                singleRecord.clear();
                //start Fragment
                getSupportFragmentManager().beginTransaction()
                        .setReorderingAllowed(true)
                        .replace(R.id.flFragment, PlotFragment.class, null)
                        .commit();



            }
        });





    }

        // Very similar Adapter to Dashboard's adapter
    public class FilesAdapter extends
            RecyclerView.Adapter<Stats.FilesAdapter.ViewHolder> {



        @NonNull
        @NotNull
        @Override
        public Stats.FilesAdapter.ViewHolder onCreateViewHolder(@NonNull @NotNull ViewGroup parent, int viewType) {
            Context context = parent.getContext();
            LayoutInflater inflater = LayoutInflater.from(context);
            View recordView = inflater.inflate(R.layout.records, parent , false );

            Stats.FilesAdapter.ViewHolder viewHolder = new Stats.FilesAdapter.ViewHolder(recordView);
            return viewHolder;
        }

        @Override
        public void onBindViewHolder(@NonNull @NotNull Stats.FilesAdapter.ViewHolder holder, int position) {
            TextView text = holder.nameFile;
            text.setText(nameofFiles.get(position));

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

        private ArrayList<String> nameofFiles;

        public FilesAdapter( ArrayList<String> nameFiles){

            nameofFiles = nameFiles;
        }


    }

    public void readNameFile(){

        File folder = new File(Environment.getExternalStorageDirectory()+"/Android/data/com.example.readernhs/files/TestDirectory");
        File[] listOfFiles = folder.listFiles();
        if(listOfFiles!=null){

            for (int i = 0; i < listOfFiles.length; i++) {
                if (listOfFiles[i].isFile()) {
                    // System.out.println("File " + listOfFiles[i].getName());
                    String name = listOfFiles[i].getName();
                    nameFiles.add(name);
                }
            }

        }

    }



    public void readFile(String fileName){

        try {
            File myObj = new File(Environment.getExternalStorageDirectory()+"/Android/data/com.example.readernhs/files/TestDirectory/"+fileName);
            Scanner myReader = new Scanner(myObj);
            while (myReader.hasNextLine()) {
                String data = myReader.nextLine();
                if(!data.contains("id") ) {

                    singleRecord.add(data);
                }

            }
            myReader.close();
        } catch (FileNotFoundException e) {
            System.out.println("An error occurred.");
            e.printStackTrace();
        }
        createMessageFormat();
    }

    // split the line and create a istance and add to ArrayList
    public void createMessageFormat(){
         messageList.clear();
        for(int i = 0 ; i < singleRecord.size() ; i ++){

            String[] parts = singleRecord.get(i).split(",");
            String data = parts[0];
            String codVersion = parts[1];
            double r =  Double.valueOf(parts[2]);
            double v1 = Double.valueOf(parts[3]);
            double v2 = Double.valueOf(parts[4]);
            double vds = Double.valueOf(parts[5]);
            int ipA = Integer.valueOf(parts[6]);
            int nativeV = Integer.valueOf(parts[7]);
            MessageFormat m1 = new MessageFormat(data,codVersion, r,v1,v2,vds,ipA,nativeV);
            messageList.add(m1);
        }

    }





}