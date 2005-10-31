//
// System.Web.Configuration.PagesSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.Web.UI;
using System.Xml;

namespace System.Web.Configuration
{
	public class PagesSection: InternalSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty asyncTimeoutProp;
		static ConfigurationProperty autoEventWireupProp;
		static ConfigurationProperty bufferProp;
		static ConfigurationProperty modeProp;
		
		static PagesSection ()
		{
			properties = new ConfigurationPropertyCollection ();
			asyncTimeoutProp = new ConfigurationProperty ("asyncTimeout", typeof(TimeSpan), null);
			autoEventWireupProp = new ConfigurationProperty ("autoEventWireup", typeof(bool), true);
			bufferProp = new ConfigurationProperty ("buffer", typeof(bool), false);
			modeProp = new ConfigurationProperty ("compilationMode", typeof (CompilationMode), CompilationMode.Always);
		}

		public PagesSection ()
		{
		}

		[TimeSpanValidator (MinValueString = "00:00:00",
				    MaxValueString = "10675199.02:48:05.4775807")]
		[TypeConverter (typeof (TimeSpanSecondsConverter))]
		[ConfigurationProperty ("asyncTimeout", DefaultValue = "00:00:45")]
		public TimeSpan AsyncTimeout {
			get { return (TimeSpan) base [asyncTimeoutProp]; }
			set { base [asyncTimeoutProp] = value; }
		}

		[ConfigurationProperty ("autoEventWireup", DefaultValue = true)]
		public bool AutoEventWireup {
			get { return (bool) base [autoEventWireupProp]; }
			set { base [autoEventWireupProp] = value; }
		}

		[ConfigurationProperty ("buffer", DefaultValue = true)]
		public bool Buffer {
			get { return (bool) base [bufferProp]; }
			set { base [bufferProp] = value; }
		}

		[ConfigurationProperty ("compilationMode", DefaultValue = CompilationMode.Always)]
		public CompilationMode CompilationMode {
			get { return (CompilationMode) base [modeProp]; }
			set { base [modeProp] = value; }
		}

#if notyet
		[MonoTODO]
		public TagPrefixCollection Controls {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO]
		[ConfigurationProperty ("enableSessionState", DefaultValue = true)]
		public PagesEnableSessionState EnableSessionState {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("enableViewState", DefaultValue = true)]
		public bool EnableViewState {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("enableViewStateMac", DefaultValue = true)]
		public bool EnableViewStateMac {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("maintainScrollPositionOnPostBack", DefaultValue = false)]
		public bool MaintainScrollPositionOnPostBack {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("masterPageFile", DefaultValue = "")]
		public string MasterPageFile {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("maxPageStateFieldLength", DefaultValue = -1)]
		public int MaxPageStateFieldLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

#if notyet
		[MonoTODO]
		[ConfigurationProperty ("namespaces")]
		public NamespaceCollection Namespaces {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO]
		[ConfigurationProperty ("pageBaseType", DefaultValue = "System.Web.UI.Page")]
		public string PageBaseType {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("pageParserFilterType", DefaultValue = "")]
		public string PageParserFilterType {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override ConfigurationPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("smartNavigation", DefaultValue = false)]
		public bool SmartNavigation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("styleSheetTheme", DefaultValue = "")]
		public string StyleSheetTheme {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

#if notyet
		[MonoTODO]
		[ConfigurationProperty ("tagMapping")]
		public TagMapCollection TagMapping {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		[MonoTODO]
		[ConfigurationProperty ("theme", DefaultValue = "")]
		public string Theme {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("userControlBaseType", DefaultValue = "System.Web.UI.UserControl")]
		public string UserControlBaseType {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[ConfigurationProperty ("validateRequest", DefaultValue = true)]
		public bool ValidateRequest {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

#if notyet
		[MonoTODO]
		[ConfigurationProperty ("viewStateEncryptionMode", DefaultValue = ViewStateEncryptionMode.Auto)]
		public ViewStateEncryptionMode ViewStateEncryptionMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif
		
		[MonoTODO]
		protected override void DeserializeSection (XmlReader reader)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
