import { Component } from '@angular/core';
import { RulesConfigComponent } from './features/rules-config/rules-config.component';
import { InvoiceUploadComponent } from './features/invoice-upload/invoice-upload.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RulesConfigComponent, InvoiceUploadComponent, DashboardComponent],
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'frontend';
}