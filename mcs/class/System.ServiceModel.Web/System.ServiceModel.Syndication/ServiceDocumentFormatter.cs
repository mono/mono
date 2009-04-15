//
// ServiceDocumentFormatter.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
//
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	[DataContract]
	public abstract class ServiceDocumentFormatter
	{
		protected static SyndicationCategory CreateCategory (InlineCategoriesDocument inlineCategories)
		{
			return inlineCategories.CreateCategory ();
		}

		protected static ResourceCollectionInfo CreateCollection (Workspace workspace)
		{
			return workspace.CreateResourceCollection ();
		}

		protected static InlineCategoriesDocument CreateInlineCategories (ResourceCollectionInfo collection)
		{
			return collection.CreateInlineCategoriesDocument ();
		}

		protected static ReferencedCategoriesDocument CreateReferencedCategories (ResourceCollectionInfo collection)
		{
			return collection.CreateReferencedCategoriesDocument ();
		}

		protected static Workspace CreateWorkspace (ServiceDocument document)
		{
			return document.CreateWorkspace ();
		}

		[MonoTODO ("Use maxExtensionSize somewhere")]
		protected static void LoadElementExtensions (XmlReader reader, CategoriesDocument categories, int maxExtensionSize)
		{
			categories.ElementExtensions.Add (reader);
		}

		[MonoTODO ("Use maxExtensionSize somewhere")]
		protected static void LoadElementExtensions (XmlReader reader,ResourceCollectionInfo collection, int maxExtensionSize)
		{
			collection.ElementExtensions.Add (reader);
		}

		[MonoTODO ("Use maxExtensionSize somewhere")]
		protected static void LoadElementExtensions (XmlReader reader, ServiceDocument document, int maxExtensionSize)
		{
			document.ElementExtensions.Add (reader);
		}

		[MonoTODO ("Use maxExtensionSize somewhere")]
		protected static void LoadElementExtensions (XmlReader reader, Workspace workspace, int maxExtensionSize)
		{
			workspace.ElementExtensions.Add (reader);
		}

		protected static bool TryParseAttribute (string name, string ns, string value, CategoriesDocument categories, string version)
		{
			return categories.TryParseAttribute (name, ns, value, version);
		}

		protected static bool TryParseAttribute (string name, string ns, string value, ResourceCollectionInfo collection, string version)
		{
			return collection.TryParseAttribute (name, ns, value, version);
		}

		protected static bool TryParseAttribute (string name, string ns, string value, ServiceDocument document, string version)
		{
			return document.TryParseAttribute (name, ns, value, version);
		}

		protected static bool TryParseAttribute (string name, string ns, string value, Workspace workspace, string version)
		{
			return workspace.TryParseAttribute (name, ns, value, version);
		}

		protected static bool TryParseElement (XmlReader reader, CategoriesDocument categories, string version)
		{
			return categories.TryParseElement (reader, version);
		}

		protected static bool TryParseElement (XmlReader reader, ResourceCollectionInfo collection, string version)
		{
			return collection.TryParseElement (reader, version);
		}

		protected static bool TryParseElement (XmlReader reader, ServiceDocument document, string version)
		{
			return document.TryParseElement (reader, version);
		}

		protected static bool TryParseElement (XmlReader reader, Workspace workspace, string version)
		{
			return workspace.TryParseElement (reader, version);
		}

		protected static void WriteAttributeExtensions (XmlWriter writer, CategoriesDocument categories, string version)
		{
			categories.WriteAttributeExtensions (writer, version);
		}

		protected static void WriteAttributeExtensions (XmlWriter writer, ResourceCollectionInfo collection, string version)
		{
			collection.WriteAttributeExtensions (writer, version);
		}

		protected static void WriteAttributeExtensions (XmlWriter writer, ServiceDocument document, string version)
		{
			document.WriteAttributeExtensions (writer, version);
		}

		protected static void WriteAttributeExtensions (XmlWriter writer, Workspace workspace, string version)
		{
			workspace.WriteAttributeExtensions (writer, version);
		}

		protected static void WriteElementExtensions (XmlWriter writer, CategoriesDocument categories, string version)
		{
			categories.WriteElementExtensions (writer, version);
		}

		protected static void WriteElementExtensions (XmlWriter writer, ResourceCollectionInfo collection, string version)
		{
			collection.WriteElementExtensions (writer, version);
		}

		protected static void WriteElementExtensions (XmlWriter writer, ServiceDocument document, string version)
		{
			document.WriteElementExtensions (writer, version);
		}

		protected static void WriteElementExtensions (XmlWriter writer, Workspace workspace, string version)
		{
			workspace.WriteElementExtensions (writer, version);
		}

		// instance members

		protected ServiceDocumentFormatter ()
			: this (new ServiceDocument ())
		{
		}

		protected ServiceDocumentFormatter (ServiceDocument documentToWrite)
		{
			SetDocument (documentToWrite);
		}

		public ServiceDocument Document { get; private set; }

		public abstract string Version { get; }


		public abstract bool CanRead (XmlReader reader);

		protected virtual ServiceDocument CreateDocumentInstance ()
		{
			return new ServiceDocument () { InternalFormatter = this };
		}

		public abstract void ReadFrom (XmlReader reader);

		protected virtual void SetDocument (ServiceDocument document)
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			Document = document;
		}

		public abstract void WriteTo (XmlWriter writer);
	}
}
