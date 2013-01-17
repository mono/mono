//
// ITestContext.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.ServiceModel.Description;

namespace MonoTests.System.ServiceModel.MetadataTests {

	public abstract class TestContext {

		#region Abstract API

		public abstract MetadataSet GetMetadata (string name);

		#endregion

		#region Default Context

		public static TestContext LoadMetadataContext = new _LoadMetadataContext ();

		public static TestContext CreateMetadataContext = new _CreateMetadataContext ();

		public static TestContext RoundTripContext = new _RoundTripContext ();

		#endregion

		#region Implementations

		class _LoadMetadataContext : TestContext {
			public override MetadataSet GetMetadata (string name)
			{
				return LoadMetadata (name);
			}
		}

		class _CreateMetadataContext : TestContext {
			public override MetadataSet GetMetadata (string name)
			{
				return MetadataSamples.GetMetadataByName (name);
			}
		}

		class _RoundTripContext : TestContext {
			public override MetadataSet GetMetadata (string name)
			{
				return RoundTrip (name);
			}
		}

		#endregion

		#region Public Static API

		public static MetadataSet LoadMetadata (string name)
		{
#if USE_EMBEDDED_METADATA
			return LoadMetadataFromResource (name);
#else
			return LoadMetadataFromFile (name);
#endif
		}

		public static void SaveMetadata (string name, MetadataSet metadata)
		{
			SaveMetadataToFile (name, metadata);
		}

		public static MetadataSet LoadMetadataFromFile (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			if (!name.EndsWith (".xml"))
				name = name + ".xml";
			var uri = new Uri (asm.CodeBase);
			var path = Path.GetDirectoryName (uri.AbsolutePath);
			path = Path.Combine (path, "Test");
			path = Path.Combine (path, "MetadataTests");
			path = Path.Combine (path, "Resources");
			var filename = Path.Combine (path, name);
			using (var stream = new StreamReader (filename)) {
				var reader = new XmlTextReader (stream);
				return MetadataSet.ReadFrom (reader);
			}
		}

		public static MetadataSet LoadMetadataFromResource (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			if (!name.EndsWith (".xml"))
				name = name + ".xml";
			
			var resname = "MetadataTests.Resources." + name;
			using (var stream = asm.GetManifestResourceStream (resname)) {
				if (stream == null)
					throw new InvalidOperationException (
						"No such resource: " + name);
				var reader = new XmlTextReader (stream);
				return MetadataSet.ReadFrom (reader);
			}
		}
		
		public static void SaveMetadataToFile (string name, MetadataSet metadata)
		{
			var filename = name + ".xml";
			if (File.Exists (filename))
				return;

			using (var file = new StreamWriter (filename, false)) {
				var writer = new XmlTextWriter (file);
				writer.Formatting = Formatting.Indented;
				metadata.WriteTo (writer);
			}

			Console.WriteLine ("Exported {0}.", filename);
		}

		internal static string SaveMetadataToString (MetadataSet metadata)
		{
			using (var ms = new MemoryStream ()) {
				var writer = new XmlTextWriter (new StreamWriter (ms));
				writer.Formatting = Formatting.Indented;
				metadata.WriteTo (writer);
				writer.Flush ();

				return Encoding.UTF8.GetString (ms.GetBuffer (), 0, (int)ms.Position);
			}
		}

		internal static MetadataSet LoadMetadataFromString (string doc)
		{
			var buffer = Encoding.UTF8.GetBytes (doc);
			using (var ms = new MemoryStream (buffer)) {
				var reader = new XmlTextReader (ms);
				return MetadataSet.ReadFrom (reader);
			}
		}

		public static MetadataSet RoundTrip (string name)
		{
			var metadata = MetadataSamples.GetMetadataByName (name);

			var doc = SaveMetadataToString (metadata);
			return LoadMetadataFromString (doc);
		}

		#endregion
	}
}

