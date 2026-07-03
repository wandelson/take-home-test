import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { LoggerService } from './app/core/logging/logger.service';

const bootstrapLogger = new LoggerService();

bootstrapApplication(AppComponent, appConfig).catch((err: unknown) => {
  bootstrapLogger.error('Application bootstrap failed', 'Bootstrap', err);
});
