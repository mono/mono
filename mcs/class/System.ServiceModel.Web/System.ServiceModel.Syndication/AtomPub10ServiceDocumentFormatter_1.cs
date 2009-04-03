using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Syndication
{
	public class AtomPub10ServiceDocumentFormatter<TServiceDocument> : AtomPub10ServiceDocumentFormatter
		where TServiceDocument : ServiceDocument, new()
	{
		public AtomPub10ServiceDocumentFormatter ()
		{
		}

		public AtomPub10ServiceDocumentFormatter (TServiceDocument documentToWrite)
			: base (documentToWrite)
		{
		}

		protected override ServiceDocument CreateDocumentInstance ()
		{
			return new TServiceDocument ();
		}
	}
}
