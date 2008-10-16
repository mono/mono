//
// index.cs: Handling of the index files
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
// Possible file format optimizations:
//   * Do not use 4 bytes for each index entry, use 3 bytes
//   * Find a way of compressing strings, there are plenty of duplicates
//     Find common roots, and use an encoding that uses a root to compress data.
//     "System", "System.Data", "System.Data class"
//     0: PLAIN: "System"
//     1: PLAIN: " class"
//     2: LINK0 PLAIN ".DATA"
//     3: LINK0 LINK1
//     
//     Maybe split everything at spaces and dots, and encode that:
//     string-1-idx "System."
//     string-1-idx "Data"
//     2-items [ string-1-idx string-2-idx]
//
//     Other variations are possible;  Like Archive "System", "System." when we
//     see "System.Data".
//
//
namespace Monodoc {
	
using System;
using System.IO;
using System.Text;
using System.Collections;

public class Topic  {
	public readonly string Caption;
	public readonly string SortKey;
	public readonly string Url;

	public Topic (string caption, string sort_key, string url)
	{
		Caption = caption;
		SortKey = sort_key;
		Url = url;
	}
}

public class IndexEntry {
	public int Position;
	public object topics;
	public int Count;
		
	public void Add (Topic t)
	{
		Count++;
		if (topics == null)
			topics = t;
		else {
			if (!(topics is ArrayList)){
				Topic temp = (Topic) topics;

				topics = new ArrayList ();
				((ArrayList)topics).Add (temp);
			}
			((ArrayList)topics).Add (t);
		}
	}

	public Topic this [int idx] {
		get {
			if (topics is Topic){
				if (idx == 0)
					return (Topic) topics;
				else
					throw new Exception ("Out of range index");
			} else {
				return (Topic) (((ArrayList)topics) [idx]);
			}
		}
	}

	//
	// Constructor from a stream
	//
	public IndexEntry (FileStream fs, BinaryReader reader, int position)
	{
		Count = reader.ReadInt32 ();
		int caption_offset = reader.ReadInt32 ();
		string caption;
		
		if (Count == 1){
			int url_offset = reader.ReadInt32 ();
			fs.Position = caption_offset;
			caption = reader.ReadString ();
			fs.Position = url_offset;
			string url = reader.ReadString ();
			topics = new Topic (caption, "", url);
		} else {
			ArrayList l = new ArrayList (Count);
			topics = l;
			int [] offsets = new int [Count];
			for (int i = 0; i < Count; i++){
				offsets [i] = reader.ReadInt32 ();
			}
			fs.Position = caption_offset;
			caption = reader.ReadString ();
			for (int i = 0; i < Count; i++){
				fs.Position = offsets [i];
				string url = reader.ReadString ();
				l.Add (new Topic (caption, "", url));
			}
		}
	}

// 	Topic ReadTopic (FileStream fs, BinaryReader reader, ref string caption)
// 	{
// 		int caption_offset = -1;
// 		if (caption == null)
// 			caption_offset = reader.ReadInt32 ();
// 		int url_offset = reader.ReadInt32 ();
//
// 		if (caption == null){
// 			fs.Position = caption_offset;
// 			caption = reader.ReadString ();
// 		}
// 		fs.Position = url_offset;
// 		string url = reader.ReadString ();
//
// 		return new Topic (caption, "", url);
// 	}
	
	//
	// Regular constructor
	
	public IndexEntry ()
	{
	}

	public void WriteTopics (IndexMaker maker, Stream stream, BinaryWriter writer)
	{
		//
		// Convention: entries with the same SortKey should have the same Caption
		//
		Position = (int) stream.Position;
		writer.Write (Count);

		if (topics is ArrayList){
			bool first = true;
			foreach (Topic t in (ArrayList) topics){
				if (first){
					writer.Write (maker.GetCode (t.Caption));
					first = false;
				}
				writer.Write (maker.GetCode (t.Url));
			}
		} else {
			Topic t = (Topic) topics;

			writer.Write (maker.GetCode (t.Caption));
			writer.Write (maker.GetCode (t.Url));
		}
	}
}

public class IndexMaker {
	Hashtable entries = new Hashtable ();
	Hashtable all_strings = new Hashtable ();

	void add_string (string s)
	{
		if (all_strings.Contains (s))
			return;
		all_strings [s] = 0;
	}
	
	public void AddTopic (Topic topic)
	{
		IndexEntry entry = (IndexEntry) entries [topic.SortKey];
		if (entry == null){
			entry = new IndexEntry ();
			entries [topic.SortKey] = entry;
		}

		add_string (topic.SortKey);
		add_string (topic.Caption);
		add_string (topic.Url);
		entry.Add (topic);
	}

	public void Add (string caption, string sort_key, string url)
	{
		Topic t = new Topic (caption, sort_key, url);
		AddTopic (t);
	}
	
	void SaveStringTable (Stream stream, BinaryWriter writer)
	{
		ICollection k = all_strings.Keys;
		string [] ks = new string [k.Count];
		k.CopyTo (ks, 0);
		
		foreach (string s in ks){
			int pos = (int) stream.Position;
			writer.Write (s);
			all_strings [s] = pos;
		}
	}

	public int GetCode (string s)
	{
		return (int) all_strings [s];
	}

	int index_position;
	
	void SaveTopics (Stream stream, BinaryWriter writer)
	{
		//
		// Convention: entries with the same SortKey should have the same Caption
		//
		foreach (IndexEntry e in entries.Values)
			e.WriteTopics (this, stream, writer);
	}

	void SaveIndexEntries (Stream stream, BinaryWriter writer)
	{
		index_position = (int) stream.Position;
		writer.Write (entries.Count);
		ICollection keys = entries.Keys;
		string [] keys_name = new string [keys.Count];
		keys.CopyTo (keys_name, 0);
		Array.Sort (keys_name, new NameSort ());
		
		foreach (string s in keys_name){
			IndexEntry e = (IndexEntry) entries [s];
			writer.Write (e.Position);
		}
	}

	class NameSort : IComparer {
		public int Compare (object a, object b)
		{
			string sa = (string) a;
			string sb = (string) b;

			return String.Compare (sa, sb, true);
		}
	}
	
	public void Save (string filename)
	{
		Encoding utf8 = new UTF8Encoding (false, true);

			using (FileStream fs = File.OpenWrite (filename)){
				BinaryWriter writer = 
						new BinaryWriter (fs, utf8);
				writer.Write (new byte [] { (byte) 'M', 
						(byte) 'o', (byte) 'i', 
						(byte) 'x'});

				// Leave room for pointer
				fs.Position = 8;

				SaveStringTable (fs, writer);
				SaveTopics (fs, writer);

				// index_position is set here
			
				SaveIndexEntries (fs, writer);

				fs.Position = 4;
				writer.Write (index_position);
			}
	}
}

public interface IListModel {
	int Rows {get; }
	string GetValue (int row);
	string GetDescription (int row);
}

public class IndexReader : IListModel {
	Encoding utf8 = new UTF8Encoding (false, true);
	FileStream fs;
	BinaryReader reader;

	// The offset of the table of entries
	int table_offset;
	int entries;

	static public IndexReader Load (string filename)
	{
		if (!File.Exists (filename))
			return null;

		try {
			return new IndexReader (filename);
		} catch {
			return null;
		}
	}
	
	IndexReader (string filename)
	{
		fs = File.OpenRead (filename);
		reader = new BinaryReader (fs, utf8);

		if (fs.ReadByte () != 'M' ||
		    fs.ReadByte () != 'o' ||
		    fs.ReadByte () != 'i' ||
		    fs.ReadByte () != 'x'){
			throw new Exception ("Corrupt index");
		}

		// Seek to index_entries
		fs.Position = reader.ReadInt32 ();
		
		entries = reader.ReadInt32 ();

		table_offset = (int) fs.Position;
	}

	public int Rows {
		get {
			return entries;
		}
	}

	public string GetValue (int row)
	{
		fs.Position = row * 4 + table_offset;
		fs.Position = reader.ReadInt32 () + 4;
		int code = reader.ReadInt32 ();
		fs.Position = code;
		string caption = reader.ReadString ();

		return caption;
	}

	public string GetDescription (int row)
	{
		return GetValue (row);
	}
	
	public IndexEntry GetIndexEntry (int row)
	{
		fs.Position = row * 4 + table_offset;
		int entry_offset = reader.ReadInt32 ();
		fs.Position = entry_offset;
		
		return new IndexEntry (fs, reader, entry_offset);
	}
}

} // Namespace
