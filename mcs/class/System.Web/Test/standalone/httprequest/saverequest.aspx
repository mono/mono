<% @Page language="c#" debug="true"%>
<html>
<script runat="server">
	void Page_Load ()
	{
		// If the request is a GET, SaveAs fails with a socket error
		// trying to get the InputStream from the request.
		// XSPWorkerRequest.ReadEntityBody is called too
		if (!IsPostBack)
			return;

		// Files as overwritten if they exist.
		Request.SaveAs ("request.txt", false);
		Request.SaveAs ("request-headers.txt", true);
	}
</script>
<body>
<form runat="server">
<asp:Button Text="click me" runat="server" />
</form>
</body>
</html>
<!--
request.txt:
__VIEWSTATE=dDwtMTk5NjUxNzkxMzs7PmyYDeLoOgzCSqBwtACVA7RAWyBP&_ctl1=click+me

request-headers.txt:
POST /saverequest.aspx?test=t HTTP/1.0
Connection: keep-alive
Keep-Alive: 300
Content-Length: 75
Content-Type: application/x-www-form-urlencoded
Accept: text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7
Accept-Encoding: gzip,deflate
Accept-Language: en-us,en;q=0.5
Host: 127.0.0.1:8080
Referer: http://127.0.0.1:8080/saverequest.aspx?test=t
User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.7.10) Gecko/20050716 Firefox/1.0.6

__VIEWSTATE=dDwtMTk5NjUxNzkxMzs7PmyYDeLoOgzCSqBwtACVA7RAWyBP&_ctl1=click+me
-->
