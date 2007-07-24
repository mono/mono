<%@ Page Language="C#" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Timer Example Page</title>
    <script runat="server">
        // stockMarketClosing set to 4:00 PM.
        static TimeSpan stockMarketClosing = new TimeSpan(16, 0, 0);
        
        // stockMarketOpening set to 9:00 AM and 1 day ahead.
        static TimeSpan stockMarketOpening = new TimeSpan(1, 9, 0, 0);
        
        protected void Page_Load(object sender, EventArgs e)
        {
            OriginalTime.Text = DateTime.Now.ToLongTimeString();
            
            // Turn off Timer on Saturday and Sunday.
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday
                    || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                Timer1.Enabled = false;
            }
        }

        protected void StockPricePanel_Load(object sender, EventArgs e)
        {
            StockPrice.Text = GetStockPrice();
            TimeOfPrice.Text = DateTime.Now.ToLongTimeString();
        }

        private string GetStockPrice()
        {
            double randomStockPrice = 50 + new Random().NextDouble();
            return randomStockPrice.ToString("C");
        }

        protected void Timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now.TimeOfDay.CompareTo(stockMarketClosing) > 0)
            {
                // Turn off Timer after closing on Friday.
                if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                {
                    Timer1.Enabled = false;
                }
                else
                {
                    // Set next Tick event to tomorrow's opening time.
                    Timer1.Interval = (int)stockMarketOpening.Subtract
                        (DateTime.Now.TimeOfDay).TotalMilliseconds;
                }
            }   
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <asp:Timer ID="Timer1" OnTick="Timer_Tick" runat="server" Interval="15000" />
      
        <asp:UpdatePanel OnLoad="StockPricePanel_Load" ID="StockPricePanel" runat="server" UpdateMode="Conditional">
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="Timer1" />
        </Triggers>
        <ContentTemplate>
            Stock price is <asp:Label id="StockPrice" runat="server"></asp:Label><BR />
            as of <asp:Label id="TimeOfPrice" runat="server"></asp:Label>  
        </ContentTemplate>
        </asp:UpdatePanel>
        <div>
        Page originally created at <asp:Label ID="OriginalTime" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>
