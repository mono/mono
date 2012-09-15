//
// ItemReference.cs: Represents "@(Reference)" in expression.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Ankit Jain (jankit@novell.com)
// 
// (C) 2005 Marek Sieradzki
// Copyright 2009 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	internal class ItemReference : IReference {
	
		string		itemName;
		Expression	transform;
		Expression	separator;
		int		start;
		int		length;
		string		original_string;
		
		public ItemReference (string original_string, string itemName, string transform, string separator, int start, int length)
		{
			this.itemName = itemName;
			this.start = start;
			this.length = length;
			this.original_string = original_string;

			// Transform and separator are never expanded for item refs
			if (transform != null) {
				this.transform = new Expression ();
				this.transform.Parse (transform, ParseOptions.AllowMetadata | ParseOptions.Split);
			}

			if (separator != null) {
				this.separator = new Expression ();
				this.separator.Parse (separator, ParseOptions.Split);
			}
		}
		
		// when evaluating property, allowItems=false, so,
		// ItemRef will _not_ get created, so this wont get hit
		// when evaluating items, expand: true
		// other cases, expand: true
		public string ConvertToString (Project project, ExpressionOptions options)
		{
			BuildItemGroup group;
			if (project.TryGetEvaluatedItemByNameBatched (itemName, out group))
				return group.ConvertToString (transform, separator, options);
			else
				return String.Empty;
		}
		
		public ITaskItem [] ConvertToITaskItemArray (Project project, ExpressionOptions options)
		{
			BuildItemGroup group;
			if (project.TryGetEvaluatedItemByNameBatched (itemName, out group))
				return group.ConvertToITaskItemArray (transform, separator, options);
			else
				return null;
		}

		public string ItemName {
			get { return itemName; }
		}

		public Expression Transform {
			get { return transform; }
		}

		public Expression Separator {
			get { return separator; }
		}

		public string OriginalString {
			get { return original_string; }
		}

		public int Start {
			get { return start; }
		}

		public int End {
			get { return start + length - 1; }
		}

		public override string ToString ()
		{
			return original_string;
		}
	}
}
