using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData;

namespace MonoTests.Common
{
	class FieldFormattingOptions : IFieldFormattingOptions
	{
		Dictionary<string, object> propertyValues = new Dictionary<string, object> ();

		public bool ApplyFormatInEditMode
		{
			get { return GetProperty <bool> ("ApplyFormatInEditMode");  }
		}

		public bool ConvertEmptyStringToNull
		{
			get { return GetProperty <bool> ("ConvertEmptyStringToNull"); }
		}

		public string DataFormatString
		{
			get { return GetProperty <string> ("DataFormatString"); }
		}

		public bool HtmlEncode
		{
			get { return GetProperty <bool> ("HtmlEncode"); }
		}

		public string NullDisplayText
		{
			get { return GetProperty <string> ("NullDisplayText"); }
		}

		T GetProperty<T> (string name)
		{
			if (String.IsNullOrEmpty (name))
				throw new ArgumentNullException ("name");
			
			object v;
			if (propertyValues.TryGetValue (name, out v)) {
				if (v == null)
					return default (T);
				if (typeof (T).IsAssignableFrom (v.GetType ())) {
					return (T) v;
				}

				throw new InvalidOperationException ("Invalid value type. Expected '" + typeof (T) + "' and got '" + v.GetType () + "'");
			}

			return default (T);
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
