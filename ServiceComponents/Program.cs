using System.ServiceProcess;
using System.Threading;

namespace WCFExample.ServiceComponents
{
    static class Program
    {
        static void Main()
        {

#if DEBUG
            WcfExampleHostService hostService = new WcfExampleHostService();
            hostService.Init();
            Thread.Sleep(Timeout.Infinite);
#else
			ServiceBase.Run(new WcfExampleHostService());
#endif

        }
    }

}