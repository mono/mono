using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit.ConsoleClient;

class MsgBus : IMessageBus
{
	public MsgBus(IMessageSink messageSink, bool stopOnFail = false) {
		Sink = messageSink;
	}

	IMessageSink Sink { get; set; }

	public bool QueueMessage(IMessageSinkMessage message) {
		return Sink.OnMessage (message);
	}

	public void Dispose() {
	}
}


class Discoverer : XunitTestFrameworkDiscoverer
{
	List<ITestClass> TestClasses;
	IEnumerator<ITestClass> TestClassesEnumerator;
	IMessageSink Sink;
	ITestFrameworkDiscoveryOptions DiscoveryOptions;
	IXunitTestCollectionFactory TestCollectionFactory;

	public Discoverer(IAssemblyInfo assemblyInfo,
					  ISourceInformationProvider sourceProvider,
					  IMessageSink diagnosticMessageSink,
					  ITestFrameworkDiscoveryOptions discoveryOptions,
					  IXunitTestCollectionFactory collectionFactory = null)
		: base (assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory) {
		TestCollectionFactory = collectionFactory;
		Sink = diagnosticMessageSink;
		DiscoveryOptions = discoveryOptions;

		TestClasses = new List<ITestClass> ();
		foreach (var type in AssemblyInfo.GetTypes (false).Where (IsValidTestClass))
			TestClasses.Add (CreateTestClass (type));
		TestClassesEnumerator = TestClasses.GetEnumerator ();
	}

	protected override ITestClass CreateTestClass(ITypeInfo @class) {
		return new TestClass(TestCollectionFactory.Get(@class), @class);
	}

	int nenumerated;

	public bool Step () {
		if (!TestClassesEnumerator.MoveNext ())
			return false;

		using (var messageBus = new MsgBus (Sink)) {
			var testClass = TestClassesEnumerator.Current;
			//Console.WriteLine (testClass.Class.Name);
			if (testClass.Class.Name == "System.Threading.ThreadPools.Tests.ThreadPoolTests")
				// FIXME: This invokes the static ctor which creates threads
				return true;
			try {
				FindTestsForType (testClass, false, messageBus, DiscoveryOptions);
			} catch (Exception ex) {
				Console.WriteLine ("EX: " + ex);
			}
		}

		nenumerated ++;
		if (nenumerated % 10 == 0)
			Console.WriteLine ("" + (nenumerated * 100) / TestClasses.Count + "%");

		return true;
	}
}

class WasmRunner : IMessageSink
{
	ITestFrameworkDiscoveryOptions DiscoveryOptions;
	Discoverer discoverer;
	ITestFrameworkExecutor executor;
	ITestFrameworkExecutionOptions executionOptions;
	List<ITestCase> testCases;
	XunitProject project;

	public WasmRunner (XunitProject project) {
		this.project = project;

		var assemblyFileName = "/" + project.Assemblies.First ().AssemblyFilename;
		testCases = new List<ITestCase> ();

		var assembly = Assembly.LoadFrom (assemblyFileName);
		var assemblyInfo = new Xunit.Sdk.ReflectionAssemblyInfo (assembly);

		var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
		var testAssembly = new TestAssembly(assemblyInfo, null);

		var collectionFactory = ExtensibilityPointFactory.GetXunitTestCollectionFactory(this, collectionBehaviorAttribute, testAssembly);

		/*
		object res = null;
		res = Activator.CreateInstance (typeof (Xunit.Sdk.MemberDataDiscoverer), true, true);
		Console.WriteLine ("DISC2: " + res);
		*/

		DiscoveryOptions = TestFrameworkOptions.ForDiscovery(null);
		executionOptions = TestFrameworkOptions.ForExecution(null);

		discoverer = new Discoverer (assemblyInfo, new NullSourceInformationProvider(), this, DiscoveryOptions, collectionFactory);

		executor = new XunitTestFrameworkExecutor (assembly.GetName (), new NullSourceInformationProvider (), this);
	}

	static object ConvertArg (object arg, Type argType) {
		if (arg == null || arg.GetType () == argType)
			return arg;
		// Not clear what conversions should be done
		// IEnumerable<T> -> T[]
		if (argType.IsArray) {
			Type t = typeof (IEnumerable<>).MakeGenericType (argType.GetElementType ());
			if (t.IsAssignableFrom (arg.GetType ())) {
				var m = typeof (Enumerable).GetMethod ("ToArray").MakeGenericMethod (new Type [] { argType.GetElementType () });
				return m.Invoke (null, new object [] { arg });
			}
		}
		try {
			if (arg != null && arg is IConvertible)
				arg = Convert.ChangeType (arg, argType);
		} catch {
		}
		return arg;
	}

	public virtual bool OnMessage (IMessageSinkMessage msg) {
		if (msg is Xunit.Sdk.TestCaseDiscoveryMessage disc_msg) {
			testCases.Add (disc_msg.TestCase);
			return true;
		}
		//Console.WriteLine ("MSG:" + msg);		
		/*
		if (msg is Xunit.Sdk.DiagnosticMessage dmsg)
			Console.WriteLine ("MSG:" + dmsg.Message);
		else if (msg is Xunit.Sdk.TestCaseDiscoveryMessage disc_msg)
			Console.WriteLine ("TEST:" + disc_msg.TestCase.DisplayName);
		else
			Console.WriteLine ("MSG:" + msg);
		*/
		return true;
	}

	public int nrun, nfail, nskipped, nfiltered;

	public int Run () {
		int state;
		int tc_index;

		Console.WriteLine ("Discovering tests...");

		int n = 0;
		while (n < 20) {
			bool res = discoverer.Step ();
			if (!res) {
				break;
			}
		}

		Console.WriteLine ("Running " +  testCases.Count + " tests...");
		state = 3;
		tc_index = 0;

		Console.WriteLine (".");
		int ncases = 0;
		string last_name = "";
		while (true) {
			if (tc_index == testCases.Count)
				break;
			var tc = testCases [tc_index] as XunitTestCase;

			tc_index ++;

			if (!project.Filters.Filter (tc)) {
				nfiltered ++;
				continue;
			}

			var itrait_attrs = tc.TestMethod.Method.GetCustomAttributes(typeof(ITraitAttribute));
			// FIXME:
			if (itrait_attrs.Count () > 0) {
				Console.WriteLine ("SKIP (ITraitAttribute): " + tc.DisplayName);
				nfiltered ++;
				continue;
			}
			/*
			foreach (var attr in itrait_attrs) {
				Console.WriteLine (attr);
				foreach (var disc_attr in attr.GetCustomAttributes (typeof (TraitDiscovererAttribute))) {
					Console.WriteLine (disc_attr);

				}
			}
			*/

			var method = (tc.Method as ReflectionMethodInfo).MethodInfo;

			if (method.ReflectedType.IsGenericTypeDefinition) {
				Console.WriteLine ("FAIL (generic): " + tc.DisplayName);
				nfail ++;
				continue;
			}

			if (tc is Xunit.Sdk.XunitTheoryTestCase) {
				if (method.IsGenericMethod) {
					Console.WriteLine ("SKIP (generic): " + tc.DisplayName);
					nfiltered ++;
					continue;
				}

				// From XunitTheoryTestCaseRunner
				var attrs = tc.TestMethod.Method.GetCustomAttributes(typeof(DataAttribute));
				bool failed = false;
				foreach (var dataAttribute in attrs) {
					var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
					var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();

					Type discovererType = null;
					if (args [1] == "xunit.core")
						discovererType = typeof (IXunitTestCollectionFactory).Assembly.GetType (args [0]);
					if (discovererType == null) {
						Console.WriteLine ("FAIL (discoverer): " + args [0] + " " + args [1]);
						failed = true;
					}

					IDataDiscoverer discoverer;
					discoverer = ExtensibilityPointFactory.GetDataDiscoverer (this, discovererType);

					var data = discoverer.GetData (dataAttribute, tc.TestMethod.Method);
					Console.WriteLine (tc.DisplayName + " [" + data.Count () + "]");
					foreach (var dataRow in data) {
						nrun ++;
						try {
							object obj = null;
							if (!method.IsStatic) {
								var constructor = method.ReflectedType.GetConstructor (Type.EmptyTypes);
								if (constructor != null) 
									obj = constructor.Invoke (null);
								else
									obj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject (method.ReflectedType);
							}

							var pars = method.GetParameters ();
							for (int i = 0; i < dataRow.Length; ++i)
								dataRow [i] = ConvertArg (dataRow [i], pars [i].ParameterType);
							method.Invoke (obj, BindingFlags.Default, null, dataRow, null);
						} catch (Exception ex) {
							Console.WriteLine ("FAIL: " + ex);
							nfail ++;
						}
					}
				}
				if (failed) {
					nfail ++;
					continue;
				}
			} else {
				if (!String.IsNullOrEmpty (tc.SkipReason)) {
					nskipped ++;
					continue;
				}
				nrun ++;
				var name = tc.DisplayName;
				if (name.Contains ("("))
					name = name.Substring (0, name.IndexOf ("("));
				if (name != last_name)
					Console.WriteLine (name);
				last_name = name;
				//Console.WriteLine (tc.DisplayName);
				try {
					object obj = null;
					if (!method.IsStatic) {
						var constructor = method.ReflectedType.GetConstructor (Type.EmptyTypes);
						if (constructor != null) 
							obj = constructor.Invoke (null);
						else
							obj = System.Runtime.Serialization.FormatterServices.GetUninitializedObject (method.ReflectedType);
					}
					var args = tc.TestMethodArguments;
					if (args != null) {
						var pars = method.GetParameters ();
						for (int i = 0; i < args.Length; ++i) {
							args [i] = ConvertArg (args [i], pars [i].ParameterType);
						}
					}
					method.Invoke (obj, args);
				} catch (Exception ex) {
					Console.WriteLine ("FAIL: " + tc.DisplayName);
					Console.WriteLine (ex);
					nfail ++;
				}
			}
		}

		//foreach (var tc in testCases)
		//	Console.WriteLine (tc.DisplayName);
		//executor.RunTests (testCases, this, executionOptions);
		Console.WriteLine ("TESTS = "  + testCases.Count + ", RUN = " + nrun + ", SKIP = " + nfiltered + ", FAIL = " + nfail);
		return nfail == 0 ? 0 : 1;
	}
}

class CmdLineParser : CommandLine
{
	public CmdLineParser (String[] args) : base (args, s => true) {
	}
}

public class XunitDriver
{
	static WasmRunner testRunner;

	static int Main (String[] args) {
		// Process rsp files
		// FIXME: This doesn't work with wasm
		/*
		var new_args = new List<string> ();
		foreach (var arg in args) {
			if (arg [0] == '@') {
				foreach (var line in File.ReadAllLines ("/" + arg.Substring (1)))
					new_args.Add (line);
			} else {
				new_args.Add (arg);
			}
		}
		*/
		var cmdline = new CmdLineParser (args);
		testRunner = new WasmRunner (cmdline.Project);
		return testRunner.Run ();
	}
}
