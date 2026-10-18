import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RulesConfigComponent } from './features/rules-config/rules-config.component';
import { InvoiceUploadComponent } from './features/invoice-upload/invoice-upload.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RulesConfigComponent, InvoiceUploadComponent, DashboardComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  currentLang: 'ES' | 'EN' = 'ES';
  isDaniOpen = false;
  daniMessages: { role: string, content: string }[] = [];

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

  ngOnInit(): void {
    this.daniMessages.push({ 
      role: 'dani', 
      content: this.translations[this.currentLang].daniGreeting || '¡Miau! Soy IA Dani. ¿En qué te puedo ayudar hoy?' 
    });
  }

  toggleLanguage() {
    this.currentLang = this.currentLang === 'ES' ? 'EN' : 'ES';
  }

  toggleDani() {
    this.isDaniOpen = !this.isDaniOpen;
  }

  sendQuickAction(text: string): void {
    this.processDaniText(text);
  }

  sendDaniMessage(inputElement: HTMLInputElement): void {
    const text = inputElement.value.trim();
    if (!text) return;
    inputElement.value = '';
    this.processDaniText(text);
  }

  private processDaniText(text: string): void {
    this.daniMessages.push({ role: 'user', content: text });

    setTimeout(() => {
      let response = 'Miau. Mi directiva es auditar facturas. Sube un documento para que el motor C# lo evalúe.';
      const query = text.toLowerCase();
      
      if (query.includes('rojo') || query.includes('ilegal')) {
        response = 'Si el sistema marca rojo, el proveedor no figura en tu VendorRepository o superaste el límite presupuestario.';
      } else if (query.includes('amarillo')) {
        response = 'El estado amarillo exige escalamiento. Ocurre si el monto excede el umbral o la IA no logró extraer el texto con precisión.';
      } else if (query.includes('ayuda') || query.includes('funciona')) {
        response = 'Sube un PDF. El sistema extraerá los datos, validará las reglas de presupuesto/proveedor y asignará un semáforo de aprobación.';
      }
      
      this.daniMessages.push({ role: 'dani', content: response });
    }, 600);
  }
}