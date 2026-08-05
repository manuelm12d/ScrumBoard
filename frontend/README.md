# ScrumBoard Frontend

SPA Angular 17 + PrimeNG (layout tipo Sakai) del monorepo ScrumBoard.

## Desarrollo

```powershell
npm install
npm start
# http://localhost:4200  → API en http://localhost:5080 (ver environments y proxy.conf.json)
```

## Pruebas

```powershell
npm test
```

## Build de producción

```powershell
npm run build -- --configuration=production
```

La imagen Docker (`Dockerfile`) construye con Node 20 y sirve el resultado con nginx
(`try_files` para el routing del SPA). El servicio `web` de Compose publica el puerto `WEB_PORT`
(por defecto 8081).
