<%@ WebService Language="C#" Class="Samples.AspNet.TestService" %>

namespace Samples.AspNet
{
    using System;
	using System.Collections;
	using System.Collections.Generic;
    using System.Web.Services;
    using System.Web.Script.Services;

	// Custom type used by WebService.
	public class SimpleClass
	{
        private string _s = "SimpleClass1";
	
       	public String s {
                    get { return _s; }
            		set { _s = value; }
        }
	}

    // Custom type used by WebService.
    public class SimpleClass2
    {
        private string _s = "SimpleClass2";

        public String s
        {
            get { return _s; }
            set { _s = value; }
        }
    }

    // The following service allows the generation
    // of the SimpleClass2 type proxy by applying the
    // attribute: GenerateScriptType. This assures
    // that the a SimpleClass2 object can be 
    // passed by the client script to the 
    // PassGenericDictionary method.
    // Note if you comment out the GenerateScriptType
    // you get an error because the SimpleClass2 type proxy
    // is not generated.
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [GenerateScriptType(typeof(SimpleClass2))]
	[ScriptService]
	public class TestService : System.Web.Services.WebService
	{
        // The following method return a list of SimpleClass
        // objects.
        // Note the SimpleClass type proxy is automatically 
        // generated because used in the return parameter.
		[WebMethod]
		[ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
		public List<SimpleClass> GetGenericList() 
        {
            List<SimpleClass> GetGenericList = new List<SimpleClass>();
            SimpleClass simple1 = new SimpleClass();
            SimpleClass simple2 = new SimpleClass();
            simple1.s = "Generics first instance";
            simple2.s = "Generics second instance";
            GetGenericList.Add(simple1);
            GetGenericList.Add(simple2);
            return GetGenericList;
        }

        // The following method return a Dictionary of strings.
        [WebMethod]
        public Dictionary<String, String> GetGenericDictionary()
        {
            //Instantiate the dictionary object.
            Dictionary<String, String> result = 
                new Dictionary<string, string>();
            
            //Add items.
            result.Add("0000FF", "Blue");
            result.Add("FF0000", "Red");
            result.Add("00FF00", "Green");
            result.Add("000000", "Black");
          
            return result;
        }

        // The following method return a Dictionary of 
        // SimpleClass objects.
        // Note the SimpleClass type proxy object is automatically 
        // generated because used in the return parameter.
        [WebMethod]
        public Dictionary<String, SimpleClass> GetGenericCustomTypeDictionary()
        {
            //Instantiate the dictionary object.
            Dictionary<String, SimpleClass> result =
                new Dictionary<string, SimpleClass>();

            SimpleClass simple = new SimpleClass();
            simple.s = "custom type instance";

            //Add items.
            result.Add("Custom type", simple);
           
            return result;
        }

        // The following method accept a Dictionary of 
        // SimpleClass2 objects.
        // Note the SimpleClass2 type proxy object is 
        // not automatically generated because used in 
        // an input parameter. You must enable its 
        // generation by applying the GenerateScriptType 
        // attribute to the service.
        [WebMethod]
        public String PassGenericDictionary(
            Dictionary<string, SimpleClass2> d)
        {
            
            string value = d["first"].s;

            string message = "Dictionary element value: " + value;
        
            return message;
        }

     
        // This function returns an array.
        [WebMethod]
        public ArrayList GetArray()
        {
            //Instantiate the array object.
            ArrayList result = new ArrayList();
            
            //Add items.
            result.Add("First element: " + "Test1");
            result.Add("Second element: " + "Test2");

            return result;
        }          
		
	}

}
