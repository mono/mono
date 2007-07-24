<!-- <Snippet8> -->
<%@ Control Language="C#" ClassName="WebUserControl" %>
<script runat="server">
    public event EventHandler InnerClick
    {
        add
        {
            DateSelectedButton.Click += value;
        }
        remove
        {
            DateSelectedButton.Click -= value;
        }
    }

    public int Range
    {
        get {
            if (ViewState["Range"] != null)
                return (Int32)ViewState["Range"];
            else
                return -1;
            }
        set { ViewState["Range"] = value; }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        if (ViewState["endDate"] != null)
        {
            SelectDates(DateTime.Today, (DateTime)ViewState["endDate"]);
            Label2.Text = Calendar1.SelectedDates[Calendar1.SelectedDates.Count-1].ToShortDateString();
        }
        else
        {
            Calendar1.SelectedDate = DateTime.Today;
        }
        Label1.Text = DateTime.Today.ToShortDateString();
    }

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        DateTime startDate = DateTime.Today;
        DateTime endDate = Calendar1.SelectedDate;
        if (endDate < startDate)
        {
            Calendar1.SelectedDate = startDate;
            Label2.Text = "Select an end date greater than today.";
            return;
        }
        SelectDates(startDate, endDate);
        ViewState["endDate"] = endDate;
    }
    private void SelectDates(DateTime startDate, DateTime endDate)
    {
        Label2.Text = endDate.ToShortDateString();
        SelectedDatesCollection selectedDates = Calendar1.SelectedDates;
        selectedDates.Clear();
        DateTime dt = startDate;
        do
        {
            selectedDates.Add(dt);
            dt = dt.Add(new TimeSpan(1, 0, 0, 0));

        } while (dt <= endDate);
        this.Range = Calendar1.SelectedDates.Count;

    }
    protected void Calendar1_VisibleMonthChanged(object sender, MonthChangedEventArgs e)
    {
        if (ViewState["endDate"] != null)
            SelectDates(DateTime.Today, (DateTime)ViewState["endDate"]);

    }

</script>

<asp:Calendar ID="Calendar1" runat="server" OnSelectionChanged="Calendar1_SelectionChanged"
    OnVisibleMonthChanged="Calendar1_VisibleMonthChanged"></asp:Calendar>
Start date:
<asp:Label ID="Label1" runat="server" />
<br />
End date:
<asp:Label ID="Label2" runat="server">...select...</asp:Label>
<br />
<asp:Button ID="DateSelectedButton" runat="server" Text="Done" />
<br />
<!-- </Snippet8> -->
