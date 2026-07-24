import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly storageKey = 'theme';
  private readonly document = inject(DOCUMENT);

  readonly currentTheme = signal<ThemeMode>(this.getInitialTheme());

  constructor() {
    this.applyTheme(this.currentTheme());
  }

  toggleTheme(): void {
    const nextTheme: ThemeMode =
      this.currentTheme() === 'light'
        ? 'dark'
        : 'light';

    this.setTheme(nextTheme);
  }

  setTheme(theme: ThemeMode): void {
    this.currentTheme.set(theme);
    this.saveTheme(theme);
    this.applyTheme(theme);
  }

  private applyTheme(theme: ThemeMode): void {
    const root = this.document.documentElement;
    root.setAttribute('data-theme', theme);
    root.style.colorScheme = theme;
    root.setAttribute('data-theme-ready', 'true');

    if (this.document.body) {
      this.document.body.classList.remove('light-theme', 'dark-theme');
      this.document.body.classList.add(`${theme}-theme`);
      this.document.body.setAttribute('data-theme', theme);
    }
  }

  private getInitialTheme(): ThemeMode {
    const savedTheme = this.readSavedTheme();

    if (savedTheme === 'light' || savedTheme === 'dark') {
      return savedTheme;
    }

    const prefersDark = this.document.defaultView?.matchMedia?.(
      '(prefers-color-scheme: dark)'
    ).matches;

    return prefersDark ? 'dark' : 'light';
  }

  private readSavedTheme(): ThemeMode | null {
    try {
      const savedTheme = this.document.defaultView?.localStorage.getItem(
        this.storageKey
      );

      return savedTheme === 'light' || savedTheme === 'dark'
        ? savedTheme
        : null;
    } catch {
      return null;
    }
  }

  private saveTheme(theme: ThemeMode): void {
    try {
      this.document.defaultView?.localStorage.setItem(this.storageKey, theme);
    } catch {
      // Storage can be unavailable in privacy-restricted browser contexts.
    }
  }
}
