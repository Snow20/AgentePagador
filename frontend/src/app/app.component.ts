import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule], // Obligatorio para usar *ngIf y *ngFor en el HTML
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  currentLang: 'ES' | 'EN' = 'ES';

  // Tipado estricto para evitar el error TS7053
  translations: Record<'ES' | 'EN', any> = {
    ES: {
      title: 'Agente Pagador',
      subtitle: 'Automatización de Facturas con IA',
      ingestTitle: 'Ingesta de Facturas',
      ingestDesc: 'Sube la factura (PDF o Imagen) para que el Agente la procese y evalúe las reglas.',
      processBtn: 'Procesar Factura',
      resultsTitle: 'Panel de Resultados',
      refreshBtn: 'Actualizar Resultados',
      noInvoices: 'No hay facturas registradas en la base de datos.',
      footer: '© Copyright hecho por Fenrirsoft 2026. Todos los derechos reservados.',
      apiError: 'Error de conexión con el Agente API.',
      processing: 'Procesando...'
    },
    EN: {
      title: 'Paying Agent',
      subtitle: 'AI Invoice Automation',
      ingestTitle: 'Invoice Ingestion',
      ingestDesc: 'Upload the invoice (PDF or Image) for the Agent to process and evaluate the rules.',
      processBtn: 'Process Invoice',
      resultsTitle: 'Results Panel',
      refreshBtn: 'Refresh Results',
      noInvoices: 'No invoices registered in the database.',
      footer: '© Copyright made by Fenrirsoft 2026. All rights reserved.',
      apiError: 'API Agent connection error.',
      processing: 'Processing...'
    }
  };

  // Variables de estado restauradas
  selectedFile: File | null = null;
  isLoading = false;
  apiError = false;
  invoices: any[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadInvoices();
  }

  get t() {
    return this.translations[this.currentLang];
  }

  toggleLanguage() {
    this.currentLang = this.currentLang === 'ES' ? 'EN' : 'ES';
  }

  // Funciones operativas restauradas
  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0] || null;
  }

  uploadInvoice() {
    if (!this.selectedFile) return;
    this.isLoading = true;
    this.apiError = false;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post(`${environment.apiUrl}/invoice`, formData).subscribe({
      next: () => {
        this.isLoading = false;
        this.selectedFile = null;
        this.loadInvoices(); // Refresca la tabla automáticamente
      },
      error: (err) => {
        console.error('Error subiendo factura:', err);
        this.isLoading = false;
        this.apiError = true;
      }
    });
  }

  loadInvoices() {
    this.http.get<any[]>(`${environment.apiUrl}/invoice/dashboard`).subscribe({
      next: (data) => {
        this.invoices = data;
        this.apiError = false;
      },
      error: (err) => {
        console.error('Error cargando facturas:', err);
        this.apiError = true;
      }
    });
  }
}