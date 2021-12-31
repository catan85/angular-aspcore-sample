import { Component, OnInit } from '@angular/core';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { ICybermesService } from '../models/ICybermesService';
import { isObservable, Observable } from 'rxjs';
import { CyermesServicesService } from '../services/cybermes-services.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-service-explorer',
  templateUrl: './service-explorer.component.html',
  styleUrls: ['./service-explorer.component.scss']
})
export class ServiceExplorerComponent implements OnInit {

  cybermesServices : Observable<ICybermesService[]>;

  constructor(
    // inietto il service che accede all'api
    private cybermesServicesService: CyermesServicesService,
  ) { 
    this.cybermesServices = this.cybermesServicesService.cybermesServices;
  }

  ngOnInit(): void {
     this.getCybermesServices();
  }

  public getCybermesServices(){
    // viene lanciato il metodo che aggiorna l'observable
    this.cybermesServicesService.getPrivate();
  }

  public download(appPath : string)
  {
    console.log('Download clicked, path is' + appPath);
    this.cybermesServicesService.downloadFile(null,appPath);
  }

}
