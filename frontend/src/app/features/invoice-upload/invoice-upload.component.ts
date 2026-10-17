import { Component } from '@angular/core';
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
  selectedFile: File | null = null;
  uploadStatus: string = '';

  constructor(private invoiceService: InvoiceService) {}

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onUpload(): void {
    if (!this.selectedFile) {
      this.uploadStatus = 'Selecciona un archivo primero.';
      return;
    }

    this.uploadStatus = 'Subiendo factura para análisis...';
    this.invoiceService.uploadInvoice(this.selectedFile).subscribe({
      next: (response) => {
        this.uploadStatus = `Éxito: ${response.message}. ID de seguimiento: ${response.id}`;
        this.selectedFile = null;
      },
      error: (err) => {
        this.uploadStatus = 'Error de conexión con el Agente API.';
        console.error('Upload Error:', err);
      }
    });
  }
}