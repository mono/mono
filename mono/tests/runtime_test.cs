
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using NUnit.Framework;

namespace MonoTests
{
	[TestFixture]
	public class RuntimeTests
	{
		readonly bool Verbose = Environment.GetEnvironmentVariable ("V") != null;

		readonly bool Stress = Environment.GetEnvironmentVariable ("MONO_TESTS_STRESS") != null;

		readonly string[] Disabled = Environment.GetEnvironmentVariable ("MONO_TESTS_DISABLED")?.Split (new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string> ();

		readonly string Make = Environment.GetEnvironmentVariable ("MONO_TESTS_MAKE") ?? "make";

		readonly string MakeJ = Environment.GetEnvironmentVariable ("MONO_TESTS_MAKE_J") ?? Environment.ProcessorCount.ToString ();

		readonly string Runtime = Environment.GetEnvironmentVariable ("MONO_TESTS_RUNTIME") ?? Process.GetCurrentProcess ().MainModule.FileName;

		readonly string AotSuffix = Environment.GetEnvironmentVariable ("MONO_TESTS_AOT_SUFFIX");

		readonly string AotFlags = Environment.GetEnvironmentVariable ("MONO_TESTS_AOT_FLAGS");

		readonly string MonoPath = Environment.GetEnvironmentVariable ("MONO_TESTS_MONO_PATH");

		readonly string MonoConfig = Environment.GetEnvironmentVariable ("MONO_TESTS_MONO_CONFIG");

		[TestFixtureSetUp]
		public void CompileTests ()
		{
			string[] tests =
				TestsRaw.Concat (StressTestsRaw)
					.Select (tcd => (string) tcd.Arguments [0])
					.Where (filename => !Disabled.Contains (filename))
					.Distinct ()
					.ToArray ();

			if (tests.Length == 0)
				Assert.Ignore ("Every tests are disabled");

			for (int i = 0, j; i < tests.Length; i = j) {
				StringBuilder sb = new StringBuilder ();

				sb.Append ("-j");
				sb.Append (MakeJ);

				for (j = i; j < i + 50 && j < tests.Length; ++j) {
					sb.Append (" \"");
					sb.Append (tests [j]);
					sb.Append ("\"");

					if (!string.IsNullOrWhiteSpace (AotSuffix)) {
						sb.Append (" \"");
						sb.Append (tests [j]);
						sb.Append (AotSuffix);
						sb.Append ("\"");
					}
				}

				string arguments = sb.ToString ();

				ProcessStartInfo psi = new ProcessStartInfo {
					FileName = Make,
					Arguments = arguments,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
				};

				using (Process process = new Process { StartInfo = psi }) {
					if (Verbose) {
						process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
						{
							if (e.Data != null)
								Console.Out.WriteLine (e.Data);
						};

						process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
						{
							if (e.Data != null)
								Console.Error.WriteLine (e.Data);
						};
					}

					process.Start ();
					process.BeginOutputReadLine ();
					process.BeginErrorReadLine ();

					process.WaitForExit ();

					if (process.ExitCode != 0)
						Assert.Fail ("Failure while building tests");
				}
			}
		}

		[Test, TestCaseSource ("Tests")]
		public int Run (string filename, int timeout, EnvDictionary env, string flags)
		{
			return RunTest (filename, timeout, env, flags);
		}

		[Test, TestCaseSource ("StressTests"), Category ("Stress")]
		public int StressRun (string filename, int timeout, EnvDictionary env, string flags)
		{
			return RunTest (filename, timeout, env, flags);
		}

		int RunTest (string filename, int timeout, EnvDictionary env, string flags)
		{
			if (Disabled.Contains (filename))
				Assert.Ignore ("Test disabled");

			if (!File.Exists (filename))
				Assert.Fail ("Test missing, cannot find file {0}", filename);

			if (!string.IsNullOrWhiteSpace (AotSuffix)) {
				if (!File.Exists (filename + AotSuffix))
					Assert.Fail ("Test missing, cannot find file {0}", filename + AotSuffix);
			}

			if (!string.IsNullOrWhiteSpace (AotFlags))
				flags += " " + AotFlags;

			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = Runtime,
				Arguments = flags + " \"" + filename + "\"",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
			};

			if (env == null)
				env = new EnvDictionary ();

			if (!string.IsNullOrWhiteSpace (MonoPath))
				env ["MONO_PATH"] = MonoPath;
			if (!string.IsNullOrWhiteSpace (MonoConfig))
				env ["MONO_CONFIG"] = MonoConfig;

			foreach (KeyValuePair<string, string> kv in env)
				psi.EnvironmentVariables [kv.Key] = kv.Value;

			using (Process process = new Process { StartInfo = psi })
			{
				if (Verbose) {
					process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
					{
						if (e.Data != null)
							Console.Out.WriteLine (e.Data);
					};

					process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
					{
						if (e.Data != null)
							Console.Error.WriteLine (e.Data);
					};
				}

				process.Start ();
				process.BeginOutputReadLine ();
				process.BeginErrorReadLine ();

				if (!process.WaitForExit (timeout * 1000)) {
					TryThreadDump (process.Id);

					try {
						process.Kill ();
					} catch {
					}

					Assert.Fail ("Test timeout {0}", filename);
				}

				/* wait for stdout and stderr to finish */
				process.WaitForExit ();

				return process.ExitCode;
			}
		}

		IEnumerable<TestCaseData> Tests
		{
			get { return TestsRaw.OrderBy (tcd => (string) tcd.Arguments [0]); }
		}

		IEnumerable<TestCaseData> TestsRaw
		{
			get {
				yield return CreateTest ("generic-unloading-sub.2.exe");
				yield return CreateTest ("create-instance.exe");
				yield return CreateTest ("bug-2907.exe");
				yield return CreateTest ("array-init.exe");
				yield return CreateTest ("arraylist.exe");
				yield return CreateTest ("assembly-load-remap.exe");
				yield return CreateTest ("assemblyresolve_event.exe");
				yield return CreateTest ("assemblyresolve_event3.exe");
				yield return CreateTest ("assemblyresolve_event4.exe");
				yield return CreateTest ("checked.exe");
				yield return CreateTest ("char-isnumber.exe");
				yield return CreateTest ("field-layout.exe");
				yield return CreateTest ("pack-layout.exe");
				yield return CreateTest ("pack-bug.exe");
				yield return CreateTest ("hash-table.exe");
				yield return CreateTest ("test-ops.exe");
				yield return CreateTest ("obj.exe");
				yield return CreateTest ("test-dup-mp.exe");
				yield return CreateTest ("string.exe");
				yield return CreateTest ("stringbuilder.exe");
				yield return CreateTest ("switch.exe");
				yield return CreateTest ("outparm.exe");
				yield return CreateTest ("delegate.exe");
				yield return CreateTest ("bitconverter.exe");
				yield return CreateTest ("exception.exe");
				yield return CreateTest ("exception2.exe");
				yield return CreateTest ("exception3.exe");
				yield return CreateTest ("exception4.exe");
				yield return CreateTest ("exception5.exe");
				yield return CreateTest ("exception6.exe");
				yield return CreateTest ("exception7.exe");
				yield return CreateTest ("exception8.exe");
				yield return CreateTest ("exception10.exe");
				yield return CreateTest ("exception11.exe");
				yield return CreateTest ("exception12.exe");
				yield return CreateTest ("exception13.exe");
				yield return CreateTest ("exception14.exe");
				yield return CreateTest ("exception15.exe");
				yield return CreateTest ("exception16.exe");
				yield return CreateTest ("exception17.exe");
				yield return CreateTest ("exception18.exe");
				yield return CreateTest ("typeload-unaligned.exe");
				yield return CreateTest ("struct.exe");
				yield return CreateTest ("valuetype-gettype.exe");
				yield return CreateTest ("typeof-ptr.exe");
				yield return CreateTest ("static-constructor.exe");
				yield return CreateTest ("pinvoke.exe");
				yield return CreateTest ("pinvoke-utf8.exe");
				yield return CreateTest ("pinvoke3.exe");
				yield return CreateTest ("pinvoke11.exe");
				yield return CreateTest ("pinvoke13.exe");
				yield return CreateTest ("pinvoke17.exe");
				yield return CreateTest ("invoke.exe");
				yield return CreateTest ("invoke2.exe");
				yield return CreateTest ("runtime-invoke.exe");
				yield return CreateTest ("invoke-string-ctors.exe");
				yield return CreateTest ("reinit.exe");
				yield return CreateTest ("box.exe");
				yield return CreateTest ("array.exe");
				yield return CreateTest ("enum.exe");
				yield return CreateTest ("enum2.exe");
				yield return CreateTest ("enum-intrins.exe");
				yield return CreateTest ("property.exe");
				yield return CreateTest ("enumcast.exe");
				yield return CreateTest ("assignable-tests.exe");
				yield return CreateTest ("array-cast.exe");
				yield return CreateTest ("array-subtype-attr.exe");
				yield return CreateTest ("cattr-compile.exe");
				yield return CreateTest ("cattr-field.exe");
				yield return CreateTest ("cattr-object.exe");
				yield return CreateTest ("custom-attr.exe");
				yield return CreateTest ("double-cast.exe");
				yield return CreateTest ("newobj-valuetype.exe");
				yield return CreateTest ("arraylist-clone.exe");
				yield return CreateTest ("setenv.exe");
				yield return CreateTest ("vtype.exe");
				yield return CreateTest ("isvaluetype.exe");
				yield return CreateTest ("iface6.exe");
				yield return CreateTest ("iface7.exe");
				yield return CreateTest ("ipaddress.exe");
				yield return CreateTest ("array-vt.exe");
				yield return CreateTest ("interface1.exe");
				yield return CreateTest ("reflection-enum.exe");
				yield return CreateTest ("reflection-prop.exe");
				yield return CreateTest ("reflection4.exe");
				yield return CreateTest ("reflection5.exe");
				yield return CreateTest ("reflection-const-field.exe");
				yield return CreateTest ("many-locals.exe");
				yield return CreateTest ("string-compare.exe");
				yield return CreateTest ("test-prime.exe");
				yield return CreateTest ("test-tls.exe");
				yield return CreateTest ("params.exe");
				yield return CreateTest ("reflection.exe");
				yield return CreateTest ("interface.exe");
				yield return CreateTest ("iface.exe");
				yield return CreateTest ("iface2.exe");
				yield return CreateTest ("iface3.exe");
				yield return CreateTest ("iface4.exe");
				yield return CreateTest ("iface-large.exe");
				yield return CreateTest ("virtual-method.exe");
				yield return CreateTest ("intptrcast.exe");
				yield return CreateTest ("indexer.exe");
				yield return CreateTest ("stream.exe");
				yield return CreateTest ("console.exe");
				yield return CreateTest ("shift.exe");
				yield return CreateTest ("jit-int.exe");
				yield return CreateTest ("jit-uint.exe");
				yield return CreateTest ("jit-long.exe");
				yield return CreateTest ("long.exe");
				yield return CreateTest ("jit-ulong.exe");
				yield return CreateTest ("jit-float.exe");
				yield return CreateTest ("pop.exe");
				yield return CreateTest ("time.exe");
				yield return CreateTest ("pointer.exe");
				yield return CreateTest ("hashcode.exe");
				yield return CreateTest ("delegate1.exe");
				yield return CreateTest ("delegate2.exe");
				yield return CreateTest ("delegate3.exe");
				yield return CreateTest ("delegate5.exe");
				yield return CreateTest ("delegate6.exe");
				yield return CreateTest ("delegate7.exe");
				yield return CreateTest ("delegate8.exe");
				yield return CreateTest ("delegate10.exe");
				yield return CreateTest ("delegate11.exe");
				yield return CreateTest ("delegate12.exe");
				yield return CreateTest ("delegate13.exe");
				yield return CreateTest ("largeexp.exe");
				yield return CreateTest ("largeexp2.exe");
				yield return CreateTest ("marshalbyref1.exe");
				yield return CreateTest ("static-ctor.exe");
				yield return CreateTest ("inctest.exe");
				yield return CreateTest ("bound.exe");
				yield return CreateTest ("array-invoke.exe");
				yield return CreateTest ("test-arr.exe");
				yield return CreateTest ("decimal.exe");
				yield return CreateTest ("decimal-array.exe");
				yield return CreateTest ("marshal.exe");
				yield return CreateTest ("marshal1.exe");
				yield return CreateTest ("marshal2.exe");
				yield return CreateTest ("marshal3.exe");
				yield return CreateTest ("marshal5.exe");
				yield return CreateTest ("marshal6.exe");
				yield return CreateTest ("marshal7.exe");
				yield return CreateTest ("marshal8.exe");
				yield return CreateTest ("marshal9.exe");
				yield return CreateTest ("marshalbool.exe");
				yield return CreateTest ("test-byval-in-struct.exe");
				yield return CreateTest ("thread.exe");
				yield return CreateTest ("thread5.exe");
				yield return CreateTest ("thread-static.exe");
				yield return CreateTest ("thread-static-init.exe");
				yield return CreateTest ("context-static.exe");
				yield return CreateTest ("float-pop.exe");
				yield return CreateTest ("interfacecast.exe");
				yield return CreateTest ("array3.exe");
				yield return CreateTest ("classinit.exe");
				yield return CreateTest ("classinit2.exe");
				yield return CreateTest ("classinit3.exe");
				yield return CreateTest ("synchronized.exe");
				yield return CreateTest ("async_read.exe");
				yield return CreateTest ("threadpool.exe");
				yield return CreateTest ("threadpool1.exe");
				yield return CreateTest ("threadpool-exceptions1.exe");
				yield return CreateTest ("threadpool-exceptions2.exe");
				yield return CreateTest ("threadpool-exceptions3.exe");
				yield return CreateTest ("threadpool-exceptions4.exe");
				yield return CreateTest ("threadpool-exceptions5.exe");
				yield return CreateTest ("threadpool-exceptions6.exe");
				yield return CreateTest ("base-definition.exe");
				yield return CreateTest ("bug-27420.exe");
				yield return CreateTest ("bug-46781.exe");
				yield return CreateTest ("bug-42136.exe");
				yield return CreateTest ("bug-59286.exe");
				yield return CreateTest ("bug-70561.exe");
				yield return CreateTest ("bug-78311.exe");
				yield return CreateTest ("bug-78653.exe");
				yield return CreateTest ("bug-78656.exe");
				yield return CreateTest ("bug-77127.exe");
				yield return CreateTest ("bug-323114.exe");
				yield return CreateTest ("bug-Xamarin-5278.exe");
				yield return CreateTest ("interlocked.exe");
				yield return CreateTest ("delegate-async-exit.exe");
				yield return CreateTest ("delegate-delegate-exit.exe");
				yield return CreateTest ("delegate-exit.exe");
				yield return CreateTest ("finalizer-abort.exe");
				yield return CreateTest ("finalizer-exception.exe");
				yield return CreateTest ("finalizer-exit.exe");
				yield return CreateTest ("finalizer-thread.exe");
				yield return CreateTest ("main-exit.exe");
				yield return CreateTest ("main-returns-abort-resetabort.exe");
				yield return CreateTest ("main-returns-background-abort-resetabort.exe");
				yield return CreateTest ("main-returns-background-resetabort.exe");
				yield return CreateTest ("main-returns-background.exe");
				yield return CreateTest ("main-returns-background-change.exe");
				yield return CreateTest ("main-returns.exe");
				yield return CreateTest ("subthread-exit.exe");
				yield return CreateTest ("desweak.exe");
				yield return CreateTest ("exists.exe");
				yield return CreateTest ("handleref.exe");
				yield return CreateTest ("dbnull-missing.exe");
				yield return CreateTest ("test-type-ctor.exe");
				yield return CreateTest ("soft-float-tests.exe");
				yield return CreateTest ("thread-exit.exe");
				yield return CreateTest ("finalize-parent.exe");
				yield return CreateTest ("interlocked-2.2.exe");
				yield return CreateTest ("pinvoke-2.2.exe");
				yield return CreateTest ("bug-78431.2.exe");
				yield return CreateTest ("bug-79684.2.exe");
				yield return CreateTest ("catch-generics.2.exe");
				yield return CreateTest ("event-get.2.exe");
				yield return CreateTest ("safehandle.2.exe");
				yield return CreateTest ("module-cctor-loader.2.exe");
				yield return CreateTest ("generics-invoke-byref.2.exe");
				yield return CreateTest ("generic-signature-compare.2.exe");
				yield return CreateTest ("generics-sharing.2.exe");
				yield return CreateTest ("shared-generic-methods.2.exe");
				yield return CreateTest ("shared-generic-synchronized.2.exe");
				yield return CreateTest ("generic-inlining.2.exe");
				yield return CreateTest ("generic-initobj.2.exe");
				yield return CreateTest ("generic-delegate.2.exe");
				yield return CreateTest ("generic-sizeof.2.exe");
				yield return CreateTest ("generic-virtual.2.exe");
				yield return CreateTest ("generic-interface-methods.2.exe");
				yield return CreateTest ("generic-array-type.2.exe");
				yield return CreateTest ("generic-method-patching.2.exe");
				yield return CreateTest ("generic-static-methods.2.exe");
				yield return CreateTest ("generic-null-call.2.exe");
				yield return CreateTest ("generic-special.2.exe");
				yield return CreateTest ("generic-exceptions.2.exe");
				yield return CreateTest ("generic-virtual2.2.exe");
				yield return CreateTest ("generic-valuetype-interface.2.exe");
				yield return CreateTest ("generic-getgenericarguments.2.exe");
				yield return CreateTest ("generic-synchronized.2.exe");
				yield return CreateTest ("generic-delegate-ctor.2.exe");
				yield return CreateTest ("generic-array-iface-set.2.exe");
				yield return CreateTest ("generic-typedef.2.exe");
				yield return CreateTest ("bug-431413.2.exe");
				yield return CreateTest ("bug-459285.2.exe");
				yield return CreateTest ("generic-virtual-invoke.2.exe");
				yield return CreateTest ("bug-461198.2.exe");
				yield return CreateTest ("generic-sealed-virtual.2.exe");
				yield return CreateTest ("generic-system-arrays.2.exe");
				yield return CreateTest ("generic-stack-traces.2.exe");
				yield return CreateTest ("generic-stack-traces2.2.exe");
				yield return CreateTest ("bug-472600.2.exe");
				yield return CreateTest ("recursive-generics.2.exe");
				yield return CreateTest ("bug-473482.2.exe");
				yield return CreateTest ("bug-473999.2.exe");
				yield return CreateTest ("bug-479763.2.exe");
				yield return CreateTest ("bug-616463.exe");
				yield return CreateTest ("bug-80392.2.exe");
				yield return CreateTest ("bug-82194.2.exe");
				yield return CreateTest ("anonarray.2.exe");
				yield return CreateTest ("ienumerator-interfaces.2.exe");
				yield return CreateTest ("array-enumerator-ifaces.2.exe");
				yield return CreateTest ("generic_type_definition_encoding.2.exe");
				yield return CreateTest ("bug-333798.2.exe");
				yield return CreateTest ("bug-348522.2.exe");
				yield return CreateTest ("bug-340662_bug.exe");
				yield return CreateTest ("bug-325283.2.exe");
				yield return CreateTest ("thunks.exe");
				yield return CreateTest ("winx64structs.exe");
				yield return CreateTest ("nullable_boxing.2.exe");
				yield return CreateTest ("valuetype-equals.exe");
				yield return CreateTest ("custom-modifiers.2.exe");
				yield return CreateTest ("bug-382986.exe");
				yield return CreateTest ("test-inline-call-stack.exe");
				yield return CreateTest ("bug-324535.exe");
				yield return CreateTest ("modules.exe");
				yield return CreateTest ("bug-81673.exe");
				yield return CreateTest ("bug-81691.exe");
				yield return CreateTest ("bug-415577.exe");
				yield return CreateTest ("filter-stack.exe");
				yield return CreateTest ("vararg.exe");
				yield return CreateTest ("vararg2.exe");
				yield return CreateTest ("bug-461867.exe");
				yield return CreateTest ("bug-461941.exe");
				yield return CreateTest ("bug-461261.exe");
				yield return CreateTest ("bug-400716.exe");
				yield return CreateTest ("bug-459094.exe");
				yield return CreateTest ("bug-467456.exe");
				yield return CreateTest ("bug-508538.exe");
				yield return CreateTest ("bug-472692.2.exe");
				yield return CreateTest ("gchandles.exe");
				yield return CreateTest ("interlocked-3.exe");
				yield return CreateTest ("interlocked-4.2.exe");
				yield return CreateTest ("w32message.exe");
				yield return CreateTest ("gc-altstack.exe");
				yield return CreateTest ("large-gc-bitmap.exe");
				yield return CreateTest ("bug-561239.exe");
				yield return CreateTest ("bug-562150.exe");
				yield return CreateTest ("bug-599469.exe");
				yield return CreateTest ("monitor-resurrection.exe");
				yield return CreateTest ("monitor-wait-abort.exe");
				yield return CreateTest ("monitor-abort.exe");
				yield return CreateTest ("bug-666008.exe");
				yield return CreateTest ("bug-685908.exe");
				yield return CreateTest ("sgen-long-vtype.exe");
				yield return CreateTest ("delegate-invoke.exe");
				yield return CreateTest ("bug-696593.exe");
				yield return CreateTest ("bug-705140.exe");
				yield return CreateTest ("bug-1147.exe");
				yield return CreateTest ("mono-path.exe");
				yield return CreateTest ("bug-bxc-795.exe");
				yield return CreateTest ("bug-3903.exe");
				yield return CreateTest ("async-with-cb-throws.exe");
				yield return CreateTest ("bug-6148.exe");
				yield return CreateTest ("bug-10127.exe");
				yield return CreateTest ("bug-18026.exe");
				yield return CreateTest ("allow-synchronous-major.exe");
				yield return CreateTest ("block_guard_restore_aligment_on_exit.exe");
				yield return CreateTest ("thread_static_gc_layout.exe");
				yield return CreateTest ("sleep.exe");
				yield return CreateTest ("bug-27147.exe");
				yield return CreateTest ("bug-30085.exe");
				yield return CreateTest ("bug-17537.exe");
				yield return CreateTest ("pinvoke_ppcc.exe");
				yield return CreateTest ("pinvoke_ppcs.exe");
				yield return CreateTest ("pinvoke_ppci.exe");
				yield return CreateTest ("pinvoke_ppcf.exe");
				yield return CreateTest ("pinvoke_ppcd.exe");
				yield return CreateTest ("bug-29585.exe");
				yield return CreateTest ("priority.exe");
				yield return CreateTest ("abort-cctor.exe");
				yield return CreateTest ("thread-native-exit.exe");
				yield return CreateTest ("reference-loader.exe");
				yield return CreateTest ("remoting4.exe");
				yield return CreateTest ("remoting1.exe");
				yield return CreateTest ("remoting2.exe");
				yield return CreateTest ("remoting3.exe");
				yield return CreateTest ("remoting5.exe");
				yield return CreateTest ("appdomain.exe");
				yield return CreateTest ("appdomain-client.exe");
				yield return CreateTest ("appdomain-unload.exe");
				yield return CreateTest ("appdomain-async-invoke.exe");
				yield return CreateTest ("appdomain-thread-abort.exe");
				yield return CreateTest ("appdomain1.exe");
				yield return CreateTest ("appdomain2.exe");
				yield return CreateTest ("appdomain-exit.exe");
				yield return CreateTest ("assemblyresolve_event2.2.exe");
				yield return CreateTest ("appdomain-unload-callback.exe");
				yield return CreateTest ("appdomain-unload-doesnot-raise-pending-events.exe");
				yield return CreateTest ("unload-appdomain-on-shutdown.exe");
				yield return CreateTest ("bug-47295.exe");
				yield return CreateTest ("loader.exe");
				yield return CreateTest ("pinvoke2.exe");
				yield return CreateTest ("generic-type-builder.2.exe");
				yield return CreateTest ("dynamic-generic-size.exe");
				yield return CreateTest ("cominterop.exe");
				yield return CreateTest ("dynamic-method-access.2.exe");
				yield return CreateTest ("dynamic-method-finalize.2.exe");
				yield return CreateTest ("dynamic-method-stack-traces.exe");
				yield return CreateTest ("generic_type_definition.2.exe");
				yield return CreateTest ("bug-333798-tb.2.exe");
				yield return CreateTest ("bug-335131.2.exe");
				yield return CreateTest ("bug-322722_patch_bx.2.exe");
				yield return CreateTest ("bug-322722_dyn_method_throw.2.exe");
				yield return CreateTest ("bug-389886-2.exe");
				yield return CreateTest ("bug-349190.2.exe");
				yield return CreateTest ("bug-389886-sre-generic-interface-instances.exe");
				yield return CreateTest ("bug-462592.exe");
				yield return CreateTest ("bug-575941.exe");
				yield return CreateTest ("bug-389886-3.exe");
				yield return CreateTest ("constant-division.exe");
				yield return CreateTest ("dynamic-method-resurrection.exe");
				yield return CreateTest ("bug-80307.exe");
				yield return CreateTest ("assembly_append_ordering.exe");
				yield return CreateTest ("bug-544446.exe");
				yield return CreateTest ("bug-36848.exe");
				yield return CreateTest ("generic-marshalbyref.2.exe");
				yield return CreateTest ("stackframes-async.2.exe");
				yield return CreateTest ("transparentproxy.exe");
				yield return CreateTest ("bug-48015.exe");
				yield return CreateTest ("delegate9.exe");
				yield return CreateTest ("marshal-valuetypes.exe");
				yield return CreateTest ("xdomain-threads.exe");
				yield return CreateTest ("monitor.exe");
				yield return CreateTest ("generic-xdomain.2.exe");
				yield return CreateTest ("threadpool-exceptions7.exe");
				yield return CreateTest ("cross-domain.exe");
				yield return CreateTest ("generic-unloading.2.exe");
				yield return CreateTest ("thread6.exe");

				yield return CreateTest ("field-access.exe");
				yield return CreateTest ("method-access.exe");
				yield return CreateTest ("ldftn-access.exe");
				yield return CreateTest ("cpblkTest.exe");
				yield return CreateTest ("vbinterface.exe");
				yield return CreateTest ("calliTest.exe");
				yield return CreateTest ("calliGenericTest.exe");
				yield return CreateTest ("ckfiniteTest.exe");
				yield return CreateTest ("fault-handler.exe");
				yield return CreateTest ("locallocTest.exe");
				yield return CreateTest ("initblkTest.exe");
				yield return CreateTest ("qt-instance.exe");
				yield return CreateTest ("vararg.exe");
				yield return CreateTest ("bug-29859.exe");
				yield return CreateTest ("bug-78549.exe");
				yield return CreateTest ("static-fields-nonconst.exe");
				yield return CreateTest ("reload-at-bb-end.exe");
				yield return CreateTest ("test-enum-indstoreil.exe");
				yield return CreateTest ("filter-bug.exe");
				yield return CreateTest ("even-odd.exe");
				yield return CreateTest ("bug-82022.exe");
				yield return CreateTest ("vt-sync-method.exe");
				yield return CreateTest ("enum_types.exe");
				yield return CreateTest ("invalid-token.exe");
				yield return CreateTest ("call_missing_method.exe");
				yield return CreateTest ("call_missing_class.exe");
				yield return CreateTest ("ldfld_missing_field.exe");
				yield return CreateTest ("ldfld_missing_class.exe");
				yield return CreateTest ("find-method.2.exe");
				yield return CreateTest ("bug-79215.2.exe");
				yield return CreateTest ("bug-79956.2.exe");
				yield return CreateTest ("bug-327438.2.exe");
				yield return CreateTest ("bug-387274.2.exe");
				yield return CreateTest ("bug-426309.2.exe");
				yield return CreateTest ("ldtoken_with_byref_typespec.2.exe");
				yield return CreateTest ("resolve_method_bug.2.exe");
				yield return CreateTest ("resolve_field_bug.2.exe");
				yield return CreateTest ("resolve_type_bug.2.exe");
				yield return CreateTest ("generics-sharing-other-exc.2.exe");
				yield return CreateTest ("generic-ldobj.2.exe");
				yield return CreateTest ("generic-mkrefany.2.exe");
				yield return CreateTest ("generic-refanyval.2.exe");
				yield return CreateTest ("generic-ldtoken.2.exe");
				yield return CreateTest ("generic-ldtoken-method.2.exe");
				yield return CreateTest ("generic-ldtoken-field.2.exe");
				yield return CreateTest ("generic-tailcall.2.exe");
				yield return CreateTest ("generic-tailcall2.2.exe");
				yield return CreateTest ("generic-array-exc.2.exe");
				yield return CreateTest ("generic-valuetype-newobj2.2.exe");
				yield return CreateTest ("generic-valuetype-newobj.2.exe");
				yield return CreateTest ("generic-constrained.2.exe");
				yield return CreateTest ("generic-type-load-exception.2.exe");
				yield return CreateTest ("bug-81466.exe");
				yield return CreateTest ("bug457574.exe");
				yield return CreateTest ("bug445361.exe");
				yield return CreateTest ("bug-463303.exe");
				yield return CreateTest ("bug469742.2.exe");
				yield return CreateTest ("bug-528055.exe");
				yield return CreateTest ("array_load_exception.exe");
				yield return CreateTest ("bug-481403.exe");
				yield return CreateTest ("interface-with-static-method.exe");
				yield return CreateTest ("bug-633291.exe");
				yield return CreateTest ("delegate-with-null-target.exe");
				yield return CreateTest ("bug-318677.exe");
				yield return CreateTest ("gsharing-valuetype-layout.exe");
				yield return CreateTest ("invalid_generic_instantiation.exe");
				yield return CreateTest ("bug-45841-fpstack-exceptions.exe");

				List<string> GenericConfigs = new List<string> {
					"gshared",
					"gshared,shared",
					"gshared,-inline",
					"gshared,-inline,shared",
				};

				foreach (string config in GenericConfigs) {
					string flags = "-O=" + config;

					yield return CreateTest ("generics-sharing.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("shared-generic-methods.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("shared-generic-synchronized.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-initobj.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generics-sharing-other-exc.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-box.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-unbox.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-delegate.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-sizeof.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-ldobj.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-mkrefany.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-refanyval.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-ldtoken.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-ldtoken-method.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-ldtoken-field.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-virtual.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-tailcall.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-interface-methods.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-array-type.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-method-patching.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-static-methods.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-null-call.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-tailcall2.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-array-exc.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-special.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-exceptions.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-delegate2.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-virtual2.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-valuetype-interface.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-valuetype-newobj.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-valuetype-newobj2.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-getgenericarguments.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-synchronized.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-delegate-ctor.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-constrained.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-431413.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-virtual-invoke.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-typedef.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-marshalbyref.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-459285.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-461198.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-sealed-virtual.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-system-arrays.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-stack-traces.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-stack-traces2.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-472600.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-473482.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-473999.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-479763.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-type-load-exception.2.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-616463.exe", flags: flags, suffix: config);
					yield return CreateTest ("bug-1147.exe", flags: flags, suffix: config);
					yield return CreateTest ("generic-type-builder.2.exe", flags: flags, suffix: config);
				}

				yield return CreateTest ("load-exceptions.exe");

				yield return CreateTest ("test-multi-netmodule-4-exe.exe");

				yield return CreateTest ("custom-attr-errors.exe");

				yield return CreateTest ("reflection-load-with-context.exe");

				for (int i = 0; i < 2; ++i) {
					EnvDictionary env = null;

					if (i == 1) {
						env = new EnvDictionary {
							{ "TEST_UNHANDLED_EXCEPTION_HANDLER", "1" },
						};
					}

					string suffix = env != null ? "handler" : "nohandler";

					yield return CreateTest ("unhandled-exception-1.exe", env: env, returns: 1,   suffix: suffix);
					yield return CreateTest ("unhandled-exception-2.exe", env: env, returns: 255, suffix: suffix);
					yield return CreateTest ("unhandled-exception-3.exe", env: env, returns: 255, suffix: suffix);
					yield return CreateTest ("unhandled-exception-4.exe", env: env, returns: 255, suffix: suffix);
					yield return CreateTest ("unhandled-exception-5.exe", env: env, returns: 255, suffix: suffix);
					yield return CreateTest ("unhandled-exception-6.exe", env: env, returns: 255, suffix: suffix);
					yield return CreateTest ("unhandled-exception-7.exe", env: env, returns: 255, suffix: suffix);
				}

				yield return CreateTest ("appdomain-loader.exe");
				yield return CreateTest ("appdomain-loader.exe", flags: "-O=gshared", suffix: "gshared");
				yield return CreateTest ("appdomain-loader.exe", env: new EnvDictionary { { "MONO_DEBUG_ASSEMBLY_UNLOAD", "1" } }, suffix: "unload");
				yield return CreateTest ("appdomain-loader.exe", env: new EnvDictionary { { "MONO_DEBUG_ASSEMBLY_UNLOAD", "1" } }, flags: "-O=gshared", suffix: "gshared+unload");
			}
		}

		IEnumerable<TestCaseData> StressTests
		{
			get { return StressTestsRaw.OrderBy (tcd => (string) tcd.Arguments [0]); }
		}

		IEnumerable<TestCaseData> StressTestsRaw
		{
			get {
				List<Tuple<string, string, string>> GCConfigs = new List<Tuple<string, string, string>> {
					Tuple.Create ("", "major=marksweep", "ms"),
					Tuple.Create ("", "major=marksweep,minor=split", "ms-split"),
					Tuple.Create ("", "major=marksweep-conc", "ms-conc"),
					Tuple.Create ("", "major=marksweep-conc,minor=split", "ms-conc-split"),
					Tuple.Create ("", "major=marksweep-conc,minor=split,alloc-ratio=95", "ms-conc-split-95"),
					Tuple.Create ("clear-at-gc", "major=marksweep", "ms-cleat-at-gc"),
					Tuple.Create ("clear-at-gc", "major=marksweep,minor=split", "ms-split-cleat-at-gc"),
					Tuple.Create ("clear-at-gc", "major=marksweep-conc", "ms-conc-cleat-at-gc"),
					Tuple.Create ("clear-at-gc", "major=marksweep-conc,minor=split", "ms-conc-split-cleat-at-gc"),
				};

				if (Stress) {
					/* They take too long on non-stress mode, so we need to modify
					 * the tests to check the presence of `MONO_TESTS_STRESS=1` and
					 * reduce the duration of the test during non-stress testing */

					foreach (var config in GCConfigs) {
						EnvDictionary env = new EnvDictionary {
							{ "MONO_ENV_OPTIONS", "--gc=sgen" },
							{ "MONO_GC_DEBUG", config.Item1 },
							{ "MONO_GC_PARAMS", config.Item2 },
						};

						yield return CreateTest ("finalizer-wait.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("critical-finalizers.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-descriptors.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-gshared-vtype.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-weakref-stress.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-cementing-stress.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-case-23400.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-new-threads-dont-join-stw.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-new-threads-dont-join-stw-2.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-new-threads-collect.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("gc-graystack-stress.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("bug-17590.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-domain-unload.exe", timeout: 900, env: env, suffix: config.Item3);
						yield return CreateTest ("sgen-domain-unload-2.exe", timeout: 900, env: env, suffix: config.Item3);
					}
				}

				foreach (var config in GCConfigs) {
					EnvDictionary env = new EnvDictionary {
						{ "MONO_ENV_OPTIONS", "--gc=sgen" },
						{ "MONO_GC_DEBUG", config.Item1 },
						{ "MONO_GC_PARAMS", "toggleref-test," + config.Item2 },
					};

					yield return CreateTest ("sgen-toggleref.exe", timeout: 900, env: env, suffix: config.Item3);
				}

				foreach (var config in GCConfigs) {
					foreach (var implementation in new List<string> { "old", "new", "tarjan" }) {
						string suffix = config.Item3 + "-" + implementation;

						EnvDictionary env = new EnvDictionary {
							{ "MONO_ENV_OPTIONS", "--gc=sgen" },
							{ "MONO_GC_DEBUG", "bridge=Bridge," + config.Item1 },
							{ "MONO_GC_PARAMS", "bridge-implementation=" + implementation + "," + config.Item2 },
						};

						yield return CreateTest ("sgen-bridge.exe", timeout: 900, env: env, suffix: suffix);
						yield return CreateTest ("sgen-bridge-major-fragmentation.exe", timeout: 900, env: env, suffix: suffix);

						EnvDictionary env2 = new EnvDictionary {
							{ "MONO_ENV_OPTIONS", "--gc=sgen" },
							{ "MONO_GC_DEBUG", "bridge=2Bridge," + config.Item1 },
							{ "MONO_GC_PARAMS", "bridge-implementation=" + implementation + "," + config.Item2 },
						};

						yield return CreateTest ("sgen-bridge-xref.exe", timeout: 900, env: env2, suffix: suffix);

						EnvDictionary env3 = new EnvDictionary {
							{ "MONO_ENV_OPTIONS", "--gc=sgen" },
							{ "MONO_GC_DEBUG", "bridge=3Bridge," + config.Item1 },
							{ "MONO_GC_PARAMS", "bridge-implementation=" + implementation + "," + config.Item2 },
						};

						yield return CreateTest ("sgen-bridge-gchandle.exe", timeout: 900, env: env3, suffix: suffix);
					}
				}

				yield return CreateTest ("appdomain-threadpool-unload.exe");
				yield return CreateTest ("namedmutex-destroy-race.exe");
				yield return CreateTest ("process-unref-race.exe");
				yield return CreateTest ("thread-suspend-selfsuspended.exe");
				yield return CreateTest ("thread-suspend-suspended.exe");

				yield return CreateTest ("process-stress-1.exe", timeout: 600);
				yield return CreateTest ("process-stress-2.exe", timeout: 600);
				yield return CreateTest ("process-stress-3.exe", timeout: 600);
				yield return CreateTest ("process-leak.exe", timeout: 600);
			}
		}

		TestCaseData CreateTest (string filename, int timeout = 300, EnvDictionary env = null, string flags = "", int returns = 0, string suffix = "")
		{
			return
				new TestCaseData (filename, timeout, env, flags)
					.Returns (returns)
					.SetCategory ("RuntimeTests")
					.SetDescription (filename + (string.IsNullOrWhiteSpace (suffix) ? string.Empty : ("_" + suffix)));
		}

		void TryThreadDump (int pid)
		{
			string filename = Path.GetTempFileName ();

			try {
				using (StreamWriter sw = new StreamWriter (new FileStream (filename, FileMode.Truncate, FileAccess.Write)))
				{
					sw.WriteLine ("attach " + pid);
					sw.WriteLine ("info threads");
					sw.WriteLine ("t a a p mono_print_thread_dump(0)");
					sw.WriteLine ("thread apply all backtrace");
					sw.Flush ();

					RunDebugProcess ("gdb", "-batch -x \"" + filename + "\" -nx");
				}

				return;
			} catch {
			}

			try {
				using (StreamWriter sw = new StreamWriter (new FileStream (filename, FileMode.Truncate, FileAccess.Write)))
				{
					sw.WriteLine ("process attach --pid " + pid);
					sw.WriteLine ("thread list");
					sw.WriteLine ("p mono_threads_perform_thread_dump_force()");
					sw.WriteLine ("thread backtrace all");
					sw.WriteLine ("detach");
					sw.WriteLine ("quit");
					sw.Flush ();

					RunDebugProcess ("lldb", "--batch --source \"" + filename + "\" --no-lldbinit");
				}

				return;
			} catch {
			}
		}

		void RunDebugProcess (string command, string arguments)
		{
			ProcessStartInfo psi = new ProcessStartInfo {
				FileName = command,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
			};

			using (Process process = new Process { StartInfo = psi })
			{
				process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
				{
					if (e.Data != null)
						Console.Out.WriteLine (e.Data);
				};

				process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
				{
					if (e.Data != null)
						Console.Error.WriteLine (e.Data);
				};

				process.Start ();
				process.BeginOutputReadLine ();
				process.BeginErrorReadLine ();

				if (!process.WaitForExit (60 * 1000)) {
					try {
						process.Kill ();
					} catch {
					}
				}
			}
		}

		public class EnvDictionary : Dictionary<string, string>
		{
			public override string ToString ()
			{
				StringBuilder sb = new StringBuilder ();
				sb.Append ("[EnvDictionary:");
				foreach (KeyValuePair<string, string> de in this)
				{
					sb.Append (" ");
					sb.Append (de.Key);
					sb.Append ("=\"");
					sb.Append (de.Value);
					sb.Append ("\"");
				}
				sb.Append ("]");
				return sb.ToString ();
			}
		}
	}
}
