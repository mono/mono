//
// ChainStream.cs: Chained stream to catch WebRequest's streams
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;

namespace Microsoft.Web.Services {

	internal class ChainStream : MemoryStream {

		private Stream original;
		private SoapEnvelope envelope;
		private Pipeline pipeline;

		public ChainStream (Stream s, SoapEnvelope env, Pipeline pipeline) : base () 
		{
			original = s;
			envelope = env;
			this.pipeline = pipeline;
		}

		public override void Close () 
		{
			try {
				// transfer MemoryStream into SoapEnvelope
				base.Position = 0;
				envelope.Load (this);
				// update the envelope then write into the original stream
				pipeline.ProcessOutputMessage (envelope);
				envelope.Save (original);
			}
			finally {
				base.Close ();
				original.Close ();
			}
		}
	}
}
