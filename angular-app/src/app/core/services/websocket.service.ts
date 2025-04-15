import { Injectable } from '@angular/core';
import { webSocket, WebSocketSubject } from 'rxjs/webSocket';
import { Observable, Subject } from 'rxjs';
import { catchError, retryWhen, delay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class WebSocketService {
  private socket$: WebSocketSubject<any> | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  constructor() {
    this.connect();
  }

  private connect() {
    this.socket$ = webSocket('ws://localhost:5000/ws/');

    this.socket$.pipe(
      catchError((error) => {
        console.error('Erro no WebSocket:', error);
        this.reconnectAttempts++;
        if (this.reconnectAttempts <= this.maxReconnectAttempts) {
          console.log(`Tentando reconectar (${this.reconnectAttempts})...`);
          return new Subject();
        } else {
          console.error('Máximo de tentativas de reconexão atingido.');
          return new Subject();  // Encerra as tentativas
        }
      }),
      retryWhen((errors) => errors.pipe(delay(3000)))  // Tenta reconectar a cada 3 segundos
    ).subscribe();

    this.socket$.next("register:web_client");
  }

  register() {
    if (this.socket$ && !this.socket$.closed) {
      this.socket$.next("register:web_client");
    }
  }

  requestImage() {
    if (this.socket$ && !this.socket$.closed) {
      this.socket$.next("requestImage");
    }
  }

  sendMessage(msg: any) {
    if (this.socket$ && !this.socket$.closed) {
      this.socket$.next(msg);
    }
  }

  getMessages(): Observable<any> {
    return this.socket$!.asObservable();
  }

  close() {
    if (this.socket$) {
      this.socket$.complete();
      this.socket$ = null;
    }
  }
}
