import { inject } from '@angular/core';
import { HttpHandlerFn, HttpRequest, HttpEvent } from '@angular/common/http';
import { Observable, catchError, throwError, finalize } from 'rxjs';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LoadingService } from '../services/loading.service';

function generateCorrelationId(): string {
  // simples UUID-like (não criptográfico)
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

export function globalHttpInterceptor(req: HttpRequest<any>, next: HttpHandlerFn): Observable<HttpEvent<any>> {
  const snack = inject(MatSnackBar);
  const loading = inject(LoadingService);

  loading.start();

  const correlationId = generateCorrelationId();
  const authReq = req.clone({
    setHeaders: {
      'X-Correlation-Id': correlationId
    }
  });

  return next(authReq).pipe(
    catchError(err => {
      let mensagem = 'Erro inesperado';
      if (err.status === 0) mensagem = 'Falha de rede ou servidor indisponível';
      else if (err.status >= 500) mensagem = 'Erro interno do servidor';
      else if (err.status === 400) {
        if (err.error) {
          if (typeof err.error === 'string') mensagem = err.error;
          else if (err.error.message) mensagem = err.error.message;
          else if (err.error.errors) mensagem = Object.values(err.error.errors).join('\n');
          else mensagem = 'Requisição inválida';
        } else mensagem = 'Requisição inválida';
      } else if (err.status === 404) mensagem = 'Recurso não encontrado';
      snack.open(mensagem, 'Fechar', { duration: 4000 });
      return throwError(() => err);
    }),
    finalize(() => loading.stop())
  );
}
