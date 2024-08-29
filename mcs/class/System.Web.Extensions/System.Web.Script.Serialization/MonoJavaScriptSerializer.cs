using System;
using System.Reflection;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;

namespace System.Web.Script.Serialization {
	internal class MonoJavaScriptSerializer : JavaScriptSerializer {
		public MonoJavaScriptSerializer() : this(null, false) {			
		}

		public MonoJavaScriptSerializer(JavaScriptTypeResolver resolver) : this(resolver, false) {			
		}

		public MonoJavaScriptSerializer(JavaScriptTypeResolver resolver, bool registerConverters) : base(resolver) {
			ScriptingJsonSerializationSection section = (ScriptingJsonSerializationSection) ConfigurationManager.GetSection ("system.web.extensions/scripting/webServices/jsonSerialization");
			if (section != null) {				
				RecursionLimit = section.RecursionLimit;
            	MaxJsonLength = section.MaxJsonLength;
				
				if (registerConverters) {
					ConvertersCollection converters = section.Converters;
					if (converters != null && converters.Count > 0) {
						var cvtlist = new List <JavaScriptConverter> ();
						Type type;
						string typeName;
						JavaScriptConverter jsc;
						
						foreach (Converter cvt in converters) {
							typeName = cvt != null ? cvt.Type : null;
							if (typeName == null)
								continue;
							
							type = Type.GetType(typeName, true);
							jsc = Activator.CreateInstance (type) as JavaScriptConverter;

							if (jsc == null)
								continue;

							cvtlist.Add (jsc);
						}
					
						RegisterConverters (cvtlist);
					}
				}
			}
		}
	}
}