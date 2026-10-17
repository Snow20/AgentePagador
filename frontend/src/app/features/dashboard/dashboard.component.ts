import { Component, OnInit } from '@angular/core';
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
  invoices: any[] = [];

  constructor(private invoiceService: InvoiceService) {}

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