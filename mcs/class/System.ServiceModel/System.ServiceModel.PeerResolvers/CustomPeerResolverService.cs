// 
// CustomPeerResolverService.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System.Collections.Generic;
using System.Transactions;
using System.Timers;

namespace System.ServiceModel.PeerResolvers
{
	[MonoTODO ("Implement cleanup and refresh")]
	// FIXME: TransactionTimeout must be null by-default.
	[ServiceBehavior (AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, 
	                  InstanceContextMode = InstanceContextMode.Single, ReleaseServiceInstanceOnTransactionComplete = true, 
	                  TransactionIsolationLevel = IsolationLevel.Unspecified, /*TransactionTimeout = null, */
	                  UseSynchronizationContext = false, ValidateMustUnderstand = true)]
	public class CustomPeerResolverService : IPeerResolverContract
	{
		bool control_shape;
		bool opened;
		// Maybe it's worth to change List<T> for a better distributed and faster collection.
		List<Node> mesh = new List<Node> ();
		object mesh_lock = new object ();
		Timer refresh_timer, cleanup_timer;

		public CustomPeerResolverService ()
		{
			refresh_timer = new Timer () { AutoReset = true };
			RefreshInterval = new TimeSpan (0, 10, 0);
			refresh_timer.Elapsed += delegate {
					// FIXME: implement
				};
			cleanup_timer = new Timer () { AutoReset = true };
			CleanupInterval = new TimeSpan (0, 1, 0);
			cleanup_timer.Elapsed += delegate {
					// FIXME: implement
				};
			control_shape = false;
			opened = false;
		}

		public TimeSpan CleanupInterval {
			get { return TimeSpan.FromMilliseconds ((int) cleanup_timer.Interval); }
			set {
				if ((value < TimeSpan.Zero) || (value > TimeSpan.MaxValue))
					throw new ArgumentOutOfRangeException (
					"The interval is either zero or greater than max value.");
				if (opened)
					throw new InvalidOperationException ("The interval must be set before it is opened");

				cleanup_timer.Interval = value.TotalMilliseconds;
			}
		}

		public bool ControlShape {
			get { return control_shape; }
			set { control_shape = value; }
		}

		public TimeSpan RefreshInterval {
			get { return TimeSpan.FromMilliseconds ((int) refresh_timer.Interval); }
			set {
				if ((value < TimeSpan.Zero) || (value > TimeSpan.MaxValue))
					throw new ArgumentOutOfRangeException (
					"The interval is either zero or greater than max value.");
				if (opened)
					throw new InvalidOperationException ("The interval must be set before it is opened");

				refresh_timer.Interval = value.TotalMilliseconds;
			}
		}

		[MonoTODO ("Do we have to unregister nodes here?")]
		public virtual void Close ()
		{
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed by a previous call to this method.");
			refresh_timer.Stop ();
			cleanup_timer.Stop ();
		}

		[MonoTODO]
		public virtual ServiceSettingsResponseInfo GetServiceSettings ()
		{
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
		
//			return new ServiceSettingsResponseInfo ();
			throw new NotImplementedException ();
		}

		public virtual void Open ()
		{
			if ((CleanupInterval == TimeSpan.Zero) || (RefreshInterval == TimeSpan.Zero))
				throw new ArgumentException ("Cleanup interval or refresh interval are set to a time span interval of zero.");

			if (opened)
				throw new InvalidOperationException ("The service has been started by a previous call to this method.");
			
			opened = true;

			refresh_timer.Start ();
			cleanup_timer.Start ();
		}

		[MonoTODO]
		public virtual RefreshResponseInfo Refresh (RefreshInfo refreshInfo)
		{
			if (refreshInfo == null)
				throw new ArgumentException ("Refresh info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
			
//			return new RefreshResponseInfo ();
			throw new NotImplementedException ();
		}

		public virtual RegisterResponseInfo Register (RegisterInfo registerInfo)
		{
			if (registerInfo == null)
				throw new ArgumentException ("Register info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
			
			return Register (registerInfo.ClientId, registerInfo.MeshId, registerInfo.NodeAddress);
		}

		public virtual RegisterResponseInfo Register (Guid clientId, string meshId, PeerNodeAddress address)
		{
			Node n = new Node () { RegistrationId = Guid.NewGuid (), MeshId = meshId, ClientId = clientId, NodeAddress = address };
			RegisterResponseInfo rri = new RegisterResponseInfo ();
			rri.RegistrationId = n.RegistrationId;
			lock (mesh_lock)
				mesh.Add (n);
			
			return rri;
		}

		public virtual ResolveResponseInfo Resolve (ResolveInfo resolveInfo)
		{
			ResolveResponseInfo rri = new ResolveResponseInfo ();
			if (resolveInfo == null)
				throw new ArgumentException ("Resolve info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
			
			foreach (var node in mesh)
				if (node.MeshId == resolveInfo.MeshId &&
				    node.ClientId == resolveInfo.ClientId)
					rri.Addresses.Add (node.NodeAddress);
			
			return rri;
		}

		public virtual void Unregister (UnregisterInfo unregisterInfo)
		{
			if (unregisterInfo == null)
				throw new ArgumentException ("Unregister info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
			
			lock (mesh_lock)
				foreach (var node in mesh)
					if (node.MeshId == unregisterInfo.MeshId &&
					    node.RegistrationId == unregisterInfo.RegistrationId) {
						mesh.Remove (node);
						break;
					}
		}

		[MonoTODO]
		public virtual RegisterResponseInfo Update (UpdateInfo updateInfo)
		{
			if (updateInfo == null)
				throw new ArgumentException ("Update info cannot be null.");
			
			if (! opened)
				throw new InvalidOperationException ("The service has never been opened or it was closed previously.");
			
//			return new RegisterResponseInfo ();
			throw new NotImplementedException ();
		}
	}
	
	internal class Node
	{
		public Guid ClientId { get; set; }
		public string MeshId { get; set; }
		public Guid RegistrationId { get; set; }
		public PeerNodeAddress NodeAddress { get; set; }
	}
}
