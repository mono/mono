// System.Runtime.Remoting.Channels.CORBA.IIOPMessage.cs
//
// Author:
//	DietmarMaurer (dietmar@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com

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

using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.CORBA {

	public sealed class IIOPMessage 
	{
		public enum MessageType : byte {
			Request = 0,
			Response = 1,
			Exception = 2,
			Unknown = 3
		}
		
		public IIOPMessage ()
		{
		}
		
		public static void SendExceptionMessage (Stream network_stream, string message)
		{
			// we use the uri field to encode the message text
			SendMessageStream (network_stream, null, MessageType.Exception, message);
		}
		
		public static void SendMessageStream (Stream network_stream, MemoryStream data, 
						      MessageType msg_type, string uri)
		{
			int data_len = 0;
			
			if (data != null)
				data_len = (int)data.Length;

			int uri_len = 0;

			if (uri != null)
				uri_len = uri.Length;

			int header_len = 12 + uri_len * 2;
			int msg_len = data_len + header_len;
			
			byte [] buffer = new byte [msg_len];

			// magic header signature
			buffer [0] = 255;
			buffer [1] = 0;
			buffer [2] = 255;
			buffer [3] = (byte) msg_type;

			// data length
			buffer [4] = (byte) data_len;
			buffer [5] = (byte) (data_len >> 8);
			buffer [6] = (byte) (data_len >> 16);
			buffer [7] = (byte) (data_len >> 24);

			// uri length
			buffer [8] = (byte) uri_len;
			buffer [9] = (byte) (uri_len >> 8);
			buffer [10] = (byte) (uri_len >> 16);
			buffer [11] = (byte) (uri_len >> 24);

			// uri
			for (int i = 0; i < uri_len; i++) {
				buffer [12 + i*2] = (byte) uri [i];
				buffer [13 + i*2] = (byte) (uri [i] >> 8);
			}

			if (data_len > 0) {
				byte [] data_buffer = data.GetBuffer ();
				for (int i = 0; i < data_len; i++)
					buffer [i + header_len] = data_buffer [i];
			}
			
			network_stream.Write (buffer, 0, msg_len);
		}
		
		public static MemoryStream ReceiveMessageStream (Stream network_stream,
								 out MessageType msg_type,
								 out string uri)
		{
			int data_len = 0;
			int uri_len = 0;
			msg_type = MessageType.Unknown;
			uri = null;
			
			// search for message header (255, 0, 255, msg_type, msg_len)
			while (true) {
				while (true) {
					int x = network_stream.ReadByte ();
					if (x != 255)
						continue;
					x = network_stream.ReadByte ();
					if (x != 0)
						continue;
					x = network_stream.ReadByte ();
					if (x != 255)
						continue;
					break;
				}

				msg_type = (MessageType)network_stream.ReadByte ();
				
				byte [] buffer = new byte [8];
				
				int bytes_read = network_stream.Read (buffer, 0, 8);
				if (bytes_read != 8)
					continue;
				
				data_len = (buffer [0] | (buffer [1] << 8) |
					    (buffer [2] << 16) | (buffer [3] << 24));
				
				uri_len = (buffer [4] | (buffer [5] << 8) |
					   (buffer [6] << 16) | (buffer [7] << 24));
				
				if (uri_len > 0) {
					byte [] uri_buffer = new byte [uri_len * 2];
					bytes_read = network_stream.Read (uri_buffer, 0, uri_len * 2);
					if (bytes_read != (uri_len * 2))
						continue;
					char [] uri_array = new char [uri_len];
					for (int i = 0; i < uri_len; i++) {
						uri_array [i] = (char) (uri_buffer [i * 2] | (uri_buffer [(i * 2) + 1] << 8));
					}
					uri = new string (uri_array);
				}
				break;
			}

			if (msg_type == MessageType.Exception)
				throw new RemotingException ("\n" + uri + "\n" + "Rethrown at:\n");
			
			byte [] stream_buffer = new byte [data_len];
			if ((network_stream.Read (stream_buffer, 0, data_len)) != data_len)
				throw new Exception ("packet size error");
			
			return new MemoryStream (stream_buffer, false);
		}		
	}
}
