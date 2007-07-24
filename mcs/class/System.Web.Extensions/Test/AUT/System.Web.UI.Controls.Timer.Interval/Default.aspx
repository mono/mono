<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Timer Example Page</title>
    <script runat="server">
        protected void Page_Load(object sender, EventArgs e)
        {
            OriginalTime.Text = DateTime.Now.ToLongTimeString();
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            StockPrice.Text = GetStockPrice();
            TimeOfPrice.Text = DateTime.Now.ToLongTimeString();
        }

        private string GetStockPrice()
        {
            double randomStockPrice = 50 + new Random().NextDouble();
            return randomStockPrice.ToString("C");
        }

        protected void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Timer1.Enabled = true;
            Timer1.Interval = 10000;
        }

        protected void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Timer1.Enabled = true;
            Timer1.Interval = 60000;
        }

        protected void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Timer1.Enabled = false;
        }
        
</script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:Timer ID="Timer1" OnTick="Timer1_Tick" runat="server" Interval="10000" />
      
        <asp:UpdatePanel ID="StockPricePanel" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="Timer1" />
        </Triggers>
        <ContentTemplate>
            Stock price is <asp:Label id="StockPrice" runat="server"></asp:Label><BR />
            as of <asp:Label id="TimeOfPrice" runat="server"></asp:Label>  
            <br />
            
        </ContentTemplate>
        </asp:UpdatePanel>
        <div>
        <br />
        Update stock price every:<br />
        <asp:RadioButton ID="RadioButton1" AutoPostBack="true" GroupName="TimerFrequency" runat="server" Text="10 seconds" OnCheckedChanged="RadioButton1_CheckedChanged" /><br />
        <asp:RadioButton ID="RadioButton2" AutoPostBack="true" GroupName="TimerFrequency" runat="server" Text="60 seconds" OnCheckedChanged="RadioButton2_CheckedChanged" /><br />
        <asp:RadioButton ID="RadioButton3" AutoPostBack="true" GroupName="TimerFrequency" runat="server" Text="Never" OnCheckedChanged="RadioButton3_CheckedChanged" />
        <br />
        Page loaded at <asp:Label ID="OriginalTime" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>
