import { Pipe, PipeTransform } from '@angular/core';
import { ValidationErrors } from '@angular/forms';

@Pipe({
  name: 'formError',
  standalone: true
})
export class FormErrorPipe implements PipeTransform {

  private readonly errorMessages: Record<string, string | ((errorValue: any) => string)> = {
    required: 'Este campo é obrigatório.',
    min: (err) => `O valor mínimo permitido é ${err.min}.`,
    max: (err) => `O valor máximo permitido é ${err.max}.`,
    minlength: (err) => `Mínimo de ${err.requiredLength} caracteres.`,
    maxlength: (err) => `Máximo de ${err.requiredLength} caracteres.`,
    email: 'Formato de e-mail inválido.',
    pattern: 'Formato inválido.'
  };

  transform(errors: ValidationErrors | null | undefined): string {
    if (!errors) {
      return '';
    }

    const firstKey = Object.keys(errors)[0];
    const messageOrFn = this.errorMessages[firstKey];

    if (!messageOrFn) {
      return 'Campo inválido.';
    }

    return typeof messageOrFn === 'function' ? messageOrFn(errors[firstKey]) : messageOrFn;
  }
}
