//
// Mono.Messaging.RabbitMQ
//
// Authors:
//	  Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
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

using System;
using System.Messaging;
using System.Reflection;
using Mono.Messaging;

using SystemAcknowledgeTypes = System.Messaging.AcknowledgeTypes;
using SystemCryptographicProviderType = System.Messaging.CryptographicProviderType;
using SystemEncryptionAlgorithm = System.Messaging.EncryptionAlgorithm;
using SystemHashAlgorithm = System.Messaging.HashAlgorithm;
using SystemMessagePriority = System.Messaging.MessagePriority;

using NUnit.Framework;

namespace MonoTests.Mono.Messaging {

	[TestFixture]
	public class MessageBaseTest {
		
		[Test]
		public void CheckDefaultValues ()
		{
            Type[] types = { 
                typeof (IMessage), typeof (object), typeof (IMessageFormatter)
            };
                
            ConstructorInfo ci = typeof (Message).GetConstructor (
                BindingFlags.NonPublic | BindingFlags.Instance, 
                Type.DefaultBinder, types, new ParameterModifier[0]);
                
            if (ci == null)
                throw new Exception ("ConstructorInfo is null");
            
            Message m = (Message) ci.Invoke (new object[] { new MessageBase (), null, null });
			
			Assert.IsNull (m.Body, "Body default should be Null");
			Assert.IsNull (m.Formatter, "Formatter default should null");
			
			Assert.AreEqual (SystemAcknowledgeTypes.None, 
					m.AcknowledgeType, 
					"AcknowledgeType default should be None");
			Assert.AreEqual (null, m.AdministrationQueue, 
					"AdministrationQueue default should be null");
			Assert.AreEqual (0, m.AppSpecific, "AppSpecific default should be 0");
			Assert.AreEqual (true, m.AttachSenderId, "AttachSenderId default should be true");
			Assert.AreEqual ("Microsoft Base Cryptographic Provider, Ver. 1.0", m.AuthenticationProviderName, 
					"AuthenticationProviderName should default to \"Microsoft Base Cryptographic Provider, Ver. 1.0\"");
			Assert.AreEqual (SystemCryptographicProviderType.RsaFull, 
					m.AuthenticationProviderType, 
					"AuthenticationProviderType should default to RsaFull");
			Assert.AreEqual (null, m.BodyStream, "BodyStream should default to null");
			Assert.AreEqual (Guid.Empty, m.ConnectorType, "ConnectorType should default to empty");
			Assert.AreEqual (null, m.CorrelationId, "CorrelationId should default to null");
			Assert.AreEqual (new byte[0], m.DestinationSymmetricKey, 
					"DestinationSymmetricKey should default to an empty array");
			Assert.AreEqual (new byte[0], m.DigitalSignature,
					"DigitalSignature default to an empty array");
			Assert.AreEqual (SystemEncryptionAlgorithm.Rc2,
					m.EncryptionAlgorithm,
					"EncryptionAlgorithm should default to Rc2");
			Assert.AreEqual (new byte[0], m.Extension, 
					"Extension should default to an empty array");
			Assert.AreEqual (SystemHashAlgorithm.Sha, m.HashAlgorithm, 
					"HashAlgorithm should default to Sha");
			Assert.AreEqual (Guid.Empty.ToString () + "\\0", m.Id, "Id should default to Guid.Empty");
			Assert.AreEqual ("", m.Label, "Label should default to \"\"");
			Assert.AreEqual (false, m.IsFirstInTransaction, "IsFirstInTransaction should default to false");
			Assert.AreEqual (false, m.IsLastInTransaction, "IsLastInTransaction should default to false");
			Assert.AreEqual (SystemMessagePriority.Normal, m.Priority,
					"Priority should default to Normal");
			Assert.AreEqual (false, m.Recoverable, "Recoverable should default to false");
			Assert.AreEqual (null, m.ResponseQueue, "ResponseQueue should default to null");
			//Assert.AreEqual (null, m.SecurityContext, "SecurityContext should default to null");
			Assert.AreEqual (new byte[0], m.SenderCertificate, 
					"SenderCertificate should default to an empty array");
			Assert.AreEqual (Message.InfiniteTimeout, m.TimeToBeReceived,
					"TimeToBeReceived should default to InfiniteTimeout");
			Assert.AreEqual (Message.InfiniteTimeout, m.TimeToReachQueue,
					"TimeToReadQueue should default to InfiniteTimeout");
			Assert.AreEqual (false, m.UseAuthentication, 
					"UseAuthentication should default to false");
			Assert.AreEqual (false, m.UseDeadLetterQueue,
					"UseDeadLetterQueue should default to false");
			Assert.AreEqual (false, m.UseEncryption, "Encryption should default to false");
			Assert.AreEqual (false, m.UseJournalQueue, 
					"UseJournalQueue should default to false");
			Assert.AreEqual (false, m.UseTracing, "UseTracing should default to false");
		}
	}
}
