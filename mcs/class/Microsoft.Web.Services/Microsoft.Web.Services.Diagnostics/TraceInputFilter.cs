//
// TraceInputFilter.cs: Trace Input Filter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Reflection;
using System.Xml;

using Microsoft.Web.Services.Configuration;

namespace Microsoft.Web.Services.Diagnostics {

	public class TraceInputFilter : SoapInputFilter {

		private const string tracename = "InputTrace.webinfo";
		private string file;

		public TraceInputFilter () 
		{
			string filename = WebServicesConfiguration.Config.TraceInput;
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
