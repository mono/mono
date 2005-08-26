<%@ Page language="c#" %>
<html>
<script runat="server">
	void Page_Load ()
	{
		try {
			for (int i = 1; i <= 40; i++) {
				System.Threading.Thread.Sleep (1000);
				Console.WriteLine (i);
			}
		} catch (System.Threading.ThreadAbortException) {
			Console.WriteLine ("Aborted! :-)");
		}
	}
</script>
<body>
This should not be seen.
</body>
</html>
