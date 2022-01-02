import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { UtilitiesService } from '../services/utilities.service';

@Component({
  selector: 'app-main-content',
  templateUrl: './main-content.component.html',
  styleUrls: ['./main-content.component.scss']
})
export class MainContentComponent implements OnInit {

  utilitiesService: UtilitiesService;

  apiMesUrlClients: Observable<string>;
  apiMesUrlReports: Observable<string>;
  apiMesUrlServices: Observable<string>;

  constructor(utilitiesService: UtilitiesService) { 
    this.utilitiesService = utilitiesService;

    // associazione degli observable locali a quelli del servizio.
    // quando il servizio aggiornerà i valori li riceveremo automaticamente 
    // negli observable
    this.apiMesUrlClients = this.utilitiesService.apiMesUrlClients;
    this.apiMesUrlReports = this.utilitiesService.apiMesUrlReports;
    this.apiMesUrlServices = this.utilitiesService.apiMesUrlServices;
  }

  ngOnInit(): void {
    this.GetAllApiMesUrls();
  }

  public GetAllApiMesUrls()
  {
    this.utilitiesService.getApiMesUrlClients();
    this.utilitiesService.getApiMesUrlReports();
    this.utilitiesService.getApiMesUrlServices();
  }

}
