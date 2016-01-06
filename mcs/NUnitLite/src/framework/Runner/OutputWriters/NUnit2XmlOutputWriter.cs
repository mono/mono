// ***********************************************************************
// Copyright (c) 2011 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.IO;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#else
using System.Collections.Specialized;
#endif

namespace NUnitLite.Runner
{
    /// <summary>
    /// NUnit2XmlOutputWriter is able to create an xml file representing
    /// the result of a test run in NUnit 2.x format.
    /// </summary>
    public class NUnit2XmlOutputWriter : OutputWriter
    {
        private XmlWriter xmlWriter;
        private DateTime startTime;

#if CLR_2_0 || CLR_4_0
        private static Dictionary<string, string> resultStates = new Dictionary<string, string>();
#else
        private static StringDictionary resultStates = new StringDictionary();
#endif

        static NUnit2XmlOutputWriter()
        {
            resultStates["Passed"] = "Success";
            resultStates["Failed"] = "Failure";
            resultStates["Failed:Error"] = "Error";
            resultStates["Failed:Cancelled"] = "Cancelled";
            resultStates["Inconclusive"] = "Inconclusive";
            resultStates["Skipped"] = "Skipped";
            resultStates["Skipped:Ignored"] = "Ignored";
            resultStates["Skipped:Invalid"] = "NotRunnable";
        }

        public NUnit2XmlOutputWriter(DateTime startTime)
        {
            this.startTime = startTime;
        }

        /// <summary>
        /// Writes the result of a test run to a specified TextWriter.
        /// </summary>
        /// <param name="result">The test result for the run</param>
        /// <param name="writer">The TextWriter to which the xml will be written</param>
        public override void WriteResultFile(ITestResult result, TextWriter writer)
        {
            // NOTE: Under .NET 1.1, XmlTextWriter does not implement IDisposable,
            // but does implement Close(). Hence we cannot use a 'using' clause.
            //using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
#if SILVERLIGHT
            XmlWriter xmlWriter = XmlWriter.Create(writer);
#else
            XmlTextWriter xmlWriter = new XmlTextWriter(writer);
            xmlWriter.Formatting = Formatting.Indented;
#endif

            try
            {
                WriteXmlOutput(result, xmlWriter);
            }
            finally
            {
                writer.Close();
            }
        }

        private void WriteXmlOutput(ITestResult result, XmlWriter xmlWriter)
        {
            this.xmlWriter = xmlWriter;

            InitializeXmlFile(result);
            WriteResultElement(result);
            TerminateXmlFile();
        }

        private void InitializeXmlFile(ITestResult result)
        {
            ResultSummary summaryResults = new ResultSummary(result);

            xmlWriter.WriteStartDocument(false);
            xmlWriter.WriteComment("This file represents the results of running a test suite");

            xmlWriter.WriteStartElement("test-results");

            xmlWriter.WriteAttributeString("name", result.FullName);
            xmlWriter.WriteAttributeString("total", summaryResults.TestCount.ToString());
            xmlWriter.WriteAttributeString("errors", summaryResults.ErrorCount.ToString());
            xmlWriter.WriteAttributeString("failures", summaryResults.FailureCount.ToString());
            xmlWriter.WriteAttributeString("not-run", summaryResults.NotRunCount.ToString());
            xmlWriter.WriteAttributeString("inconclusive", summaryResults.InconclusiveCount.ToString());
            xmlWriter.WriteAttributeString("ignored", summaryResults.IgnoreCount.ToString());
            xmlWriter.WriteAttributeString("skipped", summaryResults.SkipCount.ToString());
            xmlWriter.WriteAttributeString("invalid", summaryResults.InvalidCount.ToString());

            xmlWriter.WriteAttributeString("date", XmlConvert.ToString(startTime, "yyyy-MM-dd"));
            xmlWriter.WriteAttributeString("time", XmlConvert.ToString(startTime, "HH:mm:ss"));
            WriteEnvironment();
            WriteCultureInfo();
        }

        private void WriteCultureInfo()
        {
            xmlWriter.WriteStartElement("culture-info");
            xmlWriter.WriteAttributeString("current-culture",
                                           CultureInfo.CurrentCulture.ToString());
            xmlWriter.WriteAttributeString("current-uiculture",
                                           CultureInfo.CurrentUICulture.ToString());
            xmlWriter.WriteEndElement();
        }

        private void WriteEnvironment()
        {
            xmlWriter.WriteStartElement("environment");
            AssemblyName assemblyName = AssemblyHelper.GetAssemblyName(Assembly.GetExecutingAssembly());
            xmlWriter.WriteAttributeString("nunit-version",
                                           assemblyName.Version.ToString());
            xmlWriter.WriteAttributeString("clr-version",
                                           Environment.Version.ToString());
            xmlWriter.WriteAttributeString("os-version",
                                           Environment.OSVersion.ToString());
            xmlWriter.WriteAttributeString("platform",
                Environment.OSVersion.Platform.ToString());
#if !NETCF
            xmlWriter.WriteAttributeString("cwd",
                                           Environment.CurrentDirectory);
#if !SILVERLIGHT
            xmlWriter.WriteAttributeString("machine-name",
                                           Environment.MachineName);
            xmlWriter.WriteAttributeString("user",
                                           Environment.UserName);
            xmlWriter.WriteAttributeString("user-domain",
                                           Environment.UserDomainName);
#endif
#endif
            xmlWriter.WriteEndElement();
        }

        private void WriteResultElement(ITestResult result)
        {
            StartTestElement(result);

            WriteCategories(result);
            WriteProperties(result);

            switch (result.ResultState.Status)
            {
                case TestStatus.Skipped:
                    WriteReasonElement(result.Message);
                    break;
                case TestStatus.Failed:
                    WriteFailureElement(result.Message, result.StackTrace);
                    break;
            }
            
            if (result.Test is TestSuite)
                WriteChildResults(result);

            xmlWriter.WriteEndElement(); // test element
        }

        private void TerminateXmlFile()
        {
            xmlWriter.WriteEndElement(); // test-results
            xmlWriter.WriteEndDocument();
            xmlWriter.Flush();
            xmlWriter.Close();
        }


        #region Element Creation Helpers

        private void StartTestElement(ITestResult result)
        {
            ITest test = result.Test;
            TestSuite suite = test as TestSuite;

            if (suite != null)
            {
                xmlWriter.WriteStartElement("test-suite");
                xmlWriter.WriteAttributeString("type", suite.TestType);
                xmlWriter.WriteAttributeString("name", suite.TestType == "Assembly"
                    ? result.Test.FullName
                    : result.Test.Name);
            }
            else
            {
                xmlWriter.WriteStartElement("test-case");
                xmlWriter.WriteAttributeString("name", result.Name);
            }

            if (test.Properties.ContainsKey(PropertyNames.Description))
            {
                string description = (string)test.Properties.Get(PropertyNames.Description);
                xmlWriter.WriteAttributeString("description", description);
            }

            TestStatus status = result.ResultState.Status;
            string translatedResult = resultStates[result.ResultState.ToString()];

            if (status != TestStatus.Skipped)
            {
                xmlWriter.WriteAttributeString("executed", "True");
                xmlWriter.WriteAttributeString("result", translatedResult);
                xmlWriter.WriteAttributeString("success", status == TestStatus.Passed ? "True" : "False");
                xmlWriter.WriteAttributeString("time", result.Duration.TotalSeconds.ToString());
                xmlWriter.WriteAttributeString("asserts", result.AssertCount.ToString());
            }
            else
            {
                xmlWriter.WriteAttributeString("executed", "False");
                xmlWriter.WriteAttributeString("result", translatedResult);
            }
        }

        private void WriteCategories(ITestResult result)
        {
            IPropertyBag properties = result.Test.Properties;

            if (properties.ContainsKey(PropertyNames.Category))
            {
                xmlWriter.WriteStartElement("categories");

                foreach (string category in properties[PropertyNames.Category])
                {
                    xmlWriter.WriteStartElement("category");
                    xmlWriter.WriteAttributeString("name", category);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }
        }

        private void WriteProperties(ITestResult result)
        {
            IPropertyBag properties = result.Test.Properties;
            int nprops = 0;

            foreach (string key in properties.Keys)
            {
                if (key != PropertyNames.Category)
                {
                    if (nprops++ == 0)
                        xmlWriter.WriteStartElement("properties");

                    foreach (object prop in properties[key])
                    {
                        xmlWriter.WriteStartElement("property");
                        xmlWriter.WriteAttributeString("name", key);
                        xmlWriter.WriteAttributeString("value", prop.ToString());
                        xmlWriter.WriteEndElement();
                    }
                }
            }

            if (nprops > 0)
                xmlWriter.WriteEndElement();
        }

        private void WriteReasonElement(string message)
        {
            xmlWriter.WriteStartElement("reason");
            xmlWriter.WriteStartElement("message");
            xmlWriter.WriteCData(message);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        private void WriteFailureElement(string message, string stackTrace)
        {
            xmlWriter.WriteStartElement("failure");
            xmlWriter.WriteStartElement("message");
            WriteCData(message);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("stack-trace");
            if (stackTrace != null)
                WriteCData(stackTrace);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        private void WriteChildResults(ITestResult result)
        {
            xmlWriter.WriteStartElement("results");

            foreach (ITestResult childResult in result.Children)
                WriteResultElement(childResult);

            xmlWriter.WriteEndElement();
        }

        #endregion

        #region Output Helpers
        ///// <summary>
        ///// Makes string safe for xml parsing, replacing control chars with '?'
        ///// </summary>
        ///// <param name="encodedString">string to make safe</param>
        ///// <returns>xml safe string</returns>
        //private static string CharacterSafeString(string encodedString)
        //{
        //    /*The default code page for the system will be used.
        //    Since all code pages use the same lower 128 bytes, this should be sufficient
        //    for finding uprintable control characters that make the xslt processor error.
        //    We use characters encoded by the default code page to avoid mistaking bytes as
        //    individual characters on non-latin code pages.*/
        //    char[] encodedChars = System.Text.Encoding.Default.GetChars(System.Text.Encoding.Default.GetBytes(encodedString));

        //    System.Collections.ArrayList pos = new System.Collections.ArrayList();
        //    for (int x = 0; x < encodedChars.Length; x++)
        //    {
        //        char currentChar = encodedChars[x];
        //        //unprintable characters are below 0x20 in Unicode tables
        //        //some control characters are acceptable. (carriage return 0x0D, line feed 0x0A, horizontal tab 0x09)
        //        if (currentChar < 32 && (currentChar != 9 && currentChar != 10 && currentChar != 13))
        //        {
        //            //save the array index for later replacement.
        //            pos.Add(x);
        //        }
        //    }
        //    foreach (int index in pos)
        //    {
        //        encodedChars[index] = '?';//replace unprintable control characters with ?(3F)
        //    }
        //    return System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(encodedChars));
        //}

        private void WriteCData(string text)
        {
            int start = 0;
            while (true)
            {
                int illegal = text.IndexOf("]]>", start);
                if (illegal < 0)
                    break;
                xmlWriter.WriteCData(text.Substring(start, illegal - start + 2));
                start = illegal + 2;
                if (start >= text.Length)
                    return;
            }

            if (start > 0)
                xmlWriter.WriteCData(text.Substring(start));
            else
                xmlWriter.WriteCData(text);
        }

        #endregion
    }
}
