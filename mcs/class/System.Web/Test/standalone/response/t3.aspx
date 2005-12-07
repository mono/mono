<% @Page Language="C#" %>
<%
	//
	// This test forces chunked mode by calling flush
	// Then writes 9 bytes in two separate calls, which
	// should be sent as a single chunk
	//
	Response.Output.Write ("hello");
	Response.Flush ();
	Response.Output.Write ("world");
	Response.Output.Write ("oops");
	Response.Flush ();
%>