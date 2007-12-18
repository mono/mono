//
// System.Web.UI.Page.jvm.cs
//
// Authors:
//   Eyal Alaluf (eyala@mainsoft.com)
//
// (C) 2006 Mainsoft Co. (http://www.mainsoft.com)
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

using javax.servlet.http;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Hosting;
using System.Web.J2EE;
using System.ComponentModel;
using System.IO;
using javax.faces.context;
using javax.faces.render;
using javax.servlet;
using javax.faces;
using javax.faces.application;

namespace System.Web.UI
{
	public partial class Page
	{
		bool _emptyPortletNamespace = false;
		string _PortletNamespace = null;
		StateManager.SerializedView _facesSerializedView;


		internal string PortletNamespace
		{
			get {
				if (_PortletNamespace == null) {

					if (getFacesContext () != null) {
						_PortletNamespace = getFacesContext ().getExternalContext ().encodeNamespace (String.Empty);
					}

					_PortletNamespace = _PortletNamespace ?? String.Empty;
				}
				return _PortletNamespace;
			}
		}

		internal string theForm {
			get {
				return "theForm" + PortletNamespace;
			}
		}
		
		bool _isMultiForm = false;
		bool _isMultiFormInited = false;

		internal bool IsMultiForm {
			get {
				if (!_isMultiFormInited) {
					Mainsoft.Web.Configuration.PagesSection pageSection = (Mainsoft.Web.Configuration.PagesSection) System.Web.Configuration.WebConfigurationManager.GetSection ("mainsoft.web/pages");
					if (pageSection != null)
						_isMultiForm = pageSection.MultiForm;

					_isMultiFormInited = true;
				}
				return _isMultiForm;
			}
		}

		internal string EncodeURL (string raw) {
			//kostat: BUGBUG: complete
			return raw;
		}

	}
}
