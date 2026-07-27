import { DOCUMENT } from '@angular/common';
import { TestBed } from '@angular/core/testing';

import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.removeItem('theme');
    document.documentElement.removeAttribute('data-theme');
    document.body.classList.remove('light-theme', 'dark-theme');
    document.body.removeAttribute('data-theme');

    TestBed.configureTestingModule({});
  });

  it('kayıtlı tercih yoksa açık temayı varsayılan yapmalı', () => {
    const service = TestBed.inject(ThemeService);
    const documentRef = TestBed.inject(DOCUMENT);

    expect(service.currentTheme()).toBe('light');
    expect(documentRef.documentElement.getAttribute('data-theme')).toBe('light');
    expect(documentRef.body.classList.contains('light-theme')).toBe(true);
  });

  it('temayı anında değiştirip localStorage içinde korumalı', () => {
    const service = TestBed.inject(ThemeService);
    const documentRef = TestBed.inject(DOCUMENT);

    service.toggleTheme();

    expect(service.currentTheme()).toBe('dark');
    expect(documentRef.documentElement.getAttribute('data-theme')).toBe('dark');
    expect(documentRef.body.classList.contains('dark-theme')).toBe(true);
    expect(localStorage.getItem('theme')).toBe('dark');
  });
});
