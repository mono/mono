using System;
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NUnit.Core;

namespace NUnit.Util
{
	/// <summary>
	/// RemoteTestAgent represents a remote agent executing in another process
	/// and communicating with NUnit by TCP. Although it is similar to a
	/// TestServer, it does not publish a Uri at which clients may connect 
	/// to it. Rather, it reports back to the sponsoring TestAgency upon 
	/// startup so that the agency may in turn provide it to clients for use.
	/// </summary>
	public class RemoteTestAgent : MarshalByRefObject, IDisposable
	{
		#region Fields
		/// <summary>
		/// Url of the agency that controls this agent
		/// </summary>
		string agencyUrl;

		/// <summary>
		/// Reference to the TestAgency that controls this agent
		/// </summary>
		TestAgency agency;

		/// <summary>
		/// Channel used for communications with the agency
		/// and with clients
		/// </summary>
		private TcpChannel channel;

		/// <summary>
		/// Lock used to avoid thread contention
		/// </summary>
		private object theLock = new object();

		#endregion

		#region Constructor
		/// <summary>
		/// Construct a RemoteTestAgent given the Url of its agency
		/// </summary>
		/// <param name="agencyUrl"></param>
		public RemoteTestAgent( string agencyUrl )
		{
			this.agencyUrl = agencyUrl;
		}
		#endregion

		#region Properties
		public TestAgency Agency
		{
			get { return agency; }
		}

		public int ProcessId
		{
			get { return System.Diagnostics.Process.GetCurrentProcess().Id; }
		}
		#endregion

		#region Public Methods - Called By local Agent
		public TestRunner CreateRunner(int runnerID)
		{
			return new TestDomain( runnerID );
		}
		#endregion

		#region Public Methods
		public void Start()
		{
			NTrace.Info("Starting");
			this.channel = ServerUtilities.GetTcpChannel();
			NTrace.Debug("Acquired Tcp Channel");

			try
			{
				this.agency = (TestAgency)Activator.GetObject( typeof( TestAgency ), agencyUrl );
				NTrace.DebugFormat("Connected to TestAgency at {0}", agencyUrl);
			}
			catch( Exception ex )
			{
				NTrace.ErrorFormat( "Unable to connect to test agency at {0}", agencyUrl );
				NTrace.Error( ex.Message );
			}

			try
			{
				this.agency.Register( this, ProcessId );
				NTrace.Debug( "Registered with TestAgency" );
			}
			catch( Exception ex )
			{
				NTrace.Error( "Failed to register with TestAgency", ex );
			}
		}

		[System.Runtime.Remoting.Messaging.OneWay]
		public void Stop()
		{
			NTrace.Info( "Stopping" );
			lock( theLock )
			{
				if ( this.channel != null )
					ChannelServices.UnregisterChannel( this.channel );
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
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			this.Stop();
		}

		#endregion
	}
}
