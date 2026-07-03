import { LogLevel } from '../app/core/logging/log-level';

export interface Environment {
  production: boolean;
  apiUrl: string;
  logging: {
    level: LogLevel;
    enableHttpLogging: boolean;
  };
}
