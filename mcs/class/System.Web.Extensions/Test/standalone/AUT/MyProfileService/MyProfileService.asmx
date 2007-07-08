<%@ WebService Language="C#" Class="MyProfileService" %>

using System.Web.Services;
using System.Collections.Generic;
using System.Web.Script.Services;

[ScriptService]
public class MyProfileService  : System.Web.Services.WebService 
{

    // Returns a dictionary containing a name-value
    // pair for all profile properties enabled for
    // read access found on the current users profile.
    [WebMethod]
    public IDictionary<string, object> GetAllPropertiesForCurrentUser() 
    {
        //Place code here.
        return null;
    }

    // Given an array of one or more property names,
    // returns a dictionary containing a name-value pair
    // for each corresponding property found on the current
    // users profile that are enabled for read access.
    [WebMethod]
    public IDictionary<string, object> GetPropertiesForCurrentUser(string[] properties)
    {
        //Place code here.
        return null;
    }

    // Given a dictionary with one or more name-value pairs,
    // sets the values onto the corresponding properties of
    // the current users profile. The method returns the count 
    // of properties that were able to be updated.
    [WebMethod]
    public int SetPropertiesForCurrentUser(IDictionary<string, object> values)
    {
        //Place code here.
        return 0;
    }
    
}
