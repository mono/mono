<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
 "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    private string saveDir = @"Uploads\";
    
    protected void UploadButton_Click(object sender, EventArgs e)
    {
        if (FileUpload1.HasFile && FileUpload1.FileBytes.Length < 10000 &&
            !CheckForFileName())
        {
            string savePath = Request.PhysicalApplicationPath + saveDir +
                Server.HtmlEncode(FileName.Text);
            //Remove comment from the next line to upload file.
            //FileUpload1.SaveAs(savePath);
            UploadStatusLabel.Text = "The file was processed successfully.";
        }
        else
        {
            UploadStatusLabel.Text = "You did not specify a file to upload, or a file name, or the file was too large. Please try again.";
        }
    }

    protected void CheckButton_Click(object sender, EventArgs e)
    {
        if (FileName.Text.Length > 0)
        {
            string s = CheckForFileName() ? "exists already." : "does not exist.";
            UploadStatusLabel.Text = "The file name choosen " + s;
        }
        else
        {
            UploadStatusLabel.Text = "Specify a file name to check.";
        }
    }
    private Boolean CheckForFileName()
    {
        System.IO.FileInfo fi = new System.IO.FileInfo(Request.PhysicalApplicationPath + 
            saveDir + Server.HtmlEncode(FileName.Text));
            return fi.Exists;
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>PostBackTrigger Example</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    The upload button is defined as a PostBackTrigger.<br/>
    <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
    <fieldset>
    <legend>FileUpload in an UpdatePanel</legend>
       First, enter a file name to upload your file to: 
       <asp:TextBox ID="FileName" runat="server" />
       <asp:Button ID="CheckButton" Text="Check" runat="server" OnClick="CheckButton_Click" />
       <br />
       Then, browse and find the file to upload:
       <asp:FileUpload id="FileUpload1"                 
           runat="server">
       </asp:FileUpload>
       <br />
       <asp:Button id="UploadButton" 
           Text="Upload file"
           OnClick="UploadButton_Click"
           runat="server">
       </asp:Button>    
       <br />
       <asp:Label id="UploadStatusLabel"
           runat="server" style="color:red;">
       </asp:Label>           
    </fieldset>
    </ContentTemplate>
    <Triggers>
    <asp:PostBackTrigger ControlID="UploadButton" />
    </Triggers>
    </asp:UpdatePanel>
    </div>
    </form>
</body>
</html>
