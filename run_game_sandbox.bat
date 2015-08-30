
@echo off

if exist "%LOCALAPPDATA%\Colossal Order\" (

    dir "%LOCALAPPDATA%" | find "<SYMLINKD>" | find "Colossal Order"
	if ERRORLEVEL 0 (
	   echo Found existing symlink...
	) else (
		echo Please rename "%LOCALAPPDATA%\Colossal Order\" to "%LOCALAPPDATA%\Colossal Order.real\" to avoid clobbering your save games
		echo Program will now exit
		pause
		exit 1
	)
)

echo Relinking %LOCALAPPDATA%\Colossal Order\...

echo Linking "%LOCALAPPDATA%\Colossal Order\" => "%CD%\AppData"
rmdir  "%LOCALAPPDATA%\Colossal Order\"
mklink /D "%LOCALAPPDATA%\Colossal Order\" "%CD%\AppData"

"C:\Program Files (x86)\Steam\steam.exe" -applaunch 255710 -noWorkshop
pause

echo Restoring old %LOCALAPPDATA%\Colossal Order\...

rmdir  "%LOCALAPPDATA%\Colossal Order\"
mklink /D "%LOCALAPPDATA%\Colossal Order" "%LOCALAPPDATA%\Colossal Order.real"
