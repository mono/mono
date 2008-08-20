using System;
using System.IO;
using System.Xml;
using System.ServiceModel.Syndication;

class SyndicationSerializerUtil
{
	private	static int feedIndx = 0;
	
	public static void DumpFeed(SyndicationFeed f, string label)
	{
		string path = Path.Combine("Test", label);
		f.LastUpdatedTime = FeedLib.FixedChangedDate;

		using (XmlTextWriter writer = new XmlTextWriter(path + ".atom.xml", null))
			{
				Console.WriteLine("Writing " + path + ".atom.xml");
				writer.Formatting = Formatting.Indented;
				Atom10Serializer serializer = new Atom10Serializer();
				serializer.WriteTo(writer, f);
			}

		using (XmlTextWriter writer = new XmlTextWriter(path + ".rss.xml", null))
			{
				Console.WriteLine("Writing " + path + ".rss.xml");
				writer.Formatting = Formatting.Indented;
				Rss20Serializer serializer = new Rss20Serializer();
				serializer.WriteTo(writer, f);
			}
	}

	static int Main (string[] args)
	{
		DumpFeed(FeedLib.EmptyFeed, "EmptyFeed");
		DumpFeed(FeedLib.FeedNoItems, "FeedNoItems");
		DumpFeed(FeedLib.FeedWithItems, "FeedWithItems");
		DumpFeed(FeedLib.FeedNoItemsSimpleProps, "FeedNoItemsSimpleProps");
		return 0;
	}
}
