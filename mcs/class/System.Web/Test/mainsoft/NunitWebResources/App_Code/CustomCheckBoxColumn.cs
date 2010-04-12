using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tests
{
	public class CustomCheckBoxColumn : CheckBoxField
	{
		string caseId;

		public CustomCheckBoxColumn (string id)
		{
			this.caseId = id;
		}

		protected override void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
		{
			switch (caseId) {
				default:
				case "0":
					Case0 (cell);
				break;
    			
				case "1":
					Case1 (cell);
					break;
    			
				case "2":
					Case2 (cell);
					break;
    			
				case "3":
					Case3 (cell);
					break;
    		
				case "4":
					Case4 (cell);
					break;
    		
				case "5":
					Case5 (cell);
					break;
    		
				case "6":
					Case6 (cell);
					break;
    		
				case "7":
					Case7 (cell);
					break;
			}
		}
    	
		void Case0 (DataControlFieldCell cell)
		{
			CheckBox checkBox = new CheckBox();
			checkBox.ToolTip = "Dummy";
			cell.Controls.Add(checkBox);
			checkBox.DataBinding += OnDataBindField;
		}
        
		void Case1 (DataControlFieldCell cell)
		{
			ListBox lb = new ListBox ();
			cell.Controls.Add(lb);
			Case0 (cell);
		}
        
		void Case2 (DataControlFieldCell cell)
		{
			cell.Controls.Add(new CheckBox ());
			Case0 (cell);
			cell.Controls.Add(new CheckBox ());
		}
        
		void Case3 (DataControlFieldCell cell)
		{
			Content content = new Content ();
    	    
			CheckBox checkBox = new CheckBox();
			checkBox.ToolTip = "Dummy";
			content.Controls.Add(checkBox);
			checkBox.DataBinding += OnDataBindField;
            
			cell.Controls.Add (content);
		}
        
		void Case4 (DataControlFieldCell cell)
		{
			CheckBox checkBox = new CheckBox();
			checkBox.ToolTip = "Dummy";
			cell.Controls.Add(checkBox);
            
			ListBox lb = new ListBox ();
			lb.DataBinding += OnDataBindField;
			cell.Controls.Add(lb);
		}
        
		void Case5 (DataControlFieldCell cell)
		{
			cell.Controls.Add (new ListBox ());
		}
    	
		void Case6 (DataControlFieldCell cell)
		{
			cell.Controls.Add (new ListBox ());
			cell.Controls.Add (new CheckBox ());
		}
    	
		void Case7 (DataControlFieldCell cell)
		{
			cell.Controls.Add (new CheckBox ());
			cell.Controls.Add (new ListBox ());
		}
	}
}

