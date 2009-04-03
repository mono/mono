//
// InlineCategoriesDocument.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel.Syndication
{
	public class InlineCategoriesDocument : CategoriesDocument
	{
		public InlineCategoriesDocument ()
		{
			Categories = new Collection<SyndicationCategory> ();
		}

		public InlineCategoriesDocument (IEnumerable<SyndicationCategory> categories)
			: this ()
		{
			foreach (var i in categories)
				Categories.Add (i);
		}

		public InlineCategoriesDocument (IEnumerable<SyndicationCategory> categories, bool isFixed, string scheme)
			: this (categories)
		{
			IsFixed = isFixed;
			Scheme = scheme;
		}

		protected internal virtual SyndicationCategory CreateCategory ()
		{
			return new SyndicationCategory ();
		}

		public Collection<SyndicationCategory> Categories { get; private set; }

		public bool IsFixed { get; set; }

		public string Scheme { get; set; }
	}
}
