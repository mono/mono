//
// MonoTests.System.Runtime.Remoting.BaseCalls.cs
//
// Author: Lluis Sanchez Gual (lluis@ximian.com)
//
// 2003 (C) Copyright, Novell, Inc.
//

using System;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;
using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting
{
	
	public class NeedsContextAttribute: Attribute, IContextAttribute 
	{
		public void GetPropertiesForNewContext (IConstructionCallMessage msg) {}
		public bool IsContextOK (Context ctx, IConstructionCallMessage msg) { return false; }
	}
	
	[NeedsContextAttribute]
	public class TestCbo: ContextBoundObject
	{
		public Context GetContext ()
		{
			return Thread.CurrentContext;
		}
	}
	
	[TestFixture]
	[Category ("MobileNotWorking")] // Bug #10267
	public class ContextTest
	{
		TestCbo cbo = new TestCbo ();
		Context otherCtx;
		LocalDataStoreSlot slot;
		
		[Test]
		public void TestDoCallback ()
		{
			otherCtx = cbo.GetContext ();
			Assert.IsTrue (Thread.CurrentContext != otherCtx, "New context not created");
			
			otherCtx.DoCallBack (new CrossContextDelegate (DelegateTarget));
		}
		
		void DelegateTarget ()
		{
			Assert.IsTrue (Thread.CurrentContext == otherCtx, "Wrong context");
		}
		
		[Test]
		public void TestDatastore ()
		{
			otherCtx = cbo.GetContext ();
			
			slot = Context.AllocateDataSlot ();
			LocalDataStoreSlot namedSlot1 = Context.AllocateNamedDataSlot ("slot1");
			LocalDataStoreSlot namedSlot2 = Context.GetNamedDataSlot ("slot2");
			
			Context.SetData (slot, "data");
			Context.SetData (namedSlot1, "data1");
			Context.SetData (namedSlot2, "data2");
			
			otherCtx.DoCallBack (new CrossContextDelegate (CheckOtherContextDatastore));
			
			Assert.IsTrue (Context.GetData (slot).Equals ("data"), "Wrong data 1");
			Assert.IsTrue (Context.GetData (namedSlot1).Equals ("data1"), "Wrong data 2");
			Assert.IsTrue (Context.GetData (namedSlot2).Equals ("data2"), "Wrong data 3");
			
			try
			{
				namedSlot1 = Context.AllocateNamedDataSlot ("slot1");
				Assert.Fail ("Exception expected");
			}
			catch {}
			
			Context.FreeNamedDataSlot ("slot1");
			Context.FreeNamedDataSlot ("slot2");
			
			try
			{
				namedSlot1 = Context.AllocateNamedDataSlot ("slot1");
			}
			catch 
			{
				Assert.Fail ("Exception not expected");
			}
			
			Context.FreeNamedDataSlot ("slot1");
		}
		
		void CheckOtherContextDatastore ()
		{
			LocalDataStoreSlot namedSlot1 = Context.GetNamedDataSlot ("slot1");
			LocalDataStoreSlot namedSlot2 = Context.GetNamedDataSlot ("slot2");
			
			Assert.IsTrue (Context.GetData (slot) == null, "Slot already has data");
			Assert.IsTrue (Context.GetData (namedSlot1) == null, "Slot already has data");
			Assert.IsTrue (Context.GetData (namedSlot2) == null, "Slot already has data");
			
			Context.SetData (slot, "other data");
			Context.SetData (namedSlot1, "other data1");
			Context.SetData (namedSlot2, "other data2");
		}
		
	}
}
