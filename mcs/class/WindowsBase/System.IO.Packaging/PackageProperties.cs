// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//  Chris Toshok (toshok@ximian.com)
//  Alan McGovern (amcgovern@novell.com)
//

using System;
using System.Xml;

namespace System.IO.Packaging {

	public abstract class PackageProperties : IDisposable
	{
		internal const string NSPackageProperties = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
		internal const string NSPackagePropertiesRelation = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";
		internal const string PackagePropertiesContentType = "application/vnd.openxmlformats-package.core-properties+xml";


		static int uuid;
		
		protected PackageProperties ()
		{
			
		}

		public abstract string Category { get; set; }
		public abstract string ContentStatus { get; set; }
		public abstract string ContentType { get; set; }
		public abstract DateTime? Created { get; set; }
		public abstract string Creator { get; set; }
		public abstract string Description { get; set; }
		public abstract string Identifier { get; set; }
		public abstract string Keywords { get; set; }
		public abstract string Language { get; set; }
		public abstract string LastModifiedBy { get; set; }
		public abstract DateTime? LastPrinted { get; set; }
		public abstract DateTime? Modified { get; set; }
		internal Package Package { get; set; }
		internal PackagePart Part { get; set; }
		public abstract string Revision { get; set; }
		public abstract string Subject { get; set; }
		public abstract string Title { get; set; }
		public abstract string Version { get; set; }
				
		
		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing
		}

		internal void Flush ()
		{
			using (MemoryStream temp = new MemoryStream ()) {
				using (XmlTextWriter writer = new XmlTextWriter (temp, System.Text.Encoding.UTF8)) {
					WriteTo (writer);
					writer.Flush ();
					if (temp.Length == 0)
						return;
				}
			}
			
			if (Part == null)
			{
				int id = System.Threading.Interlocked.Increment (ref uuid);
				Uri uri = new Uri (string.Format ("/package/services/metadata/core-properties/{0}.psmdcp", id), UriKind.Relative);
				Part = Package.CreatePart (uri, PackagePropertiesContentType);
				PackageRelationship rel = Package.CreateRelationship (uri, TargetMode.Internal, NSPackagePropertiesRelation);
			}
			
			using (Stream s = Part.GetStream (FileMode.Create))
				using (XmlTextWriter writer = new XmlTextWriter (s, System.Text.Encoding.UTF8))
					WriteTo (writer);
		}
		
		internal virtual void LoadFrom (Stream stream)
		{

		}

		internal virtual void WriteTo (XmlTextWriter writer)
		{
			
		}
	}
}