
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
    "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">
    void DropDownSelection_Change(Object sender, EventArgs e)
    {
        Calendar1.DayStyle.BackColor =
            System.Drawing.Color.FromName(ColorList.SelectedItem.Value);
    }

    protected void Calendar1_SelectionChanged(object sender, EventArgs e)
    {
        SelectedDate.Text = 
            Calendar1.SelectedDate.ToString();
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>UpdatePanel Example</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" 
                               runat="server" />
            <asp:UpdatePanel ID="UpdatePanel1"
                             runat="server">
                <ContentTemplate>
                    <asp:Calendar ID="Calendar1" 
                                  ShowTitle="True"
                                  OnSelectionChanged="Calendar1_SelectionChanged"
                                  runat="server" />
                    <div>
                        Background:
                        <br />
                        <asp:DropDownList ID="ColorList" 
                                          AutoPostBack="True" 
                                          OnSelectedIndexChanged="DropDownSelection_Change"
                                          runat="server">
                            <asp:ListItem Selected="True" Value="White"> 
                            White </asp:ListItem>
                            <asp:ListItem Value="Silver"> 
                            Silver </asp:ListItem>
                            <asp:ListItem Value="DarkGray"> 
                            Dark Gray </asp:ListItem>
                            <asp:ListItem Value="Khaki"> 
                            Khaki </asp:ListItem>
                            <asp:ListItem Value="DarkKhaki"> D
                            ark Khaki </asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <br />
                    Selected date:
                    <asp:Label ID="SelectedDate" 
                               runat="server">None.</asp:Label>
                </ContentTemplate>
            </asp:UpdatePanel>
            <br />
        </div>
    </form>
</body>
</html>
