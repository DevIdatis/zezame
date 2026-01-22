import { Component } from '@angular/core';
import {
  LucideAngularModule,
  Timer,
  BarChart3,
  Users,
  Smartphone,
  Shield,
  Palette,
  Bell,
  LucideIconData,
  Zap,
} from 'lucide-angular';

interface Feature {
  icon: LucideIconData;
  title: string;
  description: string;
  color: string;
  bgColor: string;
}

@Component({
  selector: 'app-feature-section',
  imports: [LucideAngularModule],
  templateUrl: './feature-section.html',
  styleUrl: './feature-section.scss',
})
export class FeatureSection {
  Zap = Zap;
  features: Feature[] = [
    {
      icon: Timer,
      title: 'Tracking inteligente',
      description: 'Registra tus horas automáticamente con un solo clic. ¡Así de fácil!',
      color: 'text-primary',
      bgColor: 'bg-primary/10',
    },
    {
      icon: BarChart3,
      title: 'Reportes visuales',
      description: 'Gráficos bonitos y fáciles de entender para ver tu progreso.',
      color: 'text-secondary',
      bgColor: 'bg-secondary/10',
    },
    {
      icon: Users,
      title: 'Equipos felices',
      description: 'Gestiona todo tu equipo desde un solo lugar. Colabora sin esfuerzo.',
      color: 'text-purple',
      bgColor: 'bg-purple/10',
    },
    {
      icon: Smartphone,
      title: 'En cualquier lugar',
      description: 'App móvil para que nunca pierdas el control de tu tiempo.',
      color: 'text-cyan',
      bgColor: 'bg-cyan/10',
    },
    {
      icon: Zap,
      title: 'Super rápido',
      description: 'Interfaz velocísima que no te hace perder ni un segundo.',
      color: 'text-yellow',
      bgColor: 'bg-yellow/10',
    },
    {
      icon: Shield,
      title: '100% seguro',
      description: 'Tus datos protegidos con encriptación de nivel empresarial.',
      color: 'text-lime',
      bgColor: 'bg-lime/10',
    },
    {
      icon: Palette,
      title: 'Personalizable',
      description: 'Adapta Zezame a tu estilo con temas y colores únicos.',
      color: 'text-coral',
      bgColor: 'bg-coral/10',
    },
    {
      icon: Bell,
      title: 'Notificaciones',
      description: 'Recordatorios amigables para que nunca olvides fichar.',
      color: 'text-purple',
      bgColor: 'bg-purple/10',
    },
  ];
}
