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
		UnregisterInfoDC Body {
			get {
				if (body == null)
					body = new UnregisterInfoDC ();
				return body;
			}
			set { body = value; }
		}
		UnregisterInfoDC body;
		
		public UnregisterInfo ()
		{
		}
		
		public UnregisterInfo (string meshId, Guid registration_id)
		{
			Body.MeshId = meshId;
			Body.RegistrationId = registration_id;
		}
		
		public string MeshId {
			get { return Body.MeshId; }
		}
		
		public Guid RegistrationId  {
			get { return Body.RegistrationId; }
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
