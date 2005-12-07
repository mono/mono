<!-- bug 52171. on msft v1.1 with ff this does not work, however it does work on ie.
     on v2 beta 2, it works with both

     the bug report said this was in web control...
-->
<html>
	<body>
		<form runat="server">
			<asp:table runat="server" enabled="false">
				<asp:tablerow>
					<asp:tablecell><asp:checkbox runat="server" text="i should be disabled" /></asp:tablecell>
				</asp:tablerow>
			</asp:table>
		</form>
	</body>
</html>
