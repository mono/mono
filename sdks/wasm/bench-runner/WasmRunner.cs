using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.Parameters;
using BenchmarkDotNet.Toolchains.Results;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using BenchmarkDotNet.Characteristics;

//
// WebAssembly runner for BenchmarkDotNet
//

//
// The normal runner doesn't work on wasm for the following reasons:
// - InProcessNoEmitExecutor uses a thread
// - BenchmarkSwitcher -> HostEnvironmentInfo -> DotNetCliCommandExecutor spawns a process
//

// Same as InProcessNoEmitExecutor, but no threads are used
public class WasmExecutor : IExecutor
{
	private static readonly TimeSpan UnderDebuggerTimeout = TimeSpan.FromDays(1);

	public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

	public WasmExecutor (TimeSpan timeout, bool logOutput) {
		if (timeout == TimeSpan.Zero)
			timeout = DefaultTimeout;

		ExecutionTimeout = timeout;
		LogOutput = logOutput;
	}

	public TimeSpan ExecutionTimeout { get; }

	public bool LogOutput { get; }

	public ExecuteResult Execute(ExecuteParameters executeParameters) {
		var hostLogger = LogOutput ? executeParameters.Logger : NullLogger.Instance;
		var host = new InProcessHost(executeParameters.BenchmarkCase, hostLogger, executeParameters.Diagnoser);

		int exitCode = 0;

		// The class is internal
		typeof (InProcessNoEmitToolchain).Assembly.GetType ("BenchmarkDotNet.Toolchains.InProcess.NoEmit.InProcessNoEmitRunner").GetMethod ("Run").Invoke (null, new object [] { host, executeParameters.BenchmarkCase });

		return GetExecutionResult(host.RunResults, exitCode, executeParameters.Logger, executeParameters.BenchmarkCase.Config.Encoding);
	}

	private ExecuteResult GetExecutionResult(RunResults runResults, int exitCode, ILogger logger, Encoding encoding) {
		if (exitCode != 0)
			return new ExecuteResult(true, exitCode, default, Array.Empty<string>(), Array.Empty<string>());

		var lines = runResults.GetMeasurements().Select(measurement => measurement.ToOutputLine()).ToList();
		if (!runResults.GCStats.Equals(GcStats.Empty))
			lines.Add(runResults.GCStats.ToOutputLine());
		if (!runResults.ThreadingStats.Equals(ThreadingStats.Empty))
			lines.Add(runResults.ThreadingStats.ToOutputLine());

		return new ExecuteResult(true, 0, default, lines.ToArray(), Array.Empty<string>());
	}
}

public sealed class WasmToolchain : IToolchain
{
	public static readonly IToolchain Instance = new WasmToolchain (true);

	public WasmToolchain(bool logOutput) : this (
            WasmExecutor.DefaultTimeout,
            logOutput) {
	}

	public WasmToolchain(TimeSpan timeout, bool logOutput) {
		Generator = new InProcessNoEmitGenerator();
		Builder = new InProcessNoEmitBuilder();
		Executor = new WasmExecutor(timeout, logOutput);
	}

	public bool IsSupported(BenchmarkCase benchmarkCase, ILogger logger, IResolver resolver) => true;

	public string Name => nameof(WasmToolchain);

	public IGenerator Generator { get; }

	public IBuilder Builder { get; }

	public IExecutor Executor { get; }

	public bool IsInProcess => true;
}

public class WasmLogger : ILogger
{
	public WasmLogger() {
	}

	public void Write(LogKind logKind, string text) {
		Console.Write (text);
	}

	public void WriteLine() {
		Console.WriteLine ();
	}

	public void WriteLine(LogKind logKind, string text) {
		Console.WriteLine (text);
	}

	public void Flush() {
	}
}

public class Program
{
	static int Main(string[] args) {
		var assembly = Assembly.LoadFrom (args [0]);

		string filter = null;
		if (args.Length < 2) {
			Console.Error.WriteLine ("Usage: runner <benchmark assembly> <filter string>.");
			return 1;
		}
		filter = args [1];

		var logger = new WasmLogger ();
		var config = ManualConfig.CreateEmpty ();
		config.Add (Job.ShortRun.With (WasmToolchain.Instance));
		config.Add (logger);
		config.Add (DefaultExporters.Plain);

		var env_info = HostEnvironmentInfo.GetCurrent ();
		var sdk_version = new Lazy<string> (() => "WASM");
		env_info.GetType ().GetMethod ("set_DotNetSdkVersion", BindingFlags.Instance|BindingFlags.NonPublic).Invoke (env_info, new object [] { sdk_version });

		var benchmarks = new List<BenchmarkRunInfo> ();
		Console.WriteLine ("Collecting benchmarks...");
		var types = assembly.GetTypes ();
		int count = 0;
		foreach (var type in types) {
			count ++;
			if (count == types.Length / 10) {
				Console.WriteLine (".");
				count = 0;
			}
			if (type.IsGenericTypeDefinition)
				continue;
			if (!type.FullName.Contains (filter))
				continue;
			try {
				benchmarks.Add (BenchmarkConverter.TypeToBenchmarks (type, config));
				//Console.WriteLine ("" + type.FullName + " " + benchmarks.Count);
			} catch {
			}
		}

		BenchmarkRunner.Run (benchmarks.ToArray ());
		return 0;
    }
}
