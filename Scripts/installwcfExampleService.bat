
rem %SystemRoot%\sysnative\WindowsPowerShell\v1.0\powershell.exe -command "Set-ExecutionPolicy Unrestricted -Force"
%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -command "Set-ExecutionPolicy Unrestricted -Force"


IF NOT EXIST C:\Services\WcfExampleSvc mkdir C:\Services\WcfExampleSvc

cd c:\temp

rem %SystemRoot%\sysnative\WindowsPowerShell\v1.0\powershell.exe -command ".\installwcfExampleService.ps1"
rem %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -NoExit -Command "& {Start-Process Powershell.exe -ArgumentList '-ExecutionPolicy unrestricted -Force -File %~dp0installwcfExampleService.ps1' -Verb RunAs}"

%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -command ".\installwcfExampleService.ps1"

rem pause