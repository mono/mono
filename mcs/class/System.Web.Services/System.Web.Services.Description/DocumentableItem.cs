// 
// System.Web.Services.Description.DocumentableItem.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public abstract class DocumentableItem {

		#region Fields

		string documentation;

		#endregion // Fields

		#region Constructors

		protected DocumentableItem ()
		{
			documentation = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public string Documentation {
			get { return documentation; }
			set { documentation = value; }
		}
	
		#endregion // Properties
	}
}
