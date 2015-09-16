//
// DependencyGraph.cs: linker dependencies graph
//
// Author:
//   Radek Doulik (rodo@xamarin.com)
//
// Copyright 2015 Xamarin Inc (http://www.xamarin.com).
//
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace LinkerAnalyzer.Core
{
	public class VertexData {
		public string value;
		public List<int> parentIndexes;
		public int index;

		public string DepsCount {
			get {
				if (parentIndexes == null || parentIndexes.Count < 1)
					return "";
				return string.Format (" [{0} deps]", parentIndexes.Count);
			}
		}
	};

	public class DependencyGraph
	{
		protected List<VertexData> vertices = new List<VertexData> ();
		public List<VertexData> Types = new List<VertexData> ();
		Dictionary<string, int> indexes = new Dictionary<string, int> ();
		protected Dictionary<string, int> counts = new Dictionary<string, int> ();

		public void Load (string filename)
		{
			Console.WriteLine ("Loading dependency tree from: {0}", filename);

			using (var fileStream = File.OpenRead (filename))
			using (var zipStream = new GZipStream (fileStream, CompressionMode.Decompress)) {
				try {
					Load (zipStream);
				} catch (Exception) {
					Console.WriteLine ("Unable to open and read the dependecies.");
					Environment.Exit (1);
				}
			}
		}

		void Load (GZipStream zipStream) {
			using (XmlReader reader = XmlReader.Create (zipStream)) {
				while (reader.Read ()) {
					switch (reader.NodeType) {
					case XmlNodeType.Element:
						//Console.WriteLine (reader.Name);
						if (reader.Name == "edge" && reader.IsStartElement ()) {
							string b = reader.GetAttribute ("b");
							string e = reader.GetAttribute ("e");
							//Console.WriteLine ("edge value " + b + "  -->  " + e);

							if (e != b) {
								VertexData begin = Vertex (b, true);
								VertexData end = Vertex (e, true);

								if (end.parentIndexes == null)
									end.parentIndexes = new List<int> ();
								if (!end.parentIndexes.Contains (begin.index)) {
									end.parentIndexes.Add (begin.index);
									//Console.WriteLine (" end parent index: {0}", end.parentIndexes);
								}
							}
						}
						break;
					default:
						//Console.WriteLine ("node: " + reader.NodeType);
						break;
					}
				}
			}
		}

		public VertexData Vertex (string vertexName, bool create = false)
		{
			VertexData vertex;

			try {
				vertex = vertices [indexes [vertexName]];
			} catch (KeyNotFoundException) {
				if (create) {
					int index = vertices.Count;
					vertex = new VertexData () { value = vertexName, index = index };
					vertices.Add (vertex);
					indexes.Add (vertexName, index);
					string prefix = vertexName.Substring (0, vertexName.IndexOf (':'));
					if (counts.ContainsKey (prefix))
						counts [prefix]++;
					else
						counts [prefix] = 1;
					//Console.WriteLine ("prefix " + prefix + " count " + counts[prefix]);
					if (prefix == "TypeDef") {
						Types.Add (vertex);
					}
				} else
					return null;
			}

			return vertex;
		}

		public VertexData Vertex (int index)
		{
			return vertices [index];
		}
	}
}
