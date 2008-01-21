<%@ WebService Language="C#" Class="Sys.Net.UsingProxyClass.UsingProxyClass" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Sys.Net.UsingProxyClass
{
    // Define the color type to
    // exchange with the client.
    public class ColorObject
    {
        public string message;
        public string[] rgb; 

        public ColorObject()
        {
            this.message = "The default color is Red";
            this.rgb = new string[] { "FF", "00", "00" };
        }
    }

    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    public class UsingProxyClass : 
        System.Web.Services.WebService
    {

        
        // Note, because the ColorObject is the returned type
        // it does not require that you apply
        // the attribute [GenerateScriptType(typeof(ColorObject))]
        // to the service class to allow client script
        // access.
        [WebMethod]
        public ColorObject GetDefaultColor()
        {
            // Instantiate the default color object.
            ColorObject co = new ColorObject();
            return co;
        }

        [WebMethod]
        public ColorObject SetColor(ColorObject color)
        {
            // Instantiate the color object.
            ColorObject co = new ColorObject();
            
            // Assign the passed values.
            co.message = color.message;
            co.rgb = color.rgb;
          
            return co;
        }
    }

}


