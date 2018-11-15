$serviceName = 'WCFExampleService'
$svcInstallpath = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe'
$servicelocation = 'C:\Services\WcfExampleSvc\WCFExample.exe'



If (Get-Service $serviceName -ErrorAction SilentlyContinue) {

    If ((Get-Service $serviceName).Status -ne 'Running') {

        Start-Service $serviceName
        Write-Host "Starting $serviceName"

    } 

} Else {

    Write-Host "Installing $serviceName"
    Invoke-Expression "& `$svcInstallpath` /LogFile= $servicelocation"	
	Start-Service $serviceName

}

#if ($Host.Name -eq "ConsoleHost")
#{
#    Write-Host "Press any key to continue..."
#    $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp") > $null
#}
