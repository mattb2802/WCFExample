using System;
using System.ServiceModel;


namespace WCFExample.ServiceContract
{
    //[ServiceContract(Namespace = "WCFExample")]
    [ServiceContract]
    public interface ICalculator
    {
        [OperationContract]
        CalculatorResponse Add(double n1, double n2);
        [OperationContract]
        CalculatorResponse Subtract(double n1, double n2);
        [OperationContract]
        CalculatorResponse Multiply(double n1, double n2);
        [OperationContract]
        CalculatorResponse Divide(double n1, double n2);
    }
}
