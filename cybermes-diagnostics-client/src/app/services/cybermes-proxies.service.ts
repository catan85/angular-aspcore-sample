import { Injectable } from '@angular/core';
import { BASE_URL } from '../app.constants';
import { ICybermesProxy } from '../models/ICybermesProxy';
import { catchError, delay, map, retryWhen, take } from 'rxjs/operators';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams } from '@angular/common/http';
import { IMessage } from '../models/IMessage';

@Injectable({
  providedIn: 'root'
})
export class CybermesProxiesService {
  
  // Behavior subject è una sorta di broker dove si può pubblicare
  // dei valori tramite comando .next
  private _cybermesProxies: BehaviorSubject<ICybermesProxy[]>;

  // definizione del datastore che riceverà i dati
  private datastore: {
    cybermesProxies: ICybermesProxy[]
  }

  constructor(private httpClient: HttpClient) { 
    // inizializzazione valori del datastore come array vuoto di valori
    this.datastore = { cybermesProxies: [] }

    // inizializzazione behaviorSubjects (non ricordo cosa faccia sta roba)
    this._cybermesProxies = new BehaviorSubject<ICybermesProxy[]>([]);
  }


  // metodo get per l'interrogazione dell'api in modalità protetta
  get(){
      const url = `${BASE_URL}/CybermesProxiesController/GetAllProxies`;
      return this.httpClient.get<ICybermesProxy[]>(url)
          .pipe(
              retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte ogni 2 sec
              )
          .subscribe(data => {
            this.datastore.cybermesProxies = data;
            console.log(data);
  
            // alla ricezione di un valore pubblica sul behavior subject il nuovo valore ricevuto
            //this._cybermesProxies.next(Object.assign({}, this.datastore).cybermesProxies);
            this._cybermesProxies.next(this.datastore.cybermesProxies); // tolto object.assign, non so che cosa faccia ma sembra superfluo
          })
    }

  
  turnUpdateModeOn(proxy : ICybermesProxy ){
    const url = `${BASE_URL}/CybermesProxiesController/TurnUpdateModeOn`;

    this.httpClient.post<any>(url, proxy).subscribe(data => {
      console.log(data);
    });
  }

  turnUpdateModeOff(proxy : ICybermesProxy ){
    const url = `${BASE_URL}/CybermesProxiesController/TurnUpdateModeOff`;

    this.httpClient.post<any>(url, proxy).subscribe(data => {
      console.log(data);
    });
  }

    // Property get per esporre l'observable esternamente
  get cybermesProxies(): Observable<ICybermesProxy[]> {
    return this._cybermesProxies.asObservable();
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
