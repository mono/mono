<%@ Page Language="C#" MasterPageFile="MasterPageCS.master"
    Title="ScriptManagerProxy in Content Page" %>

<script runat="server">

    protected void Page_Init(object sender, EventArgs e)
    {
        ScriptManager.GetCurrent(this.Page).EnablePartialRendering = false;
        ((Button)Page.Master.FindControl("DecrementButton")).Enabled = false;
        ((Button)Page.Master.FindControl("IncrementButton")).Enabled = false;
    }

</script>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1"
    runat="Server">
        <asp:UpdatePanel ID="UpdatePanel1" 
                         UpdateMode="Conditional"
                         runat="server">
        </asp:UpdatePanel>
</asp:Content>
