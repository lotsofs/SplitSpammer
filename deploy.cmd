@echo off
rem Builds the component and copies the DLL into LiveSplit's Components folder, which LiveSplit loads it from.
rem Usage: deploy [Debug|Release] [-r] [-p]   (Debug if left out)
rem   -r  restart LiveSplit: build, kill it if it runs (FORCE: unsaved changes in it are lost), copy, start it again.
rem   -p  preview: build and open the settings panel in a window of its own, without LiveSplit and without deploying.
rem Without -r, deploy refuses to run while LiveSplit is running, because it locks the DLL.
rem LiveSplit's folder is the LiveSplitDir environment variable, or ..\..\Programs\Streaming\LiveSplit from the repo, as in the csproj.
rem The build reads LiveSplitDir from the environment too, so it references the LiveSplit the DLL is copied into.
setlocal
set "CONFIG=Debug"
set "RESTART="
set "PREVIEW="
:args
if "%~1"=="" goto argsdone
if /I "%~1"=="Debug" (set "CONFIG=Debug") else if /I "%~1"=="Release" (set "CONFIG=Release") else if /I "%~1"=="-r" (set "RESTART=1") else if /I "%~1"=="-p" (set "PREVIEW=1") else (
	echo Unknown argument %~1
	echo Usage: deploy [Debug^|Release] [-r] [-p]
	exit /b 1
)
shift
goto args
:argsdone
if not defined LiveSplitDir set "LiveSplitDir=%~dp0..\..\Programs\Streaming\LiveSplit"
set "COMPONENTS=%LiveSplitDir%\Components"

rem Preview alone: no LiveSplit involved, so nothing to stop or copy
if defined PREVIEW if not defined RESTART goto preview

rem findstr by full path: Git's tools can shadow Windows' find.
set "RUNNING="
tasklist /FI "IMAGENAME eq LiveSplit.exe" /NH | "%SystemRoot%\System32\findstr.exe" /I /C:"LiveSplit.exe" >nul
if not errorlevel 1 set "RUNNING=1"
if defined RUNNING if not defined RESTART (
	echo LiveSplit is running. Close it first: it locks the DLL. Or use deploy -r.
	exit /b 1
)
if not exist "%COMPONENTS%\" (
	echo LiveSplit's Components folder isn't at %COMPONENTS%
	exit /b 1
)

rem The project, not the solution: the tests in Tests\ aren't needed to deploy. Built before LiveSplit is stopped, so a failed build leaves it alone.
dotnet build "%~dp0LiveSplit.SplitSpammer.csproj" -c %CONFIG%
if errorlevel 1 exit /b 1

if defined RUNNING (
	taskkill /F /IM LiveSplit.exe >nul
	call :waitgone
	if defined RUNNING (
		echo LiveSplit did not exit.
		exit /b 1
	)
)

copy /Y "%~dp0bin\%CONFIG%\LiveSplit.SplitSpammer.dll" "%COMPONENTS%\" >nul
if errorlevel 1 exit /b 1
echo Deployed bin\%CONFIG%\LiveSplit.SplitSpammer.dll to %COMPONENTS%

if defined RESTART (
	start "" /D "%LiveSplitDir%" "%LiveSplitDir%\LiveSplit.exe"
	echo Started LiveSplit
)
if defined PREVIEW goto preview
exit /b 0

:preview
dotnet build "%~dp0Preview\SplitSpammer.Preview.csproj" -c %CONFIG%
if errorlevel 1 exit /b 1
"%~dp0Preview\bin\%CONFIG%\net481\SplitSpammer.Preview.exe"
exit /b 0

rem Waits up to about 10 seconds for LiveSplit to be gone; clears RUNNING when it is
:waitgone
set /a TRIES=0
:waitloop
tasklist /FI "IMAGENAME eq LiveSplit.exe" /NH | "%SystemRoot%\System32\findstr.exe" /I /C:"LiveSplit.exe" >nul
if errorlevel 1 (
	set "RUNNING="
	exit /b 0
)
set /a TRIES+=1
if %TRIES% geq 20 exit /b 1
"%SystemRoot%\System32\ping.exe" -n 1 -w 500 127.0.0.1 >nul
goto waitloop
