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
using System.Text;
using System.Configuration;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

using SysConfig = System.Configuration.Configuration;

namespace MonoTests.System.ServiceModel.MetadataTests {

	public abstract class TestContext {

		#region Abstract API

		public abstract MetadataSet GetMetadata (string name);

		public abstract XmlDocument GetConfiguration (string name);

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

			public override XmlDocument GetConfiguration (string name)
			{
				return LoadConfiguration (name);
			}
		}

		class _CreateMetadataContext : TestContext {
			public override MetadataSet GetMetadata (string name)
			{
				return MetadataSamples.GetMetadataByName (name);
			}

			public override XmlDocument GetConfiguration (string name)
			{
				var metadata = GetMetadata (name);
				var doc = new XmlDocument ();
				doc.LoadXml (SaveConfigToString (metadata));
				return doc;
			}
		}

		class _RoundTripContext : TestContext {
			public override MetadataSet GetMetadata (string name)
			{
				return RoundTrip (name);
			}

			public override XmlDocument GetConfiguration (string name)
			{
				var metadata = GetMetadata (name);
				var doc = new XmlDocument ();
				doc.LoadXml (SaveConfigToString (metadata));
				return doc;
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

		public static XmlDocument LoadConfiguration (string name)
		{
#if USE_EMBEDDED_METADATA
			return LoadConfigurationFromResource (name);
#else
			return LoadConfigurationFromFile (name);
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

		public static XmlDocument LoadConfigurationFromFile (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			if (!name.EndsWith (".config"))
				name = name + ".config";
			var uri = new Uri (asm.CodeBase);
			var path = Path.GetDirectoryName (uri.AbsolutePath);
			path = Path.Combine (path, "Test");
			path = Path.Combine (path, "MetadataTests");
			path = Path.Combine (path, "Resources");
			var filename = Path.Combine (path, name);
			using (var stream = new StreamReader (filename)) {
				var xml = new XmlDocument ();
				xml.Load (stream);
				return xml;
			}
		}

		public static XmlDocument LoadConfigurationFromResource (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			if (!name.EndsWith (".config"))
				name = name + ".config";
			
			var resname = "MetadataTests.Resources." + name;
			using (var stream = asm.GetManifestResourceStream (resname)) {
				if (stream == null)
					throw new InvalidOperationException (
						"No such resource: " + name);
				var xml = new XmlDocument ();
				xml.Load (stream);
				return xml;
			}
		}

		public static void SaveMetadataToFile (string filename, MetadataSet metadata)
		{
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

		public static void GenerateConfig (MetadataSet metadata, SysConfig config)
		{
			WsdlImporter importer = new WsdlImporter (metadata);
			
			var endpoints = importer.ImportAllEndpoints ();
			
			var generator = new ServiceContractGenerator (config);
			generator.Options = ServiceContractGenerationOptions.None;
			
			foreach (var endpoint in endpoints) {
				ChannelEndpointElement channelElement;
				generator.GenerateServiceEndpoint (endpoint, out channelElement);
			}
		}

		public static void GenerateConfig (string filename, MetadataSet metadata)
		{
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);
				
			GenerateConfig (metadata, config);
			config.Save (ConfigurationSaveMode.Minimal);
			NormalizeConfig (filename);
		}

		internal static string SaveConfigToString (MetadataSet metadata)
		{
			var filename = Path.GetTempFileName ();
			File.Delete (filename);

			try {
				GenerateConfig (filename, metadata);

				using (var reader = new StreamReader (filename))
					return reader.ReadToEnd ();
			} finally {
				File.Delete (filename);
			}
		}
		
		public static void NormalizeConfig (string filename)
		{
			// Mono-specific hack.
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				return;

			var doc = new XmlDocument ();
			doc.Load (filename);
			var nav = doc.CreateNavigator ();
			
			var empty = new List<XPathNavigator> ();
			var iter = nav.Select ("/configuration/system.serviceModel//*");
			foreach (XPathNavigator node in iter) {
				if (!node.HasChildren && !node.HasAttributes && string.IsNullOrEmpty (node.Value))
					empty.Add (node);
			}
			foreach (var node in empty)
				node.DeleteSelf ();
			
			var settings = new XmlWriterSettings ();
			settings.Indent = true;
			settings.NewLineHandling = NewLineHandling.Replace;
			
			using (var writer = XmlWriter.Create (filename, settings)) {
				doc.WriteTo (writer);
			}
			Console.WriteLine ();
		}

		#endregion
	}
}

