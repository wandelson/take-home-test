import { Environment } from './environment.model';
import { LogLevel } from '../app/core/logging/log-level';

export const environment: Environment = {
  production: false,
  apiUrl: '/api',
  logging: {
    level: LogLevel.Debug,
    enableHttpLogging: true,
  },
};
