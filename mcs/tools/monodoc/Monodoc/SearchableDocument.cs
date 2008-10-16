//
//
// SearchableDocument.cs: Abstracts our model of document from the Lucene Document 
//
// Author: Mario Sopena
//
using Monodoc.Lucene.Net.Documents;

namespace Monodoc {
struct SearchableDocument {
	public string title;
	public string url;
	public string hottext;
	public string text;
	public string examples;

	public Document LuceneDoc {
		get {
			Document doc = new Document ();
			doc.Add (Field.UnIndexed ("title", title));
			doc.Add (Field.UnIndexed ("url", url));
			doc.Add (Field.UnStored ("hottext", hottext));
			doc.Add (Field.UnStored ("text", text));
			doc.Add (Field.UnStored ("examples", examples));
			return doc;
		}
	}
}
}
