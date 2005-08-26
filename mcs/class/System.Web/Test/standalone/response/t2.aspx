<% @Page Language="C#" %>
<%
	//
	// This test sets buffering to false and writes
	// which should trigger a chunked response
	//
	
	Response.Buffer = false;
	Response.Output.Write ("hello");
%>