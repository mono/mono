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
	public class ContextText: Assertion
	{
		TestCbo cbo = new TestCbo ();
		Context otherCtx;
		LocalDataStoreSlot slot;
		
		[Test]
		public void TestDoCallback ()
		{
			otherCtx = cbo.GetContext ();
			Assert ("New context not created", Thread.CurrentContext != otherCtx);
			
			otherCtx.DoCallBack (new CrossContextDelegate (DelegateTarget));
		}
		
		void DelegateTarget ()
		{
			Assert ("Wrong context", Thread.CurrentContext == otherCtx);
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
			
			Assert ("Wrong data 1", Context.GetData (slot).Equals ("data"));
			Assert ("Wrong data 2", Context.GetData (namedSlot1).Equals ("data1"));
			Assert ("Wrong data 3", Context.GetData (namedSlot2).Equals ("data2"));
			
			try
			{
				namedSlot1 = Context.AllocateNamedDataSlot ("slot1");
				Assert ("Exception expected",false);
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
				Assert ("Exception not expected",false);
			}
			
			Context.FreeNamedDataSlot ("slot1");
		}
		
		void CheckOtherContextDatastore ()
		{
			LocalDataStoreSlot namedSlot1 = Context.GetNamedDataSlot ("slot1");
			LocalDataStoreSlot namedSlot2 = Context.GetNamedDataSlot ("slot2");
			
			Assert ("Slot already has data", Context.GetData (slot) == null);
			Assert ("Slot already has data", Context.GetData (namedSlot1) == null);
			Assert ("Slot already has data", Context.GetData (namedSlot2) == null);
			
			Context.SetData (slot, "other data");
			Context.SetData (namedSlot1, "other data1");
			Context.SetData (namedSlot2, "other data2");
		}
		
	}
}
