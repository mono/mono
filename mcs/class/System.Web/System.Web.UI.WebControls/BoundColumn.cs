/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BoundColumn
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  60%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class BoundColumn : DataGridColumn
	{
		public static readonly string thisExpr = "!";
		
		private string dataField;
		private string dataFormatString;
		private bool readOnly;

		public BoundColumn()
		{
			//TODO: The start work
			Initialize();
		}
		
		public override void Initialize()
		{
			dataField        = String.Empty;
			dataFormatString = String.Empty;
			readOnly         = false;
		}

		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			//TODO: What to do?
		}
		
		public virtual string DataField
		{
			get
			{
				return dataField;
			}
			set
			{
				dataField = value;
			}
		}
		
		public virtual string DataFormatString
		{
			get
			{
				return dataFormatString;
			}
			set
			{
				dataFormatString = value;
			}
		}
		
		public virtual bool ReadOnly
		{
			get
			{
				return readOnly;
			}
			set
			{
				readOnly = value;
			}
		}
		
		protected virtual string FormatDataValue(Object dataValue)
		{
			// TODO: How to extract the value from the object?
			// TODO: Then format the value. Here's a possible solution
			if(dataFormatString == null || dataFormatString.equals(String.Empty))
				return dataValue.toString();
			if(dataValue is DateTime)
				return ((DateTime)dataValue).toString(dataFormatString);
			// and so on for int, String, double..
			// something's wrong here. there must be some shorter method!
			//string val = dataValue.toString(dataFormatString);
		}
	}
}
