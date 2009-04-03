using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Syndication
{
	public class ReferencedCategoriesDocument : CategoriesDocument
	{
		public ReferencedCategoriesDocument ()
		{
		}

		public ReferencedCategoriesDocument (Uri link)
		{
			Link = link;
		}

		public Uri Link { get; set; }
	}
}
