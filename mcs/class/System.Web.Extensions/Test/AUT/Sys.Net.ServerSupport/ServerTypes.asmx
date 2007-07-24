<%@ WebService Language="C#" Class="Samples.AspNet.ServerTypes" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Samples.AspNet
{
   

    // Define the enum type to
    // exchange with the client.
    public enum ColorEnum
    {
        Red     = 0,
        Green   = 1,
        Blue    = 2
    }        
   
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class ServerTypes : 
        System.Web.Services.WebService
    {

        // The enum type can be accessed by the client
        // script through the generated Web service
        // proxy class in a static fashion.
        [WebMethod]
        public ColorEnum GetFirstColor()
        {
            // Return the first color
            // in the enumeration.
            return ColorEnum.Red;
        }

        [WebMethod]
        public string GetSelectedColor(ColorEnum color)
        {
            // Return the selected color value
            // in the enumeration.
            return color.ToString();
        }

    }

}


