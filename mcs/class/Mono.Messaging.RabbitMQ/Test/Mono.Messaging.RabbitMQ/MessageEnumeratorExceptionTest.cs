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

using SystemMessageEnumerator = System.Messaging.MessageEnumerator;
using SystemMessageQueueException = System.Messaging.MessageQueueException;
using SystemIMessageFormatter = System.Messaging.IMessageFormatter;

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
		[ExpectedException(typeof(SystemMessageQueueException))]
		public void RemoveCurrentThrowsConnectionException ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new ConnectionException (QueueReference.DEFAULT), null);
			SystemMessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}
		
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void RemoveCurrentThrowsMessageUnavailableException ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new MessageUnavailableException (), null);
			SystemMessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}		
		
		[Test]
		[ExpectedException(typeof(SystemMessageQueueException))]
		public void RemoveCurrentThrowsMonoMessagingException ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new MonoMessagingException (), null);
			SystemMessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}		
		
		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void RemoveCurrentThrowsMessageNotImplemented ()
		{
			mockME.ExpectAndThrow ("RemoveCurrent", new NotImplementedException (), null);
			SystemMessageEnumerator me = CreateEnumerator ((IMessageEnumerator) mockME.MockInstance);
			me.RemoveCurrent ();
		}		
	
		public SystemMessageEnumerator CreateEnumerator (IMessageEnumerator ime)
		{
            Type[] types = { 
                typeof (IMessageEnumerator), typeof (SystemIMessageFormatter)
            };
                
            ConstructorInfo ci = typeof (SystemMessageEnumerator).GetConstructor (
                BindingFlags.NonPublic | BindingFlags.Instance, 
                Type.DefaultBinder, types, new ParameterModifier[0]);
                
            if (ci == null)
                throw new Exception ("ConstructorInfo is null");
            
            return (SystemMessageEnumerator) ci.Invoke (new object[] { ime, null });
		}
	}
}
