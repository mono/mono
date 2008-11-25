using System;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Services;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// Summary description for ServerBase.
	/// </summary>
	public abstract class ServerBase : MarshalByRefObject, IDisposable
	{
		protected string uri;
		protected int port;

		private TcpChannel channel;
		private bool isMarshalled;

		private object theLock = new object();

		protected ServerBase()
		{
		}

		/// <summary>
		/// Constructor used to provide
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="port"></param>
		protected ServerBase(string uri, int port)
		{
			this.uri = uri;
			this.port = port;
		}

		public virtual void Start()
		{
			if ( uri != null && uri != string.Empty )
				lock( theLock )
				{
					this.channel = ServerUtilities.GetTcpChannel( uri + "Channel", port, 100 );

					RemotingServices.Marshal( this, uri );
					this.isMarshalled = true;
				}
		}

		[System.Runtime.Remoting.Messaging.OneWay]
		public virtual void Stop()
		{
			lock( theLock )
			{
				if ( this.isMarshalled )
				{
					RemotingServices.Disconnect( this );
					this.isMarshalled = false;
				}

				if ( this.channel != null )
				{
					ChannelServices.UnregisterChannel( this.channel );
					this.channel = null;
				}

				Monitor.PulseAll( theLock );
			}
		}

		public void WaitForStop()
		{
			lock( theLock )
			{
				Monitor.Wait( theLock );
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			this.Stop();
		}

		#endregion
	}
}
