/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BaseDataList
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  20%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public abstract class BaseDataList: WebControl
	{

		private int cellPadding = -1;
		private int cellSpacing = 0;
		private object dataSource = null;
		private string dataKeyField = String.Empty;
		private DataKeyCollection dataKeys;		// TODO: From where do get the values into it?
		private string dataMember;
		private GridLines gridLines;
		private HorizontalAlign hAlign;
		
//		private EventArgs dataBindEventArgs;
		
		public BaseDataList()
		{
			// TODO Something
			dataKeys = new DataKeyCollection(new ArrayList());
			dataMember = String.Empty;
			gridLines = GridLines.Both;
			hAlign = HorizontalAlign.NotSet;
		}
		
		public static bool IsBindableType(Type type)
		{
			//TODO: To see what has to be here
			if(type.IsPrimitive)
			{
				//Type.GetTypeFromHandle(new RuntimeTypeHandle());
			}
			return false; //for the time being, to be able to make it compile
		}
		
		public virtual int CellPadding
		{
			get
			{
				return cellPadding;
			}
			set
			{
				cellPadding = value;
			}
		}
		
		public virtual int CellSpacing
		{
			get
			{
				return cellSpacing;
			}
			set
			{
				cellSpacing = value;
			}
		}
		
		public virtual string DataKeyField
		{
			get
			{
				return dataKeyField;
			}
			set
			{
				dataKeyField = value;
			}
		}
		
		public DataKeyCollection DataKeys
		{
			get
			{
				return dataKeys;
			}
		}
		
		public string DataMember
		{
			get
			{
				return dataMember;
			}
			set
			{
				dataMember = value;
			}
		}
		
		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				dataSource = value;
			}
		}
		
		public virtual GridLines GridLines
		{
			get
			{
				return gridLines;
			}
			set
			{
				gridLines = value;
			}
		}
		
		public virtual HorizontalAlign HorizontalAlign
		{
			get
			{
				return hAlign;
			}
			set
			{
				hAlign = value;
			}
		}

		public override void DataBind()
		{
			// TODO: have to write the implementation
			// I am not sure of whether it will be of any use here since 
			// I am an abstract class, and have no identity of myself.
			//dataBindEventArgs = EventArgs.Empty;
			OnDataBinding(EventArgs.Empty);
		}

		//TODO: Check - where are the following abstract methods?
		/*
		 * CreateControlHierarchy(bool)
		 * PrepareControlHierarchy()
		*/
		
		protected override void AddParsedSubObject(object o)
		{
			// Preventing literal controls from being added as children: Do nothing here.
		}
		
		protected override void CreateChildControls()
		{
			Controls.Clear();
			if(ViewState["_!ItemCount"]!=null)
			{
				CreateControlHierarchy(true);
				ClearChildViewState();
			}
		}

		//protected override void 

		protected abstract void CreateControlHierarchy(bool useDataSource);
	}
}
