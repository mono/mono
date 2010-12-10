//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com/)
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
using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace System.Web.Util
{
	public class RequestValidator
	{
		static RequestValidator current;
		static Lazy <RequestValidator> lazyLoader;

		// The stack trace from .NET shows it uses Lazy <T>:
		//
		//  Server stack trace: 
		//    at System.Web.Configuration.ConfigUtil.GetType(String typeName, String propertyName, ConfigurationElement configElement, XmlNode node, Boolean checkAptcaBit, Boolean ignoreCase)
		//    at System.Web.Util.RequestValidator.GetCustomValidatorFromConfig()
		//    at System.Lazy`1.CreateValue()
		//
		public static RequestValidator Current {
			get {
				if (current == null)
					current = lazyLoader.Value;
				
				return current;
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				current = value;
			}
		}

		static RequestValidator ()
		{
			lazyLoader = new Lazy <RequestValidator> (new Func <RequestValidator> (LoadConfiguredValidator));
		}
		
		public RequestValidator ()
		{
		}

		protected internal virtual bool IsValidRequestString (HttpContext context, string value, RequestValidationSource requestValidationSource,
								      string collectionKey, out int validationFailureIndex)
		{
			validationFailureIndex = 0;

			return !HttpRequest.IsInvalidString (value, out validationFailureIndex);
		}

		static void ParseTypeName (string spec, out string typeName, out string assemblyName)
		{
			try {
				if (String.IsNullOrEmpty (spec)) {
					typeName = null;
					assemblyName = null;
					return;
				}

				int comma = spec.IndexOf (',');
				if (comma == -1) {
					typeName = spec;
					assemblyName = null;
					return;
				}

				typeName = spec.Substring (0, comma).Trim ();
				assemblyName = spec.Substring (comma + 1).Trim ();
			} catch {
				typeName = spec;
				assemblyName = null;
			}
		}
		
		static RequestValidator LoadConfiguredValidator ()
		{
			HttpRuntimeSection runtimeConfig = HttpRuntime.Section;
			Type validatorType = null;
			string typeSpec = runtimeConfig.RequestValidationType;
			
			try {
				validatorType = HttpApplication.LoadType <RequestValidator> (typeSpec, true);
			} catch (TypeLoadException ex) {
				string typeName, assemblyName;

				ParseTypeName (typeSpec, out typeName, out assemblyName);
				throw new ConfigurationErrorsException (
					String.Format ("Could not load type '{0}' from assembly '{1}'.", typeName, assemblyName),
					ex);
			}
			
			return (RequestValidator) Activator.CreateInstance (validatorType);
		}
	}
}
