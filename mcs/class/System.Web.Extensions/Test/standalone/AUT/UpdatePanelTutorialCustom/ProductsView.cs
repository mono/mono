using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Drawing;

namespace UpdatePanelTutorialCustom.CS
{
    public class ProductsView : CompositeControl
    {
        private int _pageSize;
        private ArrayList _cart;
        private static readonly object EventRowCommand = new object();

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public ArrayList Cart
        {
            get { return _cart; }
        }

        protected override void CreateChildControls() {
            base.CreateChildControls();

            Control parent;
            Control container;

            // Get a reference to the ScriptManager object for the page
            // if one exists.
            ScriptManager sm = ScriptManager.GetCurrent(Page);

            if (sm == null || !sm.EnablePartialRendering)
            {
                // If partial rendering is not enabled, set the parent
                // and container as a basic control. 
                container = new Control();
                parent = container;
            }
            else
            {
                // If partial rendering is enabled, set the parent as
                // a new UpdatePanel object and the container to the 
                // content template of the UpdatePanel object.
                UpdatePanel up = new UpdatePanel();
                container = up.ContentTemplateContainer;
                parent = up;
            }

            AddDataboundControls(container);

            Controls.Add(parent);
        }

        private void GridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string productID;
            
            if (e.CommandName == "AddToCart")
            {
                productID = ((GridView)sender).DataKeys[Convert.ToInt32(e.CommandArgument)].Value.ToString();
                if (_cart == null) { GetCart(); }
                if (_cart.IndexOf(productID) == -1)
                    _cart.Add(productID);
                ViewState["Cart"] = _cart;
            }

            if (e.CommandName == "RemoveFromCart")
            {
                productID = ((GridView)sender).DataKeys[Convert.ToInt32(e.CommandArgument)].Value.ToString();
                if (_cart == null) { GetCart(); }
                _cart.Remove(productID);
                ViewState["Cart"] = _cart;
            }

            this.OnRowCommand(new EventArgs());
        }

        private void GetCart()
        {
            if (ViewState["Cart"] == null)
                _cart = new ArrayList();
            else
                _cart = (ArrayList)ViewState["Cart"];
        }

        public event EventHandler RowCommand
        {
            add { Events.AddHandler(EventRowCommand, value); }
            remove { Events.RemoveHandler(EventRowCommand, value); }
        }

        protected virtual void OnRowCommand(EventArgs e)
        {
            EventHandler handler = (EventHandler)Events[EventRowCommand];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void AddDataboundControls(Control parent)
        {
            SqlDataSource ds = new SqlDataSource();
            ds.ID = "ProductsSqlDataSource";
            ds.ConnectionString = 
              ConfigurationManager.ConnectionStrings["AdventureWorksConnectionString"].ConnectionString;
            ds.SelectCommand =
              "SELECT Production.ProductDescription.Description, Production.Product.Name, Production.ProductPhoto.ThumbnailPhotoFileName, " +
              "Production.Product.ProductID " +
              "FROM Production.Product INNER JOIN " +
              "Production.ProductProductPhoto ON Production.Product.ProductID = Production.ProductProductPhoto.ProductID INNER JOIN " +
              "Production.ProductPhoto ON Production.ProductProductPhoto.ProductPhotoID = Production.ProductPhoto.ProductPhotoID INNER JOIN " +
              "Production.ProductModelProductDescriptionCulture ON  " +
              "Production.Product.ProductModelID = Production.ProductModelProductDescriptionCulture.ProductModelID INNER JOIN " +
              "Production.ProductDescription ON  " +
              "Production.ProductModelProductDescriptionCulture.ProductDescriptionID = Production.ProductDescription.ProductDescriptionID";

            GridView gv = new GridView();
            gv.ID = "ProductsGridView";
            gv.DataSourceID = ds.ID;
            gv.AutoGenerateColumns = false;
            gv.DataKeyNames = new string[] { "ProductID" };
            gv.GridLines = GridLines.None;
            gv.HeaderStyle.BackColor = Color.LightGray;
            gv.AlternatingRowStyle.BackColor = Color.LightBlue;
            gv.AllowPaging = true;
            gv.PageSize = _pageSize;
            gv.RowCommand += this.GridView_RowCommand;
           
            ImageField if1 = new ImageField();
            if1.HeaderText = "Product";
            if1.DataImageUrlField = "ThumbnailPhotoFileName";
            if1.DataImageUrlFormatString = @"..\images\thumbnails\{0}";

            BoundField bf1 = new BoundField();
            bf1.HeaderText = "Name";
            bf1.DataField = "Name";

            BoundField bf2 = new BoundField();
            bf2.HeaderText = "Description";
            bf2.DataField = "Description";

            ButtonField btf1 = new ButtonField();
            btf1.Text = "Add";
            btf1.CommandName = "AddToCart";

            ButtonField btf2 = new ButtonField();
            btf2.Text = "Remove";
            btf2.CommandName = "RemoveFromCart";

            gv.Columns.Add(if1);
            gv.Columns.Add(bf1);
            gv.Columns.Add(bf2);
            gv.Columns.Add(btf1);
            gv.Columns.Add(btf2);

            parent.Controls.Add(new LiteralControl("<br />"));
            parent.Controls.Add(gv);
            parent.Controls.Add(new LiteralControl("<br />"));
            parent.Controls.Add(ds);
        }
    }
}
