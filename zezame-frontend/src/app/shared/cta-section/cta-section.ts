import { Component } from '@angular/core';
import { LucideAngularModule, ArrowRight, Sparkles } from 'lucide-angular';

@Component({
  selector: 'app-cta-section',
  imports: [LucideAngularModule],
  templateUrl: './cta-section.html',
  styleUrl: './cta-section.scss',
})
export class CtaSection {
  ArrowRight = ArrowRight;
  Sparkles = Sparkles;
}
