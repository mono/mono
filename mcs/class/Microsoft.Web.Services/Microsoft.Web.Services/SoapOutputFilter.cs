//
// SoapOutputFilter.cs: SOAP Output Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace Microsoft.Web.Services {

	public abstract class SoapOutputFilter {

		public SoapOutputFilter () {}

		public abstract void ProcessMessage (SoapEnvelope envelope);
	} 
}
