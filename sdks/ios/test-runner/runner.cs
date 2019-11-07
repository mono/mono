using System;
using System.IO;
using System.Linq;
using System.Text;
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
}

public class TestRunner
{
	public static int Main(string[] args) {
		string host = null;
		int port = 0;

		// First argument is the connection string
		if (args [0].StartsWith ("tcp:")) {
			var parts = args [0].Split (':');
			if (parts.Length != 3)
				throw new Exception ();
			host = parts [1];
			port = Int32.Parse (parts [2]);
			args = args.Skip (1).ToArray ();
		}

		// Make sure the TLS subsystem including the DependencyInjector is initialized.
		// This would normally happen on system startup in
		// `xamarin-macios/src/ObjcRuntime/Runtime.cs`.
		MonoTlsProviderFactory.Initialize ();

#if XUNIT_RUNNER
		var writer = new TcpWriter (host, port);
		var assemblyFileName = args[0];
		var configuration = new TestAssemblyConfiguration () { ShadowCopy = false };
		var discoveryOptions = TestFrameworkOptions.ForDiscovery (configuration);
		var discoverySink = new TestDiscoverySink ();
		var diagnosticSink = new DiagnosticTextWriterMessageSink (writer);
		var testOptions = TestFrameworkOptions.ForExecution (configuration);
		var testSink = new TestMessageSink ();
		var controller = new XunitFrontController (AppDomainSupport.Denied, assemblyFileName, configFileName: null, shadowCopy: false, diagnosticMessageSink: diagnosticSink);

		writer.WriteLine ($"Discovering tests for {assemblyFileName}");
		controller.Find (includeSourceInformation: false, discoverySink, discoveryOptions);
		discoverySink.Finished.WaitOne ();
		writer.WriteLine ($"Discovery finished.");

		var summarySink = new DelegatingExecutionSummarySink (testSink, () => false, (completed, summary) => { writer.WriteLine ($"Tests run: {summary.Total}, Errors: 0, Failures: {summary.Failed}, Skipped: {summary.Skipped}{Environment.NewLine}Time: {TimeSpan.FromSeconds ((double)summary.Time).TotalSeconds}s"); });
		var resultsXmlAssembly = new XElement ("assembly");
		var resultsSink = new DelegatingXmlCreationSink (summarySink, resultsXmlAssembly);

		testSink.Execution.TestPassedEvent  += args => { writer.WriteLine ($"[PASS] {args.Message.Test.DisplayName}"); Interop.mono_sdks_ui_increment_testcase_result (0); };
		testSink.Execution.TestSkippedEvent += args => { writer.WriteLine ($"[SKIP] {args.Message.Test.DisplayName}"); Interop.mono_sdks_ui_increment_testcase_result (1); };
		testSink.Execution.TestFailedEvent  += args => { writer.WriteLine ($"[FAIL] {args.Message.Test.DisplayName}{Environment.NewLine}{ExceptionUtility.CombineMessages (args.Message)}{Environment.NewLine}{ExceptionUtility.CombineStackTraces (args.Message)}"); Interop.mono_sdks_ui_increment_testcase_result (2); };

		testSink.Execution.TestAssemblyStartingEvent += args => { writer.WriteLine ($"Running tests for {args.Message.TestAssembly.Assembly}"); };
		testSink.Execution.TestAssemblyFinishedEvent += args => { writer.WriteLine ($"Finished {args.Message.TestAssembly.Assembly}{Environment.NewLine}"); };

		controller.RunTests (discoverySink.TestCases, resultsSink, testOptions);
		resultsSink.Finished.WaitOne ();

		writer.WriteLine ($"STARTRESULTXML");
		var resultsXml = new XElement ("assemblies");
		resultsXml.Add (resultsXmlAssembly);
		resultsXml.Save (writer.RawStream);
		writer.WriteLine ();
		writer.WriteLine ($"ENDRESULTXML");

		var failed = resultsSink.ExecutionSummary.Failed > 0 || resultsSink.ExecutionSummary.Errors > 0;
		return failed ? 1 : 0;
#else
		MonoSdksTextUI runner;
		TcpWriter writer = null;
		string resultsXml = null;

		if (host != null) {
			Console.WriteLine ($"Connecting to harness at {host}:{port}.");
			resultsXml = Path.GetTempFileName ();
			args = args.Concat (new string[] {"-format:xunit", $"-result:{resultsXml}"}).ToArray ();
			writer = new TcpWriter (host, port);
			runner = new MonoSdksTextUI (writer);
		} else {
			runner = new MonoSdksTextUI ();
		}

		runner.Execute (args);

		if (resultsXml != null) {
			writer.WriteLine ($"STARTRESULTXML");
			using (var resultsXmlStream = File.OpenRead (resultsXml)) resultsXmlStream.CopyTo (writer.RawStream);
			writer.WriteLine ();
			writer.WriteLine ($"ENDRESULTXML");
		}

		return (runner.Failure ? 1 : 0);
#endif
	}
}
