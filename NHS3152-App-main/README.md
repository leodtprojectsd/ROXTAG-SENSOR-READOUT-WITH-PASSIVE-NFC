# NHS3152-App
App to read data from NHS3152 



 HOW TO INSTALL: 

 If you have to use just the application, go to  \app\build\outputs\apk\debug and download the apk.
 
 Otherwise clone or download the application and open it on Android Studio!
 
 
 

FUNZIONAMENTO APPLICAZIONE: 
Ci sono due modalità di lettura, entrambe disponibili nella schermata principale: 
•	Lettura singola :
 Appoggiando il telefono al TAG viene ricevuto e visualizzato il singolo messaggio.
•	Lettura Multiple: 
E’ possibile innanzitutto specificare la frequenza della recezione dei messaggi, il valore di default è 3000ms, è possibile cambiare il valore nell’EditText apposita. 
Successivamente vengono visualizzati i ‘record’ nella schermata principale. E’ possibile selezionare singolarmente i record da esportare schiacciando nell’apposita checkbox, o selezionare i record che fanno parte dell’ultima sessione di lettura premendo il bottone “Select last record”. 
Dopodichè per esportare effettivamente i ‘record’ bisogna premere il tasto ‘Export Selected”, l’applicazione creerà un file csv con all’interno i record selezionati. 
Inoltre è possibile pulire la schermata con il bottone “Clear data”.

Visualizzazione dei dati: 
Nella schermata ‘Explorer’ è possibile visualizzare i file creati.
I file si trovano anche nella memoria del telefono in:  “Android\data\com.example.readernhs\files\TestDirectory”
In alto è presente una lista (se almeno un file è stato creato) dei file coi i nomi corrispondenti. Premendo su il ‘file’ vengono visualizzati tutti i record che lo compongono. 

Statistiche: 
Inoltre è presente una schermata che permette di creare un grafico per i valori: 
•	R 
•	V1 
•	V2 
•	VDS 
E’ possibile selezionare un file o più premendo su essi, dopodiché premendo su il tasto ‘Plot’ disegna il grafico. 



APK in : \app\build\outputs\apk\debug
