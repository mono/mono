<%@ WebService Language="c#" Class="Util.SessionCounter" %>  

 using System;  
 using System.Web;  
 using System.Web.Services;  
 using System.Web.SessionState;  
 namespace Util
 {  
    public class SessionCounter: System.Web.Services.WebService  
    {  
		public SessionCounter ()
		{
			if (Context == null)
				throw new Exception ("Context not set in constructor");
		}
		
         [ WebMethod(EnableSession=true) ]  
         public void Reset()
         {  
            Session["counter"] = 0;
         }
	
         [ WebMethod(EnableSession=true) ]  
         public int AddOne()  
         {  
            if ( Session["counter"] == null )  
            { Session["counter"]=0; }  
            else  
            { Session["counter"]=(int)Session["counter"]+1; }  
   
            return (int)Session["counter"];  
   
          }
    }
  }
