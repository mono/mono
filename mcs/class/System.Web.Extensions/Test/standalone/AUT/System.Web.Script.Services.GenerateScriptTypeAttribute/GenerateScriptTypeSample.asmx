<%@ WebService Language="C#" Class="Samples.AspNet.ColorService" %>

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;

namespace Samples.AspNet
{
    
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ScriptService]
    [GenerateScriptType(typeof(FavoriteColors))]
    public class ColorService : System.Web.Services.WebService
    {

        [GenerateScriptType(typeof(ColorObject), 
            ScriptTypeId = "Color")]
        [WebMethod]
        public string[] GetDefaultColor()
        {
            // Instantiate the default color object.
            ColorObject co = new ColorObject();
            
            return co.rgb;
        }
        
        [WebMethod]
        public string EchoDefaultColor()
        {
            // Instantiate the default color object.
            ColorObject co = new ColorObject();

            return co.defaultColor.ToString();
        }
    }
    
    public class ColorObject
    {
        public string[] rgb =
            new string[] { "00", "00", "FF" };
        public FavoriteColors defaultColor = FavoriteColors.Blue;
    }
        
    public enum FavoriteColors
    {
        Black,
        White,
        Blue,
        Red
    }
}
