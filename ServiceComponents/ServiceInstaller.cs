
using System.ComponentModel;


namespace WCFExample.ServiceComponents
{
    [RunInstaller(true)]
    public class WcfExampleServiceInstaller : CommonInstaller
    {
        public WcfExampleServiceInstaller()
        {
            ServiceName = "WCFExampleService";
            ServiceDescription = "WCF Example Windows Service";
            AssemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            ConfigureInstaller();

        }

    }
}