using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Web.ModelBinding;
using WCFExample.ServiceContract;

namespace WCFExample
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, Namespace = "WCFExample")]
    public class CalculatorService : ICalculator
    {
        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedResponse,
            UriTemplate = "Add?n1={n1}&n2={n2}")]
        public CalculatorResponse Add(double n1, double n2)
        {
            CalculatorResponse response;
            try
            {
                response = new CalculatorResponse {Answer = n1 + n2};
            }
            catch (Exception e)
            {
                response = new CalculatorResponse {ErrorMessage = e.Message};
            }

            return response;

        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedResponse,
            UriTemplate = "Divide?n1={n1}&n2={n2}")]
        public CalculatorResponse Divide(double n1, double n2)
        {

            CalculatorResponse response;
            try
            {
                var ans = n1 / n2;
                if (double.IsInfinity(ans))
                    throw new Exception("Cannot divide by zero");

                response = new CalculatorResponse {Answer = ans};
            }
            catch (Exception e)
            {
                response = new CalculatorResponse {ErrorMessage = e.Message};
            }

            return response;

        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedResponse,
            UriTemplate = "Multiply?n1={n1}&n2={n2}")]
        public CalculatorResponse Multiply(double n1, double n2)
        {
            CalculatorResponse response;
            try
            {
                response = new CalculatorResponse {Answer = n1 * n2};
            }
            catch (Exception e)
            {
                response = new CalculatorResponse {ErrorMessage = e.Message};
            }

            return response;
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedResponse,
            UriTemplate = "Subtract?n1={n1}&n2={n2}")]
        public CalculatorResponse Subtract(double n1, double n2)
        {
            CalculatorResponse response;
            try
            {
                response = new CalculatorResponse {Answer = n1 - n2};
            }
            catch (Exception e)
            {
                response = new CalculatorResponse {ErrorMessage = e.Message};
            }

            return response;
        }
    }



    public class CalculatorResponse
    {
        public double Answer { get; set; }

        public string ErrorMessage { get; set; }

    }



}