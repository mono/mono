using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;


public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DataSet ds = new DataSet();
        ds.Tables.Add(GetTable());
       
        GridView1.DataSource = ds.Tables[0].DefaultView;
        GridView1.UseAccessibleHeader = true;

        
        GridView1.DataBind();
        GridView1.HeaderRow.TableSection = TableRowSection.TableHeader;

        GridView1.FooterRow.TableSection = TableRowSection.TableFooter;


        
    }

    public DataTable GetTable()
    {
        //
        // Here we create a DataTable with four columns.
        //
        DataTable table = new DataTable();
        table.Columns.Add("Dosage", typeof(int));
        table.Columns.Add("Drug", typeof(string));
        table.Columns.Add("Patient", typeof(string));
        table.Columns.Add("Date", typeof(string));

        //
        // Here we add five DataRows.
        //
        DateTime dt = new DateTime (2009, 11, 13);
        table.Rows.Add(25, "Indocin", "David", "2009-11-13");
        table.Rows.Add(50, "Enebrel", "Sam", "2009-11-13");
        table.Rows.Add(10, "Hydralazine", "Christoff", "2009-11-13");
        table.Rows.Add(21, "Combivent", "Janet", "2009-11-13");
        table.Rows.Add(100, "Dilantin", "Melanie", "2009-11-13");
        return table;
    }

}
