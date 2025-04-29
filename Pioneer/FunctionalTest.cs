using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Reflection;
using DA = System.ComponentModel.DataAnnotations;
using Pioneer.Models;

namespace Pioneer
{
    public class FunctionalTest
    {
        public static List<String> failMessage = new List<String>();
        public static String failureMsg = "";
        public static int failcnt = 1;
        public int totalTestcases = 0;

        public IWebDriver _driver;
        public string userport;
        public string appURL;

        [OneTimeSetUp]
        public void Setup()
        {
            try
            {
                userport = Environment.GetEnvironmentVariable("userport") == null ? "7081" : Environment.GetEnvironmentVariable("userport");

                FirefoxOptions options = new FirefoxOptions();
                // {
                //     AcceptInsecureCertificates = true
                // };

                options.AddArgument("--headless");
                _driver = new FirefoxDriver(options);

                appURL = "http://localhost:" + userport + "/";
                //_driver = new FirefoxDriver();
                _driver.Navigate().GoToUrl(appURL);
            }
            catch (Exception ex)
            {
                //Console.WriteLine("-----------" + ex.Message);
            }
        }

        public void Dispose()
        {
            if (totalTestcases > 1)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"./FailedTest.txt"))
                {
                    foreach (string line in failMessage)
                    {
                        //Console.WriteLine("line " + line);
                        file.WriteLine(line);
                    }
                }
            }
            else
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"./FailedTest.txt"))
                {
                    file.WriteLine("error");
                }
            }
            _driver.Quit();
            _driver.Dispose();
        }

        public void ExceptionCatch(string functionName, string catchMsg, string msg, string msg_name, string exceptionMsg = "")
        {
            failMessage.Add(functionName);

            if (msg == "")
            {
                msg = exceptionMsg + (exceptionMsg != "" ? " - " : "") + catchMsg + "\n";
                msg_name += "Fail " + failcnt + " -- " + functionName + "::\n" + msg;
            }
            else
                msg_name += "Fail " + failcnt + " -- " + functionName + "::\n" + msg;

            failureMsg += msg_name;
            failcnt++;
            Assert.Fail(msg);
        }
        public String SeleniumException(IWebDriver wd)
        {
            String msg = "";
            if (wd.Title == "Parser Error")
            {
                string[] stringSeparators = new string[] { "Parser Error Message:" };
                string[] result;
                result = wd.PageSource.Split(stringSeparators, StringSplitOptions.None);
                string[] stringSeparators2 = new string[] { "<b>Source Error:</b>" };
                result = result[1].Split(stringSeparators2, StringSplitOptions.None);
                msg += result[0].Replace("<br>", "").Replace("</b>", "").Replace("\r", "").Replace("\n", "");
            }
            else if (wd.Title.Contains("Error"))
            {
                msg += wd.FindElement(By.CssSelector("h2.exceptionMessage")).Text;
            }
            return msg;
        }

        [Test, Order(1)]
        public void Test1_Check_PioneerLogin_Properties()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("Pioneer", "Pioneer.Models", "PioneerLogin");
            var CurrentProperty = new KeyValuePair<string, string>();

            try
            {
                var Properties = new Dictionary<string, string>
                {
                    { "RegistrationNumber", "String" },
                    { "Password", "String" },
                };

                foreach (var property in Properties)
                {
                    CurrentProperty = property;
                    var IsFound = tb.HasProperty(property.Key, property.Value);

                    //Assert.IsTrue(IsFound, tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value));

                    if (!IsFound)
                    {
                        msg += tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value) + "\n>";
                    }
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Assert.Fail(tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key));
                ExceptionCatch(functionName, tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key), msg, msg_name);
            }
        }

        [Test, Order(2)]
        public void Test2_Check_PioneerLogin_DataAnnotations()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("Pioneer", "Pioneer.Models", "PioneerLogin");

            //(string propertyname, string attributename) PropertyUnderTest = ("", "");
            string PropertyUnderTest_propertyname = "";
            string PropertyUnderTest_attributename = "";

            try
            {
                //--------------------------------------------
                PropertyUnderTest_propertyname = "RegistrationNumber";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your registration number");

                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your password");

                //--------------------------------------------
                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "DataType";
                DataTypeAttributeTest();

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while testing {PropertyUnderTest_propertyname} for {PropertyUnderTest_attributename} attribute in {tb.type.Name}", msg, msg_name);
            }

            #region LocalFunction_KeyAttributeTest
            void KeyAttributeTest()
            {
                string Message = $"Key attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} is not found";
                var attribute = tb.GetAttributeFromProperty<DA.KeyAttribute>(PropertyUnderTest_propertyname, typeof(DA.KeyAttribute));
                //Assert.IsNotNull(attribute, Message);
                if (attribute == null)
                {
                    msg += Message + "\n";
                }
            }
            #endregion

            #region LocalFunction_RequiredAttributeTest
            void RequiredAttributeTest(string errorMessage)
            {
                string Message = $"Required attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.RequiredAttribute>(PropertyUnderTest_propertyname, typeof(DA.RequiredAttribute));

                if (attribute == null)
                {
                    msg += $"Required attribute not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }

                if (errorMessage != attribute.ErrorMessage)
                {
                    msg += $"{Message} ErrorMessage={errorMessage} \n";
                }
            }
            #endregion

            #region LocalFunction_DisplayAttributeTest
            void DisplayAttributeTest(string name)
            {
                string Message = $"Display Attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DisplayAttribute>(PropertyUnderTest_propertyname, typeof(DA.DisplayAttribute));

                if (name != attribute.Name)
                {
                    msg += $"{Message} Name = {name} \n";
                }
            }
            #endregion

            #region LocalFunction_DataTypeAttributeTest
            void DataTypeAttributeTest()
            {
                //string Message = $"DataType attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DataTypeAttribute>(PropertyUnderTest_propertyname, typeof(DA.DataTypeAttribute));

                if (attribute.DataType.ToString() != "Password")
                {
                    msg += $"DataType - Password not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }
            }
            #endregion
        }

        [Test, Order(3)]
        public void Test3_Check_Freshman_Properties()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("Pioneer", "Pioneer.Models", "Freshman");
            var CurrentProperty = new KeyValuePair<string, string>();

            try
            {
                var Properties = new Dictionary<string, string>
                {
                    { "Id", "Int32" },
                    { "RegistrationNumber", "String" },
                    { "Name", "String" },
                    { "Password", "String" },
                    { "ConfirmPassword", "String" },
                    { "Age", "Int32" },
                    { "MobileNumber", "Int64" },

                };

                foreach (var property in Properties)
                {
                    CurrentProperty = property;
                    var IsFound = tb.HasProperty(property.Key, property.Value);

                    //Assert.IsTrue(IsFound, tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value));

                    if (!IsFound)
                    {
                        msg += tb.Messages.GetPropertyNotFoundMessage(property.Key, property.Value) + "\n>";
                    }
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Assert.Fail(tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key));
                ExceptionCatch(functionName, tb.Messages.GetExceptionMessage(ex, propertyName: CurrentProperty.Key), msg, msg_name);
            }
        }

        [Test, Order(4)]
        public void Test4_Check_Freshman_DataAnnotations()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("Pioneer", "Pioneer.Models", "Freshman");

            //modified by Anand Rajan
            //(string propertyname, string attributename) PropertyUnderTest = ("", "");
            string PropertyUnderTest_propertyname = "";
            string PropertyUnderTest_attributename = "";

            try
            {
                PropertyUnderTest_propertyname = "Id";
                PropertyUnderTest_attributename = "Key";
                KeyAttributeTest();

                //--------------------------------------------
                PropertyUnderTest_propertyname = "RegistrationNumber";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your registration number");

                PropertyUnderTest_propertyname = "Name";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your name");

                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your password");

                PropertyUnderTest_propertyname = "ConfirmPassword";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please re-enter your password");

                PropertyUnderTest_propertyname = "Age";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your age");

                PropertyUnderTest_propertyname = "MobileNumber";
                PropertyUnderTest_attributename = "Required";
                RequiredAttributeTest("Please enter your mobile number");

                //--------------------------------------------
                PropertyUnderTest_propertyname = "ConfirmPassword";
                PropertyUnderTest_attributename = "Compare";
                CompareAttributeTest("Password");

                //--------------------------------------------
                PropertyUnderTest_propertyname = "Password";
                PropertyUnderTest_attributename = "DataType";
                DataTypeAttributeTest();

                PropertyUnderTest_propertyname = "ConfirmPassword";
                PropertyUnderTest_attributename = "DataType";
                DataTypeAttributeTest();


                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while testing {PropertyUnderTest_propertyname} for {PropertyUnderTest_attributename} attribute in {tb.type.Name}", msg, msg_name);
            }

            #region LocalFunction_KeyAttributeTest
            void KeyAttributeTest()
            {
                string Message = $"Key attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} is not found";
                var attribute = tb.GetAttributeFromProperty<DA.KeyAttribute>(PropertyUnderTest_propertyname, typeof(DA.KeyAttribute));
                //Assert.IsNotNull(attribute, Message);
                if (attribute == null)
                {
                    msg += Message + "\n";
                }
            }
            #endregion

            #region LocalFunction_RequiredAttributeTest
            void RequiredAttributeTest(string errorMessage)
            {
                string Message = $"Required attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.RequiredAttribute>(PropertyUnderTest_propertyname, typeof(DA.RequiredAttribute));

                if (attribute == null)
                {
                    msg += $"Required attribute not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }

                if (errorMessage != attribute.ErrorMessage)
                {
                    msg += $"{Message} ErrorMessage={errorMessage} \n";
                }
            }
            #endregion

            #region LocalFunction_DisplayAttributeTest
            void DisplayAttributeTest(string name)
            {
                string Message = $"Display Attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DisplayAttribute>(PropertyUnderTest_propertyname, typeof(DA.DisplayAttribute));

                if (name != attribute.Name)
                {
                    msg += $"{Message} Name = {name} \n";
                }
            }
            #endregion

            #region LocalFunction_CompareAttributeTest
            void CompareAttributeTest(string name)
            {
                string Message = $"Compare Attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.CompareAttribute>(PropertyUnderTest_propertyname, typeof(DA.CompareAttribute));

                if (name != attribute.OtherProperty)
                {
                    msg += $"{Message} Compare = {name} \n";
                }
            }
            #endregion

            #region LocalFunction_DataTypeAttributeTest
            void DataTypeAttributeTest()
            {
                //string Message = $"DataType attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.DataTypeAttribute>(PropertyUnderTest_propertyname, typeof(DA.DataTypeAttribute));

                if (attribute.DataType.ToString() != "Password")
                {
                    msg += $"DataType - Password not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }
            }
            #endregion

            #region LocalFunction_RangeAttributeTest
            void RangeAttributeTest(int min, int max, string errorMessage)
            {
                string Message = $"Range attribute on {PropertyUnderTest_propertyname} of {tb.type.Name} class doesnot have ";
                var attribute = tb.GetAttributeFromProperty<DA.RangeAttribute>(PropertyUnderTest_propertyname, typeof(DA.RangeAttribute));

                if (Convert.ToInt32(attribute.Minimum) != min || Convert.ToInt32(attribute.Maximum) != max)
                {
                    msg += $"Range not applied on {PropertyUnderTest_propertyname} of {tb.type.Name} class.\n";
                }

                if (errorMessage != attribute.ErrorMessage)
                {
                    msg += $"{Message} ErrorMessage={errorMessage} \n";
                }
            }
            #endregion
        }

        [Test, Order(5)]
        public void Test5_PioneerDBContext_DbSet_Property_CreationTest()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("Pioneer", "Pioneer.Models", "PioneerDBContext");
            try
            {
                var IsFound = tb.HasProperty("Freshmen", "DbSet`1");
                if (!IsFound)
                {
                    msg += tb.Messages.GetPropertyNotFoundMessage("Freshmen", "DbSet<Freshman> \n");
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                //Assert.Fail(tb.Messages.GetExceptionMessage(ex, propertyName: "Donors"));
                ExceptionCatch(functionName, tb.Messages.GetExceptionMessage(ex), msg, msg_name);
            }
        }

        [Test, Order(6)]
        [TestCase("PioneerPortal", TestName = "Test6_PioneerPortal_IsAvailable")]
        [TestCase("InsertFreshman", TestName = "Test7_InsertFreshman_IsAvailable")]
        [TestCase("Dashboard", TestName = "Test8_Dashboard_IsAvailable")]
        public void Test6_7_8_Get_ActionCreated_Test(string mname)
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;
            TestBase tb = new TestBase("Pioneer", "Pioneer.Controllers", "PioneerController");
            try
            {
                var Method = tb.type.GetMethod(mname, new Type[] { });

                if (mname == "Dashboard")
                {
                    Method = tb.type.GetMethod(mname, new Type[] { typeof(PioneerLogin) });
                }

                if (Method == null)
                {
                    msg += $"{tb.type.Name} doesnot defines action method \n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while check Get action method is present or not in {tb.type.Name}. \n", msg, msg_name);
            }
        }

        [Test, Order(7)]
        public void Test9_PioneerPortal_Post_ActionCreated_Test()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("Pioneer", "Pioneer.Controllers", "PioneerController");
            try
            {
                var Method = tb.type.GetMethod("PioneerPortal", new Type[] { typeof(PioneerLogin) });
                if (Method == null)
                {
                    msg += $"{tb.type.Name} doesnot defines create action method which accepts over object as parameter \n";
                }

                var attr = Method.GetCustomAttribute<HttpPostAttribute>();

                if (attr == null)
                {
                    msg += $"PioneerPortal action is not marked with attributes to run on http post request in {tb.type.Name} controller \n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while check PioneerPortal action method is present or not in {tb.type.Name}. \n", msg, msg_name);
            }
        }

        [Test, Order(8)]
        public void Test10_InsertFreshman_Post_ActionCreated_Test()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            TestBase tb = new TestBase("Pioneer", "Pioneer.Controllers", "PioneerController");
            try
            {
                var Method = tb.type.GetMethod("InsertFreshman", new Type[] { typeof(Freshman) });
                if (Method == null)
                {
                    msg += $"{tb.type.Name} doesnot defines create action method which accepts over object as parameter \n";
                }

                var attr = Method.GetCustomAttribute<HttpPostAttribute>();

                if (attr == null)
                {
                    msg += $"InsertFreshman action is not marked with attributes to run on http post request in {tb.type.Name} controller \n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, $"Exception while check InsertFreshman action method is present or not in {tb.type.Name}. \n", msg, msg_name);
            }
        }

        [Test, Order(11)]
        public void Test11_UI_InsertFreshman_Message()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            try
            {

                _driver.Navigate().GoToUrl(appURL + "Pioneer/InsertFreshman");
                System.Threading.Thread.Sleep(5000);

                _driver.SetElementText("RegistrationNumber", "TPu@123");
                _driver.SetElementText("Name", "TestName");
                _driver.SetElementText("Password", "TPu@123");
                _driver.SetElementText("ConfirmPassword", "TPu@123");
                _driver.SetElementText("Age", "20");
                _driver.SetElementText("MobileNumber", "8098889990");
                _driver.ClickElement("btnAdd");
                System.Threading.Thread.Sleep(5000);


                if (!_driver.PageSource.Contains("RegistrationNumber and Password should not be the same"))
                {
                    msg += $"InsertFreshman Page NOT displaying the message for Invalid Password message correctly.\n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, ex.Message + $"-Exception trying to add and display the freshman details. Exception :{ex.InnerException?.Message}\n", msg, msg_name);
            }
        }

        [Test, Order(12)]
        public void Test12_UI_Test()
        {
            totalTestcases++;
            String msg = "";
            String msg_name = "";
            string functionName = NUnit.Framework.TestContext.CurrentContext.Test.Name;

            try
            {
                _driver.Navigate().GoToUrl(appURL + "Pioneer/PioneerPortal");
                System.Threading.Thread.Sleep(5000);

                _driver.FindElement(By.Id("lnkRegister")).Click();
                //msg+="+++++++++"+_driver.PageSource.ToString();
                System.Threading.Thread.Sleep(5000);

                _driver.SetElementText("RegistrationNumber", "TPu@123");
                _driver.SetElementText("Name", "TestName1");
                _driver.SetElementText("Password", "Tpu@123");
                _driver.SetElementText("ConfirmPassword", "Tpu@123");
                _driver.SetElementText("Age", "20");
                _driver.SetElementText("MobileNumber", "8098889990");
                _driver.ClickElement("btnAdd");
                System.Threading.Thread.Sleep(5000);

                //Console.WriteLine(_driver.PageSource);
                _driver.Navigate().GoToUrl(appURL + "Pioneer/PioneerPortal");
                System.Threading.Thread.Sleep(5000);

                _driver.SetElementText("RegistrationNumber", "TPu@123");
                _driver.SetElementText("Password", "Tpu@123");

                _driver.ClickElement("btnLogin");
                System.Threading.Thread.Sleep(5000);
                msg += _driver.PageSource.ToString();

                var result = _driver.FindElement(By.Id("message"));

                if (!result.Text.ToString().ToLower().Contains("your register number")
                        || !result.Text.ToString().ToLower().Contains("tpu@123")
                        || !result.Text.ToString().ToLower().Contains("is successfully inserted to pioneer university"))
                {
                    msg += $"Dashboard Page NOT displaying the details correctly.\n";
                }

                if (msg != "")
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                ExceptionCatch(functionName, ex.Message + $"-Exception trying to add and display the freshman details. Exception :{ex.InnerException?.Message}\n", msg, msg_name);
            }
        }
    }

    public class AssertFailureMessages
    {
        private string TypeName;
        public AssertFailureMessages(string typeName)
        {
            this.TypeName = typeName;
        }
        public string GetAssemblyNotFoundMessage(string assemblyName)
        {
            return $"Could not find {assemblyName}.dll";
        }
        public string GetTypeNotFoundMessage(string assemblyName, string typeName = null)
        {
            return $"Could not find {typeName ?? TypeName} in  {assemblyName}.dll";
        }
        public string GetFieldNotFoundMessage(string fieldName, string fieldType, string typeName = null)
        {
            return $"Could not a find public field {fieldName} of {fieldType} type in {typeName ?? TypeName} class";
        }
        public string GetPropertyNotFoundMessage(string propertyName, string propertyType, string typeName = null)
        {
            return $"Could not a find public property {propertyName} of {propertyType} type in {typeName ?? TypeName} class";
        }
        public string GetFieldTypeMismatchMessage(string fieldName, string expectedFieldType, string typeName = null)
        {
            return $"{fieldName} is not of {expectedFieldType} data type in {typeName ?? TypeName} class";
        }
        public string GetExceptionTestFailureMessage(string methodName, string customExceptionTypeName, string propertyName, Exception exception, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot throws exception of type {customExceptionTypeName} on validation failure for {propertyName}.\nException Message: {exception.InnerException?.Message}\nStack Trace:{exception.InnerException?.StackTrace}";
        }

        public string GetExceptionMessage(Exception ex, string methodName = null, string fieldName = null, string propertyName = null, string typeName = null)
        {
            string testFor = methodName != null ? methodName + " method" : fieldName != null ? fieldName + " field" : propertyName != null ? propertyName + " property" : "undefined";
            //return $" Exception while testing {testFor} of {typeName ?? TypeName} class.\nException message : {ex.InnerException?.Message}\nStack Trace : {ex.InnerException?.StackTrace}";
            return $" Exception while testing {testFor} of {typeName ?? TypeName} class.\n";
        }

        public string GetReturnTypeAssertionFailMessage(string methodName, string expectedTypeName, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot return value of {expectedTypeName} data type";
        }
        public string GetReturnValueAssertionFailMessage(string methodName, object expectedValue, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot return the value {expectedValue}";
        }

        public string GetValidationFailureMessage(string methodName, string expectedValidationMessage, string propertyName, string typeName = null)
        {
            return $"{methodName} method of {typeName ?? TypeName} class doesnot return '{expectedValidationMessage}' on validation failure for property {propertyName}";
        }

    }

    public static class SeleniumExtensions
    {

        public static void SetElementText(this IWebDriver driver, string elementId, string text)
        {
            var Element = driver.FindElement(By.Id(elementId));
            Element.Clear();
            Element.SendKeys(text);
        }

        public static string GetElementText(this IWebDriver driver, string elementId)
        {
            return driver.GetElementText(elementId);
        }

        public static void ClickElement(this IWebDriver driver, string elementId)
        {
            driver.FindElement(By.Id(elementId)).Click();
        }

        //public static void SelectDropDownItemByValue(this IWebDriver driver, string elementId, string value)
        //{
        //    new SelectElement(driver.FindElement(By.Id(elementId))).SelectByValue(value);
        //}
        //public static void SelectDropDownItemByText(this IWebDriver driver, string elementId, string text)
        //{
        //    new SelectElement(driver.FindElement(By.Id(elementId))).SelectByText(text);
        //}


        public static string GetElementInnerText(this IWebDriver driver, string elementType, string attribute)
        {
            return driver.FindElement(By.XPath($"//{elementType}[{attribute}]")).GetAttribute("innerHTML");
        }

        public static int GetTableRowsCount(this IWebDriver driver, string elementId)
        {
            var Table = driver.FindElement(By.Id(elementId));
            return Table.FindElements(By.TagName("tr")).Count;
        }



    }

    public class TestBase : ATestBase
    {
        public TestBase(string assemblyName, string namespaceName, string typeName)
        {
            Console.WriteLine("-----12-------");
            Messages = new AssertFailureMessages(typeName);
            this.assemblyName = assemblyName;
            this.namespaceName = namespaceName;
            this.typeName = typeName;

            Console.WriteLine("-----13-------");
            Messages = new AssertFailureMessages(typeName);
            Console.WriteLine("-----14-------");
            assembly = Assembly.Load(assemblyName);
            Console.WriteLine("-----15-------");
            type = assembly.GetType($"{namespaceName}.{typeName}");
            Console.WriteLine("-----16-------");
        }
    }
    public abstract class ATestBase
    {
        public string assemblyName;
        public string namespaceName;
        public string typeName;
        public string controllerName;

        public AssertFailureMessages Messages;//= new AssertFailureMessages(typeName);

        protected Assembly assembly;
        public Type type;


        protected object typeInstance = null;
        protected void CreateNewTypeInstance()
        {
            typeInstance = assembly.CreateInstance(type.FullName);
        }
        public object GetTypeInstance()
        {
            if (typeInstance == null)
                CreateNewTypeInstance();
            return typeInstance;
        }
        public object InvokeMethod(string methodName, Type type, params object[] parameters)
        {
            var method = type.GetMethod(methodName);
            var instance = GetTypeInstance();
            var result = method.Invoke(instance, parameters);
            return result;
        }
        public T InvokeMethod<T>(string methodName, Type type, params object[] parameters)
        {
            var result = InvokeMethod(methodName, type, parameters);
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public bool HasField(string fieldName, string fieldType)
        {
            bool Found = false;
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null)
            {
                Found = field.FieldType.Name == fieldType;
            }
            return Found;
        }

        public bool HasProperty(string propertyName, string propertyType)
        {
            bool Found = false;
            var property = type.GetProperty(propertyName);
            if (property != null)
            {
                Found = property.PropertyType.Name == propertyType; ;
            }
            return Found;
        }

        public T GetAttributeFromProperty<T>(string propertyName, Type attribute)
        {

            var attr = type.GetProperty(propertyName).GetCustomAttribute(attribute, false);
            return (T)Convert.ChangeType(attr, typeof(T));
        }

        //public bool CheckFromUriAttribute(string methodName)
        //{
        //    ParameterInfo[] parameters = type.GetMethod(methodName).GetParameters();
        //    if (parameters.Length == 0)
        //    {
        //        return false;
        //    }

        //    Object[] myAttributes = parameters[0].GetCustomAttributes(typeof(System.Web.Http.FromUriAttribute), false);
        //    //{System.Web.Http.FromUriAttribute[0]}
        //    if (myAttributes.Length == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //public bool CheckFromBodyAttribute(string methodName)
        //{
        //    ParameterInfo[] parameters = type.GetMethod(methodName).GetParameters();
        //    if (parameters.Length == 0)
        //    {
        //        return false;
        //    }

        //    Object[] myAttributes = parameters[0].GetCustomAttributes(typeof(System.Web.Http.FromBodyAttribute), false);

        //    if (myAttributes.Length == 0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
    }
}
