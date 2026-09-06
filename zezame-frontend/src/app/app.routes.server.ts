import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // El dashboard es una SPA autenticada (depende de localStorage/token y SignalR),
  // no tiene sentido prerenderizarlo en el servidor -> render en cliente.
  { path: 'dashboard', renderMode: RenderMode.Client },
  { path: 'dashboard/**', renderMode: RenderMode.Client },

  // Landing y páginas públicas: prerender estático.
  { path: '**', renderMode: RenderMode.Prerender },
];
