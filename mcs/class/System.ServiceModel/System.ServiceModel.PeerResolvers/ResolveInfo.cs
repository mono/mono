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
		ResolveInfoDC Body {
			get {
				if (body == null)
					body = new ResolveInfoDC ();
				return body;
			}
		}
		ResolveInfoDC body;
		
		public ResolveInfo ()
		{
		}
		
		public ResolveInfo (Guid clientId, string meshId, int maxAddresses)
		{
			if (clientId == Guid.Empty)
				throw new ArgumentException ("Empty Guid");
			if (String.IsNullOrEmpty (meshId))
				throw new ArgumentNullException ("meshId");
			if (maxAddresses <= 0)
				throw new ArgumentOutOfRangeException ("maxAddresses must be positive integer");
			Body.ClientId = clientId;
			Body.MeshId = meshId;
			Body.MaxAddresses = maxAddresses;
		}
		
		public Guid ClientId {
			get { return Body.ClientId; }
		}
		public int MaxAddresses {
			get { return Body.MaxAddresses; }
		}
		public string MeshId {
			get { return Body.MeshId; }
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
