// 
// System.Web.Services.Description.DocumentableItem.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.ComponentModel;
using System.Xml.Serialization;

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

		[XmlElement ("documentation")]
		[DefaultValue ("")]
		public string Documentation {
			get { return documentation; }
			set { documentation = value; }
		}
	
		#endregion // Properties
	}
}
