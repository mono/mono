//
// System.Web.Configuration.PagesConfigurationHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2004 Novell, Inc (http://www.novel.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class PagesConfigurationHandler : IConfigurationSectionHandler
	{
		public object Create (object parent, object configContext, XmlNode section)
		{
			PagesConfiguration config = new PagesConfiguration (parent);

			if (section.HasChildNodes)
				HandlersUtil.ThrowException ("No child nodes allowed here.", section);

			string attvalue = AttValue ("buffer", section);
			if (attvalue != null)
				config.Buffer = GetBool ("buffer", attvalue, section);

			attvalue = AttValue ("enableSessionState", section);
			if (attvalue != null) {
				switch (attvalue) {
				case "true":
					config.EnableSessionState = PagesEnableSessionState.True;
					break;
				case "ReadOnly":
					config.EnableSessionState = PagesEnableSessionState.ReadOnly;
					break;
				case "false":
					config.EnableSessionState = PagesEnableSessionState.False;
					break;
				default:
					HandlersUtil.ThrowException ("The 'enableSessionState' attribute"
						+ " is case sensitive and must be one of the following values:"
						+ " false, true, ReadOnly.", section);
					break;
				}
			}

			attvalue = AttValue ("enableViewState", section);
			if (attvalue != null)
				config.EnableViewState = GetBool ("enableViewState", attvalue, section);

			attvalue = AttValue ("enableViewStateMac", section);
			if (attvalue != null)
				config.EnableViewStateMac = GetBool ("enableViewStateMac", attvalue, section);

			attvalue = AttValue ("smartNavigation", section);
			if (attvalue != null)
				config.SmartNavigation = GetBool ("smartNavigation", attvalue, section);

			attvalue = AttValue ("autoEventWireup", section);
			if (attvalue != null)
				config.AutoEventWireup = GetBool ("autoEventWireup", attvalue, section);

			attvalue = AttValue ("validateRequest", section);
			if (attvalue != null)
				config.ValidateRequest = GetBool ("validateRequest", attvalue, section);

			attvalue = AttValue ("pageBaseType", section);
			if (attvalue != null) {
				string v = attvalue.Trim ();
				if (v.Length == 0)
					HandlersUtil.ThrowException ("pageBaseType is empty.", section);

				config.PageBaseType = v;
			}

			attvalue = AttValue ("userControlBaseType", section);
			if (attvalue != null) {
				string v = attvalue.Trim ();
				if (v.Length == 0)
					HandlersUtil.ThrowException ("userControlBaseType is empty.", section);

				config.UserControlBaseType = v;
			}

			if (section.Attributes == null || section.Attributes.Count != 0)
				HandlersUtil.ThrowException ("Unknown attribute(s).", section);

			return config;
		}

		static bool GetBool (string name, string value, XmlNode section)
		{
			if (value == "true")
				return true;

			if (value != "false")
				HandlersUtil.ThrowException ("Invalid boolean value for '" + name + "'", section);

			return false;
		}

		// A few methods to save some typing
		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}
		//
	}
}

