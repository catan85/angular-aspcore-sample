import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { LoginComponent } from '../login/login.component';
import { MatDialog } from '@angular/material/dialog';
import { AuthenticationService } from '../services/authentication.service';
import { User } from '../models/User';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit {
  @Output() toggleSidenav = new EventEmitter<void>();
  constructor(
    private dialog : MatDialog,
    private authenticationService: AuthenticationService
    ) { }


  public get currentUser(): string | null {
    return this.authenticationService.currentUserValue!.username;
  }

  ngOnInit(): void {
  }

  authenticated(){
    return this.authenticationService.currentUserValue != null;
  }
  
  openLoginDialog()
  {
    console.log("premuto apertura dialog");
    
    let dialogRef = this.dialog.open(LoginComponent, {
      width: '450px'
    });

    dialogRef.afterClosed().subscribe( result => {
      console.log("Il dialog è stato chiuso", result);
    });
  }

  logout(){
    this.authenticationService.logout();
  }

}
