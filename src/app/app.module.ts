import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { Router } from '@angular/router';
import { ArcgisloginComponent } from './arcgislogin/arcgislogin.component';
import { OverviewComponent } from './overview/overview.component';
import { AppBarModule } from "@syncfusion/ej2-angular-navigations";
import { MenuModule } from '@syncfusion/ej2-angular-navigations'
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { DropDownButtonModule } from '@syncfusion/ej2-angular-splitbuttons';
import { ItemsviewComponent } from './itemsview/itemsview.component';

@NgModule({
  declarations: [	
    AppComponent,
    ArcgisloginComponent,
      OverviewComponent,
      ItemsviewComponent
   ],
  imports: [
    DropDownButtonModule,
    ButtonModule,
    MenuModule,
    AppBarModule,
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
