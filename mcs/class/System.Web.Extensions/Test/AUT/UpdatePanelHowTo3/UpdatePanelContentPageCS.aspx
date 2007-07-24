<%@ Page Language="C#" MasterPageFile="MasterPageCS.master"
    Title="ScriptManagerProxy in Content Page" %>
<%@ MasterType VirtualPath="MasterPageCS.master" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        DateTime newDateTime = DateTime.Today.Add(new TimeSpan(Master.Offset, 0, 0, 0));
        Calendar1.SelectedDate = newDateTime;
    }

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        DateTime selectedDate = Calendar1.SelectedDate;
        Master.Offset = 
           ((TimeSpan)Calendar1.SelectedDate.Subtract(DateTime.Today)).Days;

    }
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1"
    runat="Server">
    <asp:Panel ID="Panel1" 
               GroupingText="ContentPage"
               runat="server">
        <asp:UpdatePanel ID="UpdatePanel1" 
                         UpdateMode="Conditional"
                         runat="server">
            <ContentTemplate>
                <asp:Calendar ID="Calendar1"
                              OnSelectionChanged="Calendar1_SelectionChanged"
                              runat="server">
                </asp:Calendar>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</asp:Content>
