import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingService {

  getErrorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) {
      return error instanceof Error
        ? error.message
        : 'Beklenmeyen bir hata oluştu.';
    }

    if (error.status === 0) {
      return 'Sunucuya bağlanılamadı. Backend uygulamasının çalıştığını kontrol edin.';
    }

    if (typeof error.error === 'object' && error.error !== null) {
      const backendMessage =
        error.error.message ??
        error.error.Message;

      if (
        typeof backendMessage === 'string' &&
        backendMessage.trim()
      ) {
        return backendMessage;
      }
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    switch (error.status) {
      case 400:
        return 'Gönderilen bilgiler geçersiz.';

      case 401:
        return 'Email veya şifre hatalı.';

      case 403:
        return 'Bu işlem için yetkiniz bulunmuyor.';

      case 404:
        return 'İstenen kaynak bulunamadı.';

      case 409:
        return 'Bu kayıt zaten mevcut.';

      case 500:
        return 'Sunucuda beklenmeyen bir hata oluştu.';

      default:
        return `Bir hata oluştu. Hata kodu: ${error.status}`;
    }
  }
}