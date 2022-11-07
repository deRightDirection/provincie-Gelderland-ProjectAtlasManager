import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { Router } from '@angular/router';
import { ArcgisloginComponent } from './arcgislogin/arcgislogin.component';
import { OverviewComponent } from './overview/overview.component';

@NgModule({
  declarations: [	
    AppComponent,
    ArcgisloginComponent,
      OverviewComponent
   ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FlexLayoutModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor(router: Router) {
  }
}
