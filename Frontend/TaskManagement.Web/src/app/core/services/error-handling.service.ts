import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlingService {

  constructor() { }

  handleError(error: any): string {
    if (error && error.error && error.error.message) {
      return error.error.message;
    }
    if (error && error.message) {
      return error.message;
    }
    return 'An unexpected error occurred. Please try again.';
  }
}
