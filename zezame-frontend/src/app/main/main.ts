import { Component } from '@angular/core';
import { Navbar } from '../shared/navbar/navbar';
import { HeroSection } from '../shared/hero-section/hero-section';
import { FeatureSection } from '../shared/feature-section/feature-section';
import { Footer } from '../shared/footer/footer';

@Component({
  selector: 'app-main',
  imports: [Navbar, HeroSection, FeatureSection, Footer],
  templateUrl: './main.html',
  styleUrl: './main.scss',
})
export class Main {}
