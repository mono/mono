<% @Page Language="C#" %>
<%	
//	Response.Cache.SetNoServerCaching ();
//	Response.Cache.SetCacheability (HttpCacheability.Server);
//	Response.Write (Response.CacheControl);

	Response.Cache.SetCacheability (HttpCacheability.NoCache);
%>
