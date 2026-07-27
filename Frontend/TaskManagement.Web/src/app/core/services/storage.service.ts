import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  setItem<T>(key: string, value: T): void {
    try {
      const serializedValue = JSON.stringify(value);
      localStorage.setItem(key, serializedValue);
    } catch (error) {
      console.error(
        `Local storage verisi kaydedilemedi: ${key}`,
        error
      );
    }
  }

  getItem<T>(key: string): T | null {
    try {
      const storedValue = localStorage.getItem(key);

      if (storedValue === null) {
        return null;
      }

      return JSON.parse(storedValue) as T;
    } catch (error) {
      console.error(
        `Local storage verisi okunamadı: ${key}`,
        error
      );

      return null;
    }
  }

  removeItem(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }

  hasItem(key: string): boolean {
    return localStorage.getItem(key) !== null;
  }
}