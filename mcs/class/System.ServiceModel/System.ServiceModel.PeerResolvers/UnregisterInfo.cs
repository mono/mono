// 
// UnregisterInfo.cs
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
	public class UnregisterInfo
	{
		[MessageBodyMember (Name = "Unregister", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		UnregisterInfoDC body;
		
		public UnregisterInfo ()
		{
			body = new UnregisterInfoDC ();
		}
		
		public UnregisterInfo (string meshId, Guid registration_id)
			: this ()
		{
			body.MeshId = meshId;
			body.RegistrationId = registration_id;
		}
		
		public string MeshId {
			get { return body.MeshId; }
		}
		
		public Guid RegistrationId  {
			get { return body.RegistrationId; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "Unregister", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class UnregisterInfoDC
	{
		string mesh_id;
		Guid registration_id;

		public UnregisterInfoDC ()
		{
		}
		
		[DataMember]
		public string MeshId {
			get { return mesh_id; }
			set { mesh_id = value; }
		}
		
		[DataMember]
		public Guid RegistrationId  {
			get { return registration_id; }
			set { registration_id = value; }
		}
	}
}
