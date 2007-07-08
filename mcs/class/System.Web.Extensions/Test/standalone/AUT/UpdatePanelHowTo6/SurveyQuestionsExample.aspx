<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" 
"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected SortedList AnsweredQuestions
    {
        get { return (SortedList)(ViewState["AnsweredQuestions"] ?? new SortedList()); }
        set { ViewState["AnsweredQuestions"] = value; }
    }

    protected void Page_Load()
    {
        ScriptManager1.RegisterAsyncPostBackControl(SurveyDataList);
    }

    protected void ChoicesRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
    {
        SortedList answers = this.AnsweredQuestions;
        RadioButtonList r = (RadioButtonList)sender;
        answers[r.ToolTip] = r.SelectedValue;
        this.AnsweredQuestions = answers;

        ResultsList.DataSource = this.AnsweredQuestions;
        ResultsList.DataBind();

        if (this.AnsweredQuestions.Count == SurveyDataList.Items.Count)
            SubmitButton.Visible = true;

        UpdatePanel1.Update();
    }

    protected void SubmitButton_Click(object sender, EventArgs e)
    {
        // Submit responses.
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Registering Controls as Async Postback Controls</title>
    <style type="text/css">
    .AnswerFloatPanelStyle {
    background-color: bisque;
    position: absolute;
    right: 10px;
    height: 130px;
    width: 150px;
    border-right: silver thin solid; border-top: silver thin solid; 
    border-left: silver thin solid; border-bottom: silver thin solid;    
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <div id="AnswerFloatPanel" class="AnswerFloatPanelStyle" runat="server">
                <asp:UpdatePanel ID="UpdatePanel1" UpdateMode="Conditional" runat="server">
                    <ContentTemplate>
                        Completed Questions:
                        <asp:DataList ID="ResultsList" runat="server">
                            <ItemTemplate>
                                <asp:Label ID="ResultQuestion" runat="server" Text='<%# Eval("Key") %>' />
                                ::
                                <asp:Label ID="ResultAnswer" runat="server" Text='<%# Eval("Value") %>' />
                            </ItemTemplate>
                        </asp:DataList>
                        <p style="text-align: right">
                            <asp:Button ID="SubmitButton" Text="Submit" runat="server" Visible="false"
                                OnClick="SubmitButton_Click" />
                        </p>
                        <asp:Label ID="Message" runat="Server" />
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            
            <asp:XmlDataSource ID="SurveyDataSource" 
                               runat="server" 
                               XPath="/Questions/Question"
                               DataFile="~/App_Data/SurveyQuestions.xml"/>
            <asp:DataList
                ID="SurveyDataList"
                DataSourceID="SurveyDataSource"
                runat="server">

                <ItemTemplate>
                  <table cellpadding="2" cellspacing="2">
                    <tr>
                      <td valign="top">
                        <asp:Label id="QuestionLabel" Text='<%# XPath("@Title")%>' runat="server" />
                      </td>
                    </tr>
                    <tr><td>
                      <asp:RadioButtonList ID="ChoicesRadioButtonList" runat="server" 
                        DataSource='<%#XPathSelect("Choices/Choice") %>'
                        DataTextField="InnerText" DataValueField="InnerText" 
                        AutoPostBack="True"
                        ToolTip='<%# "Question" + XPath("@ID") %>'
                        OnSelectedIndexChanged="ChoicesRadioButtonList_SelectedIndexChanged"/>
                    </td></tr>
                  </table>
                  <hr />
                </ItemTemplate>
            </asp:DataList>
        </div>
    </form>
</body>
</html>
