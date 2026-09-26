@echo off
setlocal
if "%~1"=="" (
    node "%~dp0tools\generate-designers.mjs" --watch
) else (
    node "%~dp0tools\generate-designers.mjs" %*
)
set "result=%errorlevel%"
if not "%result%"=="0" (
    echo The generator failed. Review the message above, then press any key to close.
    pause >nul
)
exit /b %result%
