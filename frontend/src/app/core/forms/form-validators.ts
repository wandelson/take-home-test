import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Treats null, empty, and whitespace-only values as required. */
export const requiredNotWhitespace: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const value = control.value;
  if (value == null) {
    return { required: true };
  }

  if (typeof value === 'string' && value.trim().length === 0) {
    return { required: true };
  }

  if (typeof value === 'number' && Number.isNaN(value)) {
    return { required: true };
  }

  return null;
};
