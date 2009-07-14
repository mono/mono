// 
// ResolveInfo.cs
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
	public class ResolveInfo
	{
		[MessageBodyMember (Name = "Resolve", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		ResolveInfoDC body;
		
		public ResolveInfo ()
		{
			body = new ResolveInfoDC ();
		}
		
		public ResolveInfo (Guid clientId, string meshId, int maxAddresses)
			: this ()
		{
			if (clientId == Guid.Empty)
				throw new ArgumentException ("Empty Guid");
			if (String.IsNullOrEmpty (meshId))
				throw new ArgumentNullException ("meshId");
			if (maxAddresses <= 0)
				throw new ArgumentOutOfRangeException ("maxAddresses must be positive integer");
			body.ClientId = clientId;
			body.MeshId = meshId;
			body.MaxAddresses = maxAddresses;
		}
		
		public Guid ClientId {
			get { return body.ClientId; }
		}
		public int MaxAddresses {
			get { return body.MaxAddresses; }
		}
		public string MeshId {
			get { return body.MeshId; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "Resolve", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class ResolveInfoDC
	{
		Guid client_id;
		int max_addresses;
		string mesh_id;

		public ResolveInfoDC ()
		{
		}
		
		[DataMember]
		public Guid ClientId {
			get { return client_id; }
			set { client_id = value; }
		}
		
		[DataMember]
		public int MaxAddresses {
			get { return max_addresses; }
			set { max_addresses = value; }
		}
		
		[DataMember]
		public string MeshId {
			get { return mesh_id; }
			set { mesh_id = value; }
		}
	}
}
