using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
#if XUNIT_RUNNER
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Xml.Linq;
#else
using NUnitLite.Runner;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using Mono.Security.Interface;
#endif

class TcpWriter : TextWriter
{
	private static object locker = new object ();
	private string hostName;
	private int port;

	private TcpClient client;
	private NetworkStream stream;
	private StreamWriter writer;

	public TcpWriter (string hostName, int port)
	{
		this.hostName = hostName;
		this.port = port;
		this.client = new TcpClient (hostName, port);
		this.stream = client.GetStream ();
		this.writer = new StreamWriter (stream);
	}

	public NetworkStream RawStream { get { return stream; } }

	public override void Write (char value)
	{
		lock (locker)
			writer.Write (value);
	}

	public override void Write (string value)
	{
		lock (locker)
			writer.Write (value);
	}

	public override void WriteLine (string value)
	{
		lock (locker) {
			writer.WriteLine (value);
			writer.Flush ();
		}
	}

	public override System.Text.Encoding Encoding
	{
		get { return System.Text.Encoding.Default; }
	}
}

#if XUNIT_RUNNER
class DiagnosticTextWriterMessageSink : LongLivedMarshalByRefObject, IMessageSink
{
	TextWriter writer;

	public DiagnosticTextWriterMessageSink(TextWriter wr)
	{
		writer = wr;
	}

	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IDiagnosticMessage diagnosticMessage)
			writer.WriteLine (diagnosticMessage.Message);

		return true;
	}
}

class XunitArgumentsParser
{
	public static XunitFilters ParseArgumentsToFilter (Stack<string> arguments)
	{
		var filters = new XunitFilters ();

		while (arguments.Count > 0)
		{
			var option = arguments.Pop ();
			if (option.StartsWith ("@")) {  // response file handling
				var fileName = option.Substring (1);
				var fileContent = File.ReadAllLines (fileName);
				foreach (var line in fileContent)
				{
					if (line.StartsWith ("#") || String.IsNullOrWhiteSpace (line)) continue;

					var parts = line.Split (" ", StringSplitOptions.RemoveEmptyEntries);
					for (var i = parts.Length - 1; i >= 0; i--)
						arguments.Push (parts[i]);
				}
				continue;
			}

			switch (option)
			{
				case "-nomethod": filters.ExcludedMethods.Add (arguments.Pop ()); break;
				case "-noclass": filters.ExcludedClasses.Add (arguments.Pop ()); break;
				case "-nonamespace": filters.ExcludedNamespaces.Add (arguments.Pop ()); break;
				case "-notrait": ParseEqualSeparatedArgument (filters.ExcludedTraits, arguments.Pop ()); break;
				default: throw new ArgumentException ($"Not supported option: '{option}'");
			};
		}

		return filters;
	}

	static void ParseEqualSeparatedArgument (Dictionary<string, List<string>> targetDictionary, string argument)
	{
		var parts = argument.Split ('=');
		if (parts.Length != 2 || string.IsNullOrEmpty (parts[0]) || string.IsNullOrEmpty (parts[1]))
			throw new ArgumentException (argument);

		var name = parts[0];
		var value = parts[1];
		if (targetDictionary.TryGetValue (name, out List<string> excludedTraits)) {
			excludedTraits.Add (value);
		} else {
			targetDictionary[name] = new List<string> { value };
		}
	}
}
#else
class MonoSdksTextUI : TextUI,ITestListener
{
	public MonoSdksTextUI () : base ()
	{
	}

	public MonoSdksTextUI (TextWriter writer) : base (writer)
	{
	}

	void ITestListener.TestFinished(ITestResult result)
	{
		if (!result.Test.IsSuite) {
			switch (result.ResultState.Status)
			{
				case TestStatus.Passed: Interop.mono_sdks_ui_increment_testcase_result (0); break;
				case TestStatus.Inconclusive:
				case TestStatus.Skipped: Interop.mono_sdks_ui_increment_testcase_result (1); break;
				case TestStatus.Failed: Interop.mono_sdks_ui_increment_testcase_result (2); break;
			}
		}
	}
}
#endif

class Interop
{
	[System.Runtime.InteropServices.DllImport ("__Internal")]
	public extern static void mono_sdks_ui_increment_testcase_result (int resultType);

	[System.Runtime.InteropServices.DllImport ("__Internal")]
	public extern static void mono_sdks_ui_set_test_summary_message (string summaryMessage);
}

public class TestRunner
{
	public static int Main(string[] args) {
		var arguments = new Stack<string> ();
		string host = null;
		int port = 0;
		bool closeAfterTestRun;
		bool failed;

		for (var i = args.Length - 1; i >= 0; i--)
			arguments.Push (args[i]);

		// First argument is the connection string if we're driven by harness.exe, otherwise we're driven by UITests
		if (arguments.Count > 0 && arguments.Peek ().StartsWith("tcp:", StringComparison.Ordinal)) {
			var parts = arguments.Pop ().Split (':');
			if (parts.Length != 3)
				throw new Exception ();
			host = parts [1];
			port = Int32.Parse (parts [2]);
			closeAfterTestRun = true;
		} else {
			closeAfterTestRun = false;
		}

#if !ENABLE_NETCORE
		// Make sure the TLS subsystem including the DependencyInjector is initialized.
		// This would normally happen on system startup in
		// `xamarin-macios/src/ObjcRuntime/Runtime.cs`.
		MonoTlsProviderFactory.Initialize ();
#endif

		// some tests assert having a SynchronizationContext for MONOTOUCH, provide a default one
		SynchronizationContext.SetSynchronizationContext (new SynchronizationContext ());

#if XUNIT_RUNNER
		var writer = new TcpWriter (host, port);
		var assemblyFileName = arguments.Pop ();
		var filters = XunitArgumentsParser.ParseArgumentsToFilter (arguments);
		var configuration = new TestAssemblyConfiguration () { ShadowCopy = false };
		var discoveryOptions = TestFrameworkOptions.ForDiscovery (configuration);
		var discoverySink = new TestDiscoverySink ();
		var diagnosticSink = new DiagnosticTextWriterMessageSink (writer);
		var testOptions = TestFrameworkOptions.ForExecution (configuration);
		var testSink = new TestMessageSink ();
		var controller = new XunitFrontController (AppDomainSupport.Denied, assemblyFileName, configFileName: null, shadowCopy: false, diagnosticMessageSink: diagnosticSink);

		Interop.mono_sdks_ui_set_test_summary_message ($"Running {assemblyFileName}...");

		writer.WriteLine ($"Discovering tests for {assemblyFileName}");
		controller.Find (includeSourceInformation: false, discoverySink, discoveryOptions);
		discoverySink.Finished.WaitOne ();
		var testCasesToRun = discoverySink.TestCases.Where (filters.Filter).ToList ();
		writer.WriteLine ($"Discovery finished.");

		var summarySink = new DelegatingExecutionSummarySink (testSink, () => false, (completed, summary) => { writer.WriteLine ($"Tests run: {summary.Total}, Errors: 0, Failures: {summary.Failed}, Skipped: {summary.Skipped}{Environment.NewLine}Time: {TimeSpan.FromSeconds ((double)summary.Time).TotalSeconds}s"); });
		var resultsXmlAssembly = new XElement ("assembly");
		var resultsSink = new DelegatingXmlCreationSink (summarySink, resultsXmlAssembly);

		testSink.Execution.TestPassedEvent  += args => { writer.WriteLine ($"[PASS] {args.Message.Test.DisplayName}"); Interop.mono_sdks_ui_increment_testcase_result (0); };
		testSink.Execution.TestSkippedEvent += args => { writer.WriteLine ($"[SKIP] {args.Message.Test.DisplayName}"); Interop.mono_sdks_ui_increment_testcase_result (1); };
		testSink.Execution.TestFailedEvent  += args => { writer.WriteLine ($"[FAIL] {args.Message.Test.DisplayName}{Environment.NewLine}{ExceptionUtility.CombineMessages (args.Message)}{Environment.NewLine}{ExceptionUtility.CombineStackTraces (args.Message)}"); Interop.mono_sdks_ui_increment_testcase_result (2); };

		testSink.Execution.TestAssemblyStartingEvent += args => { writer.WriteLine ($"Running tests for {args.Message.TestAssembly.Assembly}"); };
		testSink.Execution.TestAssemblyFinishedEvent += args => { writer.WriteLine ($"Finished {args.Message.TestAssembly.Assembly}{Environment.NewLine}"); };

		controller.RunTests (testCasesToRun, resultsSink, testOptions);
		resultsSink.Finished.WaitOne ();

		var resultsXml = new XElement ("assemblies");
		resultsXml.Add (resultsXmlAssembly);
		resultsXml.Save (resultsXmlPath);

		if (host != null) {
			writer.WriteLine ($"STARTRESULTXML");
			resultsXml.Save (((TcpWriter)writer).RawStream);
			writer.WriteLine ();
			writer.WriteLine ($"ENDRESULTXML");
		}

		failed = resultsSink.ExecutionSummary.Failed > 0 || resultsSink.ExecutionSummary.Errors > 0;
#else
		MonoSdksTextUI runner;
		TextWriter writer = null;
		string resultsXmlPath = Path.GetTempFileName ();
		string assemblyFileName = arguments.Peek ();

		if (File.Exists ("nunit-excludes.txt")) {
			var excludes = File.ReadAllLines ("nunit-excludes.txt");
			arguments.Push ("-exclude:" + String.Join (",", excludes));
		}

		arguments.Push ("-labels");
		arguments.Push ("-format:xunit");
		arguments.Push ($"-result:{resultsXmlPath}");

		if (host != null) {
			Console.WriteLine ($"Connecting to harness at {host}:{port}.");
			writer = new TcpWriter (host, port);
		} else {
			writer = ConsoleWriter.Out;
		}

		Interop.mono_sdks_ui_set_test_summary_message ($"Running {assemblyFileName}...");

		runner = new MonoSdksTextUI (writer);
		runner.Execute (arguments.ToArray ());

		if (host != null) {
			writer.WriteLine ($"STARTRESULTXML");
			using (var resultsXmlStream = File.OpenRead (resultsXmlPath)) resultsXmlStream.CopyTo (((TcpWriter)writer).RawStream);
			writer.WriteLine ();
			writer.WriteLine ($"ENDRESULTXML");
		}

		failed = runner.Failure;
#endif

		Interop.mono_sdks_ui_set_test_summary_message ($"Summary: {(failed ? "Failed" : "Succeeded")} for {assemblyFileName}.");

		if (!closeAfterTestRun) {
			Thread.Sleep (Int32.MaxValue);
		}

		return failed ? 1 : 0;
	}
}
