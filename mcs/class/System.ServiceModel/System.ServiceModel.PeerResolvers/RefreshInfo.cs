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
		RefreshInfoDC Body {
			get {
				if (body == null)
					body = new RefreshInfoDC ();
				return body;
			}
		}
		RefreshInfoDC body;
		
		public RefreshInfo ()
		{
		}
		
		public RefreshInfo (string meshId, Guid regId)
			: this ()
		{
			Body.MeshId = meshId;
			Body.RegistrationId = regId;
		}
		
		public string MeshId {
			get { return Body.MeshId; }
		}
		
		public Guid RegistrationId {
			get { return Body.RegistrationId; }
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
