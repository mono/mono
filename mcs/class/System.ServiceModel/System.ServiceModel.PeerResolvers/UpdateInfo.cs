// 
// UpdateInfo.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System.Runtime.Serialization;

namespace System.ServiceModel.PeerResolvers
{
	[MessageContract (IsWrapped = false)]
	public class UpdateInfo
	{
		[MessageBodyMember (Name = "Update", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		UpdateInfoDC Body {
			get {
				if (body == null)
					body = new UpdateInfoDC ();
				return body;
			}
			set { body = value; }
		}
		UpdateInfoDC body;
		
		public UpdateInfo ()
		{
		}
		
		public UpdateInfo (Guid registrationId, Guid client, string meshId, PeerNodeAddress address)
			: this ()
		{
			Body.RegistrationId = registrationId;
			Body.ClientId = client;
			Body.MeshId = meshId;
			Body.NodeAddress = address;
		}
		
		public Guid ClientId {
			get { return Body.ClientId; }
		}
		
		public string MeshId {
			get { return Body.MeshId; }
		}
		
		public PeerNodeAddress NodeAddress {
			get { return Body.NodeAddress; }
		}
		
		public Guid RegistrationId {
			get { return Body.RegistrationId; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "Update", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class UpdateInfoDC
	{
		Guid client_id;
		string mesh_id;
		PeerNodeAddress node_address;
		Guid registration_id;

		public UpdateInfoDC ()
		{
		}
		
		[DataMember]
		public Guid ClientId {
			get { return client_id; }
			set { client_id = value; }
		}
		
		[DataMember]
		public string MeshId {
			get { return mesh_id; }
			set { mesh_id = value; }
		}
		
		[DataMember]
		public PeerNodeAddress NodeAddress {
			get { return node_address; }
			set { node_address = value; }
		}
		
		[DataMember]
		public Guid RegistrationId {
			get { return registration_id; }
			set { registration_id = value; }
		}
	}
}
