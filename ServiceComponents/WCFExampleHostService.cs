using System;

using System.Configuration;
using System.Diagnostics;

using System.Net;

using System.ServiceModel.Web;
using System.ServiceProcess;


namespace WCFExample.ServiceComponents
{
    public partial class WcfExampleHostService : ServiceBase
    {
        private const string HttpsBaseAddressFormat = "http://{0}:{1}/{2}";
        private string _httpsBaseAddress;


        public WcfExampleHostService()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            Init();

        }

        public void Init()
        {
            try
            {
     
                CalculatorRestFulService();
              
            }
            catch (Exception ex)
            {
               
                EventLog.WriteEntry("WCF Example Service failed to start. Error: " + ex.Message, EventLogEntryType.Error);
                Environment.Exit(-1);
            }

        }



        private void CalculatorRestFulService()
        {
            _httpsBaseAddress = string.Format(HttpsBaseAddressFormat, Dns.GetHostName(), ConfigurationManager.AppSettings.Get("HttpsServicePort"), ConfigurationManager.AppSettings.Get("JsonUrl"));
            Uri httpUrl = new Uri(_httpsBaseAddress);
            WebServiceHost host = new WebServiceHost(typeof(CalculatorService), httpUrl);
            host.Open();


        }



    }
}
