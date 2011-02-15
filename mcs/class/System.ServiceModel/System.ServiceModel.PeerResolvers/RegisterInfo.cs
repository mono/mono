// 
// RegisterInfo.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// Copyright (C) 2007 Novell, Inc. http://novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.Serialization;

namespace System.ServiceModel.PeerResolvers
{
	[MessageContract (IsWrapped = false)]
	public class RegisterInfo
	{
		[MessageBodyMember (Name = "Register", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		RegisterInfoDC Body {
			get {
				if (body == null)
					body = new RegisterInfoDC ();
				return body;
			}
			set { body = value; }
		}
		RegisterInfoDC body;
		
		public RegisterInfo ()
		{
		}
		
		public RegisterInfo (Guid client, string meshId, PeerNodeAddress address)
			: this ()
		{
			Body.ClientId = client;
			Body.MeshId = meshId;
			Body.NodeAddress = address;
		}
		
		public Guid ClientId {
			get { return Body.ClientId; }
		}
		
		public string MeshId {
			get { return Body.MeshId; }
		}
		
		public PeerNodeAddress NodeAddress {
			get { return Body.NodeAddress; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class RegisterInfoDC
	{
		Guid client_id;
		string mesh_id;
		PeerNodeAddress node_address;

		public RegisterInfoDC ()
		{
		}
		
		[DataMember]
		public Guid ClientId {
			get { return client_id; }
			set { client_id = value; }
		}
		
		[DataMember]
		public string MeshId {
			get { return mesh_id; }
			set { mesh_id = value; }
		}
		
		[DataMember]
		public PeerNodeAddress NodeAddress {
			get { return node_address; }
			set { node_address = value; }
		}
	}
}
