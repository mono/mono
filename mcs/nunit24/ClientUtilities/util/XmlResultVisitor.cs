// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Util
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Xml;
	using System.Reflection;
	using NUnit.Core;

	/// <summary>
	/// Summary description for XmlResultVisitor.
	/// </summary>
	public class XmlResultVisitor : ResultVisitor
	{
		private XmlTextWriter xmlWriter;
		private TextWriter writer;
		private MemoryStream memoryStream;

		public XmlResultVisitor(string fileName, TestResult result)
		{
			xmlWriter = new XmlTextWriter( new StreamWriter(fileName, false, System.Text.Encoding.UTF8) );
			Initialize(result);
		}

        public XmlResultVisitor( TextWriter writer, TestResult result )
		{
			this.memoryStream = new MemoryStream();
			this.writer = writer;
			this.xmlWriter = new XmlTextWriter( new StreamWriter( memoryStream, System.Text.Encoding.UTF8 ) );
			Initialize( result );
		}

		private void Initialize(TestResult result) 
		{
			ResultSummarizer summaryResults = new ResultSummarizer(result);

			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.WriteStartDocument(false);
			xmlWriter.WriteComment("This file represents the results of running a test suite");

			xmlWriter.WriteStartElement("test-results");

			xmlWriter.WriteAttributeString("name", summaryResults.Name);
			xmlWriter.WriteAttributeString("total", summaryResults.ResultCount.ToString());
			xmlWriter.WriteAttributeString("failures", summaryResults.FailureCount.ToString());
			xmlWriter.WriteAttributeString("not-run", summaryResults.TestsNotRun.ToString());

			DateTime now = DateTime.Now;
			xmlWriter.WriteAttributeString("date", XmlConvert.ToString( now, "yyyy-MM-dd" ) );
			xmlWriter.WriteAttributeString("time", XmlConvert.ToString( now, "HH:mm:ss" ));
			WriteEnvironment();
			WriteCultureInfo();
		}

		private void WriteCultureInfo() {
			xmlWriter.WriteStartElement("culture-info");
			xmlWriter.WriteAttributeString("current-culture",
			                               CultureInfo.CurrentCulture.ToString());
			xmlWriter.WriteAttributeString("current-uiculture",
			                               CultureInfo.CurrentUICulture.ToString());
			xmlWriter.WriteEndElement();
		}

		private void WriteEnvironment() {
			xmlWriter.WriteStartElement("environment");
			xmlWriter.WriteAttributeString("nunit-version", 
										   Assembly.GetExecutingAssembly().GetName().Version.ToString());
			xmlWriter.WriteAttributeString("clr-version", 
			                               Environment.Version.ToString());
			xmlWriter.WriteAttributeString("os-version",
			                               Environment.OSVersion.ToString());
			xmlWriter.WriteAttributeString("platform",
				Environment.OSVersion.Platform.ToString());
			xmlWriter.WriteAttributeString("cwd",
			                               Environment.CurrentDirectory);
			xmlWriter.WriteAttributeString("machine-name",
			                               Environment.MachineName);
			xmlWriter.WriteAttributeString("user",
			                               Environment.UserName);
			xmlWriter.WriteAttributeString("user-domain",
			                               Environment.UserDomainName);
			xmlWriter.WriteEndElement();
		}

		public void Visit(TestCaseResult caseResult) 
		{
			xmlWriter.WriteStartElement("test-case");
			xmlWriter.WriteAttributeString("name",caseResult.Name);

			if(caseResult.Description != null)
				xmlWriter.WriteAttributeString("description", caseResult.Description);

			xmlWriter.WriteAttributeString("executed", caseResult.Executed.ToString());
			if(caseResult.Executed)
			{
				xmlWriter.WriteAttributeString("success", caseResult.IsSuccess.ToString() );

				xmlWriter.WriteAttributeString("time", caseResult.Time.ToString("#####0.000", NumberFormatInfo.InvariantInfo));

				xmlWriter.WriteAttributeString("asserts", caseResult.AssertCount.ToString() );
				WriteCategories(caseResult);
				WriteProperties(caseResult);
				if(caseResult.IsFailure)
				{
					if(caseResult.IsFailure)
						xmlWriter.WriteStartElement("failure");
					else
						xmlWriter.WriteStartElement("error");
				
					xmlWriter.WriteStartElement("message");
					xmlWriter.WriteCData( EncodeCData( caseResult.Message ) );
					xmlWriter.WriteEndElement();
				
					xmlWriter.WriteStartElement("stack-trace");
					if(caseResult.StackTrace != null)
						xmlWriter.WriteCData( EncodeCData( StackTraceFilter.Filter( caseResult.StackTrace ) ) );
					xmlWriter.WriteEndElement();
				
					xmlWriter.WriteEndElement();
				}
				
			}
			else
			{
				WriteCategories(caseResult);
				WriteProperties(caseResult);
				xmlWriter.WriteStartElement("reason");
				xmlWriter.WriteStartElement("message");
				xmlWriter.WriteCData(caseResult.Message);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}
            
			xmlWriter.WriteEndElement();
		}

		/// <summary>
		/// Makes string safe for xml parsing, replacing control chars with '?'
		/// </summary>
		/// <param name="encodedString">string to make safe</param>
		/// <returns>xml safe string</returns>
		private static string CharacterSafeString(string encodedString)
		{
			/*The default code page for the system will be used.
			Since all code pages use the same lower 128 bytes, this should be sufficient
			for finding uprintable control characters that make the xslt processor error.
			We use characters encoded by the default code page to avoid mistaking bytes as
			individual characters on non-latin code pages.*/
			char[] encodedChars = System.Text.Encoding.Default.GetChars(System.Text.Encoding.Default.GetBytes(encodedString));
			
			System.Collections.ArrayList pos = new System.Collections.ArrayList();
			for(int x = 0 ; x < encodedChars.Length ; x++)
			{
				char currentChar = encodedChars[x];
				//unprintable characters are below 0x20 in Unicode tables
				//some control characters are acceptable. (carriage return 0x0D, line feed 0x0A, horizontal tab 0x09)
				if(currentChar < 32 && (currentChar != 9 && currentChar != 10 && currentChar != 13))
				{
					//save the array index for later replacement.
					pos.Add(x);
				}
			}
			foreach(int index in pos)
			{
				encodedChars[index] = '?';//replace unprintable control characters with ?(3F)
			}
			return System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(encodedChars));
		}

		private string EncodeCData( string text )
		{
			return CharacterSafeString( text ).Replace( "]]>", "]]&gt;" );
		}

		public void WriteCategories(TestResult result)
		{
			if (result.Test.Categories != null && result.Test.Categories.Count > 0)
			{
				xmlWriter.WriteStartElement("categories");
				foreach (string category in result.Test.Categories)
				{
					xmlWriter.WriteStartElement("category");
					xmlWriter.WriteAttributeString("name", category);
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
		}

		public void WriteProperties(TestResult result)
		{
			if (result.Test.Properties != null && result.Test.Properties.Count > 0)
			{
				xmlWriter.WriteStartElement("properties");
				foreach (string key in result.Test.Properties.Keys)
				{
					xmlWriter.WriteStartElement("property");
					xmlWriter.WriteAttributeString("name", key);
					xmlWriter.WriteAttributeString("value", result.Test.Properties[key].ToString() );
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
			}
		}

		public void Visit(TestSuiteResult suiteResult) 
		{
			xmlWriter.WriteStartElement("test-suite");
			xmlWriter.WriteAttributeString("name",suiteResult.Name);
			if(suiteResult.Description != null)
				xmlWriter.WriteAttributeString("description", suiteResult.Description);

			xmlWriter.WriteAttributeString("success", suiteResult.IsSuccess.ToString());
			xmlWriter.WriteAttributeString("time", suiteResult.Time.ToString("#####0.000", NumberFormatInfo.InvariantInfo));
			xmlWriter.WriteAttributeString("asserts", suiteResult.AssertCount.ToString() );
         
			WriteCategories(suiteResult);
			WriteProperties(suiteResult);

			if ( suiteResult.IsFailure && suiteResult.FailureSite == FailureSite.SetUp )
			{
				xmlWriter.WriteStartElement("failure");

				xmlWriter.WriteStartElement("message");
				xmlWriter.WriteCData( EncodeCData( suiteResult.Message ) );
				xmlWriter.WriteEndElement();

				xmlWriter.WriteStartElement("stack-trace");
				if(suiteResult.StackTrace != null)
					xmlWriter.WriteCData( EncodeCData( StackTraceFilter.Filter( suiteResult.StackTrace ) ) );
				xmlWriter.WriteEndElement();

				xmlWriter.WriteEndElement();
			}

			xmlWriter.WriteStartElement("results");                  
			foreach (TestResult result in suiteResult.Results)
			{
				result.Accept(this);
			}
			xmlWriter.WriteEndElement();

			xmlWriter.WriteEndElement();
		}

		public void Write()
		{
			try 
			{
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
				xmlWriter.Flush();

				if ( memoryStream != null && writer != null )
				{
					memoryStream.Position = 0;
					using ( StreamReader rdr = new StreamReader( memoryStream ) )
					{
						writer.Write( rdr.ReadToEnd() );
					}
				}

				xmlWriter.Close();
			} 
			finally 
			{
				//writer.Close();
			}
		}
	}
}
