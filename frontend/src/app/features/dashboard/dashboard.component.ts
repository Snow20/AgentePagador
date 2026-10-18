import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoiceService } from '../../core/services/invoice.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  @Input() currentLang: 'ES' | 'EN' = 'ES';
  invoices: any[] = [];

  translations: Record<'ES' | 'EN', any> = {
    ES: {
      title: 'Panel de Resultados',
      refreshBtn: 'Actualizar Resultados',
      noInvoices: 'No hay facturas registradas en la base de datos.',
      colFile: 'Archivo',
      colStatus: 'Estado',
      colTrafficLight: 'Semáforo',
      colExplanation: 'Explicación'
    },
    EN: {
      title: 'Results Panel',
      refreshBtn: 'Refresh Results',
      noInvoices: 'No invoices registered in the database.',
      colFile: 'File',
      colStatus: 'Status',
      colTrafficLight: 'Traffic Light',
      colExplanation: 'Explanation'
    }
  };

  constructor(private invoiceService: InvoiceService) {}

  get t() { return this.translations[this.currentLang]; }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.invoiceService.getDashboard().subscribe({
      next: (data) => {
        this.invoices = data;
      },
      error: (err) => console.error('Error al cargar el dashboard:', err)
    });
  }
}
