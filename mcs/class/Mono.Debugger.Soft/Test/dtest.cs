using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Debugger.Soft;
using Diag = System.Diagnostics;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

using NUnit.Framework;

#pragma warning disable 0219

namespace MonoTests
{

[TestFixture]
public class DebuggerTests
{
	VirtualMachine vm;
	MethodMirror entry_point;
	StepEventRequest step_req;

	void AssertThrows<ExType> (Action del) where ExType : Exception {
		bool thrown = false;

		try {
			del ();
		} catch (ExType) {
			thrown = true;
		}
		Assert.IsTrue (thrown);
	}

	// No other way to pass arguments to the tests ?
	public static bool listening = Environment.GetEnvironmentVariable ("DBG_SUSPEND") != null;
	public static string runtime = Environment.GetEnvironmentVariable ("DBG_RUNTIME");
	public static string agent_args = Environment.GetEnvironmentVariable ("DBG_AGENT_ARGS");

	Event GetNextEvent () {
		var es = vm.GetNextEventSet ();
		Assert.AreEqual (1, es.Events.Length);
		return es [0];
	}

	void Start (string[] args) {
		if (!listening) {
			var pi = new Diag.ProcessStartInfo ();

			if (runtime != null)
				pi.FileName = runtime;
			else
				pi.FileName = "mono";
			pi.Arguments = String.Join (" ", args);
			vm = VirtualMachineManager.Launch (pi, new LaunchOptions { AgentArgs = agent_args });
		} else {
			var ep = new IPEndPoint (IPAddress.Any, 10000);
			Console.WriteLine ("Listening on " + ep + "...");
			vm = VirtualMachineManager.Listen (ep);
		}

		var load_req = vm.CreateAssemblyLoadRequest ();
		load_req.Enable ();

		Event vmstart = GetNextEvent ();
		Assert.AreEqual (EventType.VMStart, vmstart.EventType);

		vm.Resume ();

		entry_point = null;
		step_req = null;

		Event e;

		/* Find out the entry point */
		while (true) {
			e = GetNextEvent ();

			if (e is AssemblyLoadEvent) {
				AssemblyLoadEvent ae = (AssemblyLoadEvent)e;
				entry_point = ae.Assembly.EntryPoint;
				if (entry_point != null)
					break;
			}

			vm.Resume ();
		}

		load_req.Disable ();
	}

	BreakpointEvent run_until (string name) {
		// String
		MethodMirror m = entry_point.DeclaringType.GetMethod (name);
		Assert.IsNotNull (m);
		//Console.WriteLine ("X: " + name + " " + m.ILOffsets.Count + " " + m.Locations.Count);
		vm.SetBreakpoint (m, m.ILOffsets [0]);

		Event e = null;

		while (true) {
			vm.Resume ();
			e = GetNextEvent ();
			if (e is BreakpointEvent)
				break;
		}

		Assert.IsInstanceOfType (typeof (BreakpointEvent), e);
		Assert.AreEqual (m.Name, (e as BreakpointEvent).Method.Name);

		return (e as BreakpointEvent);
	}

	Event single_step (ThreadMirror t) {
		var req = vm.CreateStepRequest (t);
		req.Enable ();

		vm.Resume ();
		Event e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);

		req.Disable ();

		return e;
	}

	void check_arg_val (StackFrame frame, int pos, Type type, object eval) {
		object val = frame.GetArgument (pos);
		Assert.IsTrue (val is PrimitiveValue);
		object v = (val as PrimitiveValue).Value;
		Assert.AreEqual (type, v.GetType ());
		if (eval is float)
			Assert.IsTrue (Math.Abs ((float)eval - (float)v) < 0.0001);
		else if (eval is double)
			Assert.IsTrue (Math.Abs ((double)eval - (double)v) < 0.0001);
		else
			Assert.AreEqual (eval, v);
	}

	void AssertValue (object expected, object val) {
		if (expected is string) {
			Assert.IsTrue (val is StringMirror);
			Assert.AreEqual (expected, (val as StringMirror).Value);
		} else if (val is StructMirror && (val as StructMirror).Type.Name == "IntPtr") {
			AssertValue (expected, (val as StructMirror).Fields [0]);
		} else {
			Assert.IsTrue (val is PrimitiveValue);
			Assert.AreEqual (expected, (val as PrimitiveValue).Value);
		}
	}

	[SetUp]
	public void SetUp () {
		Start (new string [] { "dtest-app.exe" });
	}

	[TearDown]
	public void TearDown () {
		if (vm == null)
			return;

		if (step_req != null)
			step_req.Disable ();

		vm.Resume ();
		while (true) {
			Event e = GetNextEvent ();

			if (e is VMDeathEvent)
				break;

			vm.Resume ();
		}
	}

	[Test]
	public void SimpleBreakpoint () {
		Event e;

		MethodMirror m = entry_point.DeclaringType.GetMethod ("bp1");
		Assert.IsNotNull (m);

		vm.SetBreakpoint (m, 0);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.IsTrue (e is BreakpointEvent);
		Assert.AreEqual (m.Name, (e as BreakpointEvent).Method.Name);

		// Argument checking
		AssertThrows<ArgumentException> (delegate {
				// Invalid IL offset
				vm.SetBreakpoint (m, 2);
			});
	}

	[Test]
	public void BreakpointsSameLocation () {
		MethodMirror m = entry_point.DeclaringType.GetMethod ("bp2");
		Assert.IsNotNull (m);

		vm.SetBreakpoint (m, 0);
		vm.SetBreakpoint (m, 0);

		vm.Resume ();

		var es = vm.GetNextEventSet ();
		Assert.AreEqual (2, es.Events.Length);
		Assert.IsTrue (es [0] is BreakpointEvent);
		Assert.AreEqual (m, (es [0] as BreakpointEvent).Method);

		Assert.IsTrue (es [1] is BreakpointEvent);
		Assert.AreEqual (m.Name, (es [1] as BreakpointEvent).Method.Name);
	}

	[Test]
	public void BreakpointAlreadyJITted () {
		Event e = run_until ("bp1");

		/* Place a breakpoint on bp3 */
		MethodMirror m = entry_point.DeclaringType.GetMethod ("bp3");
		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, 0);

		/* Same with generic instances */
		MethodMirror m2 = entry_point.DeclaringType.GetMethod ("bp7");
		Assert.IsNotNull (m2);
		vm.SetBreakpoint (m2, 0);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.AreEqual (m.Name, (e as BreakpointEvent).Method.Name);

		vm.Resume ();

		/* Non-shared instance */
		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.AreEqual (m2.Name, (e as BreakpointEvent).Method.Name);

		vm.Resume ();

		/* Shared instance */
		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.AreEqual (m2.Name, (e as BreakpointEvent).Method.Name);
	}

	[Test]
	public void ClearBreakpoint () {
		Event e;

		MethodMirror m = entry_point.DeclaringType.GetMethod ("bp4");
		Assert.IsNotNull (m);
		EventRequest req1 = vm.SetBreakpoint (m, 0);
		EventRequest req2 = vm.SetBreakpoint (m, 0);

		MethodMirror m2 = entry_point.DeclaringType.GetMethod ("bp5");
		Assert.IsNotNull (m2);
		vm.SetBreakpoint (m2, 0);

		/* Run until bp4 */
		vm.Resume ();

		var es = vm.GetNextEventSet ();
		Assert.AreEqual (2, es.Events.Length);
		Assert.AreEqual (EventType.Breakpoint, es [0].EventType);
		Assert.AreEqual (m.Name, (es [0] as BreakpointEvent).Method.Name);
		Assert.AreEqual (EventType.Breakpoint, es [1].EventType);
		Assert.AreEqual (m.Name, (es [1] as BreakpointEvent).Method.Name);

		/* Clear one of them */
		req1.Disable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.AreEqual (m.Name, (e as BreakpointEvent).Method.Name);

		/* Clear the other */
		req2.Disable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.AreEqual (m2.Name, (e as BreakpointEvent).Method.Name);
	}

	[Test]
	public void ClearAllBreakpoints () {
		Event e;

		MethodMirror m = entry_point.DeclaringType.GetMethod ("bp4");
		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, 0);

		MethodMirror m2 = entry_point.DeclaringType.GetMethod ("bp5");
		Assert.IsNotNull (m2);
		vm.SetBreakpoint (m2, 0);

		vm.ClearAllBreakpoints ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsTrue (!(e is BreakpointEvent));
		if (e is VMDeathEvent)
			vm = null;
	}

	[Test]
	public void BreakpointOnGShared () {
		Event e;

		MethodMirror m = entry_point.DeclaringType.GetMethod ("bp6");
		Assert.IsNotNull (m);

		vm.SetBreakpoint (m, 0);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.IsTrue (e is BreakpointEvent);
		Assert.AreEqual (m.Name, (e as BreakpointEvent).Method.Name);

		// Breakpoint on an open generic method of a closed generic class (#3422)
		var frame = e.Thread.GetFrames ()[0];
		var ginst = frame.GetValue (frame.Method.GetLocal ("gc"));
		var m2 = (ginst as ObjectMirror).Type.GetMethod ("bp");
		vm.SetBreakpoint (m2, 0);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.IsTrue (e is BreakpointEvent);
		Assert.AreEqual (m2.Name, (e as BreakpointEvent).Method.Name);
	}

	void assert_location (Event e, string method) {
		Assert.IsTrue (e is StepEvent);
		Assert.AreEqual (method, (e as StepEvent).Method.Name);
	}

	[Test]
	public void SingleStepping () {
		Event e = run_until ("single_stepping");

		var req = vm.CreateStepRequest (e.Thread);
		req.Enable ();

		step_req = req;

		// Step over 'bool b = true'
		e = step_once ();
		assert_location (e, "single_stepping");

		// Skip nop
		step_once ();

		// Step into ss1
		e = step_once ();
		assert_location (e, "ss1");

		// Skip }
		e = step_once ();

		// Step out of ss1
		e = step_once ();
		assert_location (e, "single_stepping");

		// Change to step over
		req.Disable ();
		req.Depth = StepDepth.Over;
		req.Enable ();

		// Step over ss2
		e = step_once ();
		assert_location (e, "single_stepping");

		// Change to step into
		req.Disable ();
		req.Depth = StepDepth.Into;
		req.Enable ();

		// Step into ss3
		e = step_once ();
		assert_location (e, "ss3");

		// Change to step out
		req.Disable ();
		req.Depth = StepDepth.Out;
		req.Enable ();

		// Step back into single_stepping
		e = step_once ();
		assert_location (e, "single_stepping");

		// Change to step into
		req.Disable ();
		req.Depth = StepDepth.Into;
		req.Enable ();

		// Step into ss3_2 ()
		e = step_once ();
		assert_location (e, "ss3_2");

		// Change to step over
		req.Disable ();
		req.Depth = StepDepth.Over;
		req.Enable ();

		// Step over ss3_2_2 ()
		e = step_once ();
		assert_location (e, "ss3_2");

		// Recreate the request
		req.Disable ();
		req.Enable ();

		// Skip }
		e = step_once ();

		// Step back into single_stepping () with the new request
		e = step_once ();
		assert_location (e, "single_stepping");

		// Change to step into
		req.Disable ();
		req.Depth = StepDepth.Into;
		req.Enable ();

		// Step into ss4 ()
		e = step_once ();
		assert_location (e, "ss4");

		// Skip nop
		e = step_once ();

		// Change to StepSize.Line
		req.Disable ();
		req.Depth = StepDepth.Over;
		req.Size = StepSize.Line;
		req.Enable ();

		// Step over ss1 (); ss1 ();
		e = step_once ();

		// Step into ss2 ()
		req.Disable ();
		req.Depth = StepDepth.Into;
		req.Enable ();

		e = step_once ();
		assert_location (e, "ss2");

		req.Disable ();

		// Run until ss5
		e = run_until ("ss5");

		// Add an assembly filter
		req.AssemblyFilter = new AssemblyMirror [] { (e as BreakpointEvent).Method.DeclaringType.Assembly };
		req.Enable ();

		// Skip nop
		e = step_once ();

		// Step into is_even, skipping the linq stuff
		e = step_once ();
		assert_location (e, "is_even");

		// FIXME: Check that single stepping works with lock (obj)
		
		req.Disable ();

		// Run until ss6
		e = run_until ("ss6");

		req = vm.CreateStepRequest (e.Thread);
		req.Depth = StepDepth.Over;
		req.Enable ();

		// Check that single stepping works in out-of-line bblocks
		e = step_once ();
		e = step_once ();
		assert_location (e, "ss6");
		req.Disable ();

		// Check that a step over stops at an EH clause
		e = run_until ("ss7_2");
		req = vm.CreateStepRequest (e.Thread);
		req.Depth = StepDepth.Out;
		req.Enable ();
		e = step_once ();
		assert_location (e, "ss7");
		req.Disable ();
		req = vm.CreateStepRequest (e.Thread);
		req.Depth = StepDepth.Over;
		req.Enable ();
		e = step_once ();
		assert_location (e, "ss7");
		req.Disable ();
	}

	[Test]
	public void MethodEntryExit () {
		run_until ("single_stepping");

		var req1 = vm.CreateMethodEntryRequest ();
		var req2 = vm.CreateMethodExitRequest ();

		req1.Enable ();
		req2.Enable ();

		vm.Resume ();
		Event e = GetNextEvent ();
		Assert.IsTrue (e is MethodEntryEvent);
		Assert.AreEqual ("ss1", (e as MethodEntryEvent).Method.Name);

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is MethodExitEvent);
		Assert.AreEqual ("ss1", (e as MethodExitEvent).Method.Name);

		req1.Disable ();
		req2.Disable ();
	}

	[Test]
	public void CountFilter () {
		run_until ("single_stepping");

		MethodMirror m2 = entry_point.DeclaringType.GetMethod ("ss3");
		Assert.IsNotNull (m2);
		vm.SetBreakpoint (m2, 0);

		var req1 = vm.CreateMethodEntryRequest ();
		req1.Count = 2;
		req1.Enable ();

		// Enter ss2, ss1 is skipped
		vm.Resume ();
		Event e = GetNextEvent ();
		Assert.IsTrue (e is MethodEntryEvent);
		Assert.AreEqual ("ss2", (e as MethodEntryEvent).Method.Name);

		// Breakpoint on ss3, the entry event is no longer reported
		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is BreakpointEvent);

		req1.Disable ();
	}

	[Test]
	public void Arguments () {
		object val;

		var e = run_until ("arg1");

		StackFrame frame = e.Thread.GetFrames () [0];

		check_arg_val (frame, 0, typeof (sbyte), SByte.MaxValue - 5);
		check_arg_val (frame, 1, typeof (byte), Byte.MaxValue - 5);
		check_arg_val (frame, 2, typeof (bool), true);
		check_arg_val (frame, 3, typeof (short), Int16.MaxValue - 5);
		check_arg_val (frame, 4, typeof (ushort), UInt16.MaxValue - 5);
		check_arg_val (frame, 5, typeof (char), 'F');
		check_arg_val (frame, 6, typeof (int), Int32.MaxValue - 5);
		check_arg_val (frame, 7, typeof (uint), UInt32.MaxValue - 5);
		check_arg_val (frame, 8, typeof (long), Int64.MaxValue - 5);
		check_arg_val (frame, 9, typeof (ulong), UInt64.MaxValue - 5);
		check_arg_val (frame, 10, typeof (float), 1.2345f);
		check_arg_val (frame, 11, typeof (double), 6.78910);

		e = run_until ("arg2");

		frame = e.Thread.GetFrames () [0];

		// String
		val = frame.GetArgument (0);
		AssertValue ("FOO", val);
		Assert.AreEqual ("String", (val as ObjectMirror).Type.Name);

		// null
		val = frame.GetArgument (1);
		AssertValue (null, val);

		// object
		val = frame.GetArgument (2);
		AssertValue ("BLA", val);

		// byref
		val = frame.GetArgument (3);
		AssertValue (42, val);

		// generic instance
		val = frame.GetArgument (4);
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("GClass`1", (val as ObjectMirror).Type.Name);

		// System.Object
		val = frame.GetArgument (5);
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Object", (val as ObjectMirror).Type.Name);

		// this on static methods
		val = frame.GetThis ();
		AssertValue (null, val);

		e = run_until ("arg3");

		frame = e.Thread.GetFrames () [0];

		// this
		val = frame.GetThis ();
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Tests", (val as ObjectMirror).Type.Name);

		// objref in register
		val = frame.GetArgument (0);
		AssertValue ("BLA", val);
	}

	[Test]
	public void Arrays () {
		object val;

		var e = run_until ("o2");

		StackFrame frame = e.Thread.GetFrames () [0];

		// String[]
		val = frame.GetArgument (0);
		Assert.IsTrue (val is ArrayMirror);
		ArrayMirror arr = val as ArrayMirror;
		Assert.AreEqual (2, arr.Length);
		AssertValue ("BAR", arr [0]);
		AssertValue ("BAZ", arr [1]);

		var vals = arr.GetValues (0, 2);
		Assert.AreEqual (2, vals.Count);
		AssertValue ("BAR", vals [0]);
		AssertValue ("BAZ", vals [1]);

		arr [0] = vm.RootDomain.CreateString ("ABC");
		AssertValue ("ABC", arr [0]);

		arr [0] = vm.CreateValue (null);
		AssertValue (null, arr [0]);

		arr.SetValues (0, new Value [] { vm.RootDomain.CreateString ("D1"), vm.RootDomain.CreateString ("D2") });
		AssertValue ("D1", arr [0]);
		AssertValue ("D2", arr [1]);

		// int
		val = frame.GetArgument (1);
		Assert.IsTrue (val is ArrayMirror);
		arr = val as ArrayMirror;
		Assert.AreEqual (2, arr.Length);
		AssertValue (42, arr [0]);
		AssertValue (43, arr [1]);

		// Argument checking
		AssertThrows<IndexOutOfRangeException> (delegate () {
				val = arr [2];
			});

		AssertThrows<IndexOutOfRangeException> (delegate () {
				val = arr [Int32.MinValue];
			});

		AssertThrows<IndexOutOfRangeException> (delegate () {
				vals = arr.GetValues (0, 3);
			});

		AssertThrows<IndexOutOfRangeException> (delegate () {
				arr [2] = vm.CreateValue (null);
			});

		AssertThrows<IndexOutOfRangeException> (delegate () {
				arr [Int32.MinValue] = vm.CreateValue (null);
			});

		AssertThrows<IndexOutOfRangeException> (delegate () {
				arr.SetValues (0, new Value [] { null, null, null });
			});

		// Multidim arrays
		val = frame.GetArgument (2);
		Assert.IsTrue (val is ArrayMirror);
		arr = val as ArrayMirror;
		Assert.AreEqual (2, arr.Rank);
		Assert.AreEqual (4, arr.Length);
		Assert.AreEqual (2, arr.GetLength (0));
		Assert.AreEqual (2, arr.GetLength (1));
		Assert.AreEqual (0, arr.GetLowerBound (0));
		Assert.AreEqual (0, arr.GetLowerBound (1));
		vals = arr.GetValues (0, 4);
		AssertValue (1, vals [0]);
		AssertValue (2, vals [1]);
		AssertValue (3, vals [2]);
		AssertValue (4, vals [3]);

		val = frame.GetArgument (3);
		Assert.IsTrue (val is ArrayMirror);
		arr = val as ArrayMirror;
		Assert.AreEqual (2, arr.Rank);
		Assert.AreEqual (4, arr.Length);
		Assert.AreEqual (2, arr.GetLength (0));
		Assert.AreEqual (2, arr.GetLength (1));
		Assert.AreEqual (1, arr.GetLowerBound (0));
		Assert.AreEqual (3, arr.GetLowerBound (1));

		AssertThrows<ArgumentOutOfRangeException> (delegate () {
				arr.GetLength (-1);
			});
		AssertThrows<ArgumentOutOfRangeException> (delegate () {
				arr.GetLength (2);
			});

		AssertThrows<ArgumentOutOfRangeException> (delegate () {
				arr.GetLowerBound (-1);
			});
		AssertThrows<ArgumentOutOfRangeException> (delegate () {
				arr.GetLowerBound (2);
			});

		// arrays treated as generic collections
		val = frame.GetArgument (4);
		Assert.IsTrue (val is ArrayMirror);
		arr = val as ArrayMirror;
	}

	[Test]
	public void Object_GetValue () {
		var e = run_until ("o1");
		var frame = e.Thread.GetFrames () [0];

		object val = frame.GetThis ();
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Tests", (val as ObjectMirror).Type.Name);
		ObjectMirror o = (val as ObjectMirror);

		TypeMirror t = o.Type;

		// object fields
		object f = o.GetValue (t.GetField ("field_i"));
		AssertValue (42, f);
		f = o.GetValue (t.GetField ("field_s"));
		AssertValue ("S", f);
		f = o.GetValue (t.GetField ("field_enum"));
		Assert.IsTrue (f is EnumMirror);
		Assert.AreEqual (1, (f as EnumMirror).Value);
		Assert.AreEqual ("B", (f as EnumMirror).StringValue);

		// Inherited object fields
		TypeMirror parent = t.BaseType;
		f = o.GetValue (parent.GetField ("base_field_i"));
		AssertValue (43, f);
		f = o.GetValue (parent.GetField ("base_field_s"));
		AssertValue ("T", f);

		// Static fields
		f = o.GetValue (o.Type.GetField ("static_i"));
		AssertValue (55, f);

		// generic instances
		ObjectMirror o2 = frame.GetValue (frame.Method.GetParameters ()[1]) as ObjectMirror;
		Assert.AreEqual ("GClass`1", o2.Type.Name);
		TypeMirror t2 = o2.Type;
		f = o2.GetValue (t2.GetField ("field"));
		AssertValue (42, f);

		ObjectMirror o3 = frame.GetValue (frame.Method.GetParameters ()[2]) as ObjectMirror;
		Assert.AreEqual ("GClass`1", o3.Type.Name);
		TypeMirror t3 = o3.Type;
		f = o3.GetValue (t3.GetField ("field"));
		AssertValue ("FOO", f);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
			o.GetValue (null);
			});
	}

	[Test]
	public void Object_GetValues () {
		var e = run_until ("o1");
		var frame = e.Thread.GetFrames () [0];

		object val = frame.GetThis ();
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Tests", (val as ObjectMirror).Type.Name);
		ObjectMirror o = (val as ObjectMirror);

		ObjectMirror val2 = frame.GetValue (frame.Method.GetParameters ()[0]) as ObjectMirror;

		TypeMirror t = o.Type;

		object[] vals = o.GetValues (new FieldInfoMirror [] { t.GetField ("field_i"), t.GetField ("field_s") });
		object f = vals [0];
		AssertValue (42, f);
		f = vals [1];
		AssertValue ("S", f);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
			o.GetValues (null);
			});

		AssertThrows<ArgumentNullException> (delegate () {
			o.GetValues (new FieldInfoMirror [] { null });
			});

		// field of another class
		AssertThrows<ArgumentException> (delegate () {
				o.GetValue (val2.Type.GetField ("field_j"));
			});
	}

	void TestSetValue (ObjectMirror o, string field_name, object val) {
		if (val is string)
			o.SetValue (o.Type.GetField (field_name), vm.RootDomain.CreateString ((string)val));
		else
			o.SetValue (o.Type.GetField (field_name), vm.CreateValue (val));
		Value f = o.GetValue (o.Type.GetField (field_name));
		AssertValue (val, f);
	}

	[Test]
	public void Object_SetValues () {
		var e = run_until ("o1");
		var frame = e.Thread.GetFrames () [0];

		object val = frame.GetThis ();
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Tests", (val as ObjectMirror).Type.Name);
		ObjectMirror o = (val as ObjectMirror);

		ObjectMirror val2 = frame.GetValue (frame.Method.GetParameters ()[0]) as ObjectMirror;

		TestSetValue (o, "field_i", 22);
		TestSetValue (o, "field_bool1", false);
		TestSetValue (o, "field_bool2", true);
		TestSetValue (o, "field_char", 'B');
		TestSetValue (o, "field_byte", (byte)129);
		TestSetValue (o, "field_sbyte", (sbyte)-33);
		TestSetValue (o, "field_short", (short)(Int16.MaxValue - 5));
		TestSetValue (o, "field_ushort", (ushort)(UInt16.MaxValue - 5));
		TestSetValue (o, "field_long", Int64.MaxValue - 5);
		TestSetValue (o, "field_ulong", (ulong)(UInt64.MaxValue - 5));
		TestSetValue (o, "field_float", 6.28f);
		TestSetValue (o, "field_double", 6.28);
		TestSetValue (o, "static_i", 23);
		TestSetValue (o, "field_s", "CDEF");

		Value f;

		// intptrs
		f = o.GetValue (o.Type.GetField ("field_intptr"));
		Assert.IsInstanceOfType (typeof (StructMirror), f);
		AssertValue (Int32.MaxValue - 5, (f as StructMirror).Fields [0]);

		// enums
		FieldInfoMirror field = o.Type.GetField ("field_enum");
		f = o.GetValue (field);
		(f as EnumMirror).Value = 5;
		o.SetValue (field, f);
		f = o.GetValue (field);
		Assert.AreEqual (5, (f as EnumMirror).Value);

		// null
		o.SetValue (o.Type.GetField ("field_s"), vm.CreateValue (null));
		f = o.GetValue (o.Type.GetField ("field_s"));
		AssertValue (null, f);

		// vtype instances
		field = o.Type.GetField ("generic_field_struct");
		f = o.GetValue (field);
		o.SetValue (field, f);

		// nullables
		field = o.Type.GetField ("field_nullable");
		f = o.GetValue (field);
		AssertValue (0, (f as StructMirror).Fields [0]);
		AssertValue (false, (f as StructMirror).Fields [1]);
		o.SetValue (field, vm.CreateValue (6));
		f = o.GetValue (field);
		AssertValue (6, (f as StructMirror).Fields [0]);
		AssertValue (true, (f as StructMirror).Fields [1]);
		o.SetValue (field, vm.CreateValue (null));
		f = o.GetValue (field);
		AssertValue (0, (f as StructMirror).Fields [0]);
		AssertValue (false, (f as StructMirror).Fields [1]);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
				o.SetValues (null, new Value [0]);
			});

		AssertThrows<ArgumentNullException> (delegate () {
				o.SetValues (new FieldInfoMirror [0], null);
			});

		AssertThrows<ArgumentNullException> (delegate () {
				o.SetValues (new FieldInfoMirror [] { null }, new Value [1] { null });
			});

		// vtype with a wrong type
		AssertThrows<ArgumentException> (delegate () {
				o.SetValue (o.Type.GetField ("field_struct"), o.GetValue (o.Type.GetField ("field_enum")));
			});

		// reference type not assignment compatible
		AssertThrows<ArgumentException> (delegate () {
				o.SetValue (o.Type.GetField ("field_class"), o);
			});

		// field of another class
		AssertThrows<ArgumentException> (delegate () {
				o.SetValue (val2.Type.GetField ("field_j"), vm.CreateValue (1));
			});
	}

	[Test]
	public void Type_SetValue () {
		var e = run_until ("o1");
		var frame = e.Thread.GetFrames () [0];
		Value f;

		object val = frame.GetThis ();
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Tests", (val as ObjectMirror).Type.Name);
		ObjectMirror o = (val as ObjectMirror);

		ObjectMirror val2 = frame.GetValue (frame.Method.GetParameters ()[0]) as ObjectMirror;

		o.Type.SetValue (o.Type.GetField ("static_i"), vm.CreateValue (55));
		f = o.Type.GetValue (o.Type.GetField ("static_i"));
		AssertValue (55, f);

		o.Type.SetValue (o.Type.GetField ("static_s"), vm.RootDomain.CreateString ("B"));
		f = o.Type.GetValue (o.Type.GetField ("static_s"));
		AssertValue ("B", f);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
				o.Type.SetValue (null, vm.CreateValue (0));
			});

		AssertThrows<ArgumentNullException> (delegate () {
				o.Type.SetValue (o.Type.GetField ("static_i"), null);
			});

		// field of another class
		AssertThrows<ArgumentException> (delegate () {
				o.SetValue (val2.Type.GetField ("field_j"), vm.CreateValue (1));
			});
	}

	[Test]
	public void TypeInfo () {
		Event e = run_until ("ti2");
		StackFrame frame = e.Thread.GetFrames () [0];

		TypeMirror t;

		// Array types
		t = frame.Method.GetParameters ()[0].ParameterType;

		Assert.AreEqual ("String[]", t.Name);
		Assert.AreEqual ("string[]", t.CSharpName);
		Assert.AreEqual ("Array", t.BaseType.Name);
		Assert.AreEqual (true, t.HasElementType);
		Assert.AreEqual (true, t.IsArray);
		Assert.AreEqual (1, t.GetArrayRank ());
		Assert.AreEqual ("String", t.GetElementType ().Name);

		t = frame.Method.GetParameters ()[2].ParameterType;

		Assert.AreEqual ("Int32[,]", t.Name);
		// FIXME:
		//Assert.AreEqual ("int[,]", t.CSharpName);
		Assert.AreEqual ("Array", t.BaseType.Name);
		Assert.AreEqual (true, t.HasElementType);
		Assert.AreEqual (true, t.IsArray);
		Assert.AreEqual (2, t.GetArrayRank ());
		Assert.AreEqual ("Int32", t.GetElementType ().Name);

		// Byref types
		t = frame.Method.GetParameters ()[3].ParameterType;
		// FIXME:
		//Assert.AreEqual ("Int32&", t.Name);
		//Assert.AreEqual (true, t.IsByRef);
		//Assert.AreEqual (true, t.HasElementType);

		// Pointer types
		t = frame.Method.GetParameters ()[4].ParameterType;
		// FIXME:
		//Assert.AreEqual ("Int32*", t.Name);
		Assert.AreEqual (true, t.IsPointer);
		Assert.AreEqual (true, t.HasElementType);
		Assert.AreEqual ("Int32", t.GetElementType ().Name);
		Assert.AreEqual (false, t.IsPrimitive);

		// primitive types 
		t = frame.Method.GetParameters ()[5].ParameterType;
		Assert.AreEqual (true, t.IsPrimitive);

		// value types
		t = frame.Method.GetParameters ()[6].ParameterType;
		Assert.AreEqual ("AStruct", t.Name);
		Assert.AreEqual (false, t.IsPrimitive);
		Assert.AreEqual (true, t.IsValueType);
		Assert.AreEqual (false, t.IsClass);

		// reference types
		t = frame.Method.GetParameters ()[7].ParameterType;
		Assert.AreEqual ("Tests", t.Name);
		var nested = (from nt in t.GetNestedTypes () where nt.IsNestedPublic select nt).ToArray ();
		Assert.AreEqual (1, nested.Length);
		Assert.AreEqual ("NestedClass", nested [0].Name);
		Assert.IsTrue (t.BaseType.IsAssignableFrom (t));
		Assert.IsTrue (!t.IsAssignableFrom (t.BaseType));

		// generic instances
		t = frame.Method.GetParameters ()[9].ParameterType;
		Assert.AreEqual ("GClass`1", t.Name);
		Assert.IsTrue (t.IsGenericType);
		Assert.IsFalse (t.IsGenericTypeDefinition);

		var args = t.GetGenericArguments ();
		Assert.AreEqual (1, args.Length);
		Assert.AreEqual ("Int32", args [0].Name);

		// generic type definitions
		var gtd = t.GetGenericTypeDefinition ();
		Assert.AreEqual ("GClass`1", gtd.Name);
		Assert.IsTrue (gtd.IsGenericType);
		Assert.IsTrue (gtd.IsGenericTypeDefinition);
		Assert.AreEqual (gtd, gtd.GetGenericTypeDefinition ());

		args = gtd.GetGenericArguments ();
		Assert.AreEqual (1, args.Length);
		Assert.AreEqual ("T", args [0].Name);

		// enums
		t = frame.Method.GetParameters ()[10].ParameterType;
		Assert.AreEqual ("AnEnum", t.Name);
		Assert.IsTrue (t.IsEnum);
		Assert.AreEqual ("Int32", t.EnumUnderlyingType.Name);

		// properties
		t = frame.Method.GetParameters ()[7].ParameterType;

		var props = t.GetProperties ();
		Assert.AreEqual (3, props.Length);
		foreach (PropertyInfoMirror prop in props) {
			ParameterInfoMirror[] indexes = prop.GetIndexParameters ();

			if (prop.Name == "IntProperty") {
				Assert.AreEqual ("Int32", prop.PropertyType.Name);
				Assert.AreEqual ("get_IntProperty", prop.GetGetMethod ().Name);
				Assert.AreEqual ("set_IntProperty", prop.GetSetMethod ().Name);
				Assert.AreEqual (0, indexes.Length);
			} else if (prop.Name == "ReadOnlyProperty") {
				Assert.AreEqual ("Int32", prop.PropertyType.Name);
				Assert.AreEqual ("get_ReadOnlyProperty", prop.GetGetMethod ().Name);
				Assert.AreEqual (null, prop.GetSetMethod ());
				Assert.AreEqual (0, indexes.Length);
			} else if (prop.Name == "IndexedProperty") {
				Assert.AreEqual (1, indexes.Length);
				Assert.AreEqual ("Int32", indexes [0].ParameterType.Name);
			}
		}

		// custom attributes
		t = frame.Method.GetParameters ()[8].ParameterType;
		Assert.AreEqual ("Tests2", t.Name);
		var attrs = t.GetCustomAttributes (true);
		Assert.AreEqual (3, attrs.Length);
		foreach (var attr in attrs) {
			if (attr.Constructor.DeclaringType.Name == "DebuggerDisplayAttribute") {
				Assert.AreEqual (1, attr.ConstructorArguments.Count);
				Assert.AreEqual ("Tests", attr.ConstructorArguments [0].Value);
				Assert.AreEqual (2, attr.NamedArguments.Count);
				Assert.AreEqual ("Name", attr.NamedArguments [0].Property.Name);
				Assert.AreEqual ("FOO", attr.NamedArguments [0].TypedValue.Value);
				Assert.AreEqual ("Target", attr.NamedArguments [1].Property.Name);
				Assert.IsInstanceOfType (typeof (TypeMirror), attr.NamedArguments [1].TypedValue.Value);
				Assert.AreEqual ("Int32", (attr.NamedArguments [1].TypedValue.Value as TypeMirror).Name);
			} else if (attr.Constructor.DeclaringType.Name == "DebuggerTypeProxyAttribute") {
				Assert.AreEqual (1, attr.ConstructorArguments.Count);
				Assert.IsInstanceOfType (typeof (TypeMirror), attr.ConstructorArguments [0].Value);
				Assert.AreEqual ("Tests", (attr.ConstructorArguments [0].Value as TypeMirror).Name);
			} else if (attr.Constructor.DeclaringType.Name == "BAttribute") {
				Assert.AreEqual (2, attr.NamedArguments.Count);
				Assert.AreEqual ("afield", attr.NamedArguments [0].Field.Name);
				Assert.AreEqual ("bfield", attr.NamedArguments [1].Field.Name);
			} else {
				Assert.Fail (attr.Constructor.DeclaringType.Name);
			}
		}

		var assembly = entry_point.DeclaringType.Assembly;
		var type = assembly.GetType ("Tests4");
		Assert.IsFalse (type.IsInitialized);
	}

	[Test]
	public void FieldInfo () {
		Event e = run_until ("ti2");
		StackFrame frame = e.Thread.GetFrames () [0];

		TypeMirror t;

		t = frame.Method.GetParameters ()[8].ParameterType;
		Assert.AreEqual ("Tests2", t.Name);

		var fi = t.GetField ("field_j");
		var attrs = fi.GetCustomAttributes (true);
		Assert.AreEqual (1, attrs.Length);
		var attr = attrs [0];
		Assert.AreEqual ("DebuggerBrowsableAttribute", attr.Constructor.DeclaringType.Name);
		Assert.AreEqual (1, attr.ConstructorArguments.Count);
		Assert.IsInstanceOfType (typeof (EnumMirror), attr.ConstructorArguments [0].Value);
		Assert.AreEqual ((int)System.Diagnostics.DebuggerBrowsableState.Collapsed, (attr.ConstructorArguments [0].Value as EnumMirror).Value);
	}

	[Test]
	public void PropertyInfo () {
		Event e = run_until ("ti2");
		StackFrame frame = e.Thread.GetFrames () [0];

		TypeMirror t;

		t = frame.Method.GetParameters ()[8].ParameterType;
		Assert.AreEqual ("Tests2", t.Name);

		var pi = t.GetProperty ("AProperty");
		var attrs = pi.GetCustomAttributes (true);
		Assert.AreEqual (1, attrs.Length);
		var attr = attrs [0];
		Assert.AreEqual ("DebuggerBrowsableAttribute", attr.Constructor.DeclaringType.Name);
		Assert.AreEqual (1, attr.ConstructorArguments.Count);
		Assert.IsInstanceOfType (typeof (EnumMirror), attr.ConstructorArguments [0].Value);
		Assert.AreEqual ((int)System.Diagnostics.DebuggerBrowsableState.Collapsed, (attr.ConstructorArguments [0].Value as EnumMirror).Value);
	}

	[Test]
	[Category ("only5")]
	public void Type_GetValue () {
		Event e = run_until ("o1");
		StackFrame frame = e.Thread.GetFrames () [0];

		ObjectMirror o = (frame.GetThis () as ObjectMirror);

		TypeMirror t = o.Type;

		ObjectMirror val2 = frame.GetValue (frame.Method.GetParameters ()[0]) as ObjectMirror;

		// static fields
		object f = t.GetValue (o.Type.GetField ("static_i"));
		AssertValue (55, f);

		f = t.GetValue (o.Type.GetField ("static_s"));
		AssertValue ("A", f);

		// literal static fields
		f = t.GetValue (o.Type.GetField ("literal_i"));
		AssertValue (56, f);

		f = t.GetValue (o.Type.GetField ("literal_s"));
		AssertValue ("B", f);

		// Inherited static fields
		TypeMirror parent = t.BaseType;
		f = t.GetValue (parent.GetField ("base_static_i"));
		AssertValue (57, f);

		f = t.GetValue (parent.GetField ("base_static_s"));
		AssertValue ("C", f);

		// thread static field
		f = t.GetValue (t.GetField ("tls_i"), e.Thread);
		AssertValue (42, f);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
			t.GetValue (null);
			});

		// instance fields
		AssertThrows<ArgumentException> (delegate () {
			t.GetValue (o.Type.GetField ("field_i"));
			});

		// field on another type
		AssertThrows<ArgumentException> (delegate () {
				t.GetValue (val2.Type.GetField ("static_field_j"));
			});

		// special static field
		AssertThrows<ArgumentException> (delegate () {
				t.GetValue (t.GetField ("tls_i"));
			});
	}

	[Test]
	public void Type_GetValues () {
		Event e = run_until ("o1");
		StackFrame frame = e.Thread.GetFrames () [0];

		ObjectMirror o = (frame.GetThis () as ObjectMirror);

		TypeMirror t = o.Type;

		// static fields
		object[] vals = t.GetValues (new FieldInfoMirror [] { t.GetField ("static_i"), t.GetField ("static_s") });
		object f = vals [0];
		AssertValue (55, f);

		f = vals [1];
		AssertValue ("A", f);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
			t.GetValues (null);
			});

		AssertThrows<ArgumentNullException> (delegate () {
			t.GetValues (new FieldInfoMirror [] { null });
			});
	}

	[Test]
	public void ObjRefs () {
		Event e = run_until ("objrefs1");
		StackFrame frame = e.Thread.GetFrames () [0];

		ObjectMirror o = frame.GetThis () as ObjectMirror;
		ObjectMirror child = o.GetValue (o.Type.GetField ("child")) as ObjectMirror;

		Assert.IsTrue (child.Address != 0);

		// Check that object references are internalized correctly
		Assert.AreEqual (o, frame.GetThis ());

		run_until ("objrefs2");

		// child should be gc'd now
		// This is not deterministic
		//Assert.IsTrue (child.IsCollected);

		/*
		 * No longer works since Type is read eagerly
		 */
		/*
		AssertThrows<ObjectCollectedException> (delegate () {
			TypeMirror t = child.Type;
			});
		*/
		/*
		AssertThrows<ObjectCollectedException> (delegate () {
				long addr = child.Address;
			});
		*/
	}

	[Test]
	public void Type_GetObject () {
		Event e = run_until ("o1");
		StackFrame frame = e.Thread.GetFrames () [0];

		ObjectMirror o = (frame.GetThis () as ObjectMirror);

		TypeMirror t = o.Type;

		Assert.AreEqual ("MonoType", t.GetTypeObject ().Type.Name);
	}

	[Test]
	public void VTypes () {
		Event e = run_until ("vtypes1");
		StackFrame frame = e.Thread.GetFrames () [0];

		// vtypes as fields
		ObjectMirror o = frame.GetThis () as ObjectMirror;
		var obj = o.GetValue (o.Type.GetField ("field_struct"));
		Assert.IsTrue (obj is StructMirror);
		var s = obj as StructMirror;
		Assert.AreEqual ("AStruct", s.Type.Name);
		AssertValue (42, s ["i"]);
		obj = s ["s"];
		AssertValue ("S", obj);
		AssertValue (43, s ["k"]);
		obj = o.GetValue (o.Type.GetField ("field_boxed_struct"));
		Assert.IsTrue (obj is StructMirror);
		s = obj as StructMirror;
		Assert.AreEqual ("AStruct", s.Type.Name);
		AssertValue (42, s ["i"]);

		// vtypes as arguments
		s = frame.GetArgument (0) as StructMirror;
		AssertValue (44, s ["i"]);
		obj = s ["s"];
		AssertValue ("T", obj);
		AssertValue (45, s ["k"]);

		// vtypes as array entries
		var arr = frame.GetArgument (1) as ArrayMirror;
		obj = arr [0];
		Assert.IsTrue (obj is StructMirror);
		s = obj as StructMirror;
		AssertValue (1, s ["i"]);
		AssertValue ("S1", s ["s"]);
		obj = arr [1];
		Assert.IsTrue (obj is StructMirror);
		s = obj as StructMirror;
		AssertValue (2, s ["i"]);
		AssertValue ("S2", s ["s"]);

		// Argument checking
		s = frame.GetArgument (0) as StructMirror;
		AssertThrows<ArgumentException> (delegate () {
				obj = s ["FOO"];
			});

		// generic vtype instances
		o = frame.GetThis () as ObjectMirror;
		obj = o.GetValue (o.Type.GetField ("generic_field_struct"));
		Assert.IsTrue (obj is StructMirror);
		s = obj as StructMirror;
		Assert.AreEqual ("GStruct`1", s.Type.Name);
		AssertValue (42, s ["i"]);

		// this on vtype methods
		e = run_until ("vtypes2");
		
		// Skip nop
		e = single_step (e.Thread);

		e = single_step (e.Thread);

		frame = e.Thread.GetFrames () [0];

		Assert.AreEqual ("foo", (e as StepEvent).Method.Name);
		obj = frame.GetThis ();

		Assert.IsTrue (obj is StructMirror);
		s = obj as StructMirror;
		AssertValue (44, s ["i"]);
		AssertValue ("T", s ["s"]);
		AssertValue (45, s ["k"]);

		// this on static vtype methods
		e = run_until ("vtypes3");

		// Skip nop
		e = single_step (e.Thread);

		e = single_step (e.Thread);

		frame = e.Thread.GetFrames () [0];

		Assert.AreEqual ("static_foo", (e as StepEvent).Method.Name);
		obj = frame.GetThis ();
		AssertValue (null, obj);
	}

	[Test]
	public void AssemblyInfo () {
		Event e = run_until ("single_stepping");

		StackFrame frame = e.Thread.GetFrames () [0];

		var aname = frame.Method.DeclaringType.Assembly.GetName ();
		Assert.AreEqual ("dtest-app, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", aname.ToString ());

		ModuleMirror m = frame.Method.DeclaringType.Module;

		Assert.AreEqual ("dtest-app.exe", m.Name);
		Assert.AreEqual ("dtest-app.exe", m.ScopeName);
		Assert.IsTrue (m.FullyQualifiedName.IndexOf ("dtest-app.exe") != -1);
		Guid guid = m.ModuleVersionId;
		Assert.AreEqual (frame.Method.DeclaringType.Assembly, m.Assembly);
		Assert.AreEqual (frame.Method.DeclaringType.Assembly.ManifestModule, m);

		// This is no longer true on 4.0
		//Assert.AreEqual ("Assembly", frame.Method.DeclaringType.Assembly.GetAssemblyObject ().Type.Name);

		TypeMirror t = vm.RootDomain.Corlib.GetType ("System.Diagnostics.DebuggerDisplayAttribute");
		Assert.AreEqual ("DebuggerDisplayAttribute", t.Name);
	}

	[Test]
	public void LocalsInfo () {
		Event e = run_until ("locals2");

		StackFrame frame = e.Thread.GetFrames () [0];

		var locals = frame.Method.GetLocals ();
		Assert.AreEqual (7, locals.Length);
		for (int i = 0; i < 7; ++i) {
			if (locals [i].Name == "args") {
				Assert.IsTrue (locals [i].IsArg);
				Assert.AreEqual ("String[]", locals [i].Type.Name);
			} else if (locals [i].Name == "arg") {
				Assert.IsTrue (locals [i].IsArg);
				Assert.AreEqual ("Int32", locals [i].Type.Name);
			} else if (locals [i].Name == "i") {
				Assert.IsFalse (locals [i].IsArg);
				Assert.AreEqual ("Int64", locals [i].Type.Name);
			} else if (locals [i].Name == "j") {
				Assert.IsFalse (locals [i].IsArg);
				Assert.AreEqual ("Int32", locals [i].Type.Name);
			} else if (locals [i].Name == "s") {
				Assert.IsFalse (locals [i].IsArg);
				Assert.AreEqual ("String", locals [i].Type.Name);
			} else if (locals [i].Name == "t") {
				// gshared
				Assert.IsTrue (locals [i].IsArg);
				Assert.AreEqual ("String", locals [i].Type.Name);
			} else if (locals [i].Name == "rs") {
				Assert.IsTrue (locals [i].IsArg);
				Assert.AreEqual ("String", locals [i].Type.Name);
			} else {
				Assert.Fail ();
			}
		}
	}

	Event step_once () {
		vm.Resume ();
		var e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		return e;
	}

	[Test]
	public void Locals () {
		var be = run_until ("locals1");

		StackFrame frame = be.Thread.GetFrames () [0];
		MethodMirror m1 = frame.Method;

		// Compiler generated byref local
		foreach (var l in m1.GetLocals ()) {
			// The byval flag is hidden from the type
			if (l.Name != "ri" && l.Type.Name == "Double")
				AssertValue (null, frame.GetValue (l));
		}

		be = run_until ("locals2");

		frame = be.Thread.GetFrames () [0];

		object val = frame.GetValue (frame.Method.GetLocal ("i"));
		AssertValue (0, val);

		var req = vm.CreateStepRequest (be.Thread);
		req.Enable ();
		step_req = req;

		// Skip nop
		step_once ();

		// Execute i = 42
		var e = step_once ();
		Assert.AreEqual ("locals2", (e as StepEvent).Method.Name);

		// Execute s = "AB";
		e = step_once ();
		Assert.AreEqual ("locals2", (e as StepEvent).Method.Name);

		frame = e.Thread.GetFrames () [0];

		val = frame.GetValue (frame.Method.GetLocal ("i"));
		AssertValue (42, val);

		LocalVariable[] locals = frame.Method.GetLocals ();
		var vals = frame.GetValues (locals);
		Assert.AreEqual (locals.Length, vals.Length);
		for (int i = 0; i < locals.Length; ++i) {
			if (locals [i].Name == "i")
				AssertValue (42, vals [i]);
			if (locals [i].Name == "s")
				AssertValue ("AB", vals [i]);
			if (locals [i].Name == "t")
				AssertValue ("ABC", vals [i]);
		}

		// Argument checking

		// GetValue () null
		AssertThrows<ArgumentNullException> (delegate () {
				frame.GetValue ((LocalVariable)null);
			});
		// GetValue () local from another method
		AssertThrows<ArgumentException> (delegate () {
				frame.GetValue (m1.GetLocal ("foo"));
			});

		// GetValue () null
		AssertThrows<ArgumentNullException> (delegate () {
				frame.GetValue ((ParameterInfoMirror)null);
			});
		// GetValue () local from another method
		AssertThrows<ArgumentException> (delegate () {
				frame.GetValue (m1.GetParameters ()[0]);
			});

		// GetValues () null
		AssertThrows<ArgumentNullException> (delegate () {
				frame.GetValues (null);
			});
		// GetValues () embedded null
		AssertThrows<ArgumentNullException> (delegate () {
				frame.GetValues (new LocalVariable [] { null });
			});
		// GetValues () local from another method
		AssertThrows<ArgumentException> (delegate () {
				frame.GetValues (new LocalVariable [] { m1.GetLocal ("foo") });
			});
		// return value
		AssertThrows<ArgumentException> (delegate () {
				val = frame.GetValue (frame.Method.ReturnParameter);
			});

		// invalid stack frames
		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		Assert.AreEqual ("locals2", (e as StepEvent).Method.Name);

		AssertThrows<InvalidStackFrameException> (delegate () {
				frame.GetValue (frame.Method.GetLocal ("i"));
			});

		req.Disable ();
	}

	[Test]
	public void GetVisibleVariables () {
		Event e = run_until ("locals4");

		// First scope
		var locals = e.Thread.GetFrames ()[1].GetVisibleVariables ();
		Assert.AreEqual (2, locals.Count);
		var loc = locals.First (l => l.Name == "i");
		Assert.AreEqual ("Int64", loc.Type.Name);
		loc = locals.First (l => l.Name == "s");
		Assert.AreEqual ("String", loc.Type.Name);

		loc = e.Thread.GetFrames ()[1].GetVisibleVariableByName ("i");
		Assert.AreEqual ("i", loc.Name);
		Assert.AreEqual ("Int64", loc.Type.Name);

		e = run_until ("locals5");

		// Second scope
		locals = e.Thread.GetFrames ()[1].GetVisibleVariables ();
		Assert.AreEqual (2, locals.Count);
		loc = locals.First (l => l.Name == "i");
		Assert.AreEqual ("String", loc.Type.Name);
		loc = locals.First (l => l.Name == "s");
		Assert.AreEqual ("String", loc.Type.Name);

		loc = e.Thread.GetFrames ()[1].GetVisibleVariableByName ("i");
		Assert.AreEqual ("i", loc.Name);
		Assert.AreEqual ("String", loc.Type.Name);

		// Variable in another scope
		loc = e.Thread.GetFrames ()[1].GetVisibleVariableByName ("j");
		Assert.IsNull (loc);
	}

	[Test]
	public void Exit () {
		run_until ("Main");

		vm.Exit (5);

		var e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (VMDeathEvent), e);

		var p = vm.Process;
		/* Could be a remote vm with no process */
		if (p != null) {
			p.WaitForExit ();
			Assert.AreEqual (5, p.ExitCode);

			// error handling
			AssertThrows<VMDisconnectedException> (delegate () {
					vm.Resume ();
				});
		}

		vm = null;
	}

	[Test]
	public void Dispose () {
		run_until ("Main");

		vm.Detach ();

		var e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (VMDisconnectEvent), e);

		var p = vm.Process;
		/* Could be a remote vm with no process */
		if (p != null) {
			p.WaitForExit ();
			Assert.AreEqual (3, p.ExitCode);

			// error handling
			AssertThrows<VMDisconnectedException> (delegate () {
					vm.Resume ();
				});
		}

		vm = null;
	}

	[Test]
	public void ColumnNumbers () {
		Event e = run_until ("line_numbers");

		// FIXME: Merge this with LineNumbers () when its fixed

		step_req = vm.CreateStepRequest (e.Thread);
		step_req.Depth = StepDepth.Into;
		step_req.Enable ();

		Location l;
		
		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);

		l = e.Thread.GetFrames ()[0].Location;

		Assert.AreEqual (3, l.ColumnNumber);

		step_req.Disable ();
	}

	[Test]
	// Broken by mcs+runtime changes (#5438)
	[Category("NotWorking")]
	public void LineNumbers () {
		Event e = run_until ("line_numbers");

		step_req = vm.CreateStepRequest (e.Thread);
		step_req.Depth = StepDepth.Into;
		step_req.Enable ();

		Location l;
		
		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);

		l = e.Thread.GetFrames ()[0].Location;

		Assert.IsTrue (l.SourceFile.IndexOf ("dtest-app.cs") != -1);
		Assert.AreEqual ("ln1", l.Method.Name);

		// Check hash
		using (FileStream fs = new FileStream (l.SourceFile, FileMode.Open, FileAccess.Read)) {
			MD5 md5 = MD5.Create ();
			var hash = md5.ComputeHash (fs);

			for (int i = 0; i < 16; ++i)
				Assert.AreEqual (hash [i], l.SourceFileHash [i]);
		}
		
		int line_base = l.LineNumber;

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		l = e.Thread.GetFrames ()[0].Location;
		Assert.AreEqual ("ln2", l.Method.Name);
		Assert.AreEqual (line_base + 6, l.LineNumber);

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		l = e.Thread.GetFrames ()[0].Location;
		Assert.AreEqual ("ln1", l.Method.Name);
		Assert.AreEqual (line_base + 1, l.LineNumber);

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		l = e.Thread.GetFrames ()[0].Location;
		Assert.AreEqual ("ln3", l.Method.Name);
		Assert.AreEqual (line_base + 11, l.LineNumber);

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		l = e.Thread.GetFrames ()[0].Location;
		Assert.AreEqual ("ln3", l.Method.Name);
		Assert.IsTrue (l.SourceFile.EndsWith ("FOO"));
		Assert.AreEqual (55, l.LineNumber);

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		l = e.Thread.GetFrames ()[0].Location;
		Assert.AreEqual ("ln1", l.Method.Name);
		Assert.AreEqual (line_base + 2, l.LineNumber);

		// GetSourceFiles ()
		string[] sources = l.Method.DeclaringType.GetSourceFiles ();
		Assert.AreEqual (2, sources.Length);
		Assert.AreEqual ("dtest-app.cs", sources [0]);
		Assert.AreEqual ("FOO", sources [1]);

		sources = l.Method.DeclaringType.GetSourceFiles (true);
		Assert.AreEqual (2, sources.Length);
		Assert.IsTrue (sources [0].EndsWith ("dtest-app.cs"));
		Assert.IsTrue (sources [1].EndsWith ("FOO"));
	}

	[Test]
	public void Suspend () {
		vm.Detach ();

		Start (new string [] { "dtest-app.exe", "suspend-test" });

		Event e = run_until ("suspend");

		ThreadMirror main = e.Thread;

		vm.Resume ();

		Thread.Sleep (100);

		vm.Suspend ();

		// The debuggee should be suspended while it is running the infinite loop
		// in suspend ()
		StackFrame frame = main.GetFrames ()[0];
		Assert.AreEqual ("suspend", frame.Method.Name);

		vm.Resume ();

		// resuming when not suspended
		AssertThrows<InvalidOperationException> (delegate () {
				vm.Resume ();
			});

		vm.Exit (0);

		vm = null;
	}

	[Test]
	public void AssemblyLoad () {
		Event e = run_until ("assembly_load");

		var load_req = vm.CreateAssemblyLoadRequest ();
		load_req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (AssemblyLoadEvent), e);
		Assert.IsTrue ((e as AssemblyLoadEvent).Assembly.Location.EndsWith ("System.dll"));

		var frames = e.Thread.GetFrames ();
		Assert.IsTrue (frames.Length > 0);
		Assert.AreEqual ("assembly_load", frames [0].Method.Name);
	}

	[Test]
	public void CreateValue () {
		PrimitiveValue v;

		v = vm.CreateValue (1);
		Assert.AreEqual (vm, v.VirtualMachine);
		Assert.AreEqual (1, v.Value);

		v = vm.CreateValue (null);
		Assert.AreEqual (vm, v.VirtualMachine);
		Assert.AreEqual (null, v.Value);

		// Argument checking
		AssertThrows <ArgumentException> (delegate () {
				v = vm.CreateValue ("FOO");
			});
	}

	[Test]
	public void CreateString () {
		StringMirror s = vm.RootDomain.CreateString ("ABC");

		Assert.AreEqual (vm, s.VirtualMachine);
		Assert.AreEqual ("ABC", s.Value);
		Assert.AreEqual (vm.RootDomain, s.Domain);

		// Long strings
		StringBuilder sb = new StringBuilder ();
		for (int i = 0; i < 1024; ++i)
			sb.Append ('A');
		s = vm.RootDomain.CreateString (sb.ToString ());

		// Argument checking
		AssertThrows <ArgumentNullException> (delegate () {
				s = vm.RootDomain.CreateString (null);
			});
	}

	[Test]
	public void CreateBoxedValue () {
		ObjectMirror o = vm.RootDomain.CreateBoxedValue (new PrimitiveValue (vm, 42));

		Assert.AreEqual ("Int32", o.Type.Name);
		//AssertValue (42, m.GetValue (o.Type.GetField ("m_value")));

		// Argument checking
		AssertThrows <ArgumentNullException> (delegate () {
				vm.RootDomain.CreateBoxedValue (null);
			});

		AssertThrows <ArgumentException> (delegate () {
				vm.RootDomain.CreateBoxedValue (o);
			});
	}

	[Test]
	public void Invoke () {
		Event e = run_until ("invoke1");

		StackFrame frame = e.Thread.GetFrames () [0];

		TypeMirror t = frame.Method.DeclaringType;
		ObjectMirror this_obj = (ObjectMirror)frame.GetThis ();

		TypeMirror t2 = frame.Method.GetParameters ()[0].ParameterType;

		MethodMirror m;
		Value v;

		// return void
		m = t.GetMethod ("invoke_return_void");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		Assert.IsNull (v);

		// return ref
		m = t.GetMethod ("invoke_return_ref");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		AssertValue ("ABC", v);

		// return null
		m = t.GetMethod ("invoke_return_null");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		AssertValue (null, v);

		// return primitive
		m = t.GetMethod ("invoke_return_primitive");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		AssertValue (42, v);

		// return nullable
		m = t.GetMethod ("invoke_return_nullable");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		Assert.IsInstanceOfType (typeof (StructMirror), v);
		var s = v as StructMirror;
		AssertValue (42, s.Fields [0]);
		AssertValue (true, s.Fields [1]);

		// pass nullable as this
		//m = vm.RootDomain.Corlib.GetType ("System.Object").GetMethod ("ToString");
		m = s.Type.GetMethod ("ToString");
		v = s.InvokeMethod (e.Thread, m, null);

		// return nullable null
		m = t.GetMethod ("invoke_return_nullable_null");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		Assert.IsInstanceOfType (typeof (StructMirror), v);
		s = v as StructMirror;
		AssertValue (0, s.Fields [0]);
		AssertValue (false, s.Fields [1]);

		// pass nullable as this
		//m = vm.RootDomain.Corlib.GetType ("System.Object").GetMethod ("ToString");
		m = s.Type.GetMethod ("ToString");
		v = s.InvokeMethod (e.Thread, m, null);

		// pass primitive
		m = t.GetMethod ("invoke_pass_primitive");
		Value[] args = new Value [] {
			vm.CreateValue ((byte)Byte.MaxValue),
			vm.CreateValue ((sbyte)SByte.MaxValue),
			vm.CreateValue ((short)1),
			vm.CreateValue ((ushort)1),
			vm.CreateValue ((int)1),
			vm.CreateValue ((uint)1),
			vm.CreateValue ((long)1),
			vm.CreateValue ((ulong)1),
			vm.CreateValue ('A'),
			vm.CreateValue (true),
			vm.CreateValue (3.14f),
			vm.CreateValue (3.14) };

		v = this_obj.InvokeMethod (e.Thread, m, args);
		AssertValue ((int)Byte.MaxValue + (int)SByte.MaxValue + 1 + 1 + 1 + 1 + 1 + 1 + 'A' + 1 + 3 + 3, v);

		// pass ref
		m = t.GetMethod ("invoke_pass_ref");
		v = this_obj.InvokeMethod (e.Thread, m, new Value [] { vm.RootDomain.CreateString ("ABC") });
		AssertValue ("ABC", v);

		// pass null
		m = t.GetMethod ("invoke_pass_ref");
		v = this_obj.InvokeMethod (e.Thread, m, new Value [] { vm.CreateValue (null) });
		AssertValue (null, v);

		// static
		m = t.GetMethod ("invoke_static_pass_ref");
		v = t.InvokeMethod (e.Thread, m, new Value [] { vm.RootDomain.CreateString ("ABC") });
		AssertValue ("ABC", v);

		// static invoked using ObjectMirror.InvokeMethod
		m = t.GetMethod ("invoke_static_pass_ref");
		v = this_obj.InvokeMethod (e.Thread, m, new Value [] { vm.RootDomain.CreateString ("ABC") });
		AssertValue ("ABC", v);

		// method which throws an exception
		try {
			m = t.GetMethod ("invoke_throws");
			v = this_obj.InvokeMethod (e.Thread, m, null);
			Assert.Fail ();
		} catch (InvocationException ex) {
			Assert.AreEqual ("Exception", ex.Exception.Type.Name);
		}

		// newobj
		m = t.GetMethod (".ctor");
		v = t.InvokeMethod (e.Thread, m, null);
		Assert.IsInstanceOfType (typeof (ObjectMirror), v);
		Assert.AreEqual ("Tests", (v as ObjectMirror).Type.Name);

		// Argument checking
		
		// null thread
		AssertThrows<ArgumentNullException> (delegate {
				m = t.GetMethod ("invoke_pass_ref");
				v = this_obj.InvokeMethod (null, m, new Value [] { vm.CreateValue (null) });				
			});

		// null method
		AssertThrows<ArgumentNullException> (delegate {
				v = this_obj.InvokeMethod (e.Thread, null, new Value [] { vm.CreateValue (null) });				
			});

		// invalid number of arguments
		m = t.GetMethod ("invoke_pass_ref");
		AssertThrows<ArgumentException> (delegate {
				v = this_obj.InvokeMethod (e.Thread, m, null);
			});

		// invalid type of argument (ref != primitive)
		m = t.GetMethod ("invoke_pass_ref");
		AssertThrows<ArgumentException> (delegate {
				v = this_obj.InvokeMethod (e.Thread, m, new Value [] { vm.CreateValue (1) });
			});

		// invalid type of argument (primitive != primitive)
		m = t.GetMethod ("invoke_pass_primitive_2");
		AssertThrows<ArgumentException> (delegate {
				v = this_obj.InvokeMethod (e.Thread, m, new Value [] { vm.CreateValue (1) });
			});

		// invoking a non-static method as static
		m = t.GetMethod ("invoke_pass_ref");
		AssertThrows<ArgumentException> (delegate {
				v = t.InvokeMethod (e.Thread, m, new Value [] { vm.RootDomain.CreateString ("ABC") });
			});

		// invoking a method defined in another class
		m = t2.GetMethod ("invoke");
		AssertThrows<ArgumentException> (delegate {
				v = this_obj.InvokeMethod (e.Thread, m, null);
			});
	}

	[Test]
	public void InvokeVType () {
		Event e = run_until ("invoke1");

		StackFrame frame = e.Thread.GetFrames () [0];

		var s = frame.GetArgument (1) as StructMirror;

		TypeMirror t = s.Type;

		MethodMirror m;
		Value v;

		// Pass struct as this, receive int
		m = t.GetMethod ("invoke_return_int");
		v = s.InvokeMethod (e.Thread, m, null);
		AssertValue (42, v);

		// Pass struct as this, receive intptr
		m = t.GetMethod ("invoke_return_intptr");
		v = s.InvokeMethod (e.Thread, m, null);
		AssertValue (43, v);

		// Static method
		m = t.GetMethod ("invoke_static");
		v = t.InvokeMethod (e.Thread, m, null);
		AssertValue (5, v);

		// Pass generic struct as this
		s = frame.GetArgument (2) as StructMirror;
		t = s.Type;
		m = t.GetMethod ("invoke_return_int");
		v = s.InvokeMethod (e.Thread, m, null);
		AssertValue (42, v);
	}

	[Test]
	public void BreakpointDuringInvoke () {
		Event e = run_until ("invoke1");

		MethodMirror m = entry_point.DeclaringType.GetMethod ("invoke2");
		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, 0);

		StackFrame frame = e.Thread.GetFrames () [0];
		var o = frame.GetThis () as ObjectMirror;

		bool failed = false;

		bool finished = false;
		object wait = new object ();

		// Have to invoke in a separate thread as the invoke is suspended until we
		// resume after the breakpoint
		Thread t = new Thread (delegate () {
				try {
					o.InvokeMethod (e.Thread, m, null);
				} catch {
					failed = true;
				}
				lock (wait) {
					finished = true;
					Monitor.Pulse (wait);
				}
			});

		t.Start ();

		StackFrame invoke_frame = null;

		try {
			e = GetNextEvent ();
			Assert.IsInstanceOfType (typeof (BreakpointEvent), e);
			// Check stack trace support and invokes
			var frames = e.Thread.GetFrames ();
			invoke_frame = frames [0];
			Assert.AreEqual ("invoke2", frames [0].Method.Name);
			Assert.IsTrue (frames [0].IsDebuggerInvoke);
			Assert.AreEqual ("invoke1", frames [1].Method.Name);
		} finally {
			vm.Resume ();
		}

		lock (wait) {
			if (!finished)
				Monitor.Wait (wait);
		}

		// Check that the invoke frames are no longer valid
		AssertThrows<InvalidStackFrameException> (delegate {
				invoke_frame.GetThis ();
			});

		// Check InvokeOptions.DisableBreakpoints flag
		o.InvokeMethod (e.Thread, m, null, InvokeOptions.DisableBreakpoints);
	}

	[Test]
	public void DisabledExceptionDuringInvoke () {
		Event e = run_until ("invoke_ex");

		MethodMirror m = entry_point.DeclaringType.GetMethod ("invoke_ex_inner");

		StackFrame frame = e.Thread.GetFrames () [0];
		var o = frame.GetThis () as ObjectMirror;

		var req = vm.CreateExceptionRequest (null);
		req.Enable ();

		// Check InvokeOptions.DisableBreakpoints flag
		o.InvokeMethod (e.Thread, m, null, InvokeOptions.DisableBreakpoints);

		req.Disable ();
	}

	[Test]
	public void InvokeSingleThreaded () {
		vm.Detach ();

		Start (new string [] { "dtest-app.exe", "invoke-single-threaded" });

		Event e = run_until ("invoke_single_threaded_2");

		StackFrame f = e.Thread.GetFrames ()[0];

		var obj = f.GetThis () as ObjectMirror;

		// Check that the counter value incremented by the other thread does not increase
		// during the invoke.
		object counter1 = (obj.GetValue (obj.Type.GetField ("counter")) as PrimitiveValue).Value;

		var m = obj.Type.GetMethod ("invoke_return_void");
		obj.InvokeMethod (e.Thread, m, null, InvokeOptions.SingleThreaded);

	    object counter2 = (obj.GetValue (obj.Type.GetField ("counter")) as PrimitiveValue).Value;

		Assert.AreEqual ((int)counter1, (int)counter2);

		// Test multiple invokes done in succession
		m = obj.Type.GetMethod ("invoke_return_void");
		obj.InvokeMethod (e.Thread, m, null, InvokeOptions.SingleThreaded);

		// Test events during single-threaded invokes
		vm.EnableEvents (EventType.TypeLoad);
		m = obj.Type.GetMethod ("invoke_type_load");
		obj.BeginInvokeMethod (e.Thread, m, null, InvokeOptions.SingleThreaded, delegate {
				vm.Resume ();
			}, null);

		e = GetNextEvent ();
		Assert.AreEqual (EventType.TypeLoad, e.EventType);
	}

	List<Value> invoke_results;

	[Test]
	public void InvokeMultiple () {
		Event e = run_until ("invoke1");

		StackFrame frame = e.Thread.GetFrames () [0];

		TypeMirror t = frame.Method.DeclaringType;
		ObjectMirror this_obj = (ObjectMirror)frame.GetThis ();

		TypeMirror t2 = frame.Method.GetParameters ()[0].ParameterType;

		var methods = new MethodMirror [2];
		methods [0] = t.GetMethod ("invoke_return_ref");
		methods [1] = t.GetMethod ("invoke_return_primitive");

		invoke_results = new List<Value> ();

		var r = this_obj.BeginInvokeMultiple (e.Thread, methods, null, InvokeOptions.SingleThreaded, invoke_multiple_cb, this_obj);
		WaitHandle.WaitAll (new WaitHandle[] { r.AsyncWaitHandle });
		this_obj.EndInvokeMultiple (r);
		// The callback might still be running
		while (invoke_results.Count < 2) {
			Thread.Sleep (100);
		}
		AssertValue ("ABC", invoke_results [0]);
		AssertValue (42, invoke_results [1]);
	}

	void invoke_multiple_cb (IAsyncResult ar) {
		ObjectMirror this_obj = (ObjectMirror)ar.AsyncState;

		var res = this_obj.EndInvokeMethod (ar);
		lock (invoke_results)
			invoke_results.Add (res);
	}

	[Test]
	public void GetThreads () {
		vm.GetThreads ();
	}

	[Test]
	public void Threads () {
		Event e = run_until ("threads");

		Assert.AreEqual (ThreadState.Running, e.Thread.ThreadState);

		Assert.IsTrue (e.Thread.ThreadId > 0);

		Assert.AreEqual (e.Thread.TID, e.Thread.TID);

		vm.EnableEvents (EventType.ThreadStart, EventType.ThreadDeath);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ThreadStartEvent), e);
		var state = e.Thread.ThreadState;
		Assert.IsTrue (state == ThreadState.Running || state == ThreadState.Unstarted);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ThreadDeathEvent), e);
		Assert.AreEqual (ThreadState.Stopped, e.Thread.ThreadState);
	}

	[Test]
	public void Frame_SetValue () {
		Event e = run_until ("locals2");

		StackFrame frame = e.Thread.GetFrames () [0];

		// primitive
		var l = frame.Method.GetLocal ("i");
		frame.SetValue (l, vm.CreateValue ((long)55));
		AssertValue (55, frame.GetValue (l));

		// reference
		l = frame.Method.GetLocal ("s");
		frame.SetValue (l, vm.RootDomain.CreateString ("DEF"));
		AssertValue ("DEF", frame.GetValue (l));

		// argument as local
		l = frame.Method.GetLocal ("arg");
		frame.SetValue (l, vm.CreateValue (6));
		AssertValue (6, frame.GetValue (l));

		// argument
		var p = frame.Method.GetParameters ()[1];
		frame.SetValue (p, vm.CreateValue (7));
		AssertValue (7, frame.GetValue (p));

		// gshared
		p = frame.Method.GetParameters ()[2];
		frame.SetValue (p, vm.RootDomain.CreateString ("DEF"));
		AssertValue ("DEF", frame.GetValue (p));

		// byref
		p = frame.Method.GetParameters ()[3];
		frame.SetValue (p, vm.RootDomain.CreateString ("DEF2"));
		AssertValue ("DEF2", frame.GetValue (p));

		// argument checking

		// variable null
		AssertThrows<ArgumentNullException> (delegate () {
				frame.SetValue ((LocalVariable)null, vm.CreateValue (55));
			});

		// value null
		AssertThrows<ArgumentNullException> (delegate () {
				l = frame.Method.GetLocal ("i");
				frame.SetValue (l, null);
			});

		// value of invalid type
		AssertThrows<ArgumentException> (delegate () {
				l = frame.Method.GetLocal ("i");
				frame.SetValue (l, vm.CreateValue (55));
			});
	}

	[Test]
	[Category ("only")]
	public void Frame_SetValue_Registers () {
		Event e = run_until ("locals6_1");

		StackFrame frame = e.Thread.GetFrames () [1];

		// Set 'j' to 99
		var l = frame.Method.GetLocal ("j");
		frame.SetValue (l, vm.CreateValue (99));
		AssertValue (99, frame.GetValue (l));

		// Check it during execution
		e = run_until ("locals6_2");
		frame = e.Thread.GetFrames () [0];
		AssertValue (99, frame.GetValue (frame.Method.GetParameters ()[0]));

		// Set it while in a frame which clobbers its register
		e = run_until ("locals6_3");
		frame = e.Thread.GetFrames () [1];
		frame.SetValue (l, vm.CreateValue (100));
		AssertValue (100, frame.GetValue (l));

		// Check it during execution
		e = run_until ("locals6_4");
		frame = e.Thread.GetFrames () [0];
		AssertValue (100, frame.GetValue (frame.Method.GetParameters ()[0]));

		// Signed byte value
		e = run_until ("locals6_5");
		frame = e.Thread.GetFrames () [1];
		var l2 = frame.Method.GetLocal ("sb");
		frame.SetValue (l2, vm.CreateValue ((sbyte)-99));
		AssertValue (-99, frame.GetValue (l2));

		// Check it during execution
		e = run_until ("locals6_6");
		frame = e.Thread.GetFrames () [0];
		AssertValue (-99, frame.GetValue (frame.Method.GetParameters ()[0]));
	}

	[Test]
	public void InvokeRegress () {
		Event e = run_until ("invoke1");

		StackFrame frame = e.Thread.GetFrames () [0];

		TypeMirror t = frame.Method.DeclaringType;
		ObjectMirror this_obj = (ObjectMirror)frame.GetThis ();

		TypeMirror t2 = frame.Method.GetParameters ()[0].ParameterType;

		MethodMirror m;
		Value v;

		// do an invoke
		m = t.GetMethod ("invoke_return_void");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		Assert.IsNull (v);

		// Check that the stack frames remain valid during the invoke
		Assert.AreEqual ("Tests", (frame.GetThis () as ObjectMirror).Type.Name);

		// do another invoke
		m = t.GetMethod ("invoke_return_void");
		v = this_obj.InvokeMethod (e.Thread, m, null);
		Assert.IsNull (v);

		// Try a single step after the invoke
		var req = vm.CreateStepRequest (e.Thread);
		req.Depth = StepDepth.Into;
		req.Size = StepSize.Line;
		req.Enable ();

		step_req = req;

		// Skip nop
		step_once ();

		// Step into invoke2
		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);
		Assert.AreEqual ("invoke2", (e as StepEvent).Method.Name);

		req.Disable ();

		frame = e.Thread.GetFrames () [0];
	}

	[Test]
	public void Exceptions () {
		Event e = run_until ("exceptions");
		var req = vm.CreateExceptionRequest (null);
		req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("OverflowException", (e as ExceptionEvent).Exception.Type.Name);

		var frames = e.Thread.GetFrames ();
		Assert.AreEqual ("exceptions", frames [0].Method.Name);
		req.Disable ();

		// exception type filter

		req = vm.CreateExceptionRequest (vm.RootDomain.Corlib.GetType ("System.ArgumentException"));
		req.Enable ();

		// Skip the throwing of the second OverflowException	   
		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("ArgumentException", (e as ExceptionEvent).Exception.Type.Name);
		req.Disable ();

		// exception type filter for subclasses
		req = vm.CreateExceptionRequest (vm.RootDomain.Corlib.GetType ("System.Exception"));
		req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("OverflowException", (e as ExceptionEvent).Exception.Type.Name);
		req.Disable ();

		// Implicit exceptions
		req = vm.CreateExceptionRequest (null);
		req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("NullReferenceException", (e as ExceptionEvent).Exception.Type.Name);
		req.Disable ();

		// Single stepping after an exception
		req = vm.CreateExceptionRequest (null);
		req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("Exception", (e as ExceptionEvent).Exception.Type.Name);
		frames = e.Thread.GetFrames ();
		Assert.AreEqual ("exceptions2", frames [0].Method.Name);
		req.Disable ();

		var sreq = vm.CreateStepRequest (e.Thread);
		sreq.Depth = StepDepth.Over;
		sreq.Size = StepSize.Line;
		sreq.Enable ();

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (StepEvent), e);
		frames = e.Thread.GetFrames ();
		Assert.AreEqual ("exceptions", frames [0].Method.Name);
		sreq.Disable ();

		// Argument checking
		AssertThrows<ArgumentException> (delegate {
				vm.CreateExceptionRequest (e.Thread.Type);
			});
	}

	[Test]
	public void ExceptionFilter () {
		Event e = run_until ("exception_filter");

		MethodMirror m = entry_point.DeclaringType.GetMethod ("exception_filter_filter");
		Assert.IsNotNull (m);

		vm.SetBreakpoint (m, 0);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.IsTrue (e is BreakpointEvent);
		Assert.AreEqual (m.Name, (e as BreakpointEvent).Method.Name);

		var frames = e.Thread.GetFrames ();

		Assert.IsTrue (frames [0].Location.SourceFile.IndexOf ("dtest-app.cs") != -1);
		Assert.AreEqual ("exception_filter_filter", frames [0].Location.Method.Name);

		Assert.AreEqual (0, frames [1].Location.Method.MetadataToken);
		Assert.AreEqual (0x0f, frames [1].Location.ILOffset);

		Assert.AreEqual ("exception_filter_method", frames [2].Location.Method.Name);
		Assert.AreEqual (0x06, frames [2].Location.ILOffset);

		Assert.AreEqual (0, frames [3].Location.Method.MetadataToken, 0);
		Assert.AreEqual (0, frames [3].Location.ILOffset);

		Assert.AreEqual ("exception_filter", frames [4].Location.Method.Name);
	}

	[Test]
	public void ExceptionFilter2 () {
		vm.Detach ();

		Start (new string [] { "dtest-excfilter.exe" });

		MethodMirror filter_method = entry_point.DeclaringType.GetMethod ("Filter");
		Assert.IsNotNull (filter_method);

		MethodMirror test_method = entry_point.DeclaringType.GetMethod ("Test");
		Assert.IsNotNull (test_method);

		vm.SetBreakpoint (filter_method, 0);

		vm.Resume ();

		var e = GetNextEvent ();
		Assert.AreEqual (EventType.Breakpoint, e.EventType);
		Assert.IsTrue (e is BreakpointEvent);
		Assert.AreEqual (filter_method.Name, (e as BreakpointEvent).Method.Name);

		var frames = e.Thread.GetFrames ();

		Assert.AreEqual (4, frames.Count ());

		Assert.AreEqual (filter_method.Name, frames [0].Location.Method.Name);
		Assert.AreEqual (20, frames [0].Location.LineNumber);
		Assert.AreEqual (0, frames [0].Location.ILOffset);

		Assert.AreEqual (test_method.Name, frames [1].Location.Method.Name);
		Assert.AreEqual (37, frames [1].Location.LineNumber);
		Assert.AreEqual (0x0b, frames [1].Location.ILOffset);

		Assert.AreEqual (test_method.Name, frames [2].Location.Method.Name);
		Assert.AreEqual (33, frames [2].Location.LineNumber);
		Assert.AreEqual (0x05, frames [2].Location.ILOffset);

		Assert.AreEqual (entry_point.Name, frames [3].Location.Method.Name);
		Assert.AreEqual (14, frames [3].Location.LineNumber);
		Assert.AreEqual (0x00, frames [3].Location.ILOffset);

		vm.Exit (0);

		vm = null;
	}

	[Test]
	public void EventSets () {
		//
		// Create two filter which both match the same exception
		//
		Event e = run_until ("exceptions");

		var req = vm.CreateExceptionRequest (null);
		req.Enable ();

		var req2 = vm.CreateExceptionRequest (vm.RootDomain.Corlib.GetType ("System.OverflowException"));
		req2.Enable ();

		vm.Resume ();

		var es = vm.GetNextEventSet ();
		Assert.AreEqual (2, es.Events.Length);

		e = es [0];
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("OverflowException", (e as ExceptionEvent).Exception.Type.Name);

		e = es [1];
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("OverflowException", (e as ExceptionEvent).Exception.Type.Name);

		req.Disable ();
		req2.Disable ();
	}

	//
	// Test single threaded invokes during processing of nullref exceptions.
	// These won't work if the exception handling is done from the sigsegv signal
	// handler, since the sigsegv signal is disabled until control returns from the
	// signal handler.
	//
	[Test]
	[Category ("only3")]
	public void NullRefExceptionAndSingleThreadedInvoke () {
		Event e = run_until ("exceptions");
		var req = vm.CreateExceptionRequest (vm.RootDomain.Corlib.GetType ("System.NullReferenceException"));
		req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (ExceptionEvent), e);
		Assert.AreEqual ("NullReferenceException", (e as ExceptionEvent).Exception.Type.Name);

		var ex = (e as ExceptionEvent).Exception;
		var tostring_method = vm.RootDomain.Corlib.GetType ("System.Object").GetMethod ("ToString");
		ex.InvokeMethod (e.Thread, tostring_method, null, InvokeOptions.SingleThreaded);
	}

	[Test]
	public void Domains () {
		vm.Detach ();

		Start (new string [] { "dtest-app.exe", "domain-test" });

		vm.EnableEvents (EventType.AppDomainCreate, EventType.AppDomainUnload, EventType.AssemblyUnload);

		Event e = run_until ("domains");

		vm.Resume ();
		
		e = GetNextEvent ();
		Assert.IsInstanceOfType (typeof (AppDomainCreateEvent), e);

		var domain = (e as AppDomainCreateEvent).Domain;

		// Check the object type
		e = run_until ("domains_2");
		var frame = e.Thread.GetFrames ()[0];
		var o = frame.GetArgument (0) as ObjectMirror;
		Assert.AreEqual ("CrossDomain", o.Type.Name);

		// Do a remoting invoke
		var cross_domain_type = o.Type;
		var v = o.InvokeMethod (e.Thread, cross_domain_type.GetMethod ("invoke_3"), null);
		AssertValue (42, v);

		// Run until the callback in the domain
		MethodMirror m = entry_point.DeclaringType.GetMethod ("invoke_in_domain");
		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, 0);

		while (true) {
			vm.Resume ();
			e = GetNextEvent ();
			if (e is BreakpointEvent)
				break;
		}

		Assert.AreEqual ("invoke_in_domain", (e as BreakpointEvent).Method.Name);

		// d_method is from another domain
		MethodMirror d_method = (e as BreakpointEvent).Method;
		Assert.IsTrue (m != d_method);

		var frames = e.Thread.GetFrames ();
		Assert.AreEqual ("invoke_in_domain", frames [0].Method.Name);
		Assert.AreEqual ("invoke", frames [1].Method.Name);
		Assert.AreEqual ("domains", frames [2].Method.Name);

		// Test breakpoints on already JITted methods in other domains
		m = entry_point.DeclaringType.GetMethod ("invoke_in_domain_2");
		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, 0);

		while (true) {
			vm.Resume ();
			e = GetNextEvent ();
			if (e is BreakpointEvent)
				break;
		}

		Assert.AreEqual ("invoke_in_domain_2", (e as BreakpointEvent).Method.Name);

		// This is empty when receiving the AppDomainCreateEvent
		Assert.AreEqual ("domain", domain.FriendlyName);

		// Run until the unload
		while (true) {
			vm.Resume ();
			e = GetNextEvent ();
			if (e is AssemblyUnloadEvent) {
				continue;
			} else {
				break;
			}
		}
		Assert.IsInstanceOfType (typeof (AppDomainUnloadEvent), e);
		Assert.AreEqual (domain, (e as AppDomainUnloadEvent).Domain);

		// Run past the unload
		e = run_until ("domains_3");

		// Test access to unloaded types
		// FIXME: Add an exception type for this
		AssertThrows<Exception> (delegate {
				d_method.DeclaringType.GetValue (d_method.DeclaringType.GetField ("static_i"));
			});
	}

	[Test]
	public void DynamicMethods () {
		Event e = run_until ("dyn_call");

		var m = e.Thread.GetFrames ()[1].Method;
		Assert.AreEqual ("dyn_method", m.Name);

		// Test access to IL
		var body = m.GetMethodBody ();

		ILInstruction ins = body.Instructions [0];
		Assert.AreEqual (OpCodes.Ldstr, ins.OpCode);
		Assert.AreEqual ("FOO", ins.Operand);
	}

	[Test]
	public void RefEmit () {
		vm.Detach ();

		Start (new string [] { "dtest-app.exe", "ref-emit-test" });

		Event e = run_until ("ref_emit_call");

		var m = e.Thread.GetFrames ()[1].Method;
		Assert.AreEqual ("ref_emit_method", m.Name);

		// Test access to IL
		var body = m.GetMethodBody ();

		ILInstruction ins;

		ins = body.Instructions [0];
		Assert.AreEqual (OpCodes.Ldstr, ins.OpCode);
		Assert.AreEqual ("FOO", ins.Operand);

		ins = body.Instructions [1];
		Assert.AreEqual (OpCodes.Call, ins.OpCode);
		Assert.IsInstanceOfType (typeof (MethodMirror), ins.Operand);
		Assert.AreEqual ("ref_emit_call", (ins.Operand as MethodMirror).Name);
	}

	[Test]
	public void IsAttached () {
		var f = entry_point.DeclaringType.GetField ("is_attached");

		Event e = run_until ("Main");

		AssertValue (true, entry_point.DeclaringType.GetValue (f));
	}

	[Test]
	public void StackTraceInNative () {
		// Check that stack traces can be produced for threads in native code
		vm.Detach ();

		Start (new string [] { "dtest-app.exe", "frames-in-native" });

		var e = run_until ("frames_in_native");

		// FIXME: This is racy
		vm.Resume ();

		Thread.Sleep (100);

		vm.Suspend ();

		StackFrame[] frames = e.Thread.GetFrames ();

		int frame_index = -1;
		for (int i = 0; i < frames.Length; ++i) {
			if (frames [i].Method.Name == "Sleep") {
				frame_index = i;
				break;
			}
		}

		Assert.IsTrue (frame_index != -1);
		Assert.AreEqual ("Sleep", frames [frame_index].Method.Name);
		Assert.AreEqual ("frames_in_native", frames [frame_index + 1].Method.Name);
		Assert.AreEqual ("Main", frames [frame_index + 2].Method.Name);

		// Check that invokes are disabled for such threads
		TypeMirror t = frames [frame_index + 1].Method.DeclaringType;

		var m = t.GetMethod ("invoke_static_return_void");
		AssertThrows<InvalidOperationException> (delegate {
				t.InvokeMethod (e.Thread, m, null);
			});

		// Check that the frame info is invalidated
		run_until ("frames_in_native_2");

		AssertThrows<InvalidStackFrameException> (delegate {
				Console.WriteLine (frames [frame_index].GetThis ());
			});
	}

	[Test]
	public void VirtualMachine_CreateEnumMirror () {
		var e = run_until ("o1");
		var frame = e.Thread.GetFrames () [0];

		object val = frame.GetThis ();
		Assert.IsTrue (val is ObjectMirror);
		Assert.AreEqual ("Tests", (val as ObjectMirror).Type.Name);
		ObjectMirror o = (val as ObjectMirror);

		FieldInfoMirror field = o.Type.GetField ("field_enum");
		Value f = o.GetValue (field);
		TypeMirror enumType = (f as EnumMirror).Type;

		o.SetValue (field, vm.CreateEnumMirror (enumType, vm.CreateValue (1)));
		f = o.GetValue (field);
		Assert.AreEqual (1, (f as EnumMirror).Value);

		// Argument checking
		AssertThrows<ArgumentNullException> (delegate () {
				vm.CreateEnumMirror (enumType, null);
			});

		AssertThrows<ArgumentNullException> (delegate () {
				vm.CreateEnumMirror (null, vm.CreateValue (1));
			});

		// null value
		AssertThrows<ArgumentException> (delegate () {
				vm.CreateEnumMirror (enumType, vm.CreateValue (null));
			});

		// value of a wrong type
		AssertThrows<ArgumentException> (delegate () {
				vm.CreateEnumMirror (enumType, vm.CreateValue ((long)1));
			});
	}

	[Test]
	public void VirtualMachine_EnableEvents_Breakpoint () {
		AssertThrows<ArgumentException> (delegate () {
				vm.EnableEvents (EventType.Breakpoint);
			});
	}

	[Test]
	public void SingleStepRegress654694 () {
		int il_offset = -1;

		MethodMirror m = entry_point.DeclaringType.GetMethod ("ss_regress_654694");
		foreach (Location l in m.Locations) {
			if (l.ILOffset > 0 && il_offset == -1)
				il_offset = l.ILOffset;
		}

		Event e = run_until ("ss_regress_654694");

		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, il_offset);

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsTrue (e is BreakpointEvent);

		var req = vm.CreateStepRequest (e.Thread);
		req.Depth = StepDepth.Over;
		req.Size = StepSize.Line;
		req.Enable ();

		vm.Resume ();

		e = GetNextEvent ();
		Assert.IsTrue (e is StepEvent);

		req.Disable ();
	}

	[Test]
	public void DebugBreak () {
		vm.EnableEvents (EventType.UserBreak);

		run_until ("user");

		vm.Resume ();
		var e = GetNextEvent ();
		Assert.IsTrue (e is UserBreakEvent);
	}

	[Test]
	public void DebugLog () {
		vm.EnableEvents (EventType.UserLog);

		run_until ("user");

		vm.Resume ();
		var e = GetNextEvent ();
		Assert.IsTrue (e is UserLogEvent);
		var le = e as UserLogEvent;

		Assert.AreEqual (5, le.Level);
		Assert.AreEqual ("A", le.Category);
		Assert.AreEqual ("B", le.Message);
	}

	[Test]
	public void TypeGetMethodsByNameFlags () {
		MethodMirror[] mm;
		var assembly = entry_point.DeclaringType.Assembly;
		var type = assembly.GetType ("Tests3");

		Assert.IsNotNull (type);

		mm = type.GetMethodsByNameFlags (null, BindingFlags.Static | BindingFlags.Public, false);
		Assert.AreEqual (1, mm.Length, "#1");
		Assert.AreEqual ("M1", mm[0].Name, "#2");

		mm = type.GetMethodsByNameFlags (null, BindingFlags.Static | BindingFlags.NonPublic, false);
		Assert.AreEqual (1, mm.Length, "#3");
		Assert.AreEqual ("M2", mm[0].Name, "#4");

		mm = type.GetMethodsByNameFlags (null, BindingFlags.Instance | BindingFlags.Public, false);
		Assert.AreEqual (7, mm.Length, "#5"); //M3 plus Equals, GetHashCode, GetType, ToString, .ctor

		mm = type.GetMethodsByNameFlags (null, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, false);
		Assert.AreEqual (2, mm.Length, "#7");

		mm = type.GetMethodsByNameFlags (null, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, false);
		Assert.AreEqual (1, mm.Length, "#9");

		mm = type.GetMethodsByNameFlags (null, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly, false);
		Assert.AreEqual (5, mm.Length, "#11");

		//Now with name
		mm = type.GetMethodsByNameFlags ("M1", BindingFlags.Static | BindingFlags.Public, false);
		Assert.AreEqual (1, mm.Length, "#12");
		Assert.AreEqual ("M1", mm[0].Name, "#13");

		mm = type.GetMethodsByNameFlags ("m1", BindingFlags.Static | BindingFlags.Public, true);
		Assert.AreEqual (1, mm.Length, "#14");
		Assert.AreEqual ("M1", mm[0].Name, "#15");

		mm = type.GetMethodsByNameFlags ("M1", BindingFlags.Static  | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, false);
		Assert.AreEqual (1, mm.Length, "#16");
		Assert.AreEqual ("M1", mm[0].Name, "#17");
	}

	[Test]
	[Category ("only88")]
	public void TypeLoadSourceFileFilter () {
		Event e = run_until ("type_load");

		if (!vm.Version.AtLeast (2, 7))
			return;

		string srcfile = (e as BreakpointEvent).Method.DeclaringType.GetSourceFiles (true)[0];

		var req = vm.CreateTypeLoadRequest ();
		req.SourceFileFilter = new string [] { srcfile.ToUpper () };
		req.Enable ();

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is TypeLoadEvent);
		Assert.AreEqual ("TypeLoadClass", (e as TypeLoadEvent).Type.FullName);
	}

	[Test]
	public void TypeLoadTypeNameFilter () {
		Event e = run_until ("type_load");

		var req = vm.CreateTypeLoadRequest ();
		req.TypeNameFilter = new string [] { "TypeLoadClass2" };
		req.Enable ();

		vm.Resume ();
		e = GetNextEvent ();
		Assert.IsTrue (e is TypeLoadEvent);
		Assert.AreEqual ("TypeLoadClass2", (e as TypeLoadEvent).Type.FullName);
	}

	[Test]
	public void GetTypesForSourceFile () {
		run_until ("user");

		var types = vm.GetTypesForSourceFile ("dtest-app.cs", false);
		Assert.IsTrue (types.Any (t => t.FullName == "Tests"));
		Assert.IsFalse (types.Any (t => t.FullName == "System.Int32"));

		types = vm.GetTypesForSourceFile ("DTEST-app.cs", true);
		Assert.IsTrue (types.Any (t => t.FullName == "Tests"));
		Assert.IsFalse (types.Any (t => t.FullName == "System.Int32"));
	}

	[Test]
	public void GetTypesNamed () {
		run_until ("user");

		var types = vm.GetTypes ("Tests", false);
		Assert.AreEqual (1, types.Count);
		Assert.AreEqual ("Tests", types [0].FullName);

		types = vm.GetTypes ("System.Exception", false);
		Assert.AreEqual (1, types.Count);
		Assert.AreEqual ("System.Exception", types [0].FullName);
	}

	[Test]
	public void String_GetChars () {
		object val;

		// Reuse this test
		var e = run_until ("arg2");

		var frame = e.Thread.GetFrames () [0];

		val = frame.GetArgument (0);
		Assert.IsTrue (val is StringMirror);
		AssertValue ("FOO", val);
		var s = (val as StringMirror);
		Assert.AreEqual (3, s.Length);

		var c = s.GetChars (0, 2);
		Assert.AreEqual (2, c.Length);
		Assert.AreEqual ('F', c [0]);
		Assert.AreEqual ('O', c [1]);

		AssertThrows<ArgumentException> (delegate () {		
				s.GetChars (2, 2);
			});
	}

	[Test]
	public void GetInterfaces () {
		var e = run_until ("arg2");

		var frame = e.Thread.GetFrames () [0];

		var cl1 = frame.Method.DeclaringType.Assembly.GetType ("TestIfaces");
		var ifaces = cl1.GetInterfaces ();
		Assert.AreEqual (1, ifaces.Length);
		Assert.AreEqual ("ITest", ifaces [0].Name);

		var cl2 = cl1.GetMethod ("Baz").ReturnType;
		var ifaces2 = cl2.GetInterfaces ();
		Assert.AreEqual (1, ifaces2.Length);
		Assert.AreEqual ("ITest`1", ifaces2 [0].Name);
	}

	[Test]
	public void GetInterfaceMap () {
		var e = run_until ("arg2");

		var frame = e.Thread.GetFrames () [0];

		var cl1 = frame.Method.DeclaringType.Assembly.GetType ("TestIfaces");
		var iface = cl1.Assembly.GetType ("ITest");
		var map = cl1.GetInterfaceMap (iface);
		Assert.AreEqual (cl1, map.TargetType);
		Assert.AreEqual (iface, map.InterfaceType);
		Assert.AreEqual (2, map.InterfaceMethods.Length);
		Assert.AreEqual (2, map.TargetMethods.Length);
	}

	[Test]
	public void StackAlloc_Breakpoints_Regress2775 () {
		// Check that breakpoints on arm don't overwrite stackalloc-ed memory
		var e = run_until ("regress_2755");

		var frame = e.Thread.GetFrames () [0];
		var m = e.Method;
		// This breaks at the call site
		vm.SetBreakpoint (m, m.Locations [2].ILOffset);

		vm.Resume ();
		var e2 = GetNextEvent ();
		Assert.IsTrue (e2 is BreakpointEvent);

		e = run_until ("regress_2755_3");
		frame = e.Thread.GetFrames () [1];
		var res = frame.GetValue (m.GetLocal ("sum"));
		AssertValue (0, res);
	}

	[Test]
	public void MethodInfo () {
		Event e = run_until ("locals2");

		StackFrame frame = e.Thread.GetFrames () [0];
		var m = frame.Method;

		Assert.IsTrue (m.IsGenericMethod);
		Assert.IsFalse (m.IsGenericMethodDefinition);

		var args = m.GetGenericArguments ();
		Assert.AreEqual (1, args.Length);
		Assert.AreEqual ("String", args [0].Name);

		var gmd = m.GetGenericMethodDefinition ();
		Assert.IsTrue (gmd.IsGenericMethod);
		Assert.IsTrue (gmd.IsGenericMethodDefinition);
		Assert.AreEqual (gmd, gmd.GetGenericMethodDefinition ());

		args = gmd.GetGenericArguments ();
		Assert.AreEqual (1, args.Length);
		Assert.AreEqual ("T", args [0].Name);

		var attrs = m.GetCustomAttributes (true);
		Assert.AreEqual (1, attrs.Length);
		Assert.AreEqual ("StateMachineAttribute", attrs [0].Constructor.DeclaringType.Name);
	}

	[Test]
	public void UnhandledException () {
		vm.Exit (0);

		Start (new string [] { "dtest-app.exe", "unhandled-exception" });

		var req = vm.CreateExceptionRequest (null, false, true);
		req.Enable ();

		var e = run_until ("unhandled_exception");
		vm.Resume ();

		var e2 = GetNextEvent ();
		Assert.IsTrue (e2 is ExceptionEvent);

		vm.Exit (0);
		vm = null;
	}

	[Test]
	public void UnhandledException_2 () {
		vm.Exit (0);

		Start (new string [] { "dtest-app.exe", "unhandled-exception-endinvoke" });

		var req = vm.CreateExceptionRequest (null, false, true);
		req.Enable ();

		MethodMirror m = entry_point.DeclaringType.GetMethod ("unhandled_exception_endinvoke_2");
		Assert.IsNotNull (m);
		vm.SetBreakpoint (m, m.ILOffsets [0]);

		var e = run_until ("unhandled_exception_endinvoke");
		vm.Resume ();

		var e2 = GetNextEvent ();
		Assert.IsFalse (e2 is ExceptionEvent);

		vm.Exit (0);
		vm = null;
	}

#if NET_4_5
	[Test]
	public void UnhandledExceptionUserCode () {
		vm.Detach ();

		// Exceptions caught in non-user code are treated as unhandled
		Start (new string [] { "dtest-app.exe", "unhandled-exception-user" });

		var req = vm.CreateExceptionRequest (null, false, true);
		req.AssemblyFilter = new List<AssemblyMirror> () { entry_point.DeclaringType.Assembly };
		req.Enable ();

		var e = run_until ("unhandled_exception_user");
		vm.Resume ();

		var e2 = GetNextEvent ();
		Assert.IsTrue (e2 is ExceptionEvent);

		vm.Exit (0);
		vm = null;
	}
#endif

	[Test]
	public void GCWhileSuspended () {
		// Check that objects are kept alive during suspensions
		Event e = run_until ("gc_suspend_1");

		MethodMirror m = entry_point.DeclaringType.GetMethod ("gc_suspend_invoke");

		var o = entry_point.DeclaringType.GetValue (entry_point.DeclaringType.GetField ("gc_suspend_field")) as ObjectMirror;
		//Console.WriteLine (o);

		StackFrame frame = e.Thread.GetFrames () [0];
		TypeMirror t = frame.Method.DeclaringType;
		for (int i = 0; i < 10; ++i)
			t.InvokeMethod (e.Thread, m, new Value [] { });

		// This throws an exception if the object is collected
		long addr = o.Address;

		var o2 = entry_point.DeclaringType.GetValue (entry_point.DeclaringType.GetField ("gc_suspend_field")) as ObjectMirror;
		Assert.IsNull (o2);
	}

	[Test]
	public void MakeGenericMethod () {
		Event e = run_until ("bp1");

		var intm = vm.RootDomain.GetCorrespondingType (typeof (int));
		var stringm = vm.RootDomain.GetCorrespondingType (typeof (string));
		var gm = entry_point.DeclaringType.GetMethod ("generic_method");
		var res = gm.MakeGenericMethod (new TypeMirror [] { stringm });
		var args = res.GetGenericArguments ();
		Assert.AreEqual (1, args.Length);
		Assert.AreEqual (stringm, args [0]);

		// Error checking
		AssertThrows<ArgumentNullException> (delegate {
				gm.MakeGenericMethod (null);
			});
		AssertThrows<ArgumentNullException> (delegate {
				gm.MakeGenericMethod (new TypeMirror [] { null });
			});
		AssertThrows<ArgumentException> (delegate {
				gm.MakeGenericMethod (new TypeMirror [] { stringm, stringm });
			});
		AssertThrows<InvalidOperationException> (delegate {
				gm.MakeGenericMethod (new TypeMirror [] { intm });
			});
		AssertThrows<InvalidOperationException> (delegate {
				entry_point.DeclaringType.GetMethod ("Main").MakeGenericMethod (new TypeMirror [] { intm });
			});
	}
}

}
