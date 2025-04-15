import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { WebsocketModule } from './core/services/websocket.module';

export const appConfig: ApplicationConfig = {
  providers: [importProvidersFrom(WebsocketModule),
              provideZoneChangeDetection({ eventCoalescing: true }),
              provideRouter(routes),
              provideClientHydration(withEventReplay())]
};
