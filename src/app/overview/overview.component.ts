import { Component, ViewEncapsulation } from '@angular/core';
import { ItemModel } from '@syncfusion/ej2-angular-splitbuttons';
import { MenuItemModel, MenuEventArgs  } from '@syncfusion/ej2-angular-navigations';

@Component({
  selector: 'app-overview',
  templateUrl: 'overview.component.html',
  styleUrls: ['overview.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class OverviewComponent {
  public productDropDownButtonItems: ItemModel[] = [
    { text: 'Developer' },
    { text: 'Analytics' },
    { text: 'Reporting' },
    { text: 'E-Signature' },
    { text: 'Help Desk' }
  ];

  public companyDropDownButtonItems: ItemModel[] = [
    { text: 'About Us' },
    { text: 'Customers' },
    { text: 'Blog' },
    { text: 'Careers' }
  ];

  public verticalMenuItems: MenuItemModel[] = [
    {
      iconCss: 'e-icons e-more-vertical-1',
      items: [
        { text: 'Home' },
        {
          text: 'Products',
          items: [
            { text: 'Developer' },
            { text: 'Analytics' },
            { text: 'Reporting' },
            { text: 'E-Signature' },
            { text: 'Help Desk' }
          ]
        },
        {
          text: 'Company',
          items: [
            { text: 'About Us' },
            { text: 'Customers' },
            { text: 'Blog' },
            { text: 'Careers' }
          ]
        },
        { text: 'Login' }
      ]
    }
  ];

  public colorMode : string = "Primary";
  public colorClass: string = "e-primary";
  public loginClass : string = "e-inherit login";

  public focusIn(target: EventTarget | null): void {
//    target!.parentElement!.classList.add('e-input-focus');
  }
  public focusOut(target: EventTarget | null): void {
//    target!.parentElement!.classList.remove('e-input-focus');
  }
  public onBeforeItemRender(args: MenuEventArgs): void {
    if (args.element.children.length > 0 && args.element.children[0].classList.contains("e-more-vertical-1")) {
      args.element.setAttribute('aria-label', 'more vertical');
    }
  }
  public onCreate(): void {
    const menuButtonElement = document.querySelectorAll('.color-appbar-section .e-inherit.menu');
    for (let i = 0; i < menuButtonElement.length; i++) {
      if(!(menuButtonElement[i].hasAttribute("aria-label"))) {
        menuButtonElement[i].setAttribute('aria-label','menu');
      }
    }
  }
}
