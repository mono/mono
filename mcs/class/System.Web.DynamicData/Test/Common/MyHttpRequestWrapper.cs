using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace MonoTests.Common
{
	class MyHttpRequestWrapper : HttpRequestBase
	{
		Dictionary<string, object> propertyValues = new Dictionary<string, object> ();

		public override string AppRelativeCurrentExecutionFilePath
		{
			get
			{
				string value;
				if (!GetProperty<string> ("AppRelativeCurrentExecutionFilePath", out value))
					return base.AppRelativeCurrentExecutionFilePath;

				return value;
			}
		}

		public override string PathInfo
		{
			get
			{
				string value;
				if (!GetProperty<string> ("PathInfo", out value))
					return base.PathInfo;

				return value;
			}
		}

		public override NameValueCollection QueryString
		{
			get
			{
				NameValueCollection value;
				if (!GetProperty<NameValueCollection> ("QueryString", out value))
					return base.QueryString;

				return value;
			}
		}

		bool GetProperty<T> (string name, out T value)
		{
			if (String.IsNullOrEmpty (name))
				throw new ArgumentNullException ("name");

			value = default (T);
			object v;
			if (propertyValues.TryGetValue (name, out v)) {
				if (v == null)
					return true;
				if (typeof (T).IsAssignableFrom (v.GetType ())) {
					value = (T) v;
					return true;
				}

				throw new InvalidOperationException ("Invalid value type. Expected '" + typeof (T) + "' and got '" + v.GetType () + "'");
			}

			return false;
		}

		public void SetProperty (string name, object value)
		{
			if (String.IsNullOrEmpty (name))
				return;

			if (propertyValues.ContainsKey (name))
				propertyValues[name] = value;
			else
				propertyValues.Add (name, value);
		}
	}
}
