//
// SyndicationItemFormatter.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	[DataContract]
	public abstract class SyndicationItemFormatter
	{
		#region static members

		// My assumption is that those static methods are defined to
		// be used in the derived classes and to dispatch the actual
		// processing to each syndication element. Considering that
		// they can be overriden in each element, I assume that these
		// methods depend on the elements, not in reverse order.

		protected internal static SyndicationCategory CreateCategory (SyndicationItem item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			return item.CreateCategory ();
		}

		protected internal static SyndicationLink CreateLink (SyndicationItem item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			return item.CreateLink ();
		}

		protected internal static SyndicationPerson CreatePerson (SyndicationItem item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			return item.CreatePerson ();
		}

		[MonoTODO ("use maxExtensionsSize parameter")]
		protected internal static void LoadElementExtensions (XmlReader reader, SyndicationCategory category, int maxExtensionSize)
		{
			category.ElementExtensions.Add (reader);
		}

		[MonoTODO ("use maxExtensionsSize parameter")]
		protected internal static void LoadElementExtensions (XmlReader reader, SyndicationItem item, int maxExtensionSize)
		{
			item.ElementExtensions.Add (reader);
		}

		[MonoTODO ("use maxExtensionsSize parameter")]
		protected internal static void LoadElementExtensions (XmlReader reader, SyndicationLink link, int maxExtensionSize)
		{
			link.ElementExtensions.Add (reader);
		}

		[MonoTODO ("use maxExtensionsSize parameter")]
		protected internal static void LoadElementExtensions (XmlReader reader, SyndicationPerson person, int maxExtensionSize)
		{
			person.ElementExtensions.Add (reader);
		}

		protected internal static bool TryParseAttribute (string name, string ns, string value, SyndicationCategory category, string version)
		{
			if (category == null)
				throw new ArgumentNullException ("category");
			return category.TryParseAttribute (name, ns, value, version);
		}

		protected internal static bool TryParseAttribute (string name, string ns, string value, SyndicationItem item, string version)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			return item.TryParseAttribute (name, ns, value, version);
		}

		protected internal static bool TryParseAttribute (string name, string ns, string value, SyndicationLink link, string version)
		{
			if (link == null)
				throw new ArgumentNullException ("link");
			return link.TryParseAttribute (name, ns, value, version);
		}

		protected internal static bool TryParseAttribute (string name, string ns, string value, SyndicationPerson person, string version)
		{
			if (person == null)
				throw new ArgumentNullException ("person");
			return person.TryParseAttribute (name, ns, value, version);
		}

		protected internal static bool TryParseContent (XmlReader reader, SyndicationItem item, string contentType, string version, out SyndicationContent content)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			return item.TryParseContent (reader, contentType, version, out content);
		}

		protected internal static bool TryParseElement (XmlReader reader, SyndicationCategory category, string version)
		{
			if (category == null)
				throw new ArgumentNullException ("category");
			return category.TryParseElement (reader, version);
		}

		protected internal static bool TryParseElement (XmlReader reader, SyndicationItem item, string version)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			return item.TryParseElement (reader, version);
		}

		protected internal static bool TryParseElement (XmlReader reader, SyndicationLink link, string version)
		{
			if (link == null)
				throw new ArgumentNullException ("link");
			return link.TryParseElement (reader, version);
		}

		protected internal static bool TryParseElement (XmlReader reader, SyndicationPerson person, string version)
		{
			if (person == null)
				throw new ArgumentNullException ("person");
			return person.TryParseElement (reader, version);
		}

		protected internal static void WriteAttributeExtensions (XmlWriter writer, SyndicationCategory category, string version)
		{
			if (category == null)
				throw new ArgumentNullException ("category");
			category.WriteAttributeExtensions (writer, version);
		}

		protected internal static void WriteAttributeExtensions (XmlWriter writer, SyndicationItem item, string version)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			item.WriteAttributeExtensions (writer, version);
		}

		protected internal static void WriteAttributeExtensions (XmlWriter writer, SyndicationLink link, string version)
		{
			if (link == null)
				throw new ArgumentNullException ("link");
			link.WriteAttributeExtensions (writer, version);
		}

		protected internal static void WriteAttributeExtensions (XmlWriter writer, SyndicationPerson person, string version)
		{
			if (person == null)
				throw new ArgumentNullException ("person");
			person.WriteAttributeExtensions (writer, version);
		}

		protected internal void WriteElementExtensions (XmlWriter writer, SyndicationCategory category, string version)
		{
			if (category == null)
				throw new ArgumentNullException ("category");
			category.WriteElementExtensions (writer, version);
		}

		protected internal static void WriteElementExtensions (XmlWriter writer, SyndicationItem item, string version)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			item.WriteElementExtensions (writer, version);
		}

		protected internal void WriteElementExtensions (XmlWriter writer, SyndicationLink link, string version)
		{
			if (link == null)
				throw new ArgumentNullException ("link");
			link.WriteElementExtensions (writer, version);
		}

		protected internal void WriteElementExtensions (XmlWriter writer, SyndicationPerson person, string version)
		{
			if (person == null)
				throw new ArgumentNullException ("person");
			person.WriteElementExtensions (writer, version);
		}

		#endregion

		#region instance members

		SyndicationItem item;

		protected SyndicationItemFormatter ()
		{
		}

		protected SyndicationItemFormatter (SyndicationItem itemToWrite)
		{
			if (itemToWrite == null)
				throw new ArgumentNullException ("itemToWrite");
			SetItem (itemToWrite);
		}

		protected internal virtual void SetItem (SyndicationItem item)
		{
			if (item == null)
				throw new ArgumentNullException ("item");
			this.item = item;
		}

		public SyndicationItem Item { 
			get { return item; }
		}

		public abstract string Version { get; }

		protected abstract SyndicationItem CreateItemInstance ();

		public abstract bool CanRead (XmlReader reader);

		public abstract void ReadFrom (XmlReader reader);

		public abstract void WriteTo (XmlWriter writer);

		public override string ToString ()
		{
			return String.Concat (GetType ().FullName, ", SyndicationVersion=", Version);
		}
		#endregion
	}
}
