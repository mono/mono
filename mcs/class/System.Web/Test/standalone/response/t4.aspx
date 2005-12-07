<% @Page Language="C#" %>
<%
	Response.Output.Write ("hello");
	Response.Flush ();
	Response.Buffer = false;
	Response.Output.Write ("world");
	Response.Buffer = true;
	Response.Output.Write ("oops");
	Response.Flush ();
%>