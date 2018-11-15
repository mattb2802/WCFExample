

rem %SystemRoot%\sysnative\WindowsPowerShell\v1.0\powershell.exe -command "Set-ExecutionPolicy Unrestricted -Force"
%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -command "Set-ExecutionPolicy Unrestricted -Force"

IF NOT EXIST c:\temp mkdir c:\temp

IF NOT EXIST c:\Services\WcfExampleSvc mkdir c:\Services\WcfExampleSvc

cd c:\temp

rem IF EXIST c:\temp\stopwcfExampleService.ps1 %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -command ".\stopwcfExampleService.ps1"

rem %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -NoExit -Command "& {Start-Process Powershell.exe -ArgumentList '-ExecutionPolicy unrestricted -Force -File %~dp0stopwcfExampleService.ps1' -Verb RunAs}"

%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -command ".\stopwcfExampleService.ps1"


rem del /q "c:\services\WcfExampleSvc\*.*"

rem pause



