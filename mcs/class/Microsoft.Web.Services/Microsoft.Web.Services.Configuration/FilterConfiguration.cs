//
// FilterConfiguration.cs: Filter Configuration
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services.Configuration {

	public sealed class FilterConfiguration : ConfigurationBase {

		[MonoTODO()]
		public SoapInputFilterCollection InputFilters { 
			get { return null; }
		}

		[MonoTODO()]
		public SoapOutputFilterCollection OutputFilters { 
			get { return null; }
		}
	}
}