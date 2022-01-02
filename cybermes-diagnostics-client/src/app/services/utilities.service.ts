import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, ObjectUnsubscribedError, Observable } from 'rxjs';
import { delay, retryWhen } from 'rxjs/operators';
import { BASE_URL } from '../app.constants';

@Injectable({
  providedIn: 'root'
})
export class UtilitiesService {
  // Behavior subject è una sorta di broker dove si può pubblicare
  // dei valori tramite comando .next
  private _apiMesUrlClients: BehaviorSubject<string>;
  private _apiMesUrlServices: BehaviorSubject<string>;
  private _apiMesUrlReports: BehaviorSubject<string>;

  // proprietà pubbliche che servono ad esporre i behavior subject
  // i sottoscrittori quindi creeranno un oggetto locale bindato a queste proprità così che quando
  // noi popoleremo questi valori automaticamente gli osservatori li riceveranno
  get apiMesUrlClients() : Observable<string>
  {
    return this._apiMesUrlClients.asObservable();
  }

  get apiMesUrlServices() : Observable<string>
  {
    return this._apiMesUrlServices.asObservable();
  }

  get apiMesUrlReports() : Observable<string>
  {
    return this._apiMesUrlReports.asObservable();
  }

  private datastore: {
    apiMesUrlClients: string;
    apiMesUrlServices: string;
    apiMesUrlReports: string;
  }

  constructor(private httpClient: HttpClient) { 
    this.datastore = {
      apiMesUrlClients : 'initial value',
      apiMesUrlServices : 'initial value',
      apiMesUrlReports: 'initial value'
    }

    this._apiMesUrlClients = new BehaviorSubject<string>('');
    this._apiMesUrlServices = new BehaviorSubject<string>('');
    this._apiMesUrlReports = new BehaviorSubject<string>('');

  }

  // la chiamata torna un testo in formato testo raw, non json serializzato
  // quindi dovremo chiamare httpClient.get specificando la responsetype: text

  getApiMesUrlClients(){
    const url = `${BASE_URL}/UtilitiesController/GetApiMesUrlClients`;
    return this.httpClient.get(url, { observe: 'body', responseType: 'text'})
      .pipe(
        retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte
        )
      .subscribe( data => {
        // quando ricevo il valore lo scrivo nel datastore
        this.datastore.apiMesUrlClients = data;
        console.log('received value!');
        console.log(data);
        this._apiMesUrlClients.next(this.datastore.apiMesUrlClients);
      })
  }

  getApiMesUrlServices(){
    const url = `${BASE_URL}/UtilitiesController/GetApiMesUrlServices`;
    return this.httpClient.get(url, { observe: 'body', responseType: 'text'})
      .pipe(
        retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte
        )
      .subscribe( data => {
        // quando ricevo il valore lo scrivo nel datastore
        this.datastore.apiMesUrlServices = data;
        console.log('received value!');
        console.log(data);
        this._apiMesUrlServices.next(this.datastore.apiMesUrlServices);
      })
  }

  getApiMesUrlReports(){
    const url = `${BASE_URL}/UtilitiesController/GetApiMesUrlReports`;
    return this.httpClient.get(url, { observe: 'body', responseType: 'text'})
      .pipe(
        retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte
        )
      .subscribe( data => {
        // quando ricevo il valore lo scrivo nel datastore
        this.datastore.apiMesUrlReports = data;
        console.log('received value!');
        console.log(data);
        this._apiMesUrlReports.next(this.datastore.apiMesUrlReports);
      })
  }

}
