import { Component, OnInit } from '@angular/core';
import { RulesConfigComponent } from './features/rules-config/rules-config.component';
import { InvoiceUploadComponent } from './features/invoice-upload/invoice-upload.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
// NOTA: Si usas router, debes importar RouterModule en su lugar. 
// Aquí mantenemos tus imports standalone originales.

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RulesConfigComponent, InvoiceUploadComponent, DashboardComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'] // <-- Añadido. Faltaba enlazar la hoja de estilos
})
export class AppComponent implements OnInit {
  isDarkMode = false;
  currentLang = 'ES';
  isDaniOpen = false;

  translations = {
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
  
  ngOnInit() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      this.toggleTheme(true);
    }
  }

  toggleTheme(forceDark?: boolean) {
    this.isDarkMode = forceDark !== undefined ? forceDark : !this.isDarkMode;
    if (this.isDarkMode) {
      document.documentElement.setAttribute('data-theme', 'dark');
    } else {
      document.documentElement.removeAttribute('data-theme');
    }
  }

  toggleLanguage() {
    this.currentLang = this.currentLang === 'ES' ? 'EN' : 'ES';
  }

  toggleDani() {
    this.isDaniOpen = !this.isDaniOpen;
  }
}