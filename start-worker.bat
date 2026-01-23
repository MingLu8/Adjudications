@echo off
SETLOCAL EnableDelayedExpansion

:: Define the path and the directory
SET APP_DIR=D:\_dev\adjudications\src\AdjudicationWorker\bin\Debug\net10.0
SET APP_NAME=AdjudicationWorker.exe

:: Define the port range
SET START_PORT=5050
SET END_PORT=5060

echo Launching AdjudicationWorker instances...

FOR /L %%P IN (%START_PORT%, 1, %END_PORT%) DO (
    echo Starting Instance on Port: %%P
    
    :: /D sets the start-in directory
    :: We call cmd /c to set the variable and then run the exe
    start "AdjudicationWorker: %%P" /D "%APP_DIR%" cmd /c "SET ASPNETCORE_URLS=http://localhost:%%P&& %APP_NAME%"
)

echo.
echo Launching complete.
pause