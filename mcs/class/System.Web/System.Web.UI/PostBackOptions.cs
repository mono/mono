//
// System.Web.UI.PostBackOptions.cs
//
// Authors:
//      Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.ComponentModel;

namespace System.Web.UI
{
	public sealed class PostBackOptions
	{
		Control control;
		string argument;
		string actionUrl;
		bool autoPostBack;
		bool requiresJavaScriptProtocol;
		bool trackFocus;
		bool clientSubmit;
		bool performValidation;
		string validationGroup;

		public PostBackOptions (Control targetControl)
			: this (targetControl, null, null, false, false, false, true, false, null)
		{
		}

		public PostBackOptions (Control targetControl, string argument)
			: this (targetControl, argument, null, false, false, false, true, false, null)
		{
		}

		public PostBackOptions (Control targetControl, string argument, string actionUrl, bool autoPostBack,
					bool requiresJavaScriptProtocol, bool trackFocus, bool clientSubmit,
					bool performValidation, string validationGroup)
		{
			if (targetControl == null)
				throw new ArgumentNullException ("targetControl");
			this.control = targetControl;
			this.argument = argument;
			this.actionUrl = actionUrl;
			this.autoPostBack = autoPostBack;
			this.requiresJavaScriptProtocol = requiresJavaScriptProtocol;
			this.trackFocus = trackFocus;
			this.clientSubmit = clientSubmit;
			this.performValidation = performValidation;
			this.validationGroup = validationGroup;
		}

		[DefaultValue ("")]
		public string ActionUrl {
			get { return actionUrl;	}
			set { actionUrl = value; }
		}

		[DefaultValue ("")]
		public string Argument {
			get { return  argument;	}
			set { argument = value; }
		}

		[MonoTODO ("Implement support for this in Page")]
		[DefaultValue (false)]
		public bool AutoPostBack {
			get { return autoPostBack; }
			set { autoPostBack = value; }
		}

		[DefaultValue (true)]
		public bool ClientSubmit {
			get { return clientSubmit; }
			set { clientSubmit = value; }
		}
		
		[DefaultValue (false)]
		public bool PerformValidation {
			get { return performValidation;	}
			set { performValidation = value; }
		}

		[DefaultValue (true)]
		public bool RequiresJavaScriptProtocol {
			get { return requiresJavaScriptProtocol; }
			set { requiresJavaScriptProtocol = value; }
		}

		[DefaultValue (null)]
		public Control TargetControl {
			get { return control; }
		}

		[MonoTODO ("Implement support for this in Page")]
		[DefaultValue (false)]
		public bool TrackFocus {
			get { return trackFocus; }
			set { trackFocus = value; }
		}

		[MonoTODO ("Implement support for this in Page")]
		[DefaultValue ("")]
		public string ValidationGroup {
			get { return validationGroup; }
			set { validationGroup = value; }
		}
		
		// Returns true if some of these options must be handled by
		// client script.
		internal bool RequiresSpecialPostBack {
			get { 
				return actionUrl != null || 
						validationGroup != null || 
						trackFocus || 
						autoPostBack || 
						argument != null;
			}
		}
	}
}

#endif
