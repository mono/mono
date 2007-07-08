
<%@ Page Language="C#" %>
<%@ Import Namespace="System.Collections.Generic" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
  "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    protected void NewsClick_Handler(object sender, EventArgs e)
    {
        System.Threading.Thread.Sleep(6000);
        HeadlineList.DataSource = GetHeadlines();
        HeadlineList.DataBind();
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            HeadlineList.DataSource = GetHeadlines();
            HeadlineList.DataBind();
        }
    }

    // Helper methods to simulate news headline fetch.
    private SortedList GetHeadlines()
    {
        SortedList allheadlines = new SortedList();
        allheadlines.Add(1, "Lorem ipsum dolor sit amet, consectetuer adipiscing elit.");
        allheadlines.Add(2, "Nam consectetuer metus ut arcu.");
        allheadlines.Add(3, "Integer sodales tempor orci.");
        allheadlines.Add(4, "Donec posuere laoreet leo.");
        allheadlines.Add(5, "In sollicitudin turpis ut eros.");
        allheadlines.Add(6, "Proin a magna quis dolor lacinia sagittis.");
        allheadlines.Add(7, "Maecenas tristique velit quis libero.");
        allheadlines.Add(8, "Integer facilisis faucibus mi.");
        List<int> list = GetRandomList(5, 8);
        SortedList selectedHeadlines = new SortedList();
        foreach (int i in list)
        {
            selectedHeadlines.Add(i, allheadlines[i]);
        }
        return selectedHeadlines;
        
    }
    private List<int> GetRandomList(int n, int max)
    {
        List<int> list = new List<int>();
        Random r = new Random();
        for (int i = 0; i < n; i++)
        {
            int curr = r.Next(1, max);
            while (list.Contains(curr))
            {
                curr = r.Next(1, max);
            }
            list.Add(curr);
        }
        return list;
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PageRequestManager initializeRequest Example</title>
    <style type="text/css">
    body {
        font-family: Tahoma;
    }
    a  {
        text-decoration: none;
    }
    a:hover {
        text-decoration: underline;
    }
    div.NewsPanelStyle{
       width: 300px;
       height: 300px;
    }
    div.AlertStyle {
      font-size: smaller;
      background-color: #FFC080;
      height: 20px;
      width: 300px;
      visibility: hidden;
    }
    div.NewsContainer {
      display: inline;
      float: left;
      width: 330px;
      height: 300px;
    }
	</style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <script type="text/javascript" language="javascript">
                var divElem = 'AlertDiv';
                var messageElem = 'AlertMessage';
                Sys.WebForms.PageRequestManager.getInstance().add_initializeRequest(CheckStatus);
                function CheckStatus(sender, args)
                {
                  var prm = Sys.WebForms.PageRequestManager.getInstance();
                  if (prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'CancelRefresh') {
                     prm.abortPostBack();
                  }
                  else if (prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'RefreshButton') {
                     args.set_cancel(true);
                     ActivateAlertDiv('visible', 'Still working on previous request.');
                 }
                  else if (!prm.get_isInAsyncPostBack() & args.get_postBackElement().id == 'RefreshButton') {
                     ActivateAlertDiv('visible', 'Retrieving headlines.');
                  }
                }
                function ActivateAlertDiv(visString, msg)
                {
                     var adiv = $get(divElem);
                     var aspan = $get(messageElem);
                     adiv.style.visibility = visString;
                     aspan.innerHTML = msg;
                }
            </script>
            <div class="NewsContainer" >
            <asp:UpdatePanel  ID="UpdatePanel1" UpdateMode="Conditional" runat="Server" >
                <ContentTemplate>
                    <asp:Panel ID="Panel1" runat="server" GroupingText="News Headlines" CssClass="NewsPanelStyle">
                        <br />
                        <asp:DataList ID="HeadlineList" runat="server">
                            <ItemTemplate>
                                 <a href="#"><%# Eval("Value") %></a>
                            </ItemTemplate>
                            <FooterTemplate>
                            </FooterTemplate>
                            <FooterStyle HorizontalAlign="right" />
                        </asp:DataList>
                        <p style="text-align:right">
                        <asp:Button runat="server" ID="RefreshButton" Text="Refresh"
                            OnClick="NewsClick_Handler" />
                        </p>
                        <div id="AlertDiv" class="AlertStyle">
                        <span id="AlertMessage"></span> &nbsp;&nbsp;&nbsp;&nbsp;
                        <asp:LinkButton ID="CancelRefresh" runat="server">cancel</asp:LinkButton>
                        </div>                        
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
            </div>
            Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Nam risus erat, congue
            id, cursus et, rutrum sed, nulla. Nulla tincidunt accumsan lorem. Nam consequat,
            ligula vitae aliquet lobortis, magna odio nonummy turpis, et elementum massa magna
            et sapien. Donec urna justo, pulvinar bibendum, condimentum quis, condimentum non,
            libero. Vestibulum placerat tempor ante. Fusce quis erat in eros dapibus egestas.
            Integer ac mi sed libero laoreet pretium. Pellentesque habitant morbi tristique
            senectus et netus et malesuada fames ac turpis egestas. Aliquam tempor velit in
            odio. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos
            hymenaeos. Fusce aliquet metus. Nunc luctus hendrerit pede. Sed vitae sapien nec
            diam accumsan volutpat. Nunc vitae diam et elit porta ultricies. Aliquam erat volutpat.
            Donec nec odio id lorem condimentum sollicitudin. Cras sodales risus nec nibh. Sed
            diam elit, porttitor vitae, luctus ac, sollicitudin in, magna. Mauris nonummy venenatis
            tellus. Morbi sed libero. Donec pellentesque commodo leo. Nullam tempor est eget
            tortor. Phasellus tincidunt velit nec massa. Donec et dui sed lectus malesuada adipiscing.
            In molestie dui nec ante sagittis aliquam. Curabitur sem velit, bibendum eget, dapibus
            vitae, malesuada eu, nisi. Cras tortor sapien, tincidunt at, sagittis non, elementum
            sed, turpis. Aliquam pretium nibh in sapien. Duis urna lectus, auctor lobortis,
            rutrum eget, tempor porta, lorem. In hac habitasse platea dictumst. Pellentesque
            lacinia orci aliquam sem. Ut aliquet. Nunc sollicitudin, quam ac tristique interdum,
            metus pede blandit orci, vel ornare est risus nec diam. Sed dictum. Integer aliquam
            nisl sed tortor. In volutpat dolor. Cum sociis natoque penatibus et magnis dis parturient
            montes, nascetur ridiculus mus. Donec ut sem dignissim tellus fringilla sodales.
            Sed auctor. Donec nonummy dolor sit amet metus. Curabitur convallis viverra nisl.
            Aliquam erat volutpat. Nulla malesuada sollicitudin dui. Integer facilisis. Praesent
            leo arcu, rhoncus quis, volutpat tempor, molestie quis, purus. Integer pretium ullamcorper
            magna. Donec hendrerit enim non turpis. Nunc vitae justo. Donec semper euismod urna.
            Ut sem quam, ullamcorper a, ultrices dapibus, placerat at, orci. Donec scelerisque,
            enim eget euismod egestas, dui quam placerat mauris, interdum vehicula leo magna
            id ligula. Aliquam et libero. Donec gravida erat eu metus. Nullam vehicula risus
            aliquam ante. Fusce ipsum libero, ullamcorper a, pellentesque ac, tincidunt quis,
            leo. Cras eget ligula. Aenean lectus. Fusce condimentum venenatis urna. Quisque
            porta, eros id ultrices posuere, nisl ante lacinia libero, vel laoreet diam urna
            ac neque. Integer commodo. Maecenas augue erat, sodales non, tristique vel, posuere
            eu, ante. Quisque in odio. Maecenas lorem.
        </div>
    </form>
</body>
</html>
