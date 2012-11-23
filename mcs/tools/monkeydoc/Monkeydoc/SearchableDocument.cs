//
//
// SearchableDocument.cs: Abstracts our model of document from the Lucene Document 
//
// Author: Mario Sopena
//
using Mono.Lucene.Net.Documents;

namespace MonkeyDoc
{
	struct SearchableDocument
	{
		public string title;
		public string url;
		public string fulltitle;
		public string hottext;
		public string text;
		public string examples;

		public Document LuceneDoc {
			get {
				Document doc = new Document ();
				doc.Add (UnIndexed ("title", title));
				doc.Add (UnIndexed ("url", url));
				doc.Add (UnIndexed ("fulltitle", fulltitle ?? string.Empty));
				doc.Add (UnStored ("hottext", hottext));
				doc.Add (UnStored ("text", text));
				doc.Add (UnStored ("examples", examples));
				return doc;
			}
		}

		static Field UnIndexed(System.String name, System.String value_Renamed)
		{
			return new Field(name, value_Renamed, Field.Store.YES, Field.Index.NO);
		}

		static Field UnStored(System.String name, System.String value_Renamed)
		{
			return new Field(name, value_Renamed, Field.Store.NO, Field.Index.ANALYZED);
		}
	}
}
