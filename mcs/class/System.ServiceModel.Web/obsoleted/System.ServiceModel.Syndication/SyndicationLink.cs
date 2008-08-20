//
// SyndicationLink.cs
//
// Author:
//      Stephen A Jazdzewski (Steve@Jazd.com)
//
// Copyright (C) 2007 Stephen A Jazdzewski
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
using System.Xml;
using System.Collections.Generic;

namespace System.ServiceModel.Syndication
{
	[MonoTODO]
	public class SyndicationLink {

		[MonoTODO]
		public SyndicationLink (Uri uri, string relationshipType, string title, string mediaType, long length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SyndicationLink CreateAlternateLink (Uri uri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SyndicationLink CreateAlternateLink (Uri uri, string mediaType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SyndicationLink CreateMediaEnclosureLink (Uri uri, string mediaType, long length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SyndicationLink CreateSelfLink (Uri uri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static SyndicationLink CreateSelfLink (Uri urk, string mediaType)
		{
			throw new NotImplementedException ();
		}

		public Dictionary <XmlQualifiedName, string> AttributeExtensions {
			get {throw new NotImplementedException ();}
		}

		public long Length {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		public string MediaType {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		public string Title {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		private Uri uri;
		private string relationshipType;

		public SyndicationLink(Uri uri)
		{
			this.uri = uri;
		}

		public Uri Uri
		{
			get { return uri; }
			set { uri = value; }
		}

		public string RelationshipType
		{
			get { return relationshipType; }
			set { relationshipType = value; }
		}
	}
}
