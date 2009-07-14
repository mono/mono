// 
// ResolveResponseInfo.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.ServiceModel.PeerResolvers
{
	[MessageContract (IsWrapped = false)]
	public class ResolveResponseInfo
	{
		[MessageBodyMember (Name = "ResolveResponse", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		ResolveResponseInfoDC body;
		
		public ResolveResponseInfo ()
		{
			body = new ResolveResponseInfoDC ();
		}
		
		public ResolveResponseInfo (PeerNodeAddress[] addresses)
		{
			body.Addresses = new List<PeerNodeAddress> (addresses);
		}
		
		public IList<PeerNodeAddress> Addresses {
			get { return body.Addresses; }
			set { body.Addresses = value; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "ResolveResponse", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class ResolveResponseInfoDC
	{
		IList<PeerNodeAddress> addresses;

		public ResolveResponseInfoDC ()
		{
			addresses = new List<PeerNodeAddress> ();
		}
		
		[DataMember]
		public IList<PeerNodeAddress> Addresses {
			get { return addresses; }
			set { addresses = value; }
		}
	}
}
