//
// Microsoft.Web.Services.Referral.For.cs
//
// Name: Daniel Kornhauser <dkor@alum.mit.edu>
//
// Copyright (C) Ximian, Inc. 2003
//

using System;
using System.Globalization;

namespace Microsoft.Web.Services.Referral {
	
	public class For
	{
		Uri exact;
		Uri prefix;

		public For ()
		{
		}
		
		[MonoTODO]
		public Uri Exact {
			get {
				return exact;
			}
			set {

				exact = value;
			}
		}

		[MonoTODO]
		public Uri Prefix {
			get {
				return prefix;
			}

			set {
				prefix = value;
			}
		}
	}
}
