//
// WebAttributeInfo.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

namespace System.ServiceModel.Web
{
	internal class WebAttributeInfo
	{
		string uri_template;
		WebMessageFormat request_format, response_format;
		WebMessageBodyStyle body_style;
		bool has_body_style, has_request_format, has_response_format;
		string method = "POST";

		public WebMessageBodyStyle BodyStyle {
			get { return body_style; }
			set {
				body_style = value;
				has_body_style = true;
			}
		}

		public bool IsBodyStyleSetExplicitly {
			get { return has_body_style; }
		}

		public bool IsRequestFormatSetExplicitly {
			get { return has_request_format; }
		}

		public bool IsResponseFormatSetExplicitly {
			get { return has_response_format; }
		}

		public WebMessageFormat RequestFormat {
			get { return request_format; }
			set {
				request_format = value;
				has_request_format = true;
			}
		}

		public WebMessageFormat ResponseFormat {
			get { return response_format; }
			set {
				response_format = value;
				has_response_format = true;
			}
		}

		// only meaningful for WebInvokeAttribute.
		public string Method {
			get { return method; }
			set { method = value; }
		}

		public string UriTemplate {
			get { return uri_template; }
			set { uri_template = value; }
		}

		public UriTemplate BuildUriTemplate (OperationDescription od, MessageDescription md)
		{
			if (uri_template != null)
				return new UriTemplate (uri_template);
			if (md == null)
				foreach (MessageDescription mm in od.Messages)
					if (mm.Direction == MessageDirection.Input)
						md = mm;

			StringBuilder sb = new StringBuilder ();
			sb.Append (od.Name);
			for (int i = 0; i < md.Body.Parts.Count; i++) {
				MessagePartDescription mp = md.Body.Parts [i];
				sb.Append (i == 0 ? '?' : '&');
				sb.Append (mp.Name);
				sb.Append ("={");
				sb.Append (mp.Name);
				sb.Append ('}');
			}
			return new UriTemplate (sb.ToString ());
		}

	}
}
