using System;
using System.ServiceProcess;
using System.Collections;

namespace WCFExample.ServiceComponents
{
    partial class ServiceInstallerEx : ServiceInstaller
    {
        public ServiceInstallerEx()
        {
            FailureActions = new ArrayList();

            // Register the event handlers for post install operations
            Committed += UpdateServiceConfig;
            Committed += StartIfNeeded;
            
            // Set the Log Msg Base
            logMsgBase = $"ServiceInstallerEx : {ServiceName} : ";
        }
    }
}
