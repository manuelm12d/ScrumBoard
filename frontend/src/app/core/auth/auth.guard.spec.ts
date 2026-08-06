import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', [], {
      isAuthenticated: jasmine.createSpy('isAuthenticated').and.returnValue(false) as unknown as AuthService['isAuthenticated']
    });

    // Replace computed-style getter with a plain spy function via Object.defineProperty
    Object.defineProperty(auth, 'isAuthenticated', {
      value: jasmine.createSpy('isAuthenticated').and.returnValue(false)
    });

    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: 'login', children: [] }, { path: 'projects', children: [] }]),
        { provide: AuthService, useValue: auth }
      ]
    });

    router = TestBed.inject(Router);
  });

  it('blocks navigation when there is no session', () => {
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result.toString()).toContain('login');
  });

  it('allows navigation when authenticated', () => {
    (auth.isAuthenticated as jasmine.Spy).and.returnValue(true);
    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    expect(result).toBeTrue();
  });
});
