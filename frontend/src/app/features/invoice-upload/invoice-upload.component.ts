import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvoiceService } from '../../core/services/invoice.service';

@Component({
  selector: 'app-invoice-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './invoice-upload.component.html',
  styleUrls: ['./invoice-upload.component.css']
})
export class InvoiceUploadComponent {
  @Input() currentLang: 'ES' | 'EN' = 'ES';
  selectedFile: File | null = null;
  uploadStatus: string = '';
  isError: boolean = false;

  translations: Record<'ES' | 'EN', any> = {
    ES: {
      title: 'Ingesta de Facturas',
      desc: 'Sube la factura (PDF o Imagen) para que el Agente la procese y evalúe las reglas.',
      processBtn: 'Procesar Factura',
      processing: 'Subiendo factura para análisis...',
      selectFirst: 'Selecciona un archivo primero.',
      apiError: 'Error de conexión con el Agente API.',
      success: 'Éxito'
    },
    EN: {
      title: 'Invoice Ingestion',
      desc: 'Upload the invoice (PDF or Image) for the Agent to process and evaluate the rules.',
      processBtn: 'Process Invoice',
      processing: 'Uploading invoice for analysis...',
      selectFirst: 'Select a file first.',
      apiError: 'API Agent connection error.',
      success: 'Success'
    }
  };

  constructor(private invoiceService: InvoiceService) {}

  get t() { return this.translations[this.currentLang]; }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onUpload(): void {
    if (!this.selectedFile) {
      this.isError = true;
      this.uploadStatus = this.t.selectFirst;
      return;
    }

    this.isError = false;
    this.uploadStatus = this.t.processing;
    this.invoiceService.uploadInvoice(this.selectedFile).subscribe({
      next: (response) => {
        this.uploadStatus = `${this.t.success}: ${response.message}. ID: ${response.id}`;
        this.selectedFile = null;
      },
      error: (err) => {
        this.isError = true;
        this.uploadStatus = this.t.apiError;
        console.error('Upload Error:', err);
      }
    });
  }
}
