<%@ WebService Language="C#" Class="Sys.Net.ExchangeComplexTypes.HandleColor" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Sys.Net.ExchangeComplexTypes
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
            this.message = "The default color is Red.";
            this.rgb = new string[] { "FF", "00", "00" };
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

        [WebMethod]
        
        public Dictionary<String,String> GetColorList()
        {
            //Instatiate the dictionary object.
            Dictionary<String, String> result = new Dictionary<string, string>();
   
            //Add the color items.
            result.Add("00,00,FF", "Blue");
            result.Add("FF,00,00", "Red");
            result.Add("00,FF,00", "Green");
            result.Add("00,00,00", "Black");
            
            return result;
        }          
    }

}

