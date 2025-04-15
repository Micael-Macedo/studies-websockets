import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { WebSocketComponent } from './app/components/web-socket/web-socket.component';

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
