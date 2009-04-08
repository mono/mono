// 
// PeerTransportSecuritySettings.cs
// 
// Author: 
//	Atsushi Enomoto  <atsushi@ximian.com>
// 
// Copyright (C) 2009 Novell, Inc.
// 

using System;

namespace System.ServiceModel
{
	public sealed class PeerTransportSecuritySettings
	{
		internal PeerTransportSecuritySettings ()
		{
		}

		public PeerTransportCredentialType CredentialType { get; set; }
	}
}
