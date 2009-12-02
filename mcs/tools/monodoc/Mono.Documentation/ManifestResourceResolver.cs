using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Mono.Documentation {
	public class ManifestResourceResolver : XmlUrlResolver {
		private string[] dirs;

		public ManifestResourceResolver (params string[] dirs)
		{
			this.dirs = (string[]) dirs.Clone ();
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			if (Array.IndexOf (
						Assembly.GetExecutingAssembly ().GetManifestResourceNames (), 
						relativeUri) >= 0)
					return new Uri ("x-resource:///" + relativeUri);
			foreach (var dir in dirs) {
				if (File.Exists (Path.Combine (dir, relativeUri)))
					return base.ResolveUri (new Uri ("file://" + new DirectoryInfo (dir).FullName + "/"), 
							relativeUri);
			}
			return base.ResolveUri (baseUri, relativeUri);
		}

		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (ofObjectToReturn == null)
				ofObjectToReturn = typeof(Stream);
			if (ofObjectToReturn != typeof(Stream))
				throw new XmlException ("This object type is not supported.");
			if (absoluteUri.Scheme != "x-resource")
				return base.GetEntity (absoluteUri, role, ofObjectToReturn);
			return Assembly.GetExecutingAssembly().GetManifestResourceStream (
					absoluteUri.Segments [1]);
		}
	}
}

