using System;
using System.Web.UI.WebControls;

namespace MonoBoundFieldCompatibilityIssue
{
	public partial class _Default : System.Web.UI.Page
	{
		#region [ -- Custom Column Definition -- ]

		/// <summary>
		/// Custom Column for the GridView
		/// </summary>
		class CustomColumn : BoundField
		{
			protected override void InitializeDataCell (DataControlFieldCell cell, DataControlRowState rowState)
			{
				if ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal) {
					TextBox textBox = new TextBox ();
					cell.Controls.Add (textBox);
					textBox.DataBinding += OnDataBindField;
				} else
					base.InitializeDataCell (cell, rowState);
			}
		}

		#endregion

		protected void Page_Load (object sender, EventArgs e)
		{
			if (IsPostBack) return;
			BindGridView ();
		}

		protected void OnGridViewInit (object sender, EventArgs e)
		{
			CustomColumn column = new CustomColumn (); 
			column.DataField = BoundField.ThisExpression;
			gridView.Columns.Add (column);
		}

		protected void OnGridViewRowEditing (object sender, GridViewEditEventArgs e)
		{
			gridView.EditIndex = e.NewEditIndex;
			BindGridView ();
		}

		protected void OnGridViewEditCancelling (object sender, EventArgs e)
		{
			gridView.EditIndex = -1;
			BindGridView ();
		}

		private void BindGridView ()
		{
			gridView.DataSource = new bool [2];
			gridView.DataBind ();
		}
	}
}