import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent, NoopAnimationsModule],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    fixture.detectChanges();
  });

  it('should render sign-in form', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[data-testid="login-username"]')).toBeTruthy();
    expect(element.querySelector('[data-testid="login-password"]')).toBeTruthy();
    expect(element.querySelector('[data-testid="login-submit"]')).toBeTruthy();
  });
});
