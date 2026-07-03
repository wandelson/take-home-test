import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { LogLevel } from './log-level';

@Injectable({ providedIn: 'root' })
export class LoggerService {
  private readonly minLevel = environment.logging.level;

  debug(message: string, context?: string, data?: unknown): void {
    this.write(LogLevel.Debug, 'DBG', message, context, data);
  }

  info(message: string, context?: string, data?: unknown): void {
    this.write(LogLevel.Info, 'INF', message, context, data);
  }

  warn(message: string, context?: string, data?: unknown): void {
    this.write(LogLevel.Warn, 'WRN', message, context, data);
  }

  error(message: string, context?: string, data?: unknown): void {
    this.write(LogLevel.Error, 'ERR', message, context, data);
  }

  private write(
    level: LogLevel,
    label: string,
    message: string,
    context?: string,
    data?: unknown
  ): void {
    if (level < this.minLevel) {
      return;
    }

    const timestamp = new Date().toISOString().slice(11, 23);
    const prefix = context ? `[${context}] ` : '';
    const line = `[${timestamp} ${label}] ${prefix}${message}`;

    switch (level) {
      case LogLevel.Debug:
        if (data !== undefined) {
          console.debug(line, data);
        } else {
          console.debug(line);
        }
        break;
      case LogLevel.Info:
        if (data !== undefined) {
          console.info(line, data);
        } else {
          console.info(line);
        }
        break;
      case LogLevel.Warn:
        if (data !== undefined) {
          console.warn(line, data);
        } else {
          console.warn(line);
        }
        break;
      case LogLevel.Error:
        if (data !== undefined) {
          console.error(line, data);
        } else {
          console.error(line);
        }
        break;
    }
  }
}
