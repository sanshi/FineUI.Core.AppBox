@echo off
setlocal
node "%~dp0tools\generate-designers.mjs" %*
exit /b %errorlevel%
