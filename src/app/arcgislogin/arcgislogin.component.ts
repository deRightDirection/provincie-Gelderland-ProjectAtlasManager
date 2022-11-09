import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import OAuthInfo from "@arcgis/core/identity/OAuthInfo";
import esriId from "@arcgis/core/identity/IdentityManager";
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-arcgislogin',
  templateUrl: './arcgislogin.component.html',
  styleUrls: ['./arcgislogin.component.css']
})
export class ArcgisloginComponent implements OnInit {
  public showLogin : boolean = true;
  public username : string = "";

  constructor(private _router: Router) { }

  ngOnInit() {
    esriId.registerOAuthInfos([this.info]);
  }

  private info: OAuthInfo = new OAuthInfo({
    appId: environment.appid,
    portalUrl: environment.portalurl,
    flowType: "authorization-code",
    popup: true,
    popupCallbackUrl: "assets/oauth-callback.html"
  });

  onLogin() {
    esriId.destroyCredentials();
      esriId.checkSignInStatus(this.info.portalUrl + "/sharing")
        .then(x => {
          this.goToOverview(x);
        })
        .catch(() => { });

      esriId.getCredential((this.info.portalUrl + "/sharing"), {
          oAuthPopupConfirmation: false
        })
        .then(x =>{
          this.goToOverview(x);
        })
        .catch(()=>{
          console.log("login afgebroken");
        });
  }

  goToOverview(credential : __esri.Credential): void  {
    this.username = credential.userId
    this.showLogin = false;
    this._router.navigate(['overview']);
  };
}
