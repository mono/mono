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
		[MessageBodyMember (Name = "Update", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")] // .NET indeed returns "Update" element here.
		RegisterResponseInfoDC Body {
			get {
				if (body == null)
					body = new RegisterResponseInfoDC ();
				return body;
			}
			set { body = value; }
		}
		RegisterResponseInfoDC body;
		
		public RegisterResponseInfo ()
		{
		}
		
		public RegisterResponseInfo (Guid registrationId, TimeSpan registrationLifetime)
		{
			Body.RegistrationId = registrationId;
			Body.RegistrationLifetime = registrationLifetime;
		}
		
		public Guid RegistrationId {
			get { return Body.RegistrationId; }
			set { Body.RegistrationId = value; }
		}
		
		public TimeSpan RegistrationLifetime {
			get { return Body.RegistrationLifetime; }
			set { Body.RegistrationLifetime = value; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "Update", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
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
		
		[DataMember (EmitDefaultValue = false)]
		public TimeSpan RegistrationLifetime {
			get { return registration_lifetime; }
			set { registration_lifetime = value; }
		}
	}
}
