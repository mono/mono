using System;
using System.Collections.Generic;

namespace Monodoc
{
	// All type of documents that a generator may find as input
	public enum DocumentType {
		EcmaXml, // Our main monodoc format
		EcmaSpecXml,
		Man,
		AddinXml,
		MonoBook, // This is mostly XHTML already, just need a tiny bit of processing
		Html,
		TocXml, // Used by help source displaying some kind of toc of the content they host
		PlainText,
		ErrorXml
	}

	/* This interface defines a set of transformation engine
	 * that convert multiple documentation source to a single output format
	 */
	public interface IDocGenerator<TOutput>
	{
		/* This method is responsible for finding out the documentation type
		 * for the given ID and use the right engine internally
		 * The id can be accompanied by a context dictionary giving away extra
		 * informtion to the renderer
		 */
		TOutput Generate (HelpSource hs, string internalId, Dictionary<string, string> context);
	}
}
