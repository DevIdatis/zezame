import { Component } from '@angular/core';
import { FloatingShapes } from '../floating-shapes/floating-shapes';
import { LucideAngularModule, Play, ArrowRight, CircleCheck } from 'lucide-angular';

@Component({
  selector: 'app-hero-section',
  imports: [FloatingShapes, LucideAngularModule],
  templateUrl: './hero-section.html',
  styleUrl: './hero-section.scss',
})
export class HeroSection {
  Play = Play;
  ArrowRight = ArrowRight;
  CircleCheck = CircleCheck;
  benefits: string[] = ['Sin tarjeta de crédito', 'Configura en 2 minutos', 'Soporte 24/7'];

  days: string[] = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'];

  activityHeights: number[] = [65, 80, 45, 90, 75, 30, 20];
}
