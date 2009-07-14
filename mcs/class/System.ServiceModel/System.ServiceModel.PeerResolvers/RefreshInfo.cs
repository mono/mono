// 
// RefreshInfo.cs
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
	public class RefreshInfo
	{
		[MessageBodyMember (Name = "Refresh", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		RefreshInfoDC body;
		
		public RefreshInfo ()
		{
			body = new RefreshInfoDC ();
		}
		
		public RefreshInfo (string meshId, Guid regId)
			: this ()
		{
			body.MeshId = meshId;
			body.RegistrationId = regId;
		}
		
		public string MeshId {
			get { return body.MeshId; }
		}
		
		public Guid RegistrationId {
			get { return body.RegistrationId; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "Refresh", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class RefreshInfoDC
	{
		string mesh_id;
		Guid registration_id;

		public RefreshInfoDC ()
		{
		}
		
		[DataMember]
		public string MeshId {
			get { return mesh_id; }
			set { mesh_id = value; }
		}
		
		[DataMember]
		public Guid RegistrationId {
			get { return registration_id; }
			set { registration_id = value; }
		}
	}
}
