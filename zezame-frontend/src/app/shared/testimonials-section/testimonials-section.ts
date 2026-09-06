import { Component } from '@angular/core';
import { LucideAngularModule, Star, Quote } from 'lucide-angular';

interface Testimonial {
  name: string;
  role: string;
  avatar: string;
  content: string;
  rating: number;
  color: string;
}

@Component({
  selector: 'app-testimonials-section',
  imports: [LucideAngularModule],
  templateUrl: './testimonials-section.html',
  styleUrl: './testimonials-section.scss',
})
export class TestimonialsSection {
  Star = Star;
  Quote = Quote;

  testimonials: Testimonial[] = [
    {
      name: 'María García',
      role: 'CEO, TechStartup',
      avatar: 'MG',
      content:
        'Zezame transformó cómo gestionamos el tiempo en nuestro equipo. ¡Es súper divertido de usar!',
      rating: 5,
      color: 'bg-coral',
    },
    {
      name: 'Carlos Ruiz',
      role: 'Director de RRHH, FinCorp',
      avatar: 'CR',
      content:
        'Los reportes son increíbles. Por fin entiendo dónde va el tiempo de mi equipo. 100% recomendado.',
      rating: 5,
      color: 'bg-secondary',
    },
    {
      name: 'Ana Martínez',
      role: 'Freelancer',
      avatar: 'AM',
      content:
        'Como freelancer, necesitaba algo simple pero potente. Zezame es exactamente eso. ¡Lo amo! 💜',
      rating: 5,
      color: 'bg-purple',
    },
  ];

  stars(rating: number): number[] {
    return Array.from({ length: rating });
  }
}
