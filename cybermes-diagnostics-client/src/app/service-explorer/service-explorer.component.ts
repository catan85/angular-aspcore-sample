import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { IWeatherForecast } from '../models/IWeatherForecast';
import { isObservable, Observable } from 'rxjs';
import { WeatherForecastService } from '../services/weather-forecast.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-service-explorer',
  templateUrl: './service-explorer.component.html',
  styleUrls: ['./service-explorer.component.scss']
})
export class ServiceExplorerComponent implements OnInit {

  weatherForecasts : Observable<IWeatherForecast[]>;

  constructor(
    // inietto il service che accede all'api
    private weatherForecastService: WeatherForecastService,
  ) { 
    this.weatherForecasts = this.weatherForecastService.weatherForecasts;
  }

  ngOnInit(): void {
     this.getWeatherForecasts();
  }

  public getWeatherForecasts(){
    // viene lanciato il metodo che aggiorna l'observable
    this.weatherForecastService.getPrivate();
  }
}
