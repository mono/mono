//
// System.DirectoryServices.SortOption.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2004 Andreas Nahr
//
//

using System.ComponentModel;

namespace System.DirectoryServices {

	[TypeConverter (typeof (ExpandableObjectConverter))]
	public class SortOption
	{
		private String propertyName;
		private SortDirection direction;

		public SortOption ()
		{
		}

		public SortOption (String propertyName, SortDirection direction)
		{
			this.propertyName = propertyName;
			this.direction = direction;
		}

		public String PropertyName {
			get { return propertyName; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				propertyName = value;
			}
		}

		public SortDirection Direction {
			get { return direction; }
			set { direction = value; }
		}
	}
}

