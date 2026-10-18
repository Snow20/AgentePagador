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
      colExplanation: 'Explicación',
      statusFrenada: 'Frenada',
      statusEscalada: 'Escalada',
      statusPagada: 'Pagada',
      expFrenada: 'Frenar: Infracción crítica detectada (duplicada, límite excedido o ilegal).',
      expEscalada: 'Escalar: Duda o regla que exige revisión por Finanzas, Legal o Gerencia.',
      expPagada: 'Pagar: Cumple todas las reglas, saldo suficiente.'
    },
    EN: {
      title: 'Results Panel',
      refreshBtn: 'Refresh Results',
      noInvoices: 'No invoices registered in the database.',
      colFile: 'File',
      colStatus: 'Status',
      colTrafficLight: 'Traffic Light',
      colExplanation: 'Explanation',
      statusFrenada: 'Stopped',
      statusEscalada: 'Escalated',
      statusPagada: 'Paid',
      expFrenada: 'Stop: Critical violation detected (duplicate, limit exceeded, or illegal).',
      expEscalada: 'Escalate: Doubt or rule requiring review by Finance, Legal, or Management.',
      expPagada: 'Pay: Meets all rules, sufficient balance.'
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
        this.invoices = data.map((invoice: any) => this.enforceClassificationRules(invoice));
      },
      error: (err) => console.error('Error al cargar el dashboard:', err)
    });
  }

  private enforceClassificationRules(invoice: any): any {
    const isDuplicate = invoice.isDuplicate || false;
    const isIllegal = invoice.isIllegal || false;
    const noBudget = invoice.amount > 10000; 
    const requiresHuman = invoice.needsReview || invoice.amount > 8000; 

    if (isDuplicate || isIllegal || noBudget) {
      return {
        ...invoice,
        status: this.t.statusFrenada,
        color: 'Rojo',
        explanation: this.t.expFrenada
      };
    }

    if (requiresHuman) {
      return {
        ...invoice,
        status: this.t.statusEscalada,
        color: 'Amarillo',
        explanation: this.t.expEscalada
      };
    }

    return {
      ...invoice,
      status: this.t.statusPagada,
      color: 'Verde',
      explanation: this.t.expPagada
    };
  }
}
