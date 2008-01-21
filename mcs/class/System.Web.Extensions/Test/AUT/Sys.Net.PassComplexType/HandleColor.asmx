<%@ WebService Language="C#" Class="Sys.Net.PassComplexType.HandleColor" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;

namespace Sys.Net.PassComplexType
{
    // Define the color object to
    // exchange with the client.
    public class ColorObject
    {
        public string message;
        public string[] rgb; 
        public string timeStamp;

        public ColorObject()
        {
            this.message = "The default color is Blue.";
            this.rgb = new string[] { "00", "00", "FF" };
            this.timeStamp = DateTime.Now.ToString();
        }
    }
   
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [GenerateScriptType(typeof(ColorObject))]
    [ScriptService]
    public class HandleColor : 
        System.Web.Services.WebService
    {


        [WebMethod]
        public ColorObject GetDefaultColor()
        {
            // Instantiate the default color object.
            ColorObject co = new ColorObject();
           
            return co;         
        }


        [WebMethod]
        public ColorObject ChangeDefaultColor(ColorObject color)
        {
            // Instantiate the default color object.
            ColorObject co = new ColorObject();
            // Assign the passed values.
            co.message = color.message;
            co.rgb = color.rgb;
            // Set time stamp.
            co.timeStamp = DateTime.Now.ToString();

            return co;
                        
        }

          
    }

}


