<%@ Page Language="C#" AutoEventWireup="true"  %>
<%@ Import Namespace="System.Data" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <script runat="server">
        public class DataObject
        {
            public static DataTable ds = CreateDataTable ();
            public static DataTable Select ()
            {
                return ds;
            }

            public static DataTable CreateDataTable ()
            {
                DataTable aTable = new DataTable ("A");
                DataColumn dtCol;
                DataRow dtRow;

                // Create ID column and add to the DataTable.
                dtCol = new DataColumn ();
                dtCol.DataType = Type.GetType ("System.String");
                dtCol.ColumnName = "ID";
                dtCol.Caption = "ID";
                dtCol.ReadOnly = true;
                dtCol.Unique = true;
                aTable.Columns.Add (dtCol);

                dtRow = aTable.NewRow ();
                dtRow["ID"] = "My databind test";
                aTable.Rows.Add (dtRow);

                aTable.PrimaryKey = new DataColumn[] { aTable.Columns["ID"] };
                return aTable;
            }
        }
        protected override void OnLoad (EventArgs e)
        {
            base.OnLoad (e);
            Repeater1.DataSource = DataObject.Select ();
            Repeater1.DataMember = "A";
            Repeater1.DataBind ();
        }
    </script>
</head>

<body>
    <form id="form1" runat="server">
    <div>
    <asp:Repeater ID="Repeater1"
            runat="server">
            <ItemTemplate>
                <table>
                  <tr>
                    <td><font color="blue"><%# Eval ("ID")%></font></td>
                  </tr>
                </table>
            </ItemTemplate>
        </asp:Repeater>
      </div>
    </form>
</body>
</html>
