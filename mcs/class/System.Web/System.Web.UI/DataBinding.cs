//
// System.Web.UI.DataBinding.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	public sealed class DataBinding
	{
		string propertyName;
		string propertyType;
		string expression;

		public DataBinding (string propertyName, string propertyType,
				    string expression)
		{
			this.propertyName = propertyName;
			this.propertyType = propertyType;
			this.expression = expression;
		}

		public string Expression {
			get { return expression; }
		}

		public string PropertyName {
			get { return propertyName; }
		}

		public string PropertyType {
			get { return propertyType; }
		}

		public override bool Equals (object obj)
		{
			if (((DataBinding) obj).PropertyName == this.PropertyName)
				return true;
			else
				return false;
		}

		public override int GetHashCode ()
		{
			return propertyName.GetHashCode ();
		}
	}
}
