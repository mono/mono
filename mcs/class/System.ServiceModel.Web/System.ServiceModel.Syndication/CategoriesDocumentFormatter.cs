using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	[DataContract]
	public abstract class CategoriesDocumentFormatter
	{
		protected CategoriesDocumentFormatter ()
		{
		}

		protected CategoriesDocumentFormatter (CategoriesDocument documentToWrite)
		{
			SetDocument (documentToWrite);
		}

		public CategoriesDocument Document { get; private set; }

		public abstract string Version { get; }


		public abstract bool CanRead (XmlReader reader);

		protected virtual InlineCategoriesDocument CreateInlineCategoriesDocument ()
		{
			return new InlineCategoriesDocument ();
		}

		protected virtual ReferencedCategoriesDocument CreateReferencedCategoriesDocument ()
		{
			return new ReferencedCategoriesDocument ();
		}

		public abstract void ReadFrom (XmlReader reader);

		protected virtual void SetDocument (CategoriesDocument document)
		{
			if (document == null)
				throw new ArgumentNullException ("document");
			Document = document;
		}

		public abstract void WriteTo (XmlWriter writer);
	}
}
