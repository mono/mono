using System.Windows.Forms;
using System.Data;
static class Program
{
	static void Main()
	{
		Application.Run(new Frm());
	}
}

class Frm : Form
{
	public Frm()
	{
		DataTable table = new DataTable();
		DataColumn column1 = new DataColumn();
		DataGrid grid = new DataGrid();
		table.Rows.Add();
		grid.Dock = DockStyle.Fill;
		table.Columns.Add(column1);
		Controls.Add(grid);

		DataGridTableStyle tableStyle = new DataGridTableStyle();
		DataGridColumnStyle colStyle = new DataGridTextBoxColumn();
		// colStyle.MappingName = "Column1";
		tableStyle.GridColumnStyles.Add(colStyle);
		grid.TableStyles.Add(tableStyle);
		grid.DataSource = table;
		table.Columns.Add("another hidden column");
	}
}



