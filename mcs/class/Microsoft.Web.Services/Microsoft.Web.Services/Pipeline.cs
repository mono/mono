//
// Pipeline.cs: Soap Filter Pipeline
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using Microsoft.Web.Services.Configuration;

namespace Microsoft.Web.Services {

	// Reference:
	// 1.	Inside the Web Services Enhancements Pipeline
	//	http://msdn.microsoft.com/library/en-us/dnwebsrv/html/insidewsepipe.asp

	public class Pipeline {

		private SoapInputFilterCollection input;
		private SoapOutputFilterCollection output;

		public Pipeline() 
		{
			// set to defaults
			input = (SoapInputFilterCollection) WebServicesConfiguration.FilterConfiguration.InputFilters.Clone ();
			output = (SoapOutputFilterCollection) WebServicesConfiguration.FilterConfiguration.OutputFilters.Clone ();
		}

		public Pipeline (Pipeline pipeline) 
		{
			if (pipeline == null)
				throw new ArgumentNullException ("pipeline");
			input = (SoapInputFilterCollection) pipeline.InputFilters.Clone ();
			output = (SoapOutputFilterCollection) pipeline.OutputFilters.Clone ();
		}

		public Pipeline (SoapInputFilterCollection inputFilters, SoapOutputFilterCollection outputFilters) 
		{
			if (inputFilters == null)
				throw new ArgumentNullException ("inputFilters");
			if (outputFilters == null)
				throw new ArgumentNullException ("outputFilters");
			input = (SoapInputFilterCollection) inputFilters.Clone ();
			output = (SoapOutputFilterCollection) outputFilters.Clone ();
		}

		public SoapInputFilterCollection InputFilters {
			get { return input; }
		}

		public SoapOutputFilterCollection OutputFilters { 
			get { return output; }
		}

		public void ProcessInputMessage (SoapEnvelope envelope) 
		{
			// in normal order
			for (int x=0; x < input.Count; x++)
				input [x].ProcessMessage (envelope);
		}

		public void ProcessOutputMessage (SoapEnvelope envelope) 
		{
			// in reverse order - see reference [1]
			for (int x=output.Count - 1; x >= 0; x--)
				output [x].ProcessMessage (envelope);
		}
	}
}
