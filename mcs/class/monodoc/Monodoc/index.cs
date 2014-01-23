//
// index.cs: Handling of the index files
//
// Author:
//   Miguel de Icaza (miguel@xamarin.com)
//
// (C) 2003 Ximian, Inc.
// Copyright 2003-2011 Novell Inc
// Copyright 2011 Xamarin Inc.
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

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Monodoc
{
	public class Topic
	{
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

	public class IndexEntry
	{
		List<Topic> topics;

		public int Position {
			get;
			private set;
		}

		public IList<Topic> Topics {
			get {
				return topics.AsReadOnly ();
			}
		}

		public int Count {
			get;
			private set;
		}
		
		public void Add (Topic t)
		{
			Count++;
			topics.Add (t);
		}

		public Topic this [int idx] {
			get {
				if (idx < 0 || idx > topics.Count)
					throw new ArgumentOutOfRangeException ("idx");
				return topics[idx];
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
			topics = new List<Topic> (Count);

			int [] offsets = new int [Count];
			for (int i = 0; i < Count; i++)
				offsets [i] = reader.ReadInt32 ();

			fs.Position = caption_offset;
			caption = reader.ReadString ();
			for (int i = 0; i < Count; i++){
				fs.Position = offsets [i];
				string url = reader.ReadString ();
				topics.Add (new Topic (caption, string.Empty, url));
			}
		}

		//
		// Regular constructor
	
		public IndexEntry ()
		{
			topics = new List<Topic> ();
		}

		public void WriteTopics (IndexMaker maker, Stream stream, BinaryWriter writer)
		{
			//
			// Convention: entries with the same SortKey should have the same Caption
			//
			Position = (int) stream.Position;
			writer.Write (Count);

			if (Count == 0)
				return;

			writer.Write (maker.GetCode (topics[0].Caption));
			foreach (Topic t in topics)
				writer.Write (maker.GetCode (t.Url));
		}
	}

	public class IndexMaker
	{
		Dictionary<string, IndexEntry> entries = new Dictionary<string, IndexEntry> ();
		Dictionary<string, int> all_strings = new Dictionary<string, int> ();
		int index_position;

		void AddString (string str)
		{
			if (!all_strings.ContainsKey (str))
				all_strings.Add (str, 0);
		}

		public void AddTopic (Topic topic)
		{
			IndexEntry entry;
			if (!entries.TryGetValue (topic.SortKey, out entry)) {
				entry = new IndexEntry ();
				entries[topic.SortKey] = entry;
			}

			AddString (topic.SortKey);
			AddString (topic.Caption);
			AddString (topic.Url);
			entry.Add (topic);
		}

		public void Add (string caption, string sort_key, string url)
		{
			Topic t = new Topic (caption, sort_key, url);
			AddTopic (t);
		}
	
		void SaveStringTable (Stream stream, BinaryWriter writer)
		{
			var keys = new List<string> (all_strings.Keys);
			foreach (string s in keys) {
				int pos = (int) stream.Position;
				writer.Write (s);
				all_strings [s] = pos;
			}
		}

		public int GetCode (string s)
		{
			return all_strings [s];
		}

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
			var keys = new List<string> (entries.Keys);
			keys.Sort (StringComparer.OrdinalIgnoreCase);
		
			foreach (string s in keys){
				IndexEntry e = entries [s];
				writer.Write (e.Position);
			}
		}

		public void Save (string filename)
		{
			Encoding utf8 = new UTF8Encoding (false, true);

			using (FileStream fs = File.OpenWrite (filename)){
				BinaryWriter writer = new BinaryWriter (fs, utf8);
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

	public interface IListModel
	{
		int Rows { get; }
		string GetValue (int row);
		string GetDescription (int row);
	}

	public class IndexReader : IListModel
	{
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
}

