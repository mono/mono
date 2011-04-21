//
// ScriptManagerProxy.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace System.Web.UI
{
	[PersistChildren (false)]
	[ParseChildren (true)]
	[DefaultProperty ("Scripts")]
	[NonVisualControl]
	public class ScriptManagerProxy : Control
	{
		ScriptManager _scriptManager;
		ScriptReferenceCollection _scripts;
#if NET_3_5
		CompositeScriptReference _compositeScript;
#endif
		ServiceReferenceCollection _services;
		AuthenticationServiceManager _authenticationService;
		ProfileServiceManager _profileService;

		[Category ("Behavior")]
		[MergableProperty (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public AuthenticationServiceManager AuthenticationService {
			get {
				if (_authenticationService == null)
					_authenticationService = new AuthenticationServiceManager ();
				return _authenticationService;
			}
		}

		[MergableProperty (false)]
		[Category ("Behavior")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ProfileServiceManager ProfileService {
			get {
				if (_profileService == null)
					_profileService = new ProfileServiceManager ();
				return _profileService;
			}
		}

		[Category ("Behavior")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[MergableProperty (false)]
		public ScriptReferenceCollection Scripts {
			get {
				if (_scripts == null)
					_scripts = new ScriptReferenceCollection ();

				return _scripts;
			}
		}
#if NET_3_5
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Category ("Behavior")]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[MergableProperty (false)]
		public CompositeScriptReference CompositeScript {
			get {
				if (_compositeScript == null)
					_compositeScript = new CompositeScriptReference ();
				return _compositeScript;
			}
		}
#endif
		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Category ("Behavior")]
		public ServiceReferenceCollection Services {
			get {
				if (_services == null)
					_services = new ServiceReferenceCollection ();

				return _services;
			}
		}

		ScriptManager ScriptManager {
			get {
				if (_scriptManager == null) {
					_scriptManager = ScriptManager.GetCurrent (Page);
					if (_scriptManager == null)
						throw new InvalidOperationException (String.Format ("The control with ID '{0}' requires a ScriptManager on the page. The ScriptManager must appear before any controls that need it.", ID));
				}
				return _scriptManager;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public override bool Visible {
			get {
				return base.Visible;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected internal override void OnInit (EventArgs e) {
			base.OnInit (e);
			ScriptManager.RegisterProxy (this);
		}
	}
}
