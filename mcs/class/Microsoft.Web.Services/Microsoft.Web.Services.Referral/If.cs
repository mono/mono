//
// Microsoft.Web.Service.Referral.If.cs
//
// Name: Daniel Kornhauser <dkor@alum.media.mit.edu>
//
// Copyright (C) Ximian, Inc. 2003
//
		
using System;
using System.Globalization;

namespace Microsoft.Web.Services.Referral {

	public class If
	{

		UriList uris;
		long milliseconds;

		public If()
		{
		}

		public UriList Invalidates{
			get {
				return uris; 
			}
		}

		[MonoTODO]
		public long Ttl {

			get {
				return milliseconds;
			}

			set {
				milliseconds = value;

			}
		}
	}
}
