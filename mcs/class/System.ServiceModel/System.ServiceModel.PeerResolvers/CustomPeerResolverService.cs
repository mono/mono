// 
// CustomPeerResolverService.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Transactions;
using System.Timers;

namespace System.ServiceModel.PeerResolvers
{
	[MonoTODO ("Implement cleanup and refresh")]
	[ServiceBehavior (ConcurrencyMode = ConcurrencyMode.Multiple, 
	                  InstanceContextMode = InstanceContextMode.Single,
	                  UseSynchronizationContext = false)]
	public class CustomPeerResolverService : IPeerResolverContract
	{
		static ServiceHost localhost;
		static int port;

		static void SetupCustomPeerResolverServiceHost ()
		{
			string customPort = Environment.GetEnvironmentVariable ("MONO_CUSTOMPEERRESOLVERSERVICE_PORT");
			if (customPort == null || !int.TryParse (customPort, out port))
				port = 8931;

			// launch peer resolver service locally only when it does not seem to be running ...
			var t = new TcpListener (port);
			try {
				t.Start ();
				t.Stop ();
			} catch {
				return;
			}
			Console.WriteLine ("WARNING: it is running peer resolver service locally. This means, the node registration is valid only within this application domain...");
			var host = new ServiceHost (new LocalPeerResolverService (TextWriter.Null));
			host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().InstanceContextMode = InstanceContextMode.Single;
			host.AddServiceEndpoint (typeof (ICustomPeerResolverContract), new BasicHttpBinding (), $"http://localhost:{port}");
			localhost = host;
			host.Open ();
		}

		ICustomPeerResolverClient client;
		bool control_shape, opened;
		TimeSpan refresh_interval, cleanup_interval;

		public CustomPeerResolverService ()
		{
			client = ChannelFactory<ICustomPeerResolverClient>.CreateChannel (new BasicHttpBinding (), new EndpointAddress ($"http://localhost:{port}"));

			refresh_interval = new TimeSpan (0, 10, 0);
			cleanup_interval = new TimeSpan (0, 1, 0);
		}

		public TimeSpan CleanupInterval {
			get { return cleanup_interval; }
			set { 
				if ((value < TimeSpan.Zero) || (value > TimeSpan.MaxValue))
					throw new ArgumentOutOfRangeException (
					"The interval is either zero or greater than max value.");
				if (opened)
					throw new InvalidOperationException ("The interval must be set before it is opened");

				cleanup_interval = value;
			}
		}

		public bool ControlShape {
			get { return control_shape; }
			set {
				if (opened)
					throw new InvalidOperationException ("The interval must be set before it is opened");
				control_shape = value;
			}
		}

		public TimeSpan RefreshInterval {
			get { return refresh_interval; }
			set {
				if ((value < TimeSpan.Zero) || (value > TimeSpan.MaxValue))
					throw new ArgumentOutOfRangeException (
					"The interval is either zero or greater than max value.");
				if (opened)
					throw new InvalidOperationException ("The interval must be set before it is opened");

				refresh_interval = value;
			}
		}

		[MonoTODO ("Do we have to unregister nodes here?")]
		public virtual void Close ()
		{
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed by a previous call to this method.");
			client.Close ();
			opened = false;

			if (localhost != null) {
				localhost.Close ();
				localhost = null;
			}
		}

		public virtual ServiceSettingsResponseInfo GetServiceSettings ()
		{
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
		
			return client.GetServiceSettings ();
		}

		public virtual void Open ()
		{
			if (localhost == null)
				SetupCustomPeerResolverServiceHost ();

			if ((CleanupInterval == TimeSpan.Zero) || (RefreshInterval == TimeSpan.Zero))
				throw new ArgumentException ("Cleanup interval or refresh interval are set to a time span interval of zero.");

			if (opened)
				throw new InvalidOperationException ("The service has been started by a previous call to this method.");
			
			opened = true;

			client.Open ();
			client.SetCustomServiceSettings (new PeerServiceSettingsInfo () { ControlMeshShape = control_shape, RefreshInterval = refresh_interval, CleanupInterval = cleanup_interval });
		}

		public virtual RefreshResponseInfo Refresh (RefreshInfo refreshInfo)
		{
			if (refreshInfo == null)
				throw new ArgumentException ("Refresh info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");

			return client.Refresh (refreshInfo);
		}

		public virtual RegisterResponseInfo Register (RegisterInfo registerInfo)
		{
			if (registerInfo == null)
				throw new ArgumentException ("Register info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
			
			return client.Register (registerInfo);
		}

		public virtual RegisterResponseInfo Register (Guid clientId, string meshId, PeerNodeAddress address)
		{
			return Register (new RegisterInfo (clientId, meshId, address));
		}

		public virtual ResolveResponseInfo Resolve (ResolveInfo resolveInfo)
		{
			if (resolveInfo == null)
				throw new ArgumentException ("Resolve info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");

			return client.Resolve (resolveInfo);
		}

		public virtual void Unregister (UnregisterInfo unregisterInfo)
		{
			if (unregisterInfo == null)
				throw new ArgumentException ("Unregister info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");

			client.Unregister (unregisterInfo);
		}

		public virtual RegisterResponseInfo Update (UpdateInfo updateInfo)
		{
			if (updateInfo == null)
				throw new ArgumentException ("Update info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");

			return client.Update (updateInfo);
		}
	}
}
