using System;
using WCFExample;
using WCFExample.ServiceContract;
using NUnit.Framework;


namespace UnitTests
{

    [TestFixture]
    [DefaultFloatingPointTolerance(0)]
    public class CalculatorServiceTest
    {

        private ICalculator MockCalculatorService { get; set; }


        [SetUp]
        public void SetUp()
        {
            MockCalculatorService = new CalculatorService();
        }




        [Test]
        public void Calculator_Add_Test()
        {
            try
            {
                var n1 = 2.0;
                var n2 = 5.0;

                var ss = MockCalculatorService.Add(n1, n2);

                Assert.That(n1 + n2, Is.EqualTo(ss.Answer));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void Calculator_Divide_Test()
        {
            try
            {
                var n1 = 6.0;
                var n2 = 2.0;

                var ss = MockCalculatorService.Divide(n1, n2);

                Assert.That((n1 / n2), Is.EqualTo(ss.Answer));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void Calculator_DividebyZero_Test()
        {
            try
            {
                var n1 = 6.0;
                var n2 = 0;

                var ss = MockCalculatorService.Divide(n1, n2);

                Assert.That(0, Is.EqualTo(ss.Answer));
                Assert.That(!string.IsNullOrEmpty(ss.ErrorMessage));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void Calculator_Multiply_Test()
        {
            try
            {
                var n1 = 6.0;
                var n2 = 2.0;

                var ss = MockCalculatorService.Multiply(n1, n2);

                Assert.That((n1 * n2), Is.EqualTo(ss.Answer));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void Calculator_Subtract_Test()
        {
            try
            {
                var n1 = 6.0;
                var n2 = 2.0;

                var ss = MockCalculatorService.Subtract(n1, n2);

                Assert.That((n1 - n2), Is.EqualTo(ss.Answer));

            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }



    }



}
