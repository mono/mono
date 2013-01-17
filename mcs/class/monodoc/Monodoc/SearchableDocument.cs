//
//
// SearchableDocument.cs: Abstracts our model of document from the Lucene Document 
//
// Author: Mario Sopena
//
using Lucene.Net.Documents;

namespace Monodoc
{
	struct SearchableDocument
	{
		public string Title {
			get; set;
		}

		public string Url {
			get; set;
		}

		public string FullTitle {
			get; set;
		}

		public string HotText {
			get; set;
		}

		public string Text {
			get; set;
		}

		public string Examples {
			get; set;
		}

		public SearchableDocument Reset ()
		{
			Title = Url = FullTitle = HotText = Text = Examples = null;
			return this;
		}

		public Document LuceneDoc {
			get {
				Document doc = new Document ();
				doc.Add (UnIndexed ("title", Title));
				doc.Add (UnIndexed ("url", Url));
				doc.Add (UnIndexed ("fulltitle", FullTitle ?? string.Empty));
				doc.Add (UnStored ("hottext", HotText));
				doc.Add (UnStored ("text", Text));
				doc.Add (UnStored ("examples", Examples));
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
