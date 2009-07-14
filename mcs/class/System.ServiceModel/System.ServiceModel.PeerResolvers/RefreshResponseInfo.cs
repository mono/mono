// 
// RefreshResponseInfo.cs
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
	public class RefreshResponseInfo
	{
		[MessageBodyMember (Name = "RefreshResponse", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		RefreshResponseInfoDC body;
		
		public RefreshResponseInfo ()
		{
			body = new RefreshResponseInfoDC ();
		}
		
		public RefreshResponseInfo (TimeSpan registrationLifetime, RefreshResult result)
			: this ()
		{
			body.RegistrationLifetime = registrationLifetime;
			body.Result = result;
		}
		
		public TimeSpan RegistrationLifetime {
			get { return body.RegistrationLifetime; }
			set { body.RegistrationLifetime = value; }
		}
		
		public RefreshResult Result {
			get { return body.Result; }
			set { body.Result = value; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "RefreshResponse", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class RefreshResponseInfoDC
	{
		TimeSpan registration_lifetime;
		RefreshResult result;

		public RefreshResponseInfoDC ()
		{
		}
		
		[DataMember]
		public TimeSpan RegistrationLifetime {
			get { return registration_lifetime; }
			set { registration_lifetime = value; }
		}
		
		[DataMember]
		public RefreshResult Result {
			get { return result; }
			set { result = value; }
		}
	}
}
