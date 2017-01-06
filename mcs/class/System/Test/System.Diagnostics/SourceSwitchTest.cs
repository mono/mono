//
// SourceSwitchTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


#if !MOBILE

#define TRACE

using NUnit.Framework;
using System;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Diagnostics;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class SourceSwitchTest
	{
		internal TraceSource traceSource { get; set; }
		internal TestTextWriterTraceListener txtTraceListener;


		[Test]
		public void ConstructorNullName ()
		{
			SourceSwitch s = new SourceSwitch (null);
			AssertHelper.IsEmpty (s.DisplayName);
		}

		[Test]
		public void ConstructorNullDefaultValue ()
		{
			SourceSwitch s = new SourceSwitch ("foo", null);
		}

		[Test]
		public void ConstructorDefault ()
		{
			SourceSwitch s = new SourceSwitch ("foo");
			Assert.AreEqual ("foo", s.DisplayName, "#1");
			Assert.AreEqual (SourceLevels.Off, s.Level, "#2");
			Assert.AreEqual (0, s.Attributes.Count, "#3");
		}

		[Test]
		public void ShouldTrace ()
		{
			SourceSwitch s = new SourceSwitch ("foo");
			s.Level = SourceLevels.Verbose;
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Critical), "#1");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Error), "#2");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Warning), "#3");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Information), "#4");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Verbose), "#5");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Start), "#6");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Stop), "#7");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Suspend), "#8");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Resume), "#9");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Transfer), "#10");
		}

		[Test]
		public void ShouldTrace2 ()
		{
			SourceSwitch s = new SourceSwitch ("foo");
			s.Level = SourceLevels.ActivityTracing;
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Critical), "#1");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Error), "#2");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Warning), "#3");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Information), "#4");
			Assert.IsFalse (s.ShouldTrace (TraceEventType.Verbose), "#5");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Start), "#6");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Stop), "#7");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Suspend), "#8");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Resume), "#9");
			Assert.IsTrue (s.ShouldTrace (TraceEventType.Transfer), "#10");
		}


		[SetUp]						
		public void InitalizeSourceSwitchTest()
		{
			// Initializing the TraceSource instance
			traceSource = new TraceSource ("LoggingTraceSource");
			traceSource.Listeners.Remove("Default");
			traceSource.Switch = new SourceSwitch ("MySwitch");

			// Initializing the TraceListener instance
			txtTraceListener = new TestTextWriterTraceListener (Console.Out);
			traceSource.Listeners.Add (txtTraceListener); 
		}

		[Test]						
		public void setSwitchToCritical()
		{
			traceSource.Switch.Level = SourceLevels.Critical;
			LogAllTraceLevels ();
			// Switch.Level is Critical so it should log Critical
			Assert.AreEqual (1, txtTraceListener.TotalMessageCount);
			Assert.AreEqual (1, txtTraceListener.CritialMessageCount);
		}
			
		[Test]						
		public void setSwitchToError()
		{
			traceSource.Switch.Level = SourceLevels.Error;
			LogAllTraceLevels ();
			// Switch.Level is Error so it should log Critical, Error
			Assert.AreEqual (2, txtTraceListener.TotalMessageCount);
			Assert.AreEqual (1, txtTraceListener.ErrorMessageCount);
		}

		[Test]						
		public void setSwitchToWarning()
		{
			traceSource.Switch.Level = SourceLevels.Warning;
			LogAllTraceLevels ();
			// Switch.Level is Warning so it should log Critical, Error, Warning
			Assert.AreEqual (3, txtTraceListener.TotalMessageCount);
			Assert.AreEqual (1, txtTraceListener.WarningMessageCount);
		}

		[Test]						
		public void setSwitchToInfo()
		{
			traceSource.Switch.Level = SourceLevels.Information;
			LogAllTraceLevels ();
			// Switch.Level is Information so it should log Critical, Error, Warning, Information
			Assert.AreEqual (4, txtTraceListener.TotalMessageCount);
			Assert.AreEqual (1, txtTraceListener.InfoMessageCount);
		}

		[Test]						
		public void setSwitchToVerbose()
		{
			traceSource.Switch.Level = SourceLevels.Verbose;
			LogAllTraceLevels ();
			// Switch.Level is Verbose so it should log Critical, Error, Warning, Information, Verbose
			Assert.AreEqual (5, txtTraceListener.TotalMessageCount);
			Assert.AreEqual (1, txtTraceListener.VerboseMessageCount);
		}

		void LogAllTraceLevels ()
		{
			traceSource.TraceEvent (TraceEventType.Critical, 123, "Critical Level message.");
			traceSource.TraceEvent (TraceEventType.Error, 123, "Error Level message.");
			traceSource.TraceEvent (TraceEventType.Warning, 123, "Warning Level message.");
			traceSource.TraceEvent (TraceEventType.Information, 123, "Information Level message.");
			traceSource.TraceEvent (TraceEventType.Verbose, 123, "Verbose Level message.");
			traceSource.Flush ();
		}
	}
}

#endif
