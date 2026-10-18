import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-rules-config',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rules-config.component.html',
  styleUrls: ['./rules-config.component.css']
})
export class RulesConfigComponent {
  @Input() currentLang: 'ES' | 'EN' = 'ES';
}
