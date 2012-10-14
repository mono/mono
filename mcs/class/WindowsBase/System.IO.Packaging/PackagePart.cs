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
//	Chris Toshok (toshok@ximian.com)
//  Alan McGovern (amcgovern@novell.com)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace System.IO.Packaging {

	public abstract class PackagePart
	{
		string contentType;
		
		internal bool IsRelationship { get; set; }
		
		int relationshipId;
		Dictionary<string, PackageRelationship> relationships;
		PackageRelationshipCollection relationshipsCollection = new PackageRelationshipCollection ();
		
		Dictionary<string, PackageRelationship> Relationships {
			get {
				if (relationships == null) {
					relationships = new Dictionary<string, PackageRelationship> (StringComparer.OrdinalIgnoreCase);
					if (Package.PartExists (RelationshipsPartUri))
						using (Stream s = Package.GetPart (RelationshipsPartUri).GetStream ())
							LoadRelationships (relationships, s);
				}

				return relationships;
			}
		}
		Stream PartStream { get; set;  }

		internal Uri RelationshipsPartUri {
			get; set;
		}
		
		protected PackagePart (Package package, Uri partUri)
			: this(package, partUri, null)
		{
			
		}

		protected internal PackagePart (Package package, Uri partUri, string contentType)
			: this (package, partUri, contentType, CompressionOption.Normal)
		{
			
		}

		protected internal PackagePart (Package package, Uri partUri, string contentType, CompressionOption compressionOption)
		{
			Check.Package (package);
			Check.PartUri (partUri);
			Check.ContentTypeIsValid (contentType);

			Package = package;
			Uri = partUri;
			ContentType = contentType;
			CompressionOption = compressionOption;
			RelationshipsPartUri = PackUriHelper.GetRelationshipPartUri(Uri);
		}

		public CompressionOption CompressionOption {
			get; private set;
		}

		public string ContentType {
			get {
				if (contentType == null && (contentType = GetContentTypeCore()) == null)
					throw new NotSupportedException ("If contentType is not supplied in the constructor, GetContentTypeCore must be overridden");
				return contentType;
			}
			private set {
				contentType = value;
			}
		}

		public Package Package {
			get; internal set;
		}

		public Uri Uri {
			get; private set;
		}

		private void CheckIsRelationship ()
		{
			if (IsRelationship)
				throw new InvalidOperationException ("A relationship cannot have relationships to other parts"); 
		}

		public PackageRelationship CreateRelationship (Uri targetUri, TargetMode targetMode, string relationshipType)
		{
			return CreateRelationship (targetUri, targetMode, relationshipType, null);
		}

		public PackageRelationship CreateRelationship (Uri targetUri, TargetMode targetMode, string relationshipType, string id)
		{
			return CreateRelationship (targetUri, targetMode, relationshipType, id, false);
		}

		private PackageRelationship CreateRelationship (Uri targetUri, TargetMode targetMode, string relationshipType, string id, bool loading)
		{
			Package.CheckIsReadOnly ();
			Check.TargetUri (targetUri);
			Check.RelationshipTypeIsValid (relationshipType);
			Check.IdIsValid (id);

			if (id == null)
				id = NextId ();

			if (Relationships.ContainsKey (id))
				throw new XmlException ("A relationship with this ID already exists");
			
			PackageRelationship r = new PackageRelationship (id, Package, relationshipType, Uri, targetMode, targetUri);
			Relationships.Add (r.Id, r);

			if (!loading)
				WriteRelationships ();
			return r;
		}

		public void DeleteRelationship (string id)
		{
			Package.CheckIsReadOnly ();
			CheckIsRelationship ();
			Relationships.Remove (id);
			WriteRelationships ();
		}

		void LoadRelationships (Dictionary<string, PackageRelationship> relationships, Stream stream)
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (stream);
			XmlNamespaceManager manager = new XmlNamespaceManager (doc.NameTable);
			manager.AddNamespace ("rel", Package.RelationshipNamespace);

			foreach (XmlNode node in doc.SelectNodes ("/rel:Relationships/*", manager))
			{
				TargetMode mode = TargetMode.Internal;
				if (node.Attributes["TargetMode"] != null)
					mode = (TargetMode) Enum.Parse (typeof(TargetMode), node.Attributes ["TargetMode"].Value);
				
				CreateRelationship (new Uri (node.Attributes["Target"].Value.ToString(), UriKind.Relative),
				                    mode,
				                    node.Attributes["Type"].Value.ToString (),
				                    node.Attributes["Id"].Value.ToString (),
				                    true);
			}
		}

		public bool RelationshipExists (string id)
		{
			CheckIsRelationship ();
			return Relationships.ContainsKey (id);
		}

		public PackageRelationship GetRelationship (string id)
		{
			CheckIsRelationship ();
			return Relationships [id];
		}

		public PackageRelationshipCollection GetRelationships ()
		{
			CheckIsRelationship ();
			relationshipsCollection.Relationships.Clear ();
			relationshipsCollection.Relationships.AddRange (Relationships.Values);
			return relationshipsCollection;
		}

		public PackageRelationshipCollection GetRelationshipsByType (string relationshipType)
		{
			CheckIsRelationship ();
			PackageRelationshipCollection collection = new PackageRelationshipCollection ();
			foreach (PackageRelationship r in Relationships.Values)
				if (r.RelationshipType == relationshipType)
					collection.Relationships.Add (r);
			
			return collection;
		}

		public Stream GetStream ()
		{
			return GetStream (Package.FileOpenAccess == FileAccess.Read && !IsRelationship ? FileMode.Open : FileMode.OpenOrCreate);
		}

		public Stream GetStream (FileMode mode)
		{
			return GetStream (mode, IsRelationship ? FileAccess.ReadWrite : Package.FileOpenAccess);
		}

		public Stream GetStream (FileMode mode, FileAccess access)
		{
			bool notAllowed = mode == FileMode.Append || mode == FileMode.CreateNew || mode == FileMode.Truncate;
			if (access != FileAccess.Read && notAllowed)
				throw new ArgumentException (string.Format (string.Format ("FileMode '{0}' not supported", mode)));

			if (access == FileAccess.Read && (notAllowed || mode == FileMode.Create))
				throw new IOException (string.Format ("FileMode '{0}' not allowed on a readonly stream", mode));
			
			return GetStreamCore (mode, access);
		}

		protected abstract Stream GetStreamCore (FileMode mode, FileAccess access);

		protected virtual string GetContentTypeCore ()
		{
			return null;
		}

		private string NextId ()
		{
			while (true)
			{
				string s = relationshipId.ToString ();
				if (!RelationshipExists (s))
					return s;
				relationshipId ++;
			}
		}

		void WriteRelationships ()
		{
			bool exists = Package.PartExists (RelationshipsPartUri);
			if (exists && Relationships.Count == 0)
			{
				Package.DeletePart (RelationshipsPartUri);
				return;
			}
			
			if (!exists)
			{
				PackagePart part = Package.CreatePart (RelationshipsPartUri, Package.RelationshipContentType);
				part.IsRelationship = true;
			}
			using (Stream s = Package.GetPart (RelationshipsPartUri).GetStream ())
				Package.WriteRelationships (Relationships, s);
		}
	}
}