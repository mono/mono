//
// Microsoft.Web.Services.Referral.Desc.cs
//
// Name: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Globalization;

namespace Microsoft.Web.Services.Referral {
	
	public class Desc
	{
		Uri uri;

		public Desc ()
		{
		}
		
		public Uri RefAddr {
			get {
				return uri;
			}
			set {
				if (value.AbsoluteUri != value.ToString ())
					throw new ArgumentException (
						Locale.GetText ("uri is not absolute"));
				uri = value;
			}
		}
	}
}
