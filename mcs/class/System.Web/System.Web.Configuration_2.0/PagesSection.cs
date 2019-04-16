//
// System.Web.Configuration.PagesSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Configuration;
using System.Web.UI;
using System.Xml;

namespace System.Web.Configuration
{
	public sealed class PagesSection: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty asyncTimeoutProp;
		static ConfigurationProperty autoEventWireupProp;
		static ConfigurationProperty bufferProp;
		static ConfigurationProperty controlsProp;
		static ConfigurationProperty enableEventValidationProp;
		static ConfigurationProperty enableSessionStateProp;
		static ConfigurationProperty enableViewStateProp;
		static ConfigurationProperty enableViewStateMacProp;
		static ConfigurationProperty maintainScrollPositionOnPostBackProp;
		static ConfigurationProperty masterPageFileProp;
		static ConfigurationProperty maxPageStateFieldLengthProp;
		static ConfigurationProperty modeProp;
		static ConfigurationProperty namespacesProp;
		static ConfigurationProperty pageBaseTypeProp;
		static ConfigurationProperty pageParserFilterTypeProp;
		static ConfigurationProperty smartNavigationProp;
		static ConfigurationProperty styleSheetThemeProp;
		static ConfigurationProperty tagMappingProp;
		static ConfigurationProperty themeProp;
		static ConfigurationProperty userControlBaseTypeProp;
		static ConfigurationProperty validateRequestProp;
		static ConfigurationProperty viewStateEncryptionModeProp;
		static ConfigurationProperty clientIDModeProp;
		static ConfigurationProperty controlRenderingCompatibilityVersionProp;
		static PagesSection ()
		{
			asyncTimeoutProp = new ConfigurationProperty ("asyncTimeout", typeof (TimeSpan), TimeSpan.FromSeconds (45.0),
								      PropertyHelper.TimeSpanSecondsConverter,
								      PropertyHelper.PositiveTimeSpanValidator,
								      ConfigurationPropertyOptions.None);
			autoEventWireupProp = new ConfigurationProperty ("autoEventWireup", typeof(bool), true);
			bufferProp = new ConfigurationProperty ("buffer", typeof(bool), true);
			controlsProp = new ConfigurationProperty ("controls", typeof(TagPrefixCollection), null,
								  null, null, ConfigurationPropertyOptions.None);
			enableEventValidationProp = new ConfigurationProperty ("enableEventValidation", typeof (bool), true);
			enableSessionStateProp = new ConfigurationProperty ("enableSessionState", typeof (string), "true");
			enableViewStateProp = new ConfigurationProperty ("enableViewState", typeof (bool), true);
			enableViewStateMacProp = new ConfigurationProperty ("enableViewStateMac", typeof (bool), true);
			maintainScrollPositionOnPostBackProp = new ConfigurationProperty ("maintainScrollPositionOnPostBack", typeof (bool), false);
			masterPageFileProp = new ConfigurationProperty ("masterPageFile", typeof (string), "");
			maxPageStateFieldLengthProp = new ConfigurationProperty ("maxPageStateFieldLength", typeof (int), -1);
			modeProp = new ConfigurationProperty ("compilationMode", typeof (CompilationMode), CompilationMode.Always,
							      new GenericEnumConverter (typeof (CompilationMode)), PropertyHelper.DefaultValidator,
							      ConfigurationPropertyOptions.None);
			namespacesProp = new ConfigurationProperty ("namespaces", typeof (NamespaceCollection), null,
								    null, null, ConfigurationPropertyOptions.None);
			pageBaseTypeProp = new ConfigurationProperty ("pageBaseType", typeof (string), "System.Web.UI.Page");
			pageParserFilterTypeProp = new ConfigurationProperty ("pageParserFilterType", typeof (string), "");
			smartNavigationProp = new ConfigurationProperty ("smartNavigation", typeof (bool), false);
			styleSheetThemeProp = new ConfigurationProperty ("styleSheetTheme", typeof (string), "");
			tagMappingProp = new ConfigurationProperty ("tagMapping", typeof (TagMapCollection), null,
								    null, null, ConfigurationPropertyOptions.None);
			themeProp = new ConfigurationProperty ("theme", typeof (string), "");
			userControlBaseTypeProp = new ConfigurationProperty ("userControlBaseType", typeof (string), "System.Web.UI.UserControl");
			validateRequestProp = new ConfigurationProperty ("validateRequest", typeof (bool), true);
			viewStateEncryptionModeProp = new ConfigurationProperty ("viewStateEncryptionMode", typeof (ViewStateEncryptionMode), ViewStateEncryptionMode.Auto,
										 new GenericEnumConverter (typeof (ViewStateEncryptionMode)), PropertyHelper.DefaultValidator,
										 ConfigurationPropertyOptions.None);
			clientIDModeProp = new ConfigurationProperty ("clientIDMode", typeof (ClientIDMode), ClientIDMode.Predictable,
								      new GenericEnumConverter (typeof (ClientIDMode)), PropertyHelper.DefaultValidator,
								      ConfigurationPropertyOptions.None);
			controlRenderingCompatibilityVersionProp = new ConfigurationProperty ("controlRenderingCompatibilityVersion", typeof (Version), new Version (4, 0),
											      new VersionConverter (3, 5, "The value for the property 'controlRenderingCompatibilityVersion' is not valid. The error is: The control rendering compatibility version must not be less than {1}."),
											      PropertyHelper.DefaultValidator,
											      ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (asyncTimeoutProp);
			properties.Add (autoEventWireupProp);
			properties.Add (bufferProp);
			properties.Add (controlsProp);
			properties.Add (enableEventValidationProp);
			properties.Add (enableSessionStateProp);
			properties.Add (enableViewStateProp);
			properties.Add (enableViewStateMacProp);
			properties.Add (maintainScrollPositionOnPostBackProp);
			properties.Add (masterPageFileProp);
			properties.Add (maxPageStateFieldLengthProp);
			properties.Add (modeProp);
			properties.Add (namespacesProp);
			properties.Add (pageBaseTypeProp);
			properties.Add (pageParserFilterTypeProp);
			properties.Add (smartNavigationProp);
			properties.Add (styleSheetThemeProp);
			properties.Add (tagMappingProp);
			properties.Add (themeProp);
			properties.Add (userControlBaseTypeProp);
			properties.Add (validateRequestProp);
			properties.Add (viewStateEncryptionModeProp);
			properties.Add (clientIDModeProp);
			properties.Add (controlRenderingCompatibilityVersionProp);
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

		[ConfigurationProperty ("controls")]
		public TagPrefixCollection Controls {
			get { return (TagPrefixCollection) base[controlsProp]; }
		}

		[ConfigurationProperty ("enableEventValidation", DefaultValue = true)]
		public bool EnableEventValidation {
			get { return (bool) base[enableEventValidationProp]; }
			set { base[enableEventValidationProp] = value; }
		}

		[ConfigurationProperty ("enableSessionState", DefaultValue = "true")]
		public PagesEnableSessionState EnableSessionState {
			get {
				string enableSessionState = (string) base [enableSessionStateProp];
				switch (enableSessionState) {
				case "true":
					return PagesEnableSessionState.True;
				case "false":
					return PagesEnableSessionState.False;
				case "ReadOnly":
					return PagesEnableSessionState.ReadOnly;
				}
				throw new ConfigurationErrorsException ("The 'enableSessionState'"
					+ " attribute must be one of the following values: true,"
					+ "false, ReadOnly.");
			}
			set {
				switch (value) {
				case PagesEnableSessionState.False:
					base [enableSessionStateProp] = "false";
					break;
				case PagesEnableSessionState.ReadOnly:
					base [enableSessionStateProp] = "ReadOnly";
					break;
				default:
					base [enableSessionStateProp] = "true";
					break;
				}
			}
		}

		[ConfigurationProperty ("enableViewState", DefaultValue = true)]
		public bool EnableViewState {
			get { return (bool) base[enableViewStateProp]; }
			set { base[enableViewStateProp] = value; }
		}

		[ConfigurationProperty ("enableViewStateMac", DefaultValue = true)]
		public bool EnableViewStateMac {
			get { return (bool) base[enableViewStateMacProp]; }
			set { base[enableViewStateMacProp] = value; }
		}

		[ConfigurationProperty ("maintainScrollPositionOnPostBack", DefaultValue = false)]
		public bool MaintainScrollPositionOnPostBack {
			get { return (bool) base[maintainScrollPositionOnPostBackProp]; }
			set { base [maintainScrollPositionOnPostBackProp] = value; }
		}

		[ConfigurationProperty ("masterPageFile", DefaultValue = "")]
		public string MasterPageFile {
			get { return (string) base[masterPageFileProp]; }
			set { base[masterPageFileProp] = value; }
		}

		[ConfigurationProperty ("maxPageStateFieldLength", DefaultValue = -1)]
		public int MaxPageStateFieldLength {
			get { return (int) base[maxPageStateFieldLengthProp]; }
			set { base[maxPageStateFieldLengthProp] = value; }
		}

		[ConfigurationProperty ("namespaces")]
		public NamespaceCollection Namespaces {
			get { return (NamespaceCollection) base[namespacesProp]; }
		}

		[ConfigurationProperty ("pageBaseType", DefaultValue = "System.Web.UI.Page")]
		public string PageBaseType {
			get { return (string) base[pageBaseTypeProp]; }
			set { base[pageBaseTypeProp] = value; }
		}

		[ConfigurationProperty ("pageParserFilterType", DefaultValue = "")]
		public string PageParserFilterType {
			get { return (string) base[pageParserFilterTypeProp]; }
			set { base [pageParserFilterTypeProp] = value; }
		}

		[ConfigurationProperty ("smartNavigation", DefaultValue = false)]
		public bool SmartNavigation {
			get { return (bool) base[smartNavigationProp]; }
			set { base[smartNavigationProp] = value; }
		}

		[ConfigurationProperty ("styleSheetTheme", DefaultValue = "")]
		public string StyleSheetTheme {
			get { return (string) base[styleSheetThemeProp]; }
			set { base[styleSheetThemeProp] = value; }
		}

		[ConfigurationProperty ("tagMapping")]
		public TagMapCollection TagMapping {
			get { return (TagMapCollection) base [tagMappingProp]; }
		}

		[ConfigurationProperty ("theme", DefaultValue = "")]
		public string Theme {
			get { return (string) base[themeProp]; }
			set { base[themeProp] = value; }
		}

		[ConfigurationProperty ("userControlBaseType", DefaultValue = "System.Web.UI.UserControl")]
		public string UserControlBaseType {
			get { return (string) base[userControlBaseTypeProp]; }
			set { base[userControlBaseTypeProp] = value; }
		}

		[ConfigurationProperty ("validateRequest", DefaultValue = true)]
		public bool ValidateRequest {
			get { return (bool) base[validateRequestProp]; }
			set { base[validateRequestProp] = value; }
		}

		[ConfigurationProperty ("viewStateEncryptionMode", DefaultValue = ViewStateEncryptionMode.Auto)]
		public ViewStateEncryptionMode ViewStateEncryptionMode {
			get { return (ViewStateEncryptionMode) base [viewStateEncryptionModeProp]; }
			set { base [viewStateEncryptionModeProp] = value; }
		}
		[ConfigurationProperty ("clientIDMode", DefaultValue = ClientIDMode.Predictable)]
		public ClientIDMode ClientIDMode {
			get { return (ClientIDMode) base [clientIDModeProp]; }
			set { base [clientIDModeProp] = value; }
		}

		[ConfigurationProperty ("controlRenderingCompatibilityVersion", DefaultValue = "4.0")]
		public Version ControlRenderingCompatibilityVersion {
			get { return (Version) base [controlRenderingCompatibilityVersionProp]; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				base [controlRenderingCompatibilityVersionProp] = value;
			}
		}
		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}
