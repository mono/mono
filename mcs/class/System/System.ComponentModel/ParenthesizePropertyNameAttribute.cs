//
// System.ComponentModel.ParenthesizePropertyNameAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class ParenthesizePropertyNameAttribute : Attribute
	{

        private bool parenthesis;

		public static readonly ParenthesizePropertyNameAttribute Default = new ParenthesizePropertyNameAttribute();


		public ParenthesizePropertyNameAttribute()
		{
			this.parenthesis = false;
		}

		public ParenthesizePropertyNameAttribute (bool needParenthesis)
		{
			this.parenthesis = needParenthesis;
		}

		public bool NeedParenthesis {
			get { return parenthesis; }
		}

		public override bool Equals (object o)
		{
			if (!(o is ParenthesizePropertyNameAttribute))
				return false;
			if (o == this)
				return true;
			return ((ParenthesizePropertyNameAttribute) o).NeedParenthesis == parenthesis;
		}

		public override int GetHashCode()
		{
			return parenthesis.GetHashCode ();
		}

		public override bool IsDefaultAttribute()
		{
			return parenthesis == ParenthesizePropertyNameAttribute.Default.NeedParenthesis;
		}
	}
}

