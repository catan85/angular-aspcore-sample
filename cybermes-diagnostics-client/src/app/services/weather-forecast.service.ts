import { Injectable } from '@angular/core';
import { BASE_URL } from '../app.constants';
import { IWeatherForecast } from '../models/IWeatherForecast';
import { catchError, delay, map, retryWhen, take } from 'rxjs/operators';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { IMessage } from '../models/IMessage';

@Injectable({
  providedIn: 'root'
})
export class WeatherForecastService {

  // definizione behaviorSubjects (non ricordo cosa faccia sta roba)
  private _weatherForecasts: BehaviorSubject<IWeatherForecast[]>;

  // definizione del datastore che riceverà i dati
  private datastore: {
    weatherForecasts: IWeatherForecast[]
  }

  constructor(private httpClient: HttpClient) { 
    // inizializzazione valori del datastore come array vuoto di valori
    this.datastore = { weatherForecasts: [] }

    // inizializzazione behaviorSubjects (non ricordo cosa faccia sta roba)
    this._weatherForecasts = new BehaviorSubject<IWeatherForecast[]>([]);
  }

  // metodo get per l'interrogazione dell'api
  get(){
    const url = `${BASE_URL}/WeatherForecast/Get`;
    return this.httpClient.get<IWeatherForecast[]>(url)
        .pipe(
            retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte
            )
        .subscribe(data => {
          this.datastore.weatherForecasts = data;
          console.log(data);

          // non ricordo cosa faccia sta roba
          this._weatherForecasts.next(Object.assign({}, this.datastore).weatherForecasts);
        })
  }

  // metodo get per l'interrogazione dell'api in modalità protetta
  getPrivate(){
      const url = `${BASE_URL}/WeatherForecast/GetPrivate`;
      return this.httpClient.get<IWeatherForecast[]>(url)
          .pipe(
              retryWhen(errors => errors.pipe(delay(2000))),  // riprova un numero indeterminato di volte
              )
          .subscribe(data => {
            this.datastore.weatherForecasts = data;
            console.log(data);
  
            // non ricordo cosa faccia sta roba
            this._weatherForecasts.next(Object.assign({}, this.datastore).weatherForecasts);
          })
    }

  // Property get per esporre l'observable esternamente
  get weatherForecasts(): Observable<IWeatherForecast[]> {
    return this._weatherForecasts.asObservable();
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

