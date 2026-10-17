import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-rules-config',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './rules-config.component.html',
  styleUrls: ['./rules-config.component.css']
})
export class RulesConfigComponent implements OnInit {
  rulesForm!: FormGroup;
  saveStatus: string = '';

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.rulesForm = this.fb.group({
      paymentLimit: ['', [Validators.required, Validators.min(1)]],
      periodLimit: ['', [Validators.required, Validators.min(1)]],
      humanApprovalThreshold: ['', [Validators.required, Validators.min(1)]],
      allowedCurrencies: ['USD, VES, COP, USDT', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.rulesForm.invalid) {
      this.saveStatus = 'Error: Revisa los valores ingresados. No se permiten valores negativos o vacíos.';
      return;
    }

    const rulesData = this.rulesForm.value;
    // Aquí se conectará con el servicio HTTP para enviar a la API
    console.log('Reglas a guardar:', rulesData);
    
    this.saveStatus = 'Reglas guardadas correctamente en la sesión activa.';
  }
}