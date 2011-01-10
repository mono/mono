//
// System.Web.Configuration.TraceSection
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


using System;
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class TraceSection : ConfigurationSection
	{
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty localOnlyProp;
		static ConfigurationProperty mostRecentProp;
		static ConfigurationProperty pageOutputProp;
		static ConfigurationProperty requestLimitProp;
		static ConfigurationProperty traceModeProp;
		static ConfigurationProperty writeToDiagnosticsTraceProp;
		static ConfigurationPropertyCollection properties;

		static TraceSection ()
		{
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), false);
			localOnlyProp = new ConfigurationProperty ("localOnly", typeof (bool), true);
			mostRecentProp = new ConfigurationProperty ("mostRecent", typeof (bool), false);
			pageOutputProp = new ConfigurationProperty ("pageOutput", typeof (bool), false);
			requestLimitProp = new ConfigurationProperty ("requestLimit", typeof (int), 10,
								      TypeDescriptor.GetConverter (typeof (int)),
								      PropertyHelper.IntFromZeroToMaxValidator,
								      ConfigurationPropertyOptions.None);
			traceModeProp = new ConfigurationProperty ("traceMode", typeof (TraceDisplayMode), TraceDisplayMode.SortByTime,
								   new GenericEnumConverter (typeof (TraceDisplayMode)), null,
								   ConfigurationPropertyOptions.None);
			writeToDiagnosticsTraceProp = new ConfigurationProperty ("writeToDiagnosticsTrace", typeof (bool), false);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (enabledProp);
			properties.Add (localOnlyProp);
			properties.Add (mostRecentProp);
			properties.Add (pageOutputProp);
			properties.Add (requestLimitProp);
			properties.Add (traceModeProp);
			properties.Add (writeToDiagnosticsTraceProp);
		}

		[ConfigurationProperty ("enabled", DefaultValue = "False")]
		public bool Enabled {
			get { return (bool) base [enabledProp];}
			set { base[enabledProp] = value; }
		}

		[ConfigurationProperty ("localOnly", DefaultValue = "True")]
		public bool LocalOnly {
			get { return (bool) base [localOnlyProp];}
			set { base[localOnlyProp] = value; }
		}

		[ConfigurationProperty ("mostRecent", DefaultValue = "False")]
		public bool MostRecent {
			get { return (bool) base [mostRecentProp];}
			set { base[mostRecentProp] = value; }
		}

		[ConfigurationProperty ("pageOutput", DefaultValue = "False")]
		public bool PageOutput {
			get { return (bool) base [pageOutputProp];}
			set { base[pageOutputProp] = value; }
		}

		[IntegerValidator (MinValue = 0, MaxValue = Int32.MaxValue)]
		[ConfigurationProperty ("requestLimit", DefaultValue = "10")]
		public int RequestLimit {
			get { return (int) base [requestLimitProp];}
			set { base[requestLimitProp] = value; }
		}

		[ConfigurationProperty ("traceMode", DefaultValue = "SortByTime")]
		public TraceDisplayMode TraceMode {
			get { return (TraceDisplayMode) base [traceModeProp];}
			set { base[traceModeProp] = value; }
		}

		[ConfigurationProperty ("writeToDiagnosticsTrace", DefaultValue = "False")]
		public bool WriteToDiagnosticsTrace {
			get { return (bool) base [writeToDiagnosticsTraceProp];}
			set { base[writeToDiagnosticsTraceProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif

