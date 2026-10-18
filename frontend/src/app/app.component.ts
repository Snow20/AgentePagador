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

  translations: any = {
    'ES': {
      title: 'Agente Pagador Blinka',
      subtitle: 'Automatización de Facturas con IA',
      daniGreeting: '¡Hola! Soy IA Dani, tu asistente virtual. ¿En qué te puedo ayudar hoy?'
    },
    'EN': {
      title: 'Blinka Paying Agent',
      subtitle: 'AI Invoice Automation',
      daniGreeting: 'Hello! I am AI Dani, your virtual assistant. How can I help you today?'
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