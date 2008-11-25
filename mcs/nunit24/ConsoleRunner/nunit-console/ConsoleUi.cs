// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.ConsoleRunner
{
	using System;
	using System.IO;
	using System.Reflection;
	using System.Xml;
	using System.Resources;
	using System.Text;
	using NUnit.Core;
	using NUnit.Core.Filters;
	using NUnit.Util;
	
	/// <summary>
	/// Summary description for ConsoleUi.
	/// </summary>
	public class ConsoleUi
	{
		public static readonly int OK = 0;
		public static readonly int INVALID_ARG = -1;
		public static readonly int FILE_NOT_FOUND = -2;
		public static readonly int FIXTURE_NOT_FOUND = -3;
		public static readonly int TRANSFORM_ERROR = -4;
		public static readonly int UNEXPECTED_ERROR = -100;

		public ConsoleUi()
		{
		}

		public int Execute( ConsoleOptions options )
		{
			XmlTextReader transformReader = GetTransformReader(options);
			if(transformReader == null) return FILE_NOT_FOUND;

			TextWriter outWriter = Console.Out;
			bool redirectOutput = options.output != null && options.output != string.Empty;
			if ( redirectOutput )
			{
				StreamWriter outStreamWriter = new StreamWriter( options.output );
				outStreamWriter.AutoFlush = true;
				outWriter = outStreamWriter;
			}

			TextWriter errorWriter = Console.Error;
			bool redirectError = options.err != null && options.err != string.Empty;
			if ( redirectError )
			{
				StreamWriter errorStreamWriter = new StreamWriter( options.err );
				errorStreamWriter.AutoFlush = true;
				errorWriter = errorStreamWriter;
			}

			TestRunner testRunner = MakeRunnerFromCommandLine( options );

			try
			{
				if (testRunner.Test == null)
				{
					testRunner.Unload();
					Console.Error.WriteLine("Unable to locate fixture {0}", options.fixture);
					return FIXTURE_NOT_FOUND;
				}

				EventCollector collector = new EventCollector( options, outWriter, errorWriter );

				TestFilter testFilter = TestFilter.Empty;
				if ( options.run != null && options.run != string.Empty )
				{
					Console.WriteLine( "Selected test: " + options.run );
					testFilter = new SimpleNameFilter( options.run );
				}

				if ( options.include != null && options.include != string.Empty )
				{
					Console.WriteLine( "Included categories: " + options.include );
					TestFilter includeFilter = new CategoryExpression( options.include ).Filter;
					if ( testFilter.IsEmpty )
						testFilter = includeFilter;
					else
						testFilter = new AndFilter( testFilter, includeFilter );
				}

				if ( options.exclude != null && options.exclude != string.Empty )
				{
					Console.WriteLine( "Excluded categories: " + options.exclude );
					TestFilter excludeFilter = new NotFilter( new CategoryExpression( options.exclude ).Filter );
					if ( testFilter.IsEmpty )
						testFilter = excludeFilter;
					else if ( testFilter is AndFilter )
						((AndFilter)testFilter).Add( excludeFilter );
					else
						testFilter = new AndFilter( testFilter, excludeFilter );
				}

				TestResult result = null;
				string savedDirectory = Environment.CurrentDirectory;
				TextWriter savedOut = Console.Out;
				TextWriter savedError = Console.Error;

				try
				{
					result = testRunner.Run( collector, testFilter );
				}
				finally
				{
					outWriter.Flush();
					errorWriter.Flush();

					if ( redirectOutput )
						outWriter.Close();
					if ( redirectError )
						errorWriter.Close();

					Environment.CurrentDirectory = savedDirectory;
					Console.SetOut( savedOut );
					Console.SetError( savedError );
				}

				Console.WriteLine();

				string xmlOutput = CreateXmlOutput( result );
			
				if (options.xmlConsole)
				{
					Console.WriteLine(xmlOutput);
				}
				else
				{
					try
					{
						//CreateSummaryDocument(xmlOutput, transformReader );
						XmlResultTransform xform = new XmlResultTransform( transformReader );
						xform.Transform( new StringReader( xmlOutput ), Console.Out );
					}
					catch( Exception ex )
					{
						Console.WriteLine( "Error: {0}", ex.Message );
						return TRANSFORM_ERROR;
					}
				}

				// Write xml output here
				string xmlResultFile = options.xml == null || options.xml == string.Empty
					? "TestResult.xml" : options.xml;

				using ( StreamWriter writer = new StreamWriter( xmlResultFile ) ) 
				{
					writer.Write(xmlOutput);
				}

				//if ( testRunner != null )
				//    testRunner.Unload();

				if ( collector.HasExceptions )
				{
					collector.WriteExceptions();
					return UNEXPECTED_ERROR;
				}
            
				if ( !result.IsFailure ) return OK;

				ResultSummarizer summ = new ResultSummarizer( result );
				return summ.FailureCount;
			}
			finally
			{
				testRunner.Unload();
			}
		}

		#region Helper Methods
		private static XmlTextReader GetTransformReader(ConsoleOptions parser)
		{
			XmlTextReader reader = null;
			if(parser.transform == null || parser.transform == string.Empty)
			{
				Assembly assembly = Assembly.GetAssembly(typeof(XmlResultVisitor));
				ResourceManager resourceManager = new ResourceManager("NUnit.Util.Transform",assembly);
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

		private static TestRunner MakeRunnerFromCommandLine( ConsoleOptions options )
		{
			TestPackage package;
			ConsoleOptions.DomainUsage domainUsage = ConsoleOptions.DomainUsage.Default;

			if (options.IsTestProject)
			{
				NUnitProject project = NUnitProject.LoadProject((string)options.Parameters[0]);
				string configName = options.config;
				if (configName != null)
					project.SetActiveConfig(configName);

				package = project.ActiveConfig.MakeTestPackage();
				package.TestName = options.fixture;

				domainUsage = ConsoleOptions.DomainUsage.Single;
			}
			else if (options.Parameters.Count == 1)
			{
				package = new TestPackage((string)options.Parameters[0]);
				domainUsage = ConsoleOptions.DomainUsage.Single;
			}
			else
			{
				package = new TestPackage("UNNAMED", options.Parameters);
				domainUsage = ConsoleOptions.DomainUsage.Multiple;
			}

			if (options.domain != ConsoleOptions.DomainUsage.Default)
				domainUsage = options.domain;
                    
			TestRunner testRunner = null;
				
			switch( domainUsage )
			{
				case ConsoleOptions.DomainUsage.None:
					testRunner = new NUnit.Core.RemoteTestRunner();
					// Make sure that addins are available
					CoreExtensions.Host.AddinRegistry = Services.AddinRegistry;
					break;

				case ConsoleOptions.DomainUsage.Single:
					testRunner = new TestDomain();
					break;

				case ConsoleOptions.DomainUsage.Multiple:
					testRunner = new MultipleTestDomainRunner();
					break;
			}

			package.TestName = options.fixture;
			package.Settings["ShadowCopyFiles"] = !options.noshadow;
			package.Settings["UseThreadedRunner"] = !options.nothread;
			testRunner.Load( package );

			return testRunner;
		}

		private static string CreateXmlOutput( TestResult result )
		{
			StringBuilder builder = new StringBuilder();
			XmlResultVisitor resultVisitor = new XmlResultVisitor(new StringWriter( builder ), result);
			result.Accept(resultVisitor);
			resultVisitor.Write();

			return builder.ToString();
		}
		#endregion
	}
}

