@echo off
setlocal

REM =====================================================
REM QRCERTS - DEPLOY A PRODUCCION
REM =====================================================
REM Estructura local (monorepo unificado):
REM   Backend  : C:\DEV\DN\QRCerts\QRCerts.Api
REM   Frontend : C:\DEV\DN\QRCerts\qrcerts-frontend
REM   Docker   : C:\DEV\DN\QRCerts (este folder)
REM
REM Servidor produccion:
REM   Host : root@149.50.148.247
REM   SSH  : puerto 5022
REM   Path : /root/qrcerts
REM   URL  : https://certificadosqr.store
REM =====================================================

set HOST=root@149.50.148.247
set SSH_PORT=5022
set REMOTE_PATH=/root/qrcerts
set FRONTEND_LOCAL=C:\DEV\DN\QRCerts\qrcerts-frontend
set BACKEND_LOCAL=C:\DEV\DN\QRCerts

echo.
echo [1/5] Build frontend (npm run build)...
pushd "%FRONTEND_LOCAL%"
call npm run build
if errorlevel 1 (popd & goto :err)
popd

echo.
echo [2/5] Subir frontend (dist) al servidor...
pushd "%FRONTEND_LOCAL%"
tar -cf - dist | ssh -p %SSH_PORT% %HOST% "cd %REMOTE_PATH%/src/qrcerts-frontend && rm -rf dist && tar xf -"
if errorlevel 1 (popd & goto :err)
popd

echo.
echo [3a/5] dotnet publish del backend a publish/ ...
pushd "%BACKEND_LOCAL%\QRCerts.Api"
dotnet publish -c Release -o "%BACKEND_LOCAL%\publish"
if errorlevel 1 (popd & goto :err)
popd

echo.
echo [3b/5] Subir publish/ al servidor (preserva appsettings.json de prod)...
pushd "%BACKEND_LOCAL%"
tar --exclude="publish/appsettings.json" -cf - publish | ssh -p %SSH_PORT% %HOST% "cd %REMOTE_PATH% && cp publish/appsettings.json /tmp/appsettings.prod.json 2>/dev/null; rm -rf publish && tar xf - && if [ -f /tmp/appsettings.prod.json ]; then cp /tmp/appsettings.prod.json publish/appsettings.json; else echo '!!! WARNING: no habia appsettings.json en server, hay que crearlo manualmente !!!'; fi"
if errorlevel 1 (popd & goto :err)
popd

echo.
echo [4/5] Rebuild Docker en el servidor (solo qrcerts-app, sin tocar SQL)...
ssh -p %SSH_PORT% %HOST% "cd %REMOTE_PATH% && docker compose up -d --build --no-deps qrcerts-app"
if errorlevel 1 goto :err

echo.
echo [5/5] Verificar...
ssh -p %SSH_PORT% %HOST% "docker ps --filter name=qrcerts-app --format 'table {{.Names}}\t{{.Status}}' && docker logs qrcerts-app --tail 10"
if errorlevel 1 goto :err

echo.
echo === DEPLOY OK ===
echo URL: https://certificadosqr.store
pause
exit /b 0

:err
echo.
echo !!! DEPLOY FAILED !!!
pause
exit /b 1
