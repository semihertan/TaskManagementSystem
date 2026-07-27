import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ErrorHandlingService } from './error-handling.service';

describe('ErrorHandlingService', () => {
  let service: ErrorHandlingService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ErrorHandlingService]
    });
    service = TestBed.inject(ErrorHandlingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('backend validation mesajını döndürmeli', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: {
          Email: ['Geçerli bir e-posta adresi giriniz.']
        }
      }
    });

    expect(service.getErrorMessage(error)).toBe(
      'Geçerli bir e-posta adresi giriniz.'
    );
  });
});
