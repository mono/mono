// 
// LocalPeerResolverService.cs
// 
// Author: 
//	Atsushi Enomoto  <atsushi@ximian.com>
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
#if true
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;

namespace System.ServiceModel.PeerResolvers
{
	// This implementation of peer resolver should open up node
	// registration to some extent, say, valid within the same machine.
	//
	// A correct implementation should be using something like zeroconf
	// to register the local machine as a peer node.

	class LocalPeerResolverService : ICustomPeerResolverContract
	{
		public LocalPeerResolverService (TextWriter log)
		{
			this.log = log ?? TextWriter.Null;
		}

		TextWriter log;
		Dictionary<string,Mesh> mesh_map = new Dictionary<string,Mesh> ();

		// CustomPeerResolverService delegation

		// Open(), Close(), ControlShape, RefreshInterval, CleanupInterval

		public bool ControlShape { get; set; }
		public TimeSpan RefreshInterval { get; set; }
		public TimeSpan CleanupInterval { get; set; }

		// (internal) ICustomPeerResolverContract implementation

		public PeerServiceSettingsInfo GetCustomServiceSettings ()
		{
			log.WriteLine ("REQUEST: GetCustomServiceSettings");
			return new PeerServiceSettingsInfo () {
				ControlMeshShape = this.ControlShape,
				RefreshInterval = this.RefreshInterval, 
				CleanupInterval = this.CleanupInterval };
		}

		public void SetCustomServiceSettings (PeerServiceSettingsInfo info)
		{
			log.WriteLine ("REQUEST: SetCustomServiceSettings(ControlMeshShape:{0}, RefreshInterval:{1}, CleanupInterval:{2}", info.ControlMeshShape, info.RefreshInterval, info.CleanupInterval);
			ControlShape = info.ControlMeshShape;
			RefreshInterval = info.RefreshInterval;
			CleanupInterval = info.CleanupInterval;
		}

		// IPeerResolverContract implementation

		public ServiceSettingsResponseInfo GetServiceSettings ()
		{
			return new ServiceSettingsResponseInfo () { ControlMeshShape = this.ControlShape };
		}

		public RefreshResponseInfo Refresh (RefreshInfo refreshInfo)
		{
			var r = refreshInfo;
			log.WriteLine ("REQUEST: Refresh (Mesh: {0}, Registraion: {1})", r.MeshId, r.RegistrationId);
			var mesh = GetExistingMesh (r.MeshId);
			var node = mesh.FirstOrDefault (n => n.RegistrationId == r.RegistrationId);
			if (node == null)
				return new RefreshResponseInfo () { Result = RefreshResult.RegistrationNotFound };
			node.Refresh ();
			return new RefreshResponseInfo () { Result = RefreshResult.Success, RegistrationLifetime = RefreshInterval - (DateTime.UtcNow - node.LastRefreshTime) };
		}

		public RegisterResponseInfo Register (RegisterInfo registerInfo)
		{
			var r = registerInfo;
			log.WriteLine ("REQUEST: Register (Mesh: {0}, Client: {1}, NodeAddress: endpoint {2})", r.MeshId, r.ClientId, r.NodeAddress.EndpointAddress);
			Mesh mesh;
			if (!mesh_map.TryGetValue (r.MeshId, out mesh)) {
				mesh = new Mesh (r.MeshId);
				mesh_map.Add (r.MeshId, mesh);
			}
			var node = RegisterNode (mesh, r.ClientId, r.NodeAddress);
			return new RegisterResponseInfo () { RegistrationId = node.RegistrationId };
		}

		public ResolveResponseInfo Resolve (ResolveInfo resolveInfo)
		{
			var r = resolveInfo;
			log.WriteLine ("REQUEST: Resolve (Mesh: {0}, Client: {1}, MaxAddresses: {2})", r.MeshId, r.ClientId, r.MaxAddresses);
			Mesh mesh;
			var rr = new ResolveResponseInfo ();
			if (!mesh_map.TryGetValue (r.MeshId, out mesh))
				return rr;
			// FIXME: find out how to use the argument ClientId.
			// So far, it is used to filter out the registered node from the same ClientId.
			foreach (var node in mesh.TakeWhile (n => n.ClientId != r.ClientId)) {
				rr.Addresses.Add (node.Address);
				if (rr.Addresses.Count == r.MaxAddresses)
					break;
			}
			return rr;
		}

		public void Unregister (UnregisterInfo unregisterInfo)
		{
			var u = unregisterInfo;
			log.WriteLine ("REQUEST: Unregister (Mesh: {0}, Registration: {1})", u.MeshId, u.RegistrationId);
			Mesh mesh = GetExistingMesh (u.MeshId);
			lock (mesh) {
				var node = mesh.GetRegisteredNode (u.RegistrationId);
				mesh.Remove (node);
			}
		}

		public RegisterResponseInfo Update (UpdateInfo updateInfo)
		{
			var u = updateInfo;
			log.WriteLine ("REQUEST: Update (Mesh: {0}, Registration: {1}, NodeAddress:)", u.MeshId, u.RegistrationId, u.NodeAddress);
			var mesh = GetExistingMesh (u.MeshId);
			var node = mesh.GetRegisteredNode (u.RegistrationId);
			node.Update (u.NodeAddress);
			return new RegisterResponseInfo () { RegistrationId = node.RegistrationId };
		}

		Mesh GetExistingMesh (string meshId)
		{
			Mesh mesh;
			if (!mesh_map.TryGetValue (meshId, out mesh))
				throw new InvalidOperationException (String.Format ("Specified mesh {0} does not exist", meshId));
			return mesh;
		}

		Node RegisterNode (Mesh mesh, Guid clientId, PeerNodeAddress addr)
		{
			lock (mesh) {
				var node = new Node () { ClientId = clientId, Address = addr };
				mesh.Add (node);
				node.LastRefreshTime = DateTime.UtcNow;
				return node;
			}
		}
	}

	class Mesh : List<Node>
	{
		public Mesh (string id)
		{
			Id = id;
		}

		public string Id { get; private set; }

		public Node GetRegisteredNode (Guid registrationId)
		{
			var node = this.FirstOrDefault (n => n.RegistrationId == registrationId);
			if (node == null)
				throw new InvalidOperationException (String.Format ("Node with registration Id {0} does not exist in the specified mesh {0}", registrationId, Id));
			return node;
		}
	}

	class Node
	{
		public Node ()
		{
			RegistrationId = Guid.NewGuid ();
		}

		public Guid RegistrationId { get; private set; }
		public Guid ClientId { get; set; }
		public PeerNodeAddress Address { get; set; }
		public DateTime LastRefreshTime { get; set; }

		public void Refresh ()
		{
			LastRefreshTime = DateTime.UtcNow;
		}

		public void Update (PeerNodeAddress addr)
		{
			Address = addr;
			LastRefreshTime = DateTime.UtcNow;
		}
	}
}
#endif
