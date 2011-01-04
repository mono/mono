<%@ Page Language="C#" AutoEventWireup="true" CodeFile="StateFormatter_CollectionConverter.aspx.cs" Culture="en-US" UICulture="en-US" Inherits="Sections_ECCN_test" Title="ECCN Finder" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server"></head>
<body>
    <form id="form1" runat="server">
<asp:ObjectDataSource runat="server" ID="odsYear" 
    TypeName="App.Test.ECCN" 
    SelectMethod="GetECCNYearList" />
<asp:ObjectDataSource runat="server" ID="odsECCNSummary"
    TypeName="App.Test.ECCN"
    SelectMethod="GetECCNSummaryWithFilter"
    SelectCountMethod="GetECCNSummaryCountWithFilter"
    EnablePaging="true">
        <SelectParameters>
             <asp:ControlParameter ControlID="ddlDate" Name="year" Type="Int32" />
             <asp:ControlParameter ControlID="txtSearchValue" Name="eccn" Type="String" />
        </SelectParameters>
</asp:ObjectDataSource>
<asp:Panel runat="server" ID="pnlModule">   
    <table cellpadding="3" cellspacing="0" border="0">
        <tr>
            <td width="1px"></td>
            <td></td>
        </tr>
        <tr>
            <td></td>
            <td valign="top">
                    <table cellpadding="1" cellspacing="0" width="355px" border="0">
                        <tr>
                            <td colspan="3" style="height:12px;"></td>
                        </tr>
                        <tr>
                            <td>Date Filter</td>
                            <td colspan="2">Enter a Schedule B or ECCN Code:</td>
                        </tr>
                        <tr>
                            <td><asp:DropDownList runat="server" ID="ddlDate" CssClass="ddlECCN" DataSourceID="odsYear" DataTextField="yearText" DataValueField="year"></asp:DropDownList></td>
                            <td style="width:190px;"><asp:TextBox runat="server" ID="txtSearchValue" MaxLength="14" Width="180px" /></td>
                            <td align="left"><asp:Button runat="server" ID="btnSearch" Text="Search" OnClick="btnSearch_Click" /></td>
                        </tr>
                        <tr>
                            <td colspan="3" style="height:3px;"></td>
                        </tr>
                    </table>
                        <table cellpadding="0" cellspacing="0" border="0" >
                            <tr>
                                <td>
                                    <div style="margin: 5px;">
                                        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:GridView ID="gvECCN" runat="server" DataSourceID="odsECCNSummary" 
                                                    AutoGenerateColumns="false" AllowPaging="true" Visible="false" PageSize="10" 
                                                    PagerStyle-HorizontalAlign="Center" >
                                            <Columns>
                                                <asp:BoundField DataField="rownum" HeaderText="" SortExpression="rownum">
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                    <ItemStyle Width="30px" Height="18" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="sched_b" HeaderText="Schedule B" SortExpression="sched_b" >
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                    <ItemStyle Width="140px" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="count" HeaderText="Count" SortExpression="count" DataFormatString="{0:###,###,###}">
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                    <ItemStyle Width="90px" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="total" HeaderText="Total" SortExpression="total" DataFormatString="{0:###,###,###}">
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                    <ItemStyle Width="100px" />
                                                </asp:BoundField>
                                                <asp:BoundField DataField="percent" HeaderText="Percent" SortExpression="percent" DataFormatString="{0:P}">
                                                    <HeaderStyle HorizontalAlign="Left" />
                                                    <ItemStyle Width="90px" />
                                                </asp:BoundField>
                                            </Columns>
                                        </asp:GridView><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
                                    </div>
                                </td>
                             </tr>
                        </table>
            </td>
        </tr>
        <tr>
            <td colspan="2" height="15px"></td>
        </tr>
    </table>
</asp:Panel>
 </form> 
</body>
</html>
