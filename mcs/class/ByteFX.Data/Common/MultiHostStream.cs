using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Threading;

namespace ByteFX.Data.Common
{
	internal enum MultiHostStreamErrorType 
	{
		Connecting,
		Reading,
		Writing
	}

	/// <summary>
	/// Summary description for MultiHostStream.
	/// </summary>
	internal abstract class MultiHostStream : Stream
	{
		protected Stream	stream;
		protected int		readTimeOut;
		protected Exception	baseException;

		/// <summary>
		/// Constructs a new MultiHostStream object with the given parameters
		/// </summary>
		/// <param name="hostList"></param>
		/// <param name="port"></param>
		/// <param name="readTimeOut"></param>
		/// <param name="connectTimeOut"></param>
		public MultiHostStream(string hostList, int port, int readTimeOut, int connectTimeOut)
		{
			this.readTimeOut = readTimeOut;
			ProcessHosts( hostList, port, connectTimeOut );
		}

		// abstract members
		protected abstract void TimeOut(MultiHostStreamErrorType error);
		protected abstract void Error(string msg);
		protected abstract bool CreateStream( IPAddress ip, string host, int port );
		protected abstract bool CreateStream (string fileName);
		protected abstract bool DataAvailable 
		{
			get;
		}

		private void ProcessHosts( string hostList, int port, int connectTimeOut )
		{
			int startTime = Environment.TickCount;

			int toTicks = connectTimeOut * 1000;

			// support Unix sockets
			if (hostList.StartsWith ("/")) 
			{
				CreateStream (hostList);
				return;
			} 

			//
			// Host name can contain multiple hosts, seperated by &.
			string [] dnsHosts = hostList.Split('&');
			Hashtable ips = new Hashtable();

			//
			// Each host name specified may contain multiple IP addresses
			// Lets look at the DNS entries for each host name
			foreach(string h in dnsHosts)
			{
				IPHostEntry hostAddress = Dns.GetHostByName(h);
				foreach (IPAddress addr in hostAddress.AddressList)
					ips.Add( addr, hostAddress.HostName );
			}
			IPAddress[] keys = new IPAddress[ ips.Count ];
			ips.Keys.CopyTo( keys, 0 );

			if ((Environment.TickCount - startTime) > toTicks)
			{
				TimeOut(MultiHostStreamErrorType.Connecting);
				return;
			}

			// make sure they gave us at least one host
			if (ips.Count == 0)
			{
				Error("You must specify at least one host");
				return;
			}

			int index = 0;
			// now choose a random server if there are more than one
			if (ips.Count > 1) 
			{
				System.Random random = new Random((int)DateTime.Now.Ticks);
				index = random.Next(ips.Count-1);
			}

			//
			// Lets step through our hosts until we get a connection
			for (int i=0; i < ips.Count; i++)
			{
				if ((Environment.TickCount - startTime) > toTicks) 
				{
					TimeOut(MultiHostStreamErrorType.Connecting);
					return;
				}
				if (CreateStream( (IPAddress)keys[i], (string)ips[keys[i]], port ))
					return;
			}
		}

		public override int ReadByte()
		{
			int start = Environment.TickCount;
			int ticks = readTimeOut * 1000;

			while ((Environment.TickCount - start) < ticks)
			{
				if (DataAvailable)
				{
					int b = stream.ReadByte();
					return b;
				}
				else
					Thread.Sleep(0);
			}

			TimeOut(MultiHostStreamErrorType.Reading);
			return -1;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int  numToRead = count;
			int start = Environment.TickCount;
			int ticks = readTimeOut * 1000;

			try 
			{
				while (numToRead > 0 && (Environment.TickCount - start) < ticks)
				{
					if (DataAvailable) 
					{
						int bytes_read = stream.Read( buffer, offset, numToRead);
						if (bytes_read == 0)
							return (count - numToRead);
						offset += bytes_read;
						numToRead -= bytes_read;
					}
					else
						Thread.Sleep(0);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			if (numToRead > 0)
				TimeOut(MultiHostStreamErrorType.Reading);
			return count;
		}

		public override bool CanRead
		{
			get { return stream.CanRead; }
		}

		public override bool CanWrite
		{
			get { return stream.CanWrite; }
		}

		public override bool CanSeek
		{
			get { return stream.CanSeek; }
		}

		public override long Length
		{
			get { return stream.Length; }
		}

		public override long Position 
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override void Flush() 
		{
			stream.Flush();
		}

		public override void SetLength(long length)
		{
			stream.SetLength( length );
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write( buffer, offset, count );
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			return stream.Seek( offset, origin );
		}

	}
}
