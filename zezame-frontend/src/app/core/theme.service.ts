import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type Theme = 'light' | 'dark' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);
  private readonly key = 'theme';

  theme = signal<Theme>('system');

  init(): void {
    if (!this.isBrowser) return;
    const stored = (localStorage.getItem(this.key) as Theme | null) ?? 'system';
    this.theme.set(stored);
    this.apply(stored);

    // Reacciona a cambios del sistema cuando el tema es 'system'
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
      if (this.theme() === 'system') this.apply('system');
    });
  }

  set(theme: Theme): void {
    this.theme.set(theme);
    if (!this.isBrowser) return;
    try {
      localStorage.setItem(this.key, theme);
    } catch {
      /* ignore */
    }
    this.apply(theme);
  }

  private apply(theme: Theme): void {
    if (!this.isBrowser) return;
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const dark = theme === 'dark' || (theme === 'system' && prefersDark);
    document.documentElement.classList.toggle('dark', dark);
  }
}
