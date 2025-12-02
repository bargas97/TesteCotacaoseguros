import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private activeRequests = 0;
  private readonly subject = new BehaviorSubject<boolean>(false);
  readonly loading$: Observable<boolean> = this.subject.asObservable();

  start() {
    this.activeRequests++;
    if (this.activeRequests === 1) this.subject.next(true);
  }

  stop() {
    if (this.activeRequests > 0) this.activeRequests--;
    if (this.activeRequests === 0) this.subject.next(false);
  }
}
