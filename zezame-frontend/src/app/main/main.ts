import { Component } from '@angular/core';
import { Navbar } from '../shared/navbar/navbar';
import { HeroSection } from '../shared/hero-section/hero-section';
import { FeatureSection } from '../shared/feature-section/feature-section';
import { StatsSection } from '../shared/stats-section/stats-section';
import { TestimonialsSection } from '../shared/testimonials-section/testimonials-section';
import { CtaSection } from '../shared/cta-section/cta-section';
import { Footer } from '../shared/footer/footer';

@Component({
  selector: 'app-main',
  imports: [
    Navbar,
    HeroSection,
    FeatureSection,
    StatsSection,
    TestimonialsSection,
    CtaSection,
    Footer,
  ],
  templateUrl: './main.html',
  styleUrl: './main.scss',
})
export class Main {}
