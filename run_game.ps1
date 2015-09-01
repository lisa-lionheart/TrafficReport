

$workingDir = "C:\Users\Lisa\Development\TrafficReport"
$steamdir = 'C:\Program Files (x86)\Steam'
$LogFile = "$steamdir\steamapps\common\Cities_Skylines\TrafficReport.log"
$game = $null

$appData = Get-ChildItem Env:LOCALAPPDATA
$appData = $appData.Value

if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{   
	Start-Process powershell -Verb runAs -ArgumentList "-File $workingDir\run_game.ps1" 
	Break
}

Import-Module "$workingDir\mklink"

if(Test-Path "$appData\Colossal Order") {
	$existing = Get-Item "$appData\Colossal Order"
	if($existing.LinkType -ne 'SymbolicLink') {
		echo 'Existing data folder, please rename your "Collosal Order" folder to "Colosal Order.real" ...'
		pause
		Break
	}
   
	cmd /c rmdir "$appData\Colossal Order"
}
New-Symlink "$appData\Colossal Order" "$workingDir\AppData"

& $steamdir\steam.exe -applaunch 255710 -noWorkshop

echo 'Getting handle on game..'
while($game -eq $null) {
	try {
		$game = Get-Process Cities -ErrorAction SilentlyContinue
	} Catch [system.exception] {
		echo 'Not started yet'
	}
}

echo 'Game started'

echo "Log file is $logFile"
Set-Content $logFile -Value ''

$linesRead = 0
while($game -ne $null) {
   $logData = Get-Content $logFile
   for(;$linesRead -le $logData.Count-1;$linesRead++) {
      $out = "$linesRead : "+ $logData[$linesRead]
	  write $out
   }

   sleep 1
   $game = Get-Process Cities -ErrorAction SilentlyContinue
}

echo Game Exited...

cmd /c rmdir "$appData\Colossal Order"
New-Symlink "$appData\Colossal Order" "$appData\Colossal Order.real"
echo 'Restored data folder'