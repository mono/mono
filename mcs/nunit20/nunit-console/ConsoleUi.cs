#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright © 2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright © 2000-2003 Philip A. Craig
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
' Portions Copyright © 2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright © 2000-2003 Philip A. Craig
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
	using System.Collections;
	using System.Collections.Specialized;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Xml.Xsl;
	using System.Xml.XPath;
	using System.Resources;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Diagnostics;
	using NUnit.Core;
	using NUnit.Util;
	

	/// <summary>
	/// Summary description for ConsoleUi.
	/// </summary>
	public class ConsoleUi
	{
		private NUnit.Core.TestDomain testDomain;
		private XmlTextReader transformReader;
		private bool silent;
		private string xmlOutput;

		[STAThread]
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
				Console.Error.WriteLine("fatal error: no inputs specified");
				parser.Help();
			}
			else if(!parser.Validate())
			{
				Console.Error.WriteLine("fatal error: invalid arguments");
				parser.Help();
				returnCode = 2;
			}
			else
			{
				NUnit.Core.TestDomain domain = new NUnit.Core.TestDomain();

				try
				{
					Test test = MakeTestFromCommandLine(domain, parser);

					if(test == null)
					{
						Console.Error.WriteLine("fatal error: invalid assembly {0}", parser.Parameters[0]);
						returnCode = 2;
					}
					else
					{
						Directory.SetCurrentDirectory(new FileInfo((string)parser.Parameters[0]).DirectoryName);
						string xmlResult = "TestResult.xml";
						if(parser.IsXml)
							xmlResult = parser.xml;
				
						XmlTextReader reader = GetTransformReader(parser);
						if(reader != null)
						{
							ConsoleUi consoleUi = new ConsoleUi(domain, reader, parser.xmlConsole);
							returnCode = consoleUi.Execute();

							if (parser.xmlConsole)
								Console.WriteLine(consoleUi.XmlOutput);
							using (StreamWriter writer = new StreamWriter(xmlResult)) 
							{
								writer.Write(consoleUi.XmlOutput);
							}
						}
						else
							returnCode = 3;
					}
				}
				catch( Exception ex )
				{
					Console.WriteLine( "Unhandled Exception: {0}", ex.ToString() );
				}
				finally
				{
					domain.Unload();

					if(parser.wait)
					{
						Console.Out.WriteLine("\nHit <enter> key to continue");
						Console.ReadLine();
					}
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
					Console.Error.WriteLine("Transform file: {0} does not exist", xsltInfo.FullName);
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
			Console.WriteLine();
		}

		private static Test MakeTestFromCommandLine(NUnit.Core.TestDomain testDomain, 
			ConsoleOptions parser)
		{
			if(!DoAssembliesExist(parser.Parameters)) return null; 
			
			NUnitProject project;

			if ( parser.IsTestProject )
			{
				project = NUnitProject.LoadProject( (string)parser.Parameters[0] );
				string configName = (string) parser.config;
				if ( configName != null )
					project.SetActiveConfig( configName );
			}
			else
				project = NUnitProject.FromAssemblies( (string[])parser.Parameters.ToArray( typeof( string ) ) );

			return project.LoadTest( testDomain, parser.fixture );
		}

		private static bool DoAssembliesExist(IList files)
		{
			bool exist = true; 
			foreach(string fileName in files)
				exist &= DoesFileExist(fileName);
			return exist;
		}

		private static bool DoesFileExist(string fileName)
		{
			FileInfo fileInfo = new FileInfo(fileName);
			return fileInfo.Exists;
		}

		public ConsoleUi(NUnit.Core.TestDomain testDomain, XmlTextReader reader, bool silent)
		{
			this.testDomain = testDomain;
			transformReader = reader;
			this.silent = silent;
		}

		public string XmlOutput
		{
			get { return xmlOutput; }
		}

		public int Execute()
		{
			EventListener collector = null;
			if (silent)
				collector = new NullListener();
			else
				collector = new EventCollector();
			ConsoleWriter outStream = new ConsoleWriter(Console.Out);
			ConsoleWriter errorStream = new ConsoleWriter(Console.Error);
			
			string savedDirectory = Environment.CurrentDirectory;
			TestResult result = testDomain.Run(collector, outStream, errorStream);
			Directory.SetCurrentDirectory( savedDirectory );
			
			Console.WriteLine();

			StringBuilder builder = new StringBuilder();
			XmlResultVisitor resultVisitor = new XmlResultVisitor(new StringWriter( builder ), result);
			result.Accept(resultVisitor);
			resultVisitor.Write();

			xmlOutput = builder.ToString();

			if (!silent)
				CreateSummaryDocument();

			int resultCode = 0;
			if(result.IsFailure)
				resultCode = 1;
			return resultCode;
		}

		private void CreateSummaryDocument()
		{
			XPathDocument originalXPathDocument = new XPathDocument(new StringReader(xmlOutput));
			XslTransform summaryXslTransform = new XslTransform();
			summaryXslTransform.Load(transformReader);
			
			summaryXslTransform.Transform(originalXPathDocument,null,Console.Out);
		}

		private class EventCollector : LongLivingMarshalByRefObject, EventListener
		{
			private int level;
			private int testRunCount;
			private int testIgnoreCount;
			private int failureCount;
			StringCollection messages;
		
			private bool debugger = false;

			public EventCollector()
			{
				debugger = Debugger.IsAttached;
				level = 0;
			}

			public void TestFinished(TestCaseResult testResult)
			{
				if(testResult.Executed)
				{
					testRunCount++;
					if(testResult.IsFailure)
					{	
						failureCount++;
						Console.Write("F");
						if ( debugger )
							messages.Add( ParseTestCaseResult( testResult ) );
					}
				}
				else
				{
					testIgnoreCount++;
					Console.Write("N");
				}
			}

			public void TestStarted(TestCase testCase)
			{
				Console.Write(".");
			}

			public void SuiteStarted(TestSuite suite) 
			{
				if ( debugger && level++ == 0 )
				{
					messages = new StringCollection();
					testRunCount = 0;
					testIgnoreCount = 0;
					failureCount = 0;
					Debug.WriteLine( "################################ UNIT TESTS ################################" );
					Debug.WriteLine( "Running tests in '" + suite.FullName + "'..." );
				}
			}

			public void SuiteFinished(TestSuiteResult suiteResult) 
			{
				if ( debugger && --level == 0 ) 
				{
					Debug.WriteLine( "############################################################################" );

					if (messages.Count == 0) 
					{
						Debug.WriteLine( "##############                 S U C C E S S               #################" );
					}
					else 
					{
						Debug.WriteLine( "##############                F A I L U R E S              #################" );
						
						foreach ( string s in messages ) 
						{
							Debug.WriteLine(s);
						}
					}

					Debug.WriteLine( "############################################################################" );
					Debug.WriteLine( "Executed tests : " + testRunCount );
					Debug.WriteLine( "Ignored tests  : " + testIgnoreCount );
					Debug.WriteLine( "Failed tests   : " + failureCount );
					Debug.WriteLine( "Total time     : " + suiteResult.Time + " seconds" );
					Debug.WriteLine( "############################################################################");
				}
			}

			private string ParseTestCaseResult( TestCaseResult result ) 
			{
				string[] trace = result.StackTrace.Split( System.Environment.NewLine.ToCharArray() );
			
				foreach (string s in trace) 
				{
					if ( s.IndexOf( result.Test.FullName ) >= 0 ) 
					{
						string link = Regex.Replace( s.Trim(), @"at " + result.Test.FullName + @"\(\) in (.*):line (.*)", "$1($2)");

						string message = string.Format("{1}: {0}", 
							result.Message.Replace(System.Environment.NewLine, "; "), 
							result.Test.FullName).Trim(' ', ':');
					
						return string.Format("{0}: {1}", link, message);
					}
				}

				return result.Message;
			}
		}
	}
}
