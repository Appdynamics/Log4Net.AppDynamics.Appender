echo Test script to send 1000000 requests
@echo off

set countfiles=1000000

:loop

set /a countfiles -= 1
echo %countfiles% 
curl -k https://localhost:5001
PING localhost -n 2 >NUL
if %countfiles% GTR 0 goto loop
