//
// System.Web.UI.DataBinding.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;

namespace System.Web.UI {

	public sealed class DataBinding
	{
		string propertyName;
		Type propertyType;
		string expression;

		public DataBinding (string propertyName, Type propertyType,
				    string expression)
		{
			this.propertyName = propertyName;
			this.propertyType = propertyType;
			this.expression = expression;
		}

		public string Expression {
			get { return expression; }
			set { expression = value; }
		}

		public string PropertyName {
			get { return propertyName; }
		}

		public Type PropertyType {
			get { return propertyType; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is DataBinding))
				return false;
			
			DataBinding o = (DataBinding) obj;
			return (o.Expression == expression &&
				o.PropertyName == propertyName &&
				o.PropertyType == propertyType);
		}

		public override int GetHashCode ()
		{
			return propertyName.GetHashCode () +
			       (propertyType.GetHashCode () << 1) +
			       (expression.GetHashCode () << 2) ;
		}
	}
}
