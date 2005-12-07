<!-- bug 51487 -->
<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
 
 <html>
 <script language="C#" runat="server">
 
    ICollection CreateDataSource() 
    {
       DataTable dt = new DataTable();
       DataRow dr;
 
       dt.Columns.Add(new DataColumn("IntegerValue", typeof(Int32)));
       dt.Columns.Add(new DataColumn("StringValue", typeof(string)));
       dt.Columns.Add(new DataColumn("DateTimeValue", typeof(string)));
       dt.Columns.Add(new DataColumn("BoolValue", typeof(bool)));
  
       DataView dv = new DataView(dt);
       return dv;
    }
 
    void Page_Load(Object sender, EventArgs e) 
    {
		
      BindGrid();
    }
 
    void MyDataGrid_Page(Object sender, DataGridPageChangedEventArgs e) 
    {
       MyDataGrid.CurrentPageIndex = e.NewPageIndex;
       BindGrid();
    }
 
    void BindGrid() 
    {
       MyDataGrid.DataSource = CreateDataSource();
       MyDataGrid.DataBind();
       ShowStats();
    }
 
    void ShowStats() 
    {
       lblEnabled.Text = "AllowPaging is " + MyDataGrid.AllowPaging;
       lblCurrentIndex.Text = "CurrentPageIndex is " + MyDataGrid.CurrentPageIndex;
       lblPageCount.Text = "PageCount is " + MyDataGrid.PageCount;
       lblPageSize.Text = "PageSize is " + MyDataGrid.PageSize;
    }
 
 
 </script>
 
 <body>
 
    <h3>DataGrid Paging Example</h3>
 
    <form runat=server>
 
       <asp:DataGrid id="MyDataGrid" runat="server"        
			AllowPaging="True"
            PageSize="10"
            OnPageIndexChanged="MyDataGrid_Page"
            BorderColor="black"
            BorderWidth="1"
            GridLines="Both"
            CellPadding="3"
            CellSpacing="0"
            Font-Name="Verdana"
            Font-Size="8pt">

            <PagerStyle Mode="NumericPages"
                        HorizontalAlign="Right" />

       </asp:DataGrid>
  
       <table bgcolor="#eeeeee" cellpadding="6">
          <tr>
             <td nowrap>
                
 
                   <asp:Label id="lblEnabled" 
                        runat="server"/><br>
                   <asp:Label id="lblCurrentIndex" 
                        runat="server"/><br>
                   <asp:Label id="lblPageCount" 
                        runat="server"/><br>
                   <asp:Label id="lblPageSize" 
                        runat="server"/><br>
 
                
             </td>
          </tr>
       </table>
 
    </form>
 
 </body>
 </html>
   
