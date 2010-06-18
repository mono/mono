<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyPage.aspx.cs" Inherits="MyPage" EnableViewState="false"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <%= MonoTests.stand_alone.WebHarness.HtmlDiff.BEGIN_TAG %><asp:ListView ID="ListViewTest" DataSourceID="ObjectDataSource1" runat="server" >
        <ItemTemplate><%# Container.DataItem %> </ItemTemplate>
        <LayoutTemplate>
        <div runat="server" id="itemPlaceHolder"></div>
        </LayoutTemplate>
        </asp:ListView>
        <asp:DataPager runat="server" ID="DataPager1" PagedControlID="ListViewTest">
			<Fields>
				<asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="true"
					ShowLastPageButton="false" ShowNextPageButton="false" ShowPreviousPageButton="true"/>
				<asp:NumericPagerField ButtonCount="5" />
				<asp:NextPreviousPagerField ButtonType="Link" ShowFirstPageButton="false"
					ShowLastPageButton="true" ShowNextPageButton="true" ShowPreviousPageButton="false" />
			</Fields>
		</asp:DataPager><%= MonoTests.stand_alone.WebHarness.HtmlDiff.END_TAG %>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" EnablePaging="True"
            SelectMethod="GetData" SelectCountMethod="GetCount">
        </asp:ObjectDataSource>		
    </div>
    <script runat="server" type="text/C#">
		protected void Page_LoadComplete(object sender, EventArgs e)
		{
			ObjectDataSource1.TypeName = this.GetType().AssemblyQualifiedName;
		}
			public int[] GetData(int startRowIndex, int maximumRows)
			{
				int[] ret = new int[13];
				for (int i = 0; i < 13; i++)
					ret [i] = startRowIndex + i;
				return ret;
			}

			public int GetCount()
			{
				return GetData(0, 13).Length;
			}			
</script>
    </form>
</body>
</html>
