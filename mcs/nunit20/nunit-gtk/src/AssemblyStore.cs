//
// AssemblyStore.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Reflection;
using GLib;
using Gtk;
using NUnit.Core;

namespace Mono.NUnit.GUI
{
	delegate void FixtureAddedEventHandler (object sender, FixtureAddedEventArgs args);
	delegate void FixtureLoadErrorHandler (object sender, FixtureLoadErrorEventArgs args);

	class FixtureAddedEventArgs : EventArgs
	{
		int total;
		int current;

		public FixtureAddedEventArgs (int current, int total)
		{
			this.total = total;
			this.current = current;
		}

		public int Total {
			get { return total; }
		}

		public int Current {
			get { return current; }
		}
	}
	
	class FixtureLoadErrorEventArgs : EventArgs
	{
		string message;
		string filename;

		public FixtureLoadErrorEventArgs (string filename, Exception e)
		{
			this.filename = filename;
			message = e.Message;
		}

		public string FileName {
			get { return filename; }
		}

		public string Message {
			get { return message; }
		}
	}
	
	class AssemblyStore : TreeStore, EventListener
	{
		string assemblyName;
		Hashtable iters;
		TestSuite rootTS;
		int totalTests;
		int currentTest;

		bool runningTest;
		EventListener listener;
		Test test;

		Exception exception;

		static Value grayCircle;

		public event FixtureAddedEventHandler FixtureAdded;
		public event FixtureLoadErrorHandler FixtureLoadError;

		public AssemblyStore (string assemblyName)
			: base ((int) TypeFundamentals.TypeInt, (int) TypeFundamentals.TypeString)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("asemblyName");
			// "g_type_init () should be called" if I put this code in a static ctor.
			if (grayCircle == null)
				grayCircle = new Value ((int) CircleColor.None);

			this.assemblyName = assemblyName;
		}

		public void Load ()
		{
			Idle.Add (new IdleHandler (Populate));
		}

		public void RunTestAtIter (TreeIter iter, EventListener listener, ref int ntests)
		{
			if (runningTest)
				throw new InvalidOperationException ("Already running some test(s).");
			
			foreach (TreeIter i in iters.Values)
				SetValue (i, 0, grayCircle);

			if (iter == TreeIter.Zero)
				return;

			string path = GetPath (iter).ToString ();
			if (path == null || path == "")
				return;

			test = LookForTestByPath (path, null);
			if (test == null)
				return;

			ntests = test.CountTestCases;
			runningTest = true;
			this.listener = listener;
			Idle.Add (new IdleHandler (InternalRunTest));
		}

		public new void Clear ()
		{
			base.Clear ();
			iters = null;
		}
		
		bool InternalRunTest ()
		{
			try {
				test.Run (this);
			} finally {
				runningTest = false;
			}
			return false;
		}

		Test LookForTestByPath (string path, Test t)
		{
			string [] parts = path.Split (':');
			if (t == null) {
				if (parts.Length > 1)
					return LookForTestByPath (String.Join (":", parts, 1, parts.Length - 1), rootTS);

				return rootTS;
			}

			Test ret;
			//Console.WriteLine ("Count: {0} Index: {1} path: '{2}'", t.Tests.Count, parts [0], path);
			if (parts.Length == 1) {
				ret = (Test) t.Tests [Int32.Parse (path)];
				return ret;
			}

			ret = (Test) t.Tests [Int32.Parse (parts [0])];
			//Console.WriteLine ("Recurse: " + ret.FullName + " " + String.Join (":", parts, 1, parts.Length - 1));
			return LookForTestByPath (String.Join (":", parts, 1, parts.Length - 1), ret);
						  
		}

		TreeIter AddFixture (TreeIter parent, string fullName)
		{
			TreeIter iter;
			string typeName = fullName;
			string [] parts = typeName.Split ('.');
			string index = "";

			foreach (string s in parts) {
				if (index == "")
					index = s;
				else
					index += "." + s;

				if (iters.ContainsKey (index)) {
					parent = (TreeIter) iters [index];
					continue;
				}
				
				Append (out iter, parent);
				SetValue (iter, 0, grayCircle);
				SetValue (iter, 1, new Value (s));
				parent = iter;
				iters [index] = iter;
			}

			return parent;
		}

		void AddSuite (TreeIter parent, TestSuite suite)
		{
			TreeIter next;
			foreach (Test t in suite.Tests) {
				next = AddFixture (parent, t.FullName);
				while (GLib.MainContext.Iteration ());
				if (t.IsSuite)
					AddSuite (next, (TestSuite) t);
				else if (FixtureAdded != null)
					FixtureAdded (this, new FixtureAddedEventArgs (++currentTest, totalTests));

			}
		}

		bool Populate ()
		{
			Clear ();
			iters = new Hashtable ();
			TreeIter first;
			Append (out first);
			SetValue (first, 0, new Value ((int) CircleColor.None));
			SetValue (first, 1, new Value (assemblyName));
			iters [assemblyName] = first;
			ResolveEventHandler reh = new ResolveEventHandler (TryLoad);
			AppDomain.CurrentDomain.AssemblyResolve += reh;

			try {
				rootTS = new TestSuiteBuilder ().Build (assemblyName);
			} catch (Exception e) {
				if (FixtureLoadError != null) {
					exception = e;
					Idle.Add (new IdleHandler (TriggerError));
				}
				return false;
			} finally {
				AppDomain.CurrentDomain.AssemblyResolve -= reh;
			}

			currentTest = 0;
			totalTests = rootTS.CountTestCases;
			AddSuite (first, rootTS);

			return false;
		}

		bool TriggerError ()
		{
			FixtureLoadError (this, new FixtureLoadErrorEventArgs (assemblyName, exception));
			exception = null;
			return false;
		}

		Assembly TryLoad (object sender, ResolveEventArgs args)
		{
			try {
				// NUnit2 uses Assembly.Load on the filename without extension.
				// This is done just to allow loading from a full path name.
				return Assembly.LoadFrom (assemblyName);
			} catch { }

			return null;
		}

		// Interface NUnit.Core.EventListener
		void EventListener.TestStarted (TestCase testCase)
		{
			if (listener != null)
				listener.TestStarted (testCase);

			while (GLib.MainContext.Iteration ());
		}
			
		void EventListener.TestFinished (TestCaseResult result)
		{
			if (listener != null)
				listener.TestFinished (result);

			SetIconFromResult (result);
			while (GLib.MainContext.Iteration ());
		}

		void EventListener.SuiteStarted (TestSuite suite)
		{
			if (listener != null)
				listener.SuiteStarted (suite);

			while (GLib.MainContext.Iteration ());
		}

		void EventListener.SuiteFinished (TestSuiteResult result)
		{
			if (listener != null)
				listener.SuiteFinished (result);

			SetIconFromResult (result);
			while (GLib.MainContext.Iteration ());
		}

		void SetIconFromResult (TestResult result)
		{
			CircleColor color;
			if (!result.Executed)
				color = CircleColor.NotRun;
			else if (result.IsFailure)
				color = CircleColor.Failure;
			else if (result.IsSuccess)
				color = CircleColor.Success;
			else {
				Console.WriteLine ("Warning: unexpected combination.");
				color = CircleColor.None;
			}

			string fullname = result.Test.FullName;
			if (iters.ContainsKey (fullname)) {
				TreeIter iter = (TreeIter) iters [fullname];
				SetValue (iter, 0, new Value ((int) color));
			} else {
				Console.WriteLine ("Don't know anything about " + fullname);
			}
		}
	}
}

