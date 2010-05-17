<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" EnableViewState="false" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Linq" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:ListView ID="ListViewTest2" DataSourceID="ObjectDataSource1" runat="server" >
        <ItemTemplate><%# Container.DataItem %></ItemTemplate>
        <LayoutTemplate>
        <div runat="server" id="itemPlaceHolder"></div>
        </LayoutTemplate>
        </asp:ListView>
        <asp:DataPager runat="server" ID="DataPager1" PagedControlID="ListViewTest2">
			<Fields>
				<asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="true"
					ShowLastPageButton="false" ShowNextPageButton="false" ShowPreviousPageButton="true"/>
				<asp:NumericPagerField ButtonCount="5" />
				<asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="false"
					ShowLastPageButton="true" ShowNextPageButton="true" ShowPreviousPageButton="false" />
			</Fields>
		</asp:DataPager>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" EnablePaging="True"
            SelectMethod="GetPagedData" SelectCountMethod="GetCount">
        </asp:ObjectDataSource>	
        <br /><div>
        DataPager.TotalRowCount = <%=DataPager1.TotalRowCount%><br />
        Actual TotalRowCount = <%=this.GetCount()%></div><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
    </div>
    <script runat="server" type="text/C#">
		protected void Page_Load(object sender, EventArgs e)
		{
			ObjectDataSource1.TypeName = sender.GetType().AssemblyQualifiedName;
		}

		public List<int> GetPagedData(int startRowIndex, int maximumRows)
		{
			return GetAllData().Skip(startRowIndex).Take(maximumRows).ToList();
		}

		public int GetCount()
		{
			return GetAllData().Length;
		}

		public int[] GetAllData()
		{
			return new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
		}
  	</script>
	</form>
</body>
</html>
