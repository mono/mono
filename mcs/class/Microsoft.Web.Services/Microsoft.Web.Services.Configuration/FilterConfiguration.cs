//
// FilterConfiguration.cs: Filter Configuration
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

using Microsoft.Web.Services;
using Microsoft.Web.Services.Configuration;
using Microsoft.Web.Services.Security;
using Microsoft.Web.Services.Timestamp;
using Microsoft.Web.Services.Referral;
using Microsoft.Web.Services.Routing;

namespace Microsoft.Web.Services.Configuration {

	public sealed class FilterConfiguration : ConfigurationBase {

		private SoapInputFilterCollection input;
		private SoapOutputFilterCollection output;

		[MonoTODO("see <filters> in WSE documentation")]
		internal FilterConfiguration () 
		{
			input = new SoapInputFilterCollection ();
			// the following 4 filters always seems present (notwithstanding config)
			input.Add (new SecurityInputFilter ());
			input.Add (new TimestampInputFilter ());
			input.Add (new ReferralInputFilter ());
			input.Add (new RoutingInputFilter ());
			// TODO: add custom input filters

			output = new SoapOutputFilterCollection ();
			// the following 4 filters always seems present (notwithstanding config)
			output.Add (new SecurityOutputFilter ());
			output.Add (new TimestampOutputFilter ());
			output.Add (new ReferralOutputFilter ());
			output.Add (new RoutingOutputFilter ());
			// TODO: add custom output filters
		}

		public SoapInputFilterCollection InputFilters { 
			get { return input; }
		}

		public SoapOutputFilterCollection OutputFilters { 
			get { return output; }
		}
	}
}