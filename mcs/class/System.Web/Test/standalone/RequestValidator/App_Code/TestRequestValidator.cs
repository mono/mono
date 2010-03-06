using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using System.Web.Util;

namespace Tests
{
	public class TestRequestValidator : RequestValidator
	{
		static readonly object call = new object ();
		List<Dictionary<string, object>> calls;

		public List <Dictionary<string, object>> Calls {
			get {
				if (calls == null) {
					calls = new List<Dictionary<string, object>> ();
					AppDomain.CurrentDomain.SetData ("TestRunData", calls);
				}

				return calls;
			}
		}

		protected override bool IsValidRequestString (HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
		{
			lock (call) {
				var dict = new Dictionary<string, object> ();

				dict ["calledFrom"] = Environment.StackTrace;
				dict ["rawUrl"] = HttpContext.Current.Request.RawUrl;
				dict ["context"] = context != null;
				dict ["value"] = value;
				dict ["requestValidationSource"] = (int)requestValidationSource;
				dict ["collectionKey"] = collectionKey;
				
				bool ret = base.IsValidRequestString (context, value, requestValidationSource, collectionKey, out validationFailureIndex);

				dict ["returnValue"] = ret;
				dict ["validationFailureIndex"] = validationFailureIndex;

				Calls.Add (dict);
				return ret;
			}
		}
	}
}