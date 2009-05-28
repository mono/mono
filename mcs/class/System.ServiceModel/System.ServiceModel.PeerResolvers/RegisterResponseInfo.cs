// 
// RegisterResponseInfo.cs
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
	public class RegisterResponseInfo
	{
		[MessageBodyMember (Name = "RegisterResponse", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		RegisterResponseInfoDC body;
		
		public RegisterResponseInfo ()
		{
			body = new RegisterResponseInfoDC ();
		}
		
		public RegisterResponseInfo (Guid registrationId, TimeSpan registrationLifetime)
		{
			body.RegistrationId = registrationId;
			body.RegistrationLifetime = registrationLifetime;
		}
		
		public Guid RegistrationId {
			get { return body.RegistrationId; }
			set { body.RegistrationId = value; }
		}
		
		public TimeSpan RegistrationLifetime {
			get { return body.RegistrationLifetime; }
			set { body.RegistrationLifetime = value; }
		}
		
		[MonoTODO]
		public bool HasBody ()
		{
			throw new NotImplementedException ();
		}
	}
	
	[DataContract (Name = "RegisterResponse", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class RegisterResponseInfoDC
	{
		Guid registration_id;
		TimeSpan registration_lifetime;

		public RegisterResponseInfoDC ()
		{
		}
		
		[DataMember]
		public Guid RegistrationId {
			get { return registration_id; }
			set { registration_id = value; }
		}
		
		[DataMember]
		public TimeSpan RegistrationLifetime {
			get { return registration_lifetime; }
			set { registration_lifetime = value; }
		}
	}
}
