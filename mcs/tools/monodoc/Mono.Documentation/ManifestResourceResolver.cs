using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Mono.Documentation {
	public class ManifestResourceResolver : XmlUrlResolver {
		private string dir;

		public ManifestResourceResolver (string dir)
		{
			this.dir = dir;
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			using (Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream (relativeUri)) {
				if (s != null)
					return new Uri ("x-resource:///" + relativeUri);
			}
			return base.ResolveUri (
					new Uri ("file://" + new DirectoryInfo (dir).FullName + "/"), 
					relativeUri);
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

