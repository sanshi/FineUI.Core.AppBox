@echo off
setlocal
chcp 65001 >nul
if "%~1"=="" (
    node "%~dp0generate-theme.mjs" --watch
) else (
    node "%~dp0generate-theme.mjs" %*
)
set "result=%errorlevel%"
if not "%result%"=="0" (
    echo Theme generation failed. Review the message above, then press any key to close.
    pause >nul
)
exit /b %result%
