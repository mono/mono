// System.Runtime.Remoting.Channels.Tcp.TcpMessageIO.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002 Lluis Sanchez Gual

using System.Runtime.Serialization;
using System.Collections;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace System.Runtime.Remoting.Channels.Tcp 
{
	enum MessageStatus { MethodMessage = 0, CancelSignal = 1, Unknown = 10}

	internal class TcpMessageIO
	{
		static byte[][] _msgHeaders = 
			  {
				  new byte[] { (byte)'.', (byte)'N', (byte)'E', (byte)'T', 0 },
				  new byte[] { 255, 255, 255, 255, 255 }
			  };
							
		public static int DefaultStreamBufferSize = 1000;

		// Identifies an incoming message
		public static MessageStatus ReceiveMessageStatus (Stream networkStream)
		{
			try
			{
				bool[] isOnTrack = new bool[_msgHeaders.Length];
				bool atLeastOneOnTrack = true;
				int i = 0;

				while (atLeastOneOnTrack)
				{
					atLeastOneOnTrack = false;
					byte c = (byte)networkStream.ReadByte();
					for (int n = 0; n<_msgHeaders.Length; n++)
					{
						if (i > 0 && !isOnTrack[n]) continue;

						isOnTrack[n] = (c == _msgHeaders[n][i]);
						if (isOnTrack[n] && (i == _msgHeaders[n].Length-1)) return (MessageStatus) n;
						atLeastOneOnTrack = atLeastOneOnTrack || isOnTrack[n];
					}
					i++;
				}
				return MessageStatus.Unknown;
			}
			catch (IOException)
			{
				// Stream closed
				return MessageStatus.CancelSignal;
			}
		}

		public static void SendMessageStream (Stream networkStream, Stream data, ITransportHeaders requestHeaders, byte[] buffer)
		{
			if (buffer == null) buffer = new byte[DefaultStreamBufferSize];

			// Writes the message start header
			byte[] dotnetHeader = _msgHeaders[(int) MessageStatus.MethodMessage];
			networkStream.Write(dotnetHeader, 0, dotnetHeader.Length);

			// Writes the length of the stream being sent (not including the headers)
			int num = (int)data.Length;
			buffer [0] = (byte) num;
			buffer [1] = (byte) (num >> 8);
			buffer [2] = (byte) (num >> 16);
			buffer [3] = (byte) (num >> 24);
			networkStream.Write(buffer, 0, 4);

			// Writes the message headers
			SendHeaders (networkStream, requestHeaders, buffer);

			// Writes the stream
			if (data is MemoryStream)
			{
				// The copy of the stream can be optimized. The internal
				// buffer of MemoryStream can be used.
				MemoryStream memStream = (MemoryStream)data;
				networkStream.Write (memStream.GetBuffer(), 0, (int)memStream.Length);
			}
			else
			{
				int nread = data.Read (buffer, 0, buffer.Length);
				while (nread > 0)
				{
					networkStream.Write (buffer, 0, nread);
					nread = data.Read (buffer, 0, buffer.Length);
				}
			}
		}

		private static void SendHeaders(Stream networkStream, ITransportHeaders requestHeaders, byte[] buffer)
		{
			if (requestHeaders == null) 
				SendString (networkStream, "", buffer);
			else
			{
				// Writes the headers as a sequence of strings
				IEnumerator e = requestHeaders.GetEnumerator();
				while (e.MoveNext())
				{
					DictionaryEntry hdr = (DictionaryEntry)e.Current;
					SendString (networkStream, hdr.Key.ToString(), buffer);
					SendString (networkStream, hdr.Value.ToString(), buffer);
				}
				SendString (networkStream, "", buffer);
			}
		}
		
		public static ITransportHeaders ReceiveHeaders (Stream networkStream, byte[] buffer)
		{
			TransportHeaders headers = new TransportHeaders();

			string key = ReceiveString (networkStream, buffer);
			while (key != string.Empty)
			{
				headers[key] = ReceiveString (networkStream, buffer);
				key = ReceiveString (networkStream, buffer);
			}
			return headers;
		}
		
		public static Stream ReceiveMessageStream (Stream networkStream, out ITransportHeaders headers, byte[] buffer)
		{
			if (buffer == null) buffer = new byte[DefaultStreamBufferSize];

			// Gets the length of the data stream
			int nr = 0;
			while (nr < 4)
				nr += networkStream.Read (buffer, nr, 4 - nr);

			int byteCount = (buffer [0] | (buffer [1] << 8) |
				(buffer [2] << 16) | (buffer [3] << 24));

			// Reads the headers

			headers = ReceiveHeaders (networkStream, buffer);

			byte[] resultBuffer = new byte[byteCount];

			nr = 0;
			while (nr < byteCount)
				nr += networkStream.Read (resultBuffer, nr, byteCount - nr);
			
			return new MemoryStream(resultBuffer);
		}		

		private static void SendString (Stream networkStream, string str, byte[] buffer)
		{
			// Allocates a buffer. Use the internal buffer if it is 
			// big enough. If not, create a new one.

			int maxBytes = Encoding.UTF8.GetMaxByteCount(str.Length)+4;	//+4 bytes for storing the string length
			if (maxBytes > buffer.Length)
				buffer = new byte[maxBytes];

			int num = Encoding.UTF8.GetBytes (str, 0, str.Length, buffer, 4);

			// store number of bytes (not number of chars!)

			buffer [0] = (byte) num;
			buffer [1] = (byte) (num >> 8);
			buffer [2] = (byte) (num >> 16);
			buffer [3] = (byte) (num >> 24);

			// Write the string bytes
			networkStream.Write (buffer, 0, num + 4);
		}

		private static string ReceiveString (Stream networkStream, byte[] buffer)
		{
			int nr = 0;
			while (nr < 4)
				nr += networkStream.Read (buffer, nr, 4 - nr);

			// Reads the number of bytes (not chars!)

			int byteCount = (buffer [0] | (buffer [1] << 8) |
				(buffer [2] << 16) | (buffer [3] << 24));

			if (byteCount == 0) return string.Empty;

			// Allocates a buffer of the correct size. Use the
			// internal buffer if it is big enough

			if (byteCount > buffer.Length)
				buffer = new byte[byteCount];

			// Reads the string

			nr = 0;
			while (nr < byteCount)
				nr += networkStream.Read (buffer, nr, byteCount - nr);

			char[] chars = Encoding.UTF8.GetChars(buffer, 0, byteCount);
			return new string(chars);
		}
		
	}
}
