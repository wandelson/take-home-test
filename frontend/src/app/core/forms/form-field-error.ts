import { AbstractControl } from '@angular/forms';

export function fieldError(
  control: AbstractControl | null | undefined,
  messages: Record<string, string>
): string | null {
  if (!control?.invalid || !(control.touched || control.dirty)) {
    return null;
  }

  for (const [key, message] of Object.entries(messages)) {
    if (control.hasError(key)) {
      return message;
    }
  }

  return null;
}
