import { Injectable } from '@angular/core';
import { BASE_URL } from '../app.constants';
import { ICybermesService } from '../models/ICybermesService';
import { catchError, delay, map, retryWhen, take } from 'rxjs/operators';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { IMessage } from '../models/IMessage';

@Injectable({
  providedIn: 'root'
})
export class CyermesServicesService {

  // Property get per esporre l'observable esternamente
  get cybermesServices(): Observable<ICybermesService[]> {
    return this._cybermesServices.asObservable();
  }

  // Behavior subject è una sorta di broker dove si può pubblicare
  // dei valori tramite comando .next
  private _cybermesServices: BehaviorSubject<ICybermesService[]>;

  // definizione del datastore che riceverà i dati
  private datastore: {
    cybermesServices: ICybermesService[]
  }

  constructor(private httpClient: HttpClient) { 
    // inizializzazione valori del datastore come array vuoto di valori
    this.datastore = { cybermesServices: [] }

    // inizializzazione behaviorSubjects (non ricordo cosa faccia sta roba)
    this._cybermesServices = new BehaviorSubject<ICybermesService[]>([]);
  }


  // metodo get per l'interrogazione dell'api in modalità protetta
  get(){
      const url = `${BASE_URL}/CybermesServicesController/GetAllServices`;
      return this.httpClient.get<ICybermesService[]>(url)
          .pipe(
              retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte, ogni 2 sec
              )
          .subscribe(data => {
            this.datastore.cybermesServices = data;
            console.log(data);
  
            // alla ricezione di un valore pubblica sul behavior subject il nuovo valore ricevuto
            this._cybermesServices.next(Object.assign({}, this.datastore).cybermesServices);
          })
    }


    public downloadFile(filename: string | null = null, applicationPath: string): void{

      const url = `${BASE_URL}/CybermesServicesController/DownloadAllLogFiles`;
      this.httpClient.get(
        url,
        {
          responseType: 'blob' as 'json',
          params: new HttpParams().set('applicationPath', applicationPath),
          observe: 'response'
        }
        ).subscribe(
          (response: any) =>{
              let dataType = response.body.type;
              let binaryData = [];
              binaryData.push(response.body);
              let downloadLink = document.createElement('a');
              downloadLink.href = window.URL.createObjectURL(new Blob(binaryData, {type: dataType}));
              if (filename)
              {
                downloadLink.setAttribute('download', filename);
              }
              else
              {
                var contentDisposition = response.headers.get('content-disposition');
            
                var dispositions = contentDisposition.replaceAll(' ','').split(';');
                dispositions.forEach((element: string) => 
                {
                  if (element.startsWith('filename='))
                  {
                    var assignedFilename = element.replace('filename=','');
                    downloadLink.setAttribute('download', assignedFilename);
                  }
                });
              }
                
              document.body.appendChild(downloadLink);
              downloadLink.click();
          }
      )
  }
    


  handleError(error: HttpErrorResponse)
  {
    let errorMessage = 'Errore sconosciuto';
    let errore: IMessage;

    errore = error.error;

    if (error.error instanceof ErrorEvent)
    {
      errorMessage = `Errore: ${error.error.message}`;
    }else{
      errorMessage = errore.messaggio;
    }

    window.alert(errorMessage);
    
    return throwError(error);
  }
}

