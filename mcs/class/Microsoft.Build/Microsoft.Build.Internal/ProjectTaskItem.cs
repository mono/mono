//
// ProjectTaskItem.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.IO;

namespace Microsoft.Build.Internal
{
	class ProjectTaskItem : ITaskItem
	{
		ProjectItemElement item;
		string evaluated_include_part;
		
		public ProjectTaskItem (ProjectItemElement item, string evaluatedIncludePart)
		{
			this.item = item;
			this.evaluated_include_part = WindowsCompatibilityExtensions.FindMatchingPath (evaluatedIncludePart);
		}
		#region ITaskItem implementation
		System.Collections.IDictionary ITaskItem.CloneCustomMetadata ()
		{
			var ret = new System.Collections.Hashtable ();
			foreach (var p in item.Metadata)
				ret [p.Name] = p;
			return ret;
		}
		void ITaskItem.CopyMetadataTo (ITaskItem destinationItem)
		{
			throw new NotImplementedException ();
		}
		string ITaskItem.GetMetadata (string metadataName)
		{
			var wk = ProjectCollection.GetWellKnownMetadata (metadataName, evaluated_include_part, Path.GetFullPath, null);
			if (wk != null)
				return wk;
			var mde = item.Metadata.FirstOrDefault (m => m.Name == metadataName);
			return mde != null ? mde.Value : string.Empty;
		}
		void ITaskItem.RemoveMetadata (string metadataName)
		{
			throw new NotImplementedException ();
		}
		void ITaskItem.SetMetadata (string metadataName, string metadataValue)
		{
			throw new NotImplementedException ();
		}
		string ITaskItem.ItemSpec {
			get { return evaluated_include_part; }
			set { throw new NotImplementedException (); }
		}
		int ITaskItem.MetadataCount {
			get {
				throw new NotImplementedException ();
			}
		}
		System.Collections.ICollection ITaskItem.MetadataNames {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}

