import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainContentComponent } from './main-content/main-content.component';
import { ServiceExplorerComponent } from './service-explorer/service-explorer.component';


const routes: Routes = [
  { path: 'main-content', component: MainContentComponent },
  { path: 'service-explorer', component: ServiceExplorerComponent },
  { path: '', redirectTo: '/main-content', pathMatch: 'full' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
