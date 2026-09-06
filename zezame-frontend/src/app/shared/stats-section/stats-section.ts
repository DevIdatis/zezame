import { Component } from '@angular/core';
import { LucideAngularModule, TrendingUp } from 'lucide-angular';

interface Stat {
  value: string;
  label: string;
  emoji: string;
}

@Component({
  selector: 'app-stats-section',
  imports: [LucideAngularModule],
  templateUrl: './stats-section.html',
  styleUrl: './stats-section.scss',
})
export class StatsSection {
  TrendingUp = TrendingUp;

  stats: Stat[] = [
    { value: '10K+', label: 'Equipos activos', emoji: '🎯' },
    { value: '2M+', label: 'Horas registradas', emoji: '⏰' },
    { value: '99.9%', label: 'Uptime', emoji: '🚀' },
    { value: '4.9/5', label: 'Valoración', emoji: '⭐' },
  ];
}
