@echo off

set countfiles=1000

:loop

set /a countfiles -= 1
echo %countfiles% 
curl -k https://localhost:5001

if %countfiles% GTR 0 goto loop

