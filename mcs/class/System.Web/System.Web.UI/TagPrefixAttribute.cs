//
// System.Web.UI.TagPrefixAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Assembly)]
	public sealed class TagPrefixAttribute : Attribute
	{
		string namespaceName;
		string tagPrefix;
		
		public TagPrefixAttribute (string namespaceName,
					   string tagPrefix)
		{
			if (namespaceName == null || tagPrefix == null)
				throw new ArgumentNullException ();

			this.namespaceName = namespaceName;
			this.tagPrefix = tagPrefix;
		}

		public string NamespaceName {
			get { return namespaceName; }
		}

		public string TagPrefix {
			get { return tagPrefix; }
		}
	}
}
