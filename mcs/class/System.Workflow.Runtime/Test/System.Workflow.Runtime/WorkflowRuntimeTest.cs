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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using NUnit.Framework;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace MonoTests.System.Workflow.Runtime
{
	public class WorkflowLoaderServiceTest1 : WorkflowLoaderService
	{
		public WorkflowLoaderServiceTest1 ()
		{

		}

		protected override Activity CreateInstance (Type workflowType)
		{
			return null;
		}

		protected override Activity CreateInstance (XmlReader workflowDefinitionReader, XmlReader rulesReader)
		{
			return null;
		}
	}

	public class WorkflowLoaderServiceTest2 : WorkflowLoaderService
	{
		public WorkflowLoaderServiceTest2 ()
		{

		}

		protected override Activity CreateInstance (Type workflowType)
		{
			return null;
		}

		protected override Activity CreateInstance (XmlReader workflowDefinitionReader, XmlReader rulesReader)
		{
			return null;
		}
	}

	[TestFixture]
	public class WorkflowRuntimeTest
	{

		[Test]
		public void Services ()
		{
			// By default there are no services
			WorkflowRuntime wr = new WorkflowRuntime ();
			Assert.AreEqual (0, (wr.GetAllServices (typeof (WorkflowLoaderService))).Count, "C1#1");
			//Assert.AreEqual (0, (wr.GetAllServices (typeof (WorkflowPersistenceService))).Count, "C1#2");
			//Assert.AreEqual (0, (wr.GetAllServices (typeof (WorkflowQueuingService))).Count, "C1#3");

			// Can have to diferent instances of the same class
			WorkflowRuntime wr3 = new WorkflowRuntime ();
			WorkflowLoaderServiceTest1 wls = new WorkflowLoaderServiceTest1 ();
			wr3.AddService (wls);
			Assert.AreEqual (wls, wr3.GetService (typeof (WorkflowLoaderService)), "C1#2");

			wr3.AddService (new WorkflowLoaderServiceTest1 ());

			Assert.AreEqual (2, (wr3.GetAllServices (typeof (WorkflowLoaderService))).Count, "C1#3");
			Assert.AreEqual (2, wr3.GetAllServices <WorkflowLoaderService> ().Count, "C1#3");

			wr.AddService (new WorkflowLoaderServiceTest1 ());

			//foreach (object t in wr.GetAllServices (typeof (WorkflowLoaderService))) {
			//	Console.WriteLine ("Types {0}", t.GetType ());
			//}
		}

		[Test]
		public void Start ()
		{
			WorkflowRuntime wr = new WorkflowRuntime ();
			wr.StartRuntime ();

			Assert.AreEqual (1, (wr.GetAllServices (typeof (WorkflowLoaderService))).Count, "C1#1");

			foreach (object t in wr.GetAllServices (typeof (WorkflowLoaderService))) {
				Console.WriteLine ("Types {0}", t.GetType ());
			}
		}

		[Test]
		public void CreateGetWorkflow ()
		{
			Guid guid1 = Guid.NewGuid ();
			Guid guid2 = Guid.NewGuid ();

			WorkflowRuntime wr = new WorkflowRuntime ();
			Assert.AreEqual (false, wr.IsStarted, "C1#1");
			WorkflowInstance wi1 = wr.CreateWorkflow (typeof (SequentialWorkflowActivity), null, guid1);
			Assert.AreEqual (wi1.InstanceId, guid1, "C1#2");

			Assert.AreEqual (true, wr.IsStarted, "C1#3");
			WorkflowInstance wi2 = wr.CreateWorkflow (typeof (SequenceActivity), null, guid2);
			Assert.AreEqual (wi2.InstanceId, guid2, "C1#4");
		}


		// Exceptions
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddServiceTwice ()
		{
			WorkflowRuntime wr = new WorkflowRuntime ();
			WorkflowLoaderServiceTest1 wl = new WorkflowLoaderServiceTest1 ();
			wr.AddService (wl);
			wr.AddService (wl);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RemoveUnexistantService ()
		{
			WorkflowRuntime wr = new WorkflowRuntime ();
			WorkflowLoaderServiceTest1 wl = new WorkflowLoaderServiceTest1 ();
			wr.RemoveService (wl);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetServiceWithTwoA ()
		{
			WorkflowRuntime wr = new WorkflowRuntime ();
			WorkflowLoaderServiceTest1 wl = new WorkflowLoaderServiceTest1 ();
			WorkflowLoaderServiceTest2 w2 = new WorkflowLoaderServiceTest2 ();
			wr.AddService (wl);
			wr.AddService (w2);
			wr.GetService (typeof (WorkflowLoaderService));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetServiceWithTwoB ()
		{
			WorkflowRuntime wr = new WorkflowRuntime ();
			WorkflowLoaderServiceTest1 wl = new WorkflowLoaderServiceTest1 ();
			WorkflowLoaderServiceTest2 w2 = new WorkflowLoaderServiceTest2 ();
			wr.AddService (wl);
			wr.AddService (w2);
			wr.GetService <WorkflowLoaderService> ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetInstanceNoRuntimeStarted ()
		{
			// This operation can only be performed with a started WorkflowRuntime
			WorkflowRuntime wr = new WorkflowRuntime ();
			wr.GetWorkflow (Guid.NewGuid ());
		}
	}
}

