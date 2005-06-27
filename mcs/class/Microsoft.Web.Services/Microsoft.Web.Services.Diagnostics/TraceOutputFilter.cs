//
// TraceOutputFilter.cs: Trace Output Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

using Microsoft.Web.Services.Configuration;

namespace Microsoft.Web.Services.Diagnostics {

	public class TraceOutputFilter : SoapOutputFilter {

		private const string tracename = "OutputTrace.webinfo";
		private string file;

		public TraceOutputFilter () 
		{
			string filename = WebServicesConfiguration.Config.TraceOutput;
			if (filename == null)
				filename = tracename;
			file = TraceFilter.GetCompleteFilename (filename);
		}

		public override void ProcessMessage (SoapEnvelope envelope) 
		{
			TraceFilter.WriteEnvelope (file, envelope);
		}
	}
}
