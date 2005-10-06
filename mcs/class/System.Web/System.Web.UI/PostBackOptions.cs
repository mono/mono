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

namespace System.Web.UI
{
	public sealed class PostBackOptions
	{
		private Control control;
		private string argument;
		private string actionUrl;
		private bool autoPostBack;
		private bool requiresJavaScriptProtocol;
		private bool trackFocus;
		private bool clientSubmit;
		private bool performValidation;
		private string validationGroup;
		
		public PostBackOptions (Control control) : this (control, null, null, false, false, false, 
								false, false, null)
		{
		}

		public PostBackOptions (Control control, string argument) : this (control, argument, null, false, 
										false, false, false, false, null)
		{
		}

		public PostBackOptions (Control control, string argument, string actionUrl, bool isAutoPostBack,
					bool isJavaScriptProtocolRequired, bool isTrackFocus, bool isClientSubmit,
					bool isValidationPerformed, string validatingGroup)
		{
			this.control = control;
			this.argument = argument;
			this.actionUrl = actionUrl;
			this.autoPostBack = isAutoPostBack;
			this.requiresJavaScriptProtocol = isJavaScriptProtocolRequired;
			this.trackFocus = isTrackFocus;
			this.clientSubmit = isClientSubmit;
			this.performValidation = isValidationPerformed;
			this.validationGroup = validatingGroup;
		}

		public string ActionUrl {
			get { return actionUrl;	}
			set { actionUrl = value; }
		}

		public string Argument {
			get { return  argument;	}
			set { argument = value; }
		}

		[MonoTODO ("Implement support for this in Page")]
		public bool AutoPostBack {
			get { return autoPostBack; }
			set { autoPostBack = value; }
		}

		public bool ClientSubmit {
			get { return clientSubmit; }
			set { clientSubmit = value; }
		}
		
		public bool PerformValidation {
			get { return performValidation;	}
			set { performValidation = value; }
		}

		public bool RequiresJavaScriptProtocol {
			get { return requiresJavaScriptProtocol; }
			set { requiresJavaScriptProtocol = value; }
		}

		public Control TargetControl {
			get { return control; }
			set { control = value; }
		}

		[MonoTODO ("Implement support for this in Page")]
		public bool TrackFocus {
			get { return trackFocus; }
			set { trackFocus = value; }
		}

		[MonoTODO ("Implement support for this in Page")]
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
