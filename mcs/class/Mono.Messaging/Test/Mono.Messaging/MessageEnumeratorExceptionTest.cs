//
// MessageEnumeratorTest.cs -
//	NUnit Test Cases for MessageEnumerator
//
// Author:
//	Michael Barker  <mike@middlesoft.co.uk>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System;
using System.Security;
using System.Security.Permissions;
using System.Reflection;

using Mono.Messaging;

using NUnit.Framework;
using NUnit.Mocks;


namespace MonoTests.Mono.Messaging {
	
	[TestFixture]
	public class MessageEnumeratorExceptionTest
	{
		private DynamicMock mockME;
		
		[SetUp]
		public void Init ()
		{
			mockME = new DynamicMock (typeof (IMessageEnumerator));
		}

		[Test]
		[ExpectedException("System.Messaging.MessageQueueException")]
		public void RemoveCurrentThrowsConnectionException ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new ConnectionException (QueueReference.DEFAULT), null);
			System.Messaging.MessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}
		
		[Test]
		[ExpectedException("System.InvalidOperationException")]
		public void RemoveCurrentThrowsMessageUnavailableException ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new MessageUnavailableException (), null);
			System.Messaging.MessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}		
		
		[Test]
		[ExpectedException("System.Messaging.MessageQueueException")]
		public void RemoveCurrentThrowsMonoMessagingException ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new MonoMessagingException (), null);
			System.Messaging.MessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}		
		
		[Test]
		[ExpectedException("System.NotImplementedException")]
		public void RemoveCurrentThrowsMessageNotImplemented ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new NotImplementedException (), null);
			System.Messaging.MessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}		
	
		public System.Messaging.MessageEnumerator CreateEnumerator (IMessageEnumerator ime)
		{
            Type[] types = { 
                typeof (IMessageEnumerator), typeof (System.Messaging.IMessageFormatter)
            };
                
            ConstructorInfo ci = typeof (System.Messaging.MessageEnumerator).GetConstructor (
                BindingFlags.NonPublic | BindingFlags.Instance, 
                Type.DefaultBinder, types, new ParameterModifier[0]);
                
            if (ci == null)
                throw new Exception ("ConstructorInfo is null");
            
            return (System.Messaging.MessageEnumerator) ci.Invoke (new object[] { ime, null });
		}
	}
}
