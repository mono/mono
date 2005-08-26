<% @Page Language="C#" Debug="true" %>
<%
	Response.ContentType = "text/plain";
	Response.Write ("Only 'no-cache', 'public' and 'private' should be OK.\n");
	foreach (string s in new string [] { "no-cache", "no-store", "public", "private", "no-transform", "must-revalidate", "proxy-revalidate", "max-age=10", "s-maxage=20", "cache-extension"} ){

		Response.Write (s);
		try {
			Response.CacheControl = s;
			Response.Output.WriteLine (" OK");
		} catch {
			Response.Output.WriteLine (" Failed");
		}
	}
%>

