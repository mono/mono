//
// CallContextTest.cs - Unit tests for 
//	System.Runtime.Remoting.Messaging.CallContext
//
// Author:
//     Chris F Carroll <chris.carroll@unforgettable.me.uk>
//
// Copyright (C) 2013 Chris F Carroll (http://cafe-encounter.net)
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
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System;

namespace MonoTests.System.Runtime.Remoting.Messaging
{
	[TestFixture]
	public class CallContextTest
	{
		const string dataSlotName = "DataSlotKey";
		const string normaldata = "normalData";
		const int testWaitTimeoutMillis = 3000;
		EventWaitHandle spawnedThreadCompletedEWH;
		bool testSpawnsThreads;
		bool spawnedThreadDidRunBeforeTimeout;
		string retrievedData;

		[SetUp]
		public void SetUp ()
		{
			if (ExecutionContext.IsFlowSuppressed ()) {
				throw new InvalidOperationException (
					"These tests presuppose a normal ExecutionContext, in which flow has not been suppressed.");
			}
			spawnedThreadCompletedEWH = new EventWaitHandle (false, EventResetMode.AutoReset);
			testSpawnsThreads = true; //since most do, make this the default
			spawnedThreadDidRunBeforeTimeout = false;
			retrievedData = null;
		}

		[TearDown]
		public void TearDown ()
		{
			CallContext.FreeNamedDataSlot (dataSlotName);

			if (testSpawnsThreads && !spawnedThreadDidRunBeforeTimeout) {
				Assert.Fail ("Timed out waiting for spawned thread to run. Gave up after {0} seconds", 1m / 1000 * testWaitTimeoutMillis);
			}
		}

		[Test]
		public void FreeNamedDataSlot_ShouldClearLogicalData ()
		{
			//A
			CallContext.LogicalSetData ("slotkey", "illogical");
			//A
			CallContext.FreeNamedDataSlot ("slotkey");
			//A
			Assert.IsNull (CallContext.LogicalGetData ("slotkey"), "Illogical slot should be null");
			Assert.IsNull (CallContext.GetData ("slotkey"), "Illogical slot should be null");
			//cleanup
			testSpawnsThreads = false;
		}

		[Test]
		public void FreeNamedDataSlot_ShouldClearIllogicalData ()
		{
			//A
			CallContext.SetData ("slotkey", "illogical");
			//A
			CallContext.FreeNamedDataSlot ("slotkey");
			//A
			Assert.IsNull (CallContext.LogicalGetData ("slotkey"), "Illogical slot should be null");
			Assert.IsNull (CallContext.GetData ("slotkey"), "Illogical slot should be null");
			//cleanup
			testSpawnsThreads = false;
		}

		[Test]
		public void FreeNamedDataSlot_ShouldClearBothLogicalAndIllogicalData ()
		{
			//A
			CallContext.LogicalSetData ("slotkey","logical");
			CallContext.SetData ("slotkey", "illogical");
			//A
			CallContext.FreeNamedDataSlot ("slotkey");
			//A
			Assert.IsNull (CallContext.LogicalGetData ("slotkey"), "Illogical slot should be null");
			Assert.IsNull (CallContext.GetData ("slotkey"), "Illogical slot should be null");
			//cleanup
			testSpawnsThreads = false;
		}
			
		[Test]
		public void LogicalSetData_ShouldFlowToANewThreadStartedWithNewThreadStart ()
		{
			//A
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			new Thread (RetrieveLogicalDataAndSignalDone).Start ();
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data should flow to a new thread.");
		}

		[Test]
		public void LogicalSetData_ShouldFlowToANewThreadStartedWithThreadPoolQueueUserWorkItem ()
		{
			//A
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			ThreadPool.QueueUserWorkItem (o => RetrieveLogicalDataAndSignalDone ());
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data should flow to a new thread.");
		}

		[Test]
		public void LogicalSetData_ShouldFlowToNewThreadsBothStartedAndContinuedWithTaskFactory ()
		{
			//A
			var dataRetrievedInTask = null as string;
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			Task.Factory
				.StartNew (
				() => {
					dataRetrievedInTask = (string)CallContext.LogicalGetData (dataSlotName);
				})
				.ContinueWith (
				task => RetrieveLogicalDataAndSignalDone ()
			);
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreEqual ("logicalData", dataRetrievedInTask, "CallContext Logical Data set before a task should flow to the task.");
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data set before a task should flow to a continuation of a task.");
		}

		[Test]
		public void LogicalSetData_ShouldNotFlowFromATaskToItsContinuation ()
		{
			//A
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			Task.Factory
				.StartNew (
				() => CallContext.LogicalSetData (dataSlotName, "Set in Task"))
				.ContinueWith (
				task => RetrieveLogicalDataAndSignalDone ()
			);
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreNotEqual ("Set in Task", retrievedData, "CallContext Logical Data set during a task should not flow to its continuation.");
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data set before a task should flow to a continuation of a task.");
		}

		[Test]
		public void IllogicalSetData_ShouldNotFlowFromATaskToItsContinuation ()
		{
			//A
			CallContext.SetData (dataSlotName, "normalData");
			//A
			Task.Factory
				.StartNew (
				() => CallContext.SetData (dataSlotName, "Set in Task"))
				.ContinueWith (
				task => RetrieveIllogicalDataAndSignalDone ()
			);
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreNotEqual ("Set in Task", retrievedData, "CallContext Illogical Data set during a task should not flow to its continuation.");
			Assert.IsNull (retrievedData, "CallContext Illogical Data set before a task should not flow to a continuation of a task.");
		}

		[Test]
		public void IllogicalSetData_ShouldNotFlowToNewThreadsStartedOrContinuedWithTaskFactory ()
		{
			//A
			string dataRetrievedInTask = null;
			CallContext.SetData (dataSlotName, normaldata);
			//A
			Task.Factory
				.StartNew (
				() => {
					dataRetrievedInTask = (string)CallContext.GetData (dataSlotName);
				})
				.ContinueWith (
				task => RetrieveIllogicalDataAndSignalDone ()
			);
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreNotEqual (normaldata, retrievedData, "CallContext Illogical Data set before a task should not flow to a new thread started by TaskFactory.");
			Assert.IsNull (retrievedData);
			Assert.AreNotEqual (normaldata, dataRetrievedInTask, "CallContext Illogical Data set before a task should not flow to a continuation of a task");
			Assert.IsNull (dataRetrievedInTask);
		}

		[Test]
		public void IllogicalSetData_ShouldNotFlowToANewThreadStartedWithNewThreadStart ()
		{
			//A
			CallContext.SetData (dataSlotName, normaldata);
			//A
			new Thread (RetrieveIllogicalDataAndSignalDone).Start ();
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreNotEqual (normaldata, retrievedData, "CallContext Illogical Data should not flow to a new thread.");
			Assert.IsNull (retrievedData);
		}

		[Test]
		public void IllogicalSetData_ShouldNotFlowToANewThreadStartedWithThreadPoolQueueUserWorkItem ()
		{
			//A
			CallContext.SetData (dataSlotName, normaldata);
			//A
			ThreadPool.QueueUserWorkItem (o => RetrieveIllogicalDataAndSignalDone ());
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreNotEqual (normaldata, retrievedData, "CallContext Illogical Data should not flow to a new thread.");
			Assert.IsNull (retrievedData);
		}

		[Test]
		public void LogicalSetData_ShouldFlowToIllogicalGetDataAsSeenByANewThreadStartedWithNewThreadStart ()
		{
			//A
			CallContext.SetData (dataSlotName, "normalData");
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			new Thread (RetrieveIllogicalDataAndSignalDone).Start ();
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data should flow to a new thread.");
		}

		[Test]
		public void LogicalSetData_ShouldFlowToIllogicalGetDataAsSeenByANewThreadStartedWithThreadPoolQueueUserWorkItem ()
		{
			//A
			CallContext.SetData (dataSlotName, "normalData");
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			ThreadPool.QueueUserWorkItem (o => RetrieveIllogicalDataAndSignalDone ());
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data should flow to a new thread.");
		}

		[Test]
		public void LogicalSetData_ShouldFlowToIllogicalGetDataAsSeenByNewThreadsBothStartedAndContinuedWithTaskFactory ()
		{
			//A
			var dataRetrievedInTask = null as string;
			CallContext.SetData (dataSlotName, "normalData");
			CallContext.LogicalSetData (dataSlotName, "logicalData");
			//A
			Task.Factory
				.StartNew (
				() => {
					dataRetrievedInTask = (string)CallContext.GetData (dataSlotName);
				})
				.ContinueWith (
				task => RetrieveIllogicalDataAndSignalDone ()
			);
			//Wait
			spawnedThreadCompletedEWH.WaitOne (testWaitTimeoutMillis);
			//A
			Assert.AreEqual ("logicalData", dataRetrievedInTask, "CallContext Logical Data set before a task should flow to the task.");
			Assert.AreEqual ("logicalData", retrievedData, "CallContext Logical Data set before a task should flow to a continuation of a task.");
		}

		void RetrieveLogicalDataAndSignalDone ()
		{
			retrievedData = (string)CallContext.LogicalGetData (dataSlotName); //GetLogicalData
			spawnedThreadDidRunBeforeTimeout = true;
			spawnedThreadCompletedEWH.Set ();
		}

		void RetrieveIllogicalDataAndSignalDone ()
		{
			retrievedData = (string)CallContext.GetData (dataSlotName); //GetData, not Logical
			spawnedThreadDidRunBeforeTimeout = true;
			spawnedThreadCompletedEWH.Set ();
		}
	}
}

