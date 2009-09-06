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
//

using System;

namespace System.IO.Packaging {

	public class PackageRelationship
	{
		internal PackageRelationship (string id, Package package, string relationshipType,
		                              Uri sourceUri, TargetMode targetMode, Uri targetUri)
		{
			Check.IdIsValid (id);
			Check.Package (package);
			Check.RelationshipTypeIsValid (relationshipType);
			Check.SourceUri (sourceUri);
			Check.TargetUri (targetUri);

			Id = id;
			Package = package;
			RelationshipType = relationshipType;
			SourceUri = sourceUri;
			TargetMode = targetMode;
			TargetUri = targetUri;
		}

		public string Id {
			get; private set;
		}
		public Package Package {
			get; private set;
		}
		public string RelationshipType {
			get; private set;
		}
		public Uri SourceUri {
			get; private set;
		}
		public TargetMode TargetMode {
			get; private set;
		}
		public Uri TargetUri  {
			get; private set;
		}
	}
}
