using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

using Lucene.Net.Index;
using Lucene.Net.Documents;

using Monodoc.Ecma;
using Monodoc.Storage;
using Mono.Utilities;

namespace Monodoc.Providers
{
	public class EcmaUncompiledHelpSource : EcmaHelpSource
	{
		readonly DirectoryInfo basedir;
		readonly string basedoc;

		public readonly string BasePath;

		public new string Name {
			get;
			private set;
		}
	
		/* base_file: the directory containing the index.xml file, usually in Mono land .../Documentation/en
		 * markName: if true, we encase the node caption with [] to clearly mark it's from an uncompiled source
		 */
		public EcmaUncompiledHelpSource (string base_file, bool markName = true) : base ()
		{
			basedir = new DirectoryInfo (base_file);
			BasePath = basedir.FullName;
		
			basedoc = Path.Combine (basedir.FullName, "index.xml");
		
			Name = ((string)XDocument.Load (basedoc).Root.Element ("Title")) ?? "UnnamedUncompiledSource";
			if (markName)
				Name = '[' + Name + ']';
			Tree.RootNode.Caption = Name;

			Func<XElement, string> indexGenerator = type => {
				var nsName = (string)type.Parent.Attribute ("Name");
				var typeName = (string)type.Attribute ("Name");
				return Path.ChangeExtension (nsName + '/' + typeName, ".xml");
			};

			this.Storage = new UncompiledDocStorage (BasePath);

			EcmaDoc.PopulateTreeFromIndexFile (basedoc, UriPrefix, Tree, null, null, indexGenerator);
		}

		protected override string UriPrefix {
			get {
				return "uncompiled:";
			}
		}

		public override Stream GetImage (string url)
		{
			var path = Path.Combine (BasePath, "_images", url);
			return File.Exists (path) ? File.OpenRead (path) : (Stream)null;
		}
	}
}
