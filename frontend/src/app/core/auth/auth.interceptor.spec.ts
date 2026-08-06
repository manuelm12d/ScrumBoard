import { HttpRequest, HttpResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  it('attaches the Bearer token to outgoing requests', (done) => {
    const auth = {
      getToken: () => 'test-token',
      logout: jasmine.createSpy('logout')
    };

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }]
    });

    const request = new HttpRequest('GET', '/api/projects');

    TestBed.runInInjectionContext(() => {
      authInterceptor(request, (req) => {
        expect(req.headers.get('Authorization')).toBe('Bearer test-token');
        return of(new HttpResponse({ status: 200 }));
      }).subscribe({
        next: () => done(),
        error: done.fail
      });
    });
  });
});
