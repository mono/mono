using System;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Construction;

namespace Microsoft.Build.Internal
{
	class ProjectTaskItem : ITaskItem
	{
		ProjectItemElement item;
		string evaluated_include_element;
		
		public ProjectTaskItem (ProjectItemElement item, string evaluatedIncludeElement)
		{
			this.item = item;
			this.evaluated_include_element = evaluatedIncludeElement;
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
			throw new NotImplementedException ();
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
			get { return evaluated_include_element; }
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

