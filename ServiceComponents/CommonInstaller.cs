using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;

namespace WCFExample.ServiceComponents
{
    [RunInstaller(true)]
    public abstract class CommonInstaller : Installer
    {
        private readonly IContainer _components;
        protected ServiceInstallerEx ServiceInstaller;
        private ServiceProcessInstaller _serviceProcessInstaller;

        protected string ServiceName { get; set; }
        protected string ServiceDescription { get; set; }
        protected string AssemblyLocation { get; set; }
        protected ServiceAccount? Account { get; set; }
        protected string Username { get; set; }
        protected string Password { get; set; }
        public bool ShouldDelayAutoStart { get; set; }

        protected CommonInstaller()
        {
            _components = new Container();
        }

		protected void ConfigureInstaller()
		{
			if(string.IsNullOrEmpty(ServiceName))
				throw new Exception("ServiceName cannot be empty.");

			if (string.IsNullOrEmpty(ServiceDescription))
				throw new Exception("ServiceDescription cannot be empty.");

			if(string.IsNullOrEmpty(AssemblyLocation))
				throw new Exception("Assembly location cannot be empty.");

		    ServiceInstaller = new ServiceInstallerEx
		    {
		        StartType = ServiceStartMode.Automatic,
		        StartOnInstall = false,
		        ServiceName = ServiceName,
		        DisplayName = ServiceName.Replace(".", " "),
		        Description = $"Version: {FileVersionInfo.GetVersionInfo(AssemblyLocation).FileVersion}. {ServiceDescription}",
		        FailCountResetTime = 60,
		        FailRebootMsg = "Failed to Reboot",
		        DelayedAutoStart = ShouldDelayAutoStart
		    };

		    ServiceInstaller.FailureActions.Add(new FailureAction(RecoverAction.Restart, 5000));
			ServiceInstaller.FailureActions.Add(new FailureAction(RecoverAction.Restart, 5000));
			ServiceInstaller.FailureActions.Add(new FailureAction(RecoverAction.None, 5000));

			_serviceProcessInstaller = new ServiceProcessInstaller();

			if(Account.HasValue)
			{
				_serviceProcessInstaller.Account = Account.Value;
				_serviceProcessInstaller.Username = Username;
				_serviceProcessInstaller.Password = Password;
			}
			else
			{
				_serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
				_serviceProcessInstaller.Username = null;
				_serviceProcessInstaller.Password = null;
			}

			Installers.Add(ServiceInstaller);
			Installers.Add(_serviceProcessInstaller);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_components?.Dispose();

			if (disposing)
				_serviceProcessInstaller?.Dispose();

			if (disposing)
				ServiceInstaller?.Dispose();

			base.Dispose(disposing);
		}
	}


    public enum RecoverAction
    {
        None = 0,
        Restart = 1,
        Reboot = 2,
        Runcommand = 3
    }

}
