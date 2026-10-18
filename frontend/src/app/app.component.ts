import { Component } from '@angular/core';
import { RulesConfigComponent } from './features/rules-config/rules-config.component';
import { InvoiceUploadComponent } from './features/invoice-upload/invoice-upload.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RulesConfigComponent, InvoiceUploadComponent, DashboardComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  currentLang: 'ES' | 'EN' = 'ES';
  isDaniOpen = false;

  translations: Record<'ES' | 'EN', any> = {
    'ES': {
      title: 'Agente Pagador',
      subtitle: 'Automatización de Facturas con IA',
      daniGreeting: '¡Hola! Soy IA Dani, tu asistente virtual. ¿En qué te puedo ayudar hoy?',
      daniPlaceholder: 'Escribe tu mensaje...',
      daniSend: 'Enviar',
      footer: '© Copyright hecho por Fenrirsoft 2026. Todos los derechos reservados.'
    },
    'EN': {
      title: 'Paying Agent',
      subtitle: 'AI Invoice Automation',
      daniGreeting: 'Hello! I am AI Dani, your virtual assistant. How can I help you today?',
      daniPlaceholder: 'Type your message...',
      daniSend: 'Send',
      footer: '© Copyright made by Fenrirsoft 2026. All rights reserved.'
    }
  };

  toggleLanguage() {
    this.currentLang = this.currentLang === 'ES' ? 'EN' : 'ES';
  }

  toggleDani() {
    this.isDaniOpen = !this.isDaniOpen;
  }
}