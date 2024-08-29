using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

using MonoTests.SystemWeb.Framework;

namespace StandAloneRunnerSupport {
    public class TestRunnerHost : MarshalByRefObject {

        const string HTML_NAMESPACE = "http://www.w3.org/1999/xhtml";

        public TestRunner CreateTest() {
            return new TestRunner();
        }

        public Response RunTest(TestRunItem tri, Response previousResponse) {
            Response response;
            string[] formValues;

            TestRunner runner = new TestRunner();
            
            if (runner == null) {
                throw new InvalidOperationException ("runner must not be null.");
            }
						
            if (tri.PostValues != null && previousResponse != null)
                formValues = ExtractFormAndHiddenControls (previousResponse);
            else
                formValues = null;

            SetRunnerDomainData (tri.AppDomainData, runner.Domain);
            response = runner.Run (tri.Url, tri.PathInfo, tri.PostValues, formValues);
            if (tri.Callback == null)
                return response;

            tri.TestRunData = runner.TestRunData;
            tri.StatusCode = runner.StatusCode;
            tri.Redirected = runner.Redirected;
            tri.RedirectLocation = runner.RedirectLocation;	

            if (tri.Callback != null)
                tri.Callback (response.Body, tri);

            return response;
        }

        string[] ExtractFormAndHiddenControls (Response response)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument ();
            htmlDoc.LoadHtml (response.Body);

            var tempxml = new StringBuilder ();
            var tsw = new StringWriter (tempxml);
            htmlDoc.OptionOutputAsXml = true;
            htmlDoc.Save (tsw);

            var doc = new XmlDocument ();
            doc.LoadXml (tempxml.ToString ());

            XmlNamespaceManager nsmgr = new XmlNamespaceManager (doc.NameTable);
            nsmgr.AddNamespace ("html", HTML_NAMESPACE);

            XmlNode formNode = doc.SelectSingleNode ("//html:form", nsmgr);
            if (formNode == null)
                    throw new ArgumentException ("Form was not found in document: " + response.Body);

            string actionUrl = formNode.Attributes ["action"].Value;
            XmlNode method = formNode.Attributes ["method"];
            var data = new List <string> ();
            string name, value;
        
            foreach (XmlNode inputNode in doc.SelectNodes ("//html:input[@type='hidden']", nsmgr)) {
                name = inputNode.Attributes["name"].Value;
                                if (String.IsNullOrEmpty (name))
                                        continue;

                XmlAttribute attr = inputNode.Attributes["value"];
                if (attr != null)
                        value = attr.Value;
                else
                        value = String.Empty;

                data.Add (name);
                data.Add (value);
            }

            return data.ToArray ();
        }

        void SetRunnerDomainData (object[] data, AppDomain domain)
		{
			int len = data != null ? data.Length : 0;
			if (len == 0)
				return;

			if (len % 2 != 0)
				throw new ArgumentException ("Must have an even number of elements.", "data");

			string name;
			for (int i = 0; i < len; i += 2) {
				name = data [i] as string;
				if (String.IsNullOrEmpty (name))
					throw new InvalidOperationException (String.Format ("Name at index {0} must not be null or empty.", i));

				domain.SetData (name, data [i + 1]);
			}
		}


    }
}
