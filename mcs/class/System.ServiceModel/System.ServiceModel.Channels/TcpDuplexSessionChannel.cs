// 
// TcpDuplexSessionChannel.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Channels
{
	internal class TcpDuplexSessionChannel : DuplexChannelBase, IDuplexSessionChannel
	{

		TcpChannelInfo info;
		TcpClient client;
		bool is_service_side;
		EndpointAddress local_address;
		TcpListener tcp_listener;
		TimeSpan timeout;
		
		public TcpDuplexSessionChannel (ChannelFactoryBase factory, TcpChannelInfo info, EndpointAddress address, Uri via)
			: base (factory, address, via)
		{
			is_service_side = false;
			this.info = info;
		}
		
		public TcpDuplexSessionChannel (ChannelListenerBase listener, TcpChannelInfo info, TcpClient acceptedRequest, TimeSpan timeout)
			: base (listener)
		{
			is_service_side = true;
			this.info = info;
			this.client = acceptedRequest;
			this.timeout = timeout;

			Stream s = client.GetStream ();

			//while (s.CanRead)
			//	Console.Write ("{0:X02} ", s.ReadByte ());
			
			for (int i = 0; i < 6; i++)
				s.ReadByte ();
			
			int size = s.ReadByte ();
			
			for (int i = 0; i < size; i++)
				s.ReadByte (); // URI
			
			s.ReadByte ();
			s.ReadByte ();
			s.ReadByte ();
			s.WriteByte (0x0B);
		}
		
		public MessageEncoder Encoder {
			get { return info.MessageEncoder; }
		}

		public override EndpointAddress LocalAddress {
			get { return local_address; }
		}
		
		// FIXME: implement
		public IDuplexSession Session {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override IAsyncResult BeginSend (Message message, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginSend (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void EndSend (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Send (Message message)
		{
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			
			try
			{
				NetworkStream stream = client.GetStream ();
				MyBinaryWriter bw = new MyBinaryWriter (stream);
				bw.Write ((byte) 6);
				Encoder.WriteMessage (message, ms);
				bw.WriteBytes (ms.ToArray ());
				bw.Write ((byte) 7);
				bw.Flush ();

				stream.ReadByte (); // 7
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		
		[MonoTODO]
		public override void Send (Message message, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginTryReceive (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override IAsyncResult BeginWaitForMessage (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override Message EndReceive (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool EndTryReceive (IAsyncResult result, out Message message)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool EndWaitForMessage (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override Message Receive ()
		{
			Stream s = client.GetStream ();
			s.ReadByte (); // 6
			MyBinaryReader br = new MyBinaryReader (s);
//			string msg = br.ReadString ();
//			br.Read7BitEncodedInt ();
			byte [] buffer = new byte [65536];
			buffer = br.ReadBytes ();
			MemoryStream ms = new MemoryStream ();
			ms.Write (buffer, 0, buffer.Length);
			ms.Seek (0, SeekOrigin.Begin);
			
//			while (s.CanRead)
//				Console.Write ("{0:X02} ", s.ReadByte ());
			
			Message msg = null;
			// FIXME: To supply maxSizeOfHeaders.
			msg = Encoder.ReadMessage (ms, 0x10000);
			s.ReadByte (); // 7
//			Console.WriteLine (msg);
			s.WriteByte (7);
			s.Flush ();

			return msg;
		}
		
		[MonoTODO]
		public override Message Receive (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool TryReceive (TimeSpan timeout, out Message message)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override bool WaitForMessage (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
		
		// CommunicationObject
		
		[MonoTODO]
		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginClose (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IAsyncResult OnBeginOpen (TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnClose (TimeSpan timeout)
		{
			client.Close ();
		}
		
		[MonoTODO]
		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnOpen (TimeSpan timeout)
		{
			if (! is_service_side) {
				int explicitPort = RemoteAddress.Uri.Port;
				client = new TcpClient (RemoteAddress.Uri.Host, explicitPort <= 0 ? TcpTransportBindingElement.DefaultPort : explicitPort);
				                        //RemoteAddress.Uri.Port);
				
				NetworkStream ns = client.GetStream ();
				ns.WriteByte (0);
				ns.WriteByte (1);
				ns.WriteByte (0);
				ns.WriteByte (1);
				ns.WriteByte (2);
				ns.WriteByte (2);
				byte [] bytes = System.Text.Encoding.UTF8.GetBytes (RemoteAddress.Uri.ToString ());
				ns.WriteByte ((byte) bytes.Length);
				ns.Write (bytes, 0, bytes.Length);
				ns.WriteByte (3);
				ns.WriteByte (3);
				ns.WriteByte (0xC);
				int hoge = ns.ReadByte ();
				//while (ns.CanRead)
				//	Console.Write ("{0:X02} ", ns.ReadByte ());
			}
			// Service side.
			/*
			else
				Console.WriteLine ("Server side.");
			*/
		}
		
		// FIXME: To look for other way to do this.
		class MyBinaryReader : BinaryReader
		{
			public MyBinaryReader (Stream s)
				: base (s)
			{
			}
			
			public byte [] ReadBytes ()
			{
				byte [] buffer = new byte [65536];
				int length = Read7BitEncodedInt ();
				
				if (length > 65536)
					throw new InvalidOperationException ("The message is too large.");
				
				Read (buffer, 0, length);
				
				return buffer;
			}
		}
		
		class MyBinaryWriter : BinaryWriter
		{
			public MyBinaryWriter (Stream s)
				: base (s)
			{
			}
			
			public void WriteBytes (byte [] bytes)
			{
				Write7BitEncodedInt (bytes.Length);
				Write (bytes);
			}
		}
	}
}
