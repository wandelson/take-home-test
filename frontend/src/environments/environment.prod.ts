import { Environment } from './environment.model';
import { LogLevel } from '../app/core/logging/log-level';

export const environment: Environment = {
  production: true,
  apiUrl: '/api',
  logging: {
    level: LogLevel.Warn,
    enableHttpLogging: false,
  },
};
