#region Copyright (c) 2002, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov
' Copyright © 2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright © 2002 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov 
' or Copyright © 2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

namespace NUnit.Console
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Xsl;
	using System.Xml.XPath;
	using System.Resources;
	using System.Text;
	using NUnit.Core;
	using NUnit.Util;
	

	/// <summary>
	/// Summary description for ConsoleUi.
	/// </summary>
	public class ConsoleUi
	{
		private NUnit.Framework.TestDomain testDomain;
		private string outputFile;
		private XmlTextReader transformReader;

		public static int Main(string[] args)
		{
			int returnCode = 0;

			ConsoleOptions parser = new ConsoleOptions(args);
			if(!parser.nologo)
				WriteCopyright();

			if(parser.help)
			{
				parser.Help();
			}
			else if(parser.NoArgs) 
			{
				Console.Error.WriteLine("\nfatal error: no inputs specified");
				parser.Help();
			}
			else if(!parser.Validate())
			{
				Console.Error.WriteLine("\nfatal error: invalid arguments");
				parser.Help();
				returnCode = 2;
			}
			else
			{
				ConsoleWriter outStream = new ConsoleWriter(Console.Out);
				ConsoleWriter errorStream = new ConsoleWriter(Console.Error);
				NUnit.Framework.TestDomain domain = new NUnit.Framework.TestDomain(outStream, errorStream);

				Test test = MakeTestFromCommandLine(domain, parser);
				try
				{
					if(test == null)
					{
						Console.Error.WriteLine("\nfatal error: invalid assembly {0}", parser.Assembly);
						returnCode = 2;
					}
					else
					{
						Directory.SetCurrentDirectory(new FileInfo(parser.Assembly).DirectoryName);
						string xmlResult = "TestResult.xml";
						if(parser.IsXml)
							xmlResult = parser.xml;

						XmlTextReader reader = GetTransformReader(parser);
						if(reader != null)
						{
							ConsoleUi consoleUi = new ConsoleUi(domain, xmlResult, reader);
							returnCode = consoleUi.Execute();
							if(parser.wait)
							{
								Console.Out.WriteLine("Hit <enter> key to continue");
								Console.ReadLine();
							}
						}
						else
							returnCode = 3;
					}
				}
				finally
				{
					domain.Unload();
				}
			}


			return returnCode;
		}

		private static XmlTextReader GetTransformReader(ConsoleOptions parser)
		{
			XmlTextReader reader = null;
			if(!parser.IsTransform)
			{
				Assembly assembly = Assembly.GetAssembly(typeof(XmlResultVisitor));
				ResourceManager resourceManager = new ResourceManager("NUnit.Framework.Transform",assembly);
				string xmlData = (string)resourceManager.GetObject("Summary.xslt");

				reader = new XmlTextReader(new StringReader(xmlData));
			}
			else
			{
				FileInfo xsltInfo = new FileInfo(parser.transform);
				if(!xsltInfo.Exists)
				{
					Console.Error.WriteLine("\nTransform file: {0} does not exist", xsltInfo.FullName);
					reader = null;
				}
				else
				{
					reader = new XmlTextReader(xsltInfo.FullName);
				}
			}

			return reader;
		}

		private static void WriteCopyright()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			System.Version version = executingAssembly.GetName().Version;

			object[] objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
			AssemblyProductAttribute productAttr = (AssemblyProductAttribute)objectAttrs[0];

			objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			AssemblyCopyrightAttribute copyrightAttr = (AssemblyCopyrightAttribute)objectAttrs[0];

			Console.WriteLine(String.Format("{0} version {1}", productAttr.Product, version.ToString(3)));
			Console.WriteLine(copyrightAttr.Copyright);
		}

		private static Test MakeTestFromCommandLine(NUnit.Framework.TestDomain testDomain, 
			ConsoleOptions parser)
		{
			Test test = null;

			if(!DoesFileExist(parser.Assembly)) return null; 
			
			if(parser.IsAssembly)
			{
				test = testDomain.Load(parser.Assembly);
				if(test == null) Console.WriteLine("\nfatal error: assembly ({0}) is invalid", parser.Assembly);
			}
			else if(parser.IsFixture)
			{
				test = testDomain.Load(parser.fixture, parser.Assembly);
				if(test == null) Console.WriteLine("\nfatal error: fixture ({0}) in assembly ({1}) is invalid", parser.fixture, parser.Assembly);
			}
			return test;
		}

		private static bool DoesFileExist(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			return fileInfo.Exists;
		}

		private static void WriteHelp(TextWriter writer)
		{
			writer.WriteLine("\n\n         NUnit console options\n");
			writer.WriteLine("/assembly:<assembly name>                            Assembly to test");
			writer.WriteLine("/fixture:<class name> /assembly:<assembly name>      Fixture or Suite to run");
			writer.WriteLine("\n\n         XML formatting options");
			writer.WriteLine("/xml:<file>                 XML result file to generate");
			writer.WriteLine("/transform:<file>           XSL transform file");
		}

		public ConsoleUi(NUnit.Framework.TestDomain testDomain, string xmlFile, XmlTextReader reader)
		{
			this.testDomain = testDomain;
			outputFile = xmlFile;
			transformReader = reader;
		}

		public int Execute()
		{
			EventCollector collector = new EventCollector();
			TestResult result = testDomain.Run(collector);

			Console.WriteLine("\n");
			XmlResultVisitor resultVisitor = new XmlResultVisitor(outputFile, result);
			result.Accept(resultVisitor);
			resultVisitor.Write();
			CreateSummaryDocument();

			int resultCode = 0;
			if(result.IsFailure)
				resultCode = 1;
			return resultCode;
		}

		private void CreateSummaryDocument()
		{
			XPathDocument originalXPathDocument = new XPathDocument (outputFile);
			XslTransform summaryXslTransform = new XslTransform();
			summaryXslTransform.Load(transformReader);
			
			summaryXslTransform.Transform(originalXPathDocument,null,Console.Out);
		}

		private class EventCollector : LongLivingMarshalByRefObject, EventListener
		{
			public void TestFinished(TestCaseResult testResult)
			{
				if(testResult.Executed)
				{
					if(testResult.IsFailure)
					{	
						Console.Write("F");
					}
				}
				else
					Console.Write("N");
			}

			public void TestStarted(TestCase testCase)
			{
				Console.Write(".");
			}

			public void SuiteStarted(TestSuite suite) {}
			public void SuiteFinished(TestSuiteResult result) {}
		}

	}
}
