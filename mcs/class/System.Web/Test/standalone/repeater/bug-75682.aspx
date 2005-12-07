<!-- the bug report states this works in xsp but not in xsp2 -->
<%@ Page Language="C#" %>

<script language="C#" runat="server">
		protected override void OnInit(EventArgs e)
        {
            Load += new EventHandler(Page_Load);
        }


        private void Page_Load(object sender, EventArgs e)
        {
            ArrayList al = new ArrayList();
            al.Add("One");
            al.Add("Two");
            al.Add("Three");
            al.Add("Four");
            al.Add("Five");

            repeater.ItemDataBound += new RepeaterItemEventHandler(OnBound);
            repeater.DataSource = al;
            repeater.DataBind();
        }


        private void OnBound(object sender, RepeaterItemEventArgs e)
        {
            if(e.Item.ItemType == ListItemType.Item ||
               e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Literal lit = (Literal) e.Item.FindControl("lit");
                lit.Text = e.Item.DataItem.ToString();

                ArrayList al = new ArrayList();
                al.Add("A");
                al.Add("B");
                al.Add("C");

                Repeater nestRepeater = (Repeater)
e.Item.FindControl("nestRepeater");

                nestRepeater.ItemDataBound += new
RepeaterItemEventHandler(OnNestBound);
                nestRepeater.DataSource = al;
                nestRepeater.DataBind();
            }
        }


        private void OnNestBound(object sender, RepeaterItemEventArgs e)
        {
            if(e.Item.ItemType == ListItemType.Item ||
               e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Literal lit = (Literal) e.Item.FindControl("lit");
                lit.Text = e.Item.DataItem.ToString();
            }
        }
</script>


<html>
<body>


Results:

<p/>

<asp:Repeater id="repeater" runat="server">
	<HeaderTemplate>
  		<ul>
	</HeaderTemplate>

	<ItemTemplate>
 		<li><asp:Literal id="lit" runat="server" />
			<asp:Repeater id="nestRepeater" runat="server">
 				<HeaderTemplate>
					<ul>
				</HeaderTemplate>
				<ItemTemplate>
				 	<li><asp:Literal id="lit" runat="server" /></li>
				</ItemTemplate>
				<FooterTemplate>
					</ul>
				</FooterTemplate>
			</asp:Repeater>
    	</li>
	</ItemTemplate>

	<FooterTemplate>
		</ul>
	</FooterTemplate>
</asp:Repeater>

</body>
</html>


