import { Component, OnInit } from '@angular/core';
import { Observable, timer } from 'rxjs';
import { ICybermesProxy } from '../models/ICybermesProxy';
import { ICybermesService } from '../models/ICybermesService';
import { CybermesProxiesService } from '../services/cybermes-proxies.service';
import { FormControl, FormGroup } from "@angular/forms";

@Component({
  selector: 'app-proxy-manager',
  templateUrl: './proxy-manager.component.html',
  styleUrls: ['./proxy-manager.component.scss']
})
export class ProxyManagerComponent implements OnInit {

  cybermesProxies : Observable<ICybermesProxy[]>;

  public formGroup: FormGroup;

  constructor(
    // inietto il service che accede all'api
    private cybermesProxiesService: CybermesProxiesService,
  ) 
  { 
    this.cybermesProxies = this.cybermesProxiesService.cybermesProxies;
    this.formGroup = new FormGroup({
      updateToggleSlider: new FormControl(true)
    });
  }

  ngOnInit(): void {
     this.getCybermesProxies();

     this.formGroup.get("updateToggleSlider")!.valueChanges.subscribe({
      next: value => {
        console.log("formcontrol value changed", value);
      }
    });
  }

  public getCybermesProxies(){
    // viene lanciato il metodo che aggiorna l'observable
    this.cybermesProxiesService.get();
  }


  public updateToggleChanged(proxyToUpdate: ICybermesProxy)
  {
    if (proxyToUpdate.updateMode)
    {
      console.log("update [ON]");
      this.cybermesProxiesService.turnUpdateModeOn(proxyToUpdate);
    }else{
      console.log("update [OFF]");
      this.cybermesProxiesService.turnUpdateModeOff(proxyToUpdate);
    }
    console.log(proxyToUpdate);

    // if update fails..
    // timer(500).subscribe(x => { this.changeUpdateToggleValueThroughFormControl(false); })
  }

  public changeUpdateToggleValueThroughFormControl(status : boolean): void {
    const fc = this.formGroup.get("updateToggleSlider");
    fc!.setValue(status);
  }


}
