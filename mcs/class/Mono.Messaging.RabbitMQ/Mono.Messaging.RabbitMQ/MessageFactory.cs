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
using System.Collections;
using System.IO;
using System.Text;

using Mono.Messaging;

using RabbitMQ.Client;
using RabbitMQ.Client.Content;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using RabbitMQ.Util;

namespace Mono.Messaging.RabbitMQ {

	public class MessageFactory {
		
		private static readonly string SENDER_VERSION_KEY = "SenderVersion";
		private static readonly string SOURCE_MACHINE_KEY = "SourceMachine";
		private static readonly string BODY_TYPE_KEY = "BodyType";
		
		public static IMessageBuilder WriteMessage (IModel ch, IMessage msg)
		{
			BasicMessageBuilder mb = new BasicMessageBuilder (ch);
			mb.Properties.MessageId = msg.Id;
			if (msg.CorrelationId != null)
				mb.Properties.CorrelationId = msg.CorrelationId;
			mb.Properties.Timestamp = MessageFactory.DateTimeToAmqpTimestamp (DateTime.Now);
			Hashtable headers = new Hashtable ();
			headers[SENDER_VERSION_KEY] = msg.SenderVersion;
			headers[SOURCE_MACHINE_KEY] = (string) System.Environment.MachineName;
			headers[BODY_TYPE_KEY] = msg.BodyType;
			
			mb.Properties.Headers = headers;
			Stream s = msg.BodyStream;
			s.Seek (0, SeekOrigin.Begin);
			byte[] buf = new byte[s.Length];			
			int numRead = msg.BodyStream.Read (buf, 0, buf.Length);
			mb.BodyStream.Write (buf, 0, buf.Length);
			return mb;
		}		
		
		public static IMessage ReadMessage (BasicDeliverEventArgs result)
		{
			MessageBase msg = new MessageBase ();
			Stream s = new MemoryStream ();
			s.Write (result.Body, 0, result.Body.Length);
			Console.WriteLine ("Body.Length Out {0}", result.Body.Length);
			DateTime arrivedTime = DateTime.Now;
			long senderVersion = (long) result.BasicProperties.Headers[SENDER_VERSION_KEY];
			string sourceMachine = GetString (result.BasicProperties, SOURCE_MACHINE_KEY);
			DateTime sentTime = AmqpTimestampToDateTime (result.BasicProperties.Timestamp);
			msg.SetDeliveryInfo (Acknowledgment.None,
			                     arrivedTime,
			                     null,
			                     result.BasicProperties.MessageId,
			                     MessageType.Normal,
			                     new byte[0],
			                     senderVersion,
			                     sourceMachine,
			                     null);
			msg.CorrelationId = result.BasicProperties.CorrelationId;
			msg.BodyStream = s;
			msg.BodyType = (int) result.BasicProperties.Headers[BODY_TYPE_KEY];
			return msg;
		}
		
		public static string GetString (IBasicProperties properties, String key)
		{
			byte[] b = (byte[]) properties.Headers[key];
			if (b == null)
				return null;
			
			return Encoding.UTF8.GetString (b);
		}
		
		public static AmqpTimestamp DateTimeToAmqpTimestamp (DateTime t)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan ts = t.ToUniversalTime () - epoch;
			return new AmqpTimestamp((long) ts.TotalSeconds);
		}
		
		public static DateTime AmqpTimestampToDateTime (AmqpTimestamp ats)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds (ats.UnixTime).ToLocalTime ();
		}
		
	}
}
