<% @Page Language="C#" %>

<%
	//
	// This test sets buffering to false and writes
	// which should trigger a chunked response.
	// 
	// This shuts down the connection at the end.
	//
	
	Response.Buffer = false;
	Response.Output.Write ("hello");
	Response.Close ();
%>
