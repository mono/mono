<!-- bug 50390 -->
<%@ Page Language="C#" %>
<%@ Import Namespace="System.Data" %>
 
<html>
   <script runat="server">
       DataTable Cart = new DataTable();
      DataView CartView;   
 
      void Page_Load(Object sender, EventArgs e) 
      {
          GetSource();
         if (!IsPostBack)
            BindGrid();
      }
 
      void ItemsGrid_Edit(Object sender, DataGridCommandEventArgs e) 
      {
         ItemsGrid.EditItemIndex = e.Item.ItemIndex;
         BindGrid();
      }
 
      void ItemsGrid_Cancel(Object sender, DataGridCommandEventArgs e) 
      {
         ItemsGrid.EditItemIndex = -1;
         BindGrid();
      }
 
      void ItemsGrid_Update(Object sender, DataGridCommandEventArgs e) 
      {
         TextBox qtyText = (TextBox)e.Item.Cells[3].Controls[0];
         TextBox priceText = (TextBox)e.Item.Cells[4].Controls[0];
 
         // Retrieve the updated values.
         String item = e.Item.Cells[2].Text;
         String qty = qtyText.Text;
         String price = priceText.Text;
        
         DataRow dr;
 
         CartView.RowFilter = "Item='" + item + "'";
         if (CartView.Count > 0)
            CartView.Delete(0);
         CartView.RowFilter = "";
 
         dr = Cart.NewRow();
         dr[0] = Convert.ToInt32(qty);
         dr[1] = item;

         if(price[0] == '$')
            dr[2] = Convert.ToDouble(price.Substring(1));
         else
            dr[2] = Convert.ToDouble(price);

         Cart.Rows.Add(dr);
 
         ItemsGrid.EditItemIndex = -1;
         BindGrid();

      }
 
      void BindGrid() 
      {
         ItemsGrid.DataSource = CartView;
         ItemsGrid.DataBind();
      }

      void GetSource()
      {
         if (ViewState["ShoppingCart"] == null) 
         {     
            DataRow dr;  
 
            Cart.Columns.Add(new DataColumn("Qty", typeof(Int32)));
            Cart.Columns.Add(new DataColumn("Item", typeof(String)));
            Cart.Columns.Add(new DataColumn("Price", typeof(Double)));

            ViewState ["ShoppingCart"] = Cart;
             
            // Populate the DataTable with sample data.
            for (int i = 1; i <= 9; i++) 
            {
               dr = Cart.NewRow();
               if (i % 2 != 0)
               {
                  dr[0] = 2;
               }
               else
               {
                  dr[0] = 1;
               }
               dr[1] = "Item " + i.ToString();
               dr[2] = (1.23 * (i + 1));
               Cart.Rows.Add(dr);
            }

         } 

         else
         {

            Cart = (DataTable)ViewState["ShoppingCart"];

         }         
 
         // Create a DataView and specify the field to sort by.
         CartView = new DataView(Cart);
         CartView.Sort="Item";

         return;

      }

      void ItemsGrid_Command(Object sender, DataGridCommandEventArgs e)
      {

         switch(((LinkButton)e.CommandSource).CommandName)
         {

            case "Delete":
               DeleteItem(e);
               break;

            // Add other cases here, if there are multiple ButtonColumns in 
            // the DataGrid control.

            default:
               // Do nothing.
               break;

         }

      }

      void DeleteItem(DataGridCommandEventArgs e)
      {

         // e.Item is the table row where the command is raised. For bound
         // columns, the value is stored in the Text property of a TableCell.
         TableCell itemCell = e.Item.Cells[2];
         string item = itemCell.Text;

         // Remove the selected item from the data source.         
         CartView.RowFilter = "Item='" + item + "'";
         if (CartView.Count > 0) 
         {     
            CartView.Delete(0);
         }
         CartView.RowFilter = "";

         // Rebind the data source to refresh the DataGrid control.
         BindGrid();

      }
 
   </script>
 
<body>
 
   <form runat="server">

      <h3>DataGrid Editing Example</h3>
 
      <asp:DataGrid id="ItemsGrid"
           BorderColor="black"
           BorderWidth="1"
           CellPadding="3"
           OnEditCommand="ItemsGrid_Edit"
           OnCancelCommand="ItemsGrid_Cancel"
           OnUpdateCommand="ItemsGrid_Update"
           OnItemCommand="ItemsGrid_Command"
           AutoGenerateColumns="false"
           runat="server">

         <HeaderStyle BackColor="#aaaadd">
         </HeaderStyle>
 
         <Columns>

            <asp:EditCommandColumn
                 EditText="Edit"
                 CancelText="Cancel"
                 UpdateText="Update" 
                 HeaderText="Edit item">

               <ItemStyle Wrap="False">
               </ItemStyle>

               <HeaderStyle Wrap="False">
               </HeaderStyle>

            </asp:EditCommandColumn>

            <asp:ButtonColumn 
                 HeaderText="Delete item" 
                 ButtonType="LinkButton" 
                 Text="Delete" 
                 CommandName="Delete"/>  
 
            <asp:BoundColumn HeaderText="Item" 
                 ReadOnly="True" 
                 DataField="Item"/>
 
            <asp:BoundColumn HeaderText="Quantity" 
                 DataField="Qty"/>
 
            <asp:BoundColumn HeaderText="Price"
                 DataField="Price"
                 DataFormatString="{0:c}"/>
 
         </Columns>
 
      </asp:DataGrid>

   </form>
 
</body>
</html>
