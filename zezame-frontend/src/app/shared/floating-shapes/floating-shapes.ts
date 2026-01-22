import { Component } from '@angular/core';
import { LucideAngularModule, Clock, Zap, Star, Heart, Sparkles } from 'lucide-angular';

@Component({
  selector: 'app-floating-shapes',
  imports: [LucideAngularModule],
  templateUrl: './floating-shapes.html',
  styleUrl: './floating-shapes.scss',
})
export class FloatingShapes {
  Clock = Clock;
  Zap = Zap;
  Star = Star;
  Heart = Heart;
  Sparkles = Sparkles;
}
