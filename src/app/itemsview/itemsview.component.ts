import Portal from '@arcgis/core/portal/Portal';
import PortalItem from "@arcgis/core/portal/PortalItem";
import PortalQueryResult from "@arcgis/core/portal/PortalQueryResult";
import { Component, OnInit, AfterViewInit, ViewEncapsulation, Inject } from '@angular/core';

@Component({
  selector: 'app-itemsview',
  templateUrl: './itemsview.component.html',
  styleUrls: ['./itemsview.component.css'],
    encapsulation: ViewEncapsulation.None
})
export class ItemsviewComponent  implements OnInit {
  private AGOLPortal : Portal = new Portal();
  public DataList: any = [];
    public ngOnInit(): void {
      this.AGOLPortal.authMode = 'immediate';
      this.AGOLPortal.load().then(() => {
        console.log(this.AGOLPortal.restUrl);
        console.log("portal id: " + this.AGOLPortal.id);
 //       var tagSearches = bouwBlokken.flatMap(bb => "(tags:\"" + bb + "," + x + "\" AND NOT tags:\"vervallen\")");
        let queryParams = {
          num: 150,

//          query: "(" + tagSearches.join(" OR ") + ") orgid:" + this.AGOLPortal.id + " (type:(\"Feature Service\"))"
          query: "orgid:" + this.AGOLPortal.id + " AND type:(\"Web Map\")"
        };
        console.log(queryParams);
        this.AGOLPortal.queryItems(queryParams).then((result: PortalQueryResult) => {
          var xx = this.getBouwblokNamen(result);
        });
      });
    }
    ngAfterViewInit(): void {
    }

    /** haal alle bouwblok namen op */
  getBouwblokNamen(result: PortalQueryResult){
    console.log(result.results.length);

    var cards: Object[] = [];

    result.results.forEach((item: PortalItem) => {
      console.log(item.title);
      console.log(item.thumbnailUrl);
      var card = {
        Title : item.title,
        Thumbnail: item.thumbnailUrl
      }
      cards.push(card);
    });
    this.DataList = cards;
  }
}
