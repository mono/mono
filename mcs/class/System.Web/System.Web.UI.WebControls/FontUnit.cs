/**
 * Namespace: System.Web.UI.WebControls
 * Struct:    FontUnit
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public struct FontUnit
	{
		public static readonly FontUnit Empty   = new FontUnit();
		public static readonly FontUnit Large   = new FontUnit(FontSize.Large);
		public static readonly FontUnit Larger  = new FontUnit(FontSize.Larger);
		public static readonly FontUnit Medium  = new FontUnit(FontSize.Medium);
		public static readonly FontUnit Small   = new FontUnit(FontSize.Small);
		public static readonly FontUnit Smaller = new FontUnit(FontSize.Smaller);
		public static readonly FontUnit XLarge  = new FontUnit(FontSize.XLarge);
		public static readonly FontUnit XSmall  = new FontUnit(FontSize.XSmall);
		public static readonly FontUnit XXLarge = new FontUnit(FontSize.XXLarge);
		public static readonly FontUnit XXSmall = new FontUnit(FontSize.XXSmall);

		private FontSize type;
		private Unit     val;

		public FontUnit(FontSize type)
		{
			if(!Enum.IsDefined(typeof(FontSize), type))
				throw new ArgumentException();
			this.type = type;
			if(this.type == FontSize.AsUnit)
			{
				val = Unit.Point(10);
			} else
			{
				val = Unit.Empty;
			}
		}

		public FontUnit(int value)
		{
			type = FontSize.AsUnit;
			val = Unit.Point(value);
		}

		public FontUnit(string value): this(value, CultureInfo.CurrentCulture)
		{
		}

		public FontUnit(Unit value)
		{
			if(val.IsEmpty)
			{
				type = FontSize.NotSet;
				val  = Unit.Empty;
			} else
			{
				type = FontSize.AsUnit;
				val  = value;
			}
		}

		public FontUnit(string value, CultureInfo culture)
		{
			type = FontSize.NotSet;
			val  = Unit.Empty;
			if(value != null && value != String.Empty)
			{
				string low = value.ToLower(culture);
				int index = GetTypeFromString(low);
				if( index != -1)
				{
					type = (FontSize)fs;
					return;
				} else
				{
					val = new Unit(value, culture, UnitType.Point);
					type = FontSize.AsUnit;
				}
			}
		}

		private int GetTypeFromString(string strVal)
		{
			string[] values = {
				"smaller",
				"larger",
				"xx-small",
				"x-small",
				"small",
				"medium",
				"large",
				"xlarge",
				"xxlarge"
			}
			int i = 0;
			foreach(string valType in values)
			{
				if(strVal == valType)
				{
					return (i + 2);
				}
				i++;
			}
			return -1;
		}

		public static FontUnit Parse(string s)
		{
			Parse(s, CultureInfo.CurrentCulture);
		}

		public static FontUnit Parse(string s, CultureInfo culture)
		{
			return new FontUnit(s, culture);
		}

		public static FontUnit Point(int n)
		{
			return new FontUnit(n);
		}

		public static bool operator ==(FontUnit left, FontUnit right)
		{
			return (left.type == right.type && left.val == right.val);
		}

		public static bool operator !=(FontUnit left, FontUnit right)
		{
			return !(left == right);
		}

		public static implicit operator FontUnit(int n)
		{
			return FontUnit.Point(n);
		}

		public override bool Equals(object obj)
		{
			if(obj!= null && obj is FontUnit)
			{
				FontUnit that = (FontUnit)obj;
				return (this.type == that.type && this.val == that.val);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ( (type.GetHashCode() << 2) | val.GetHashCode() );
		}

		public override string ToString()
		{
			ToString(CultureInfo.CurrentCulture);
		}

		public override string ToString(CultureInfo culture)
		{
			if(IsEmpty)
			{
				return String.Empty;
			}
			//string strRepr = String.Empty;
			switch(type)
			{
				case FontSize.AsUnit:  return val.ToString(culture);
				case FontSize.XXSmall: return "XX-Small";
				case FontSize.XSmall:  return "X-Small";
				case FontSize.XLarge:  return "X-Large";
				case FontSize.XXLarge: return "XX-Large";
				default:               return PropertyConverter.EnumToString(typeof(FontSize), type);
			}
		}

		public bool IsEmpty
		{
			get
			{
				return (type == FontSize.NotSet);
			}
		}

		public FontSize Type
		{
			get
			{
				return type;
			}
		}

		public Unit Unit
		{
			get
			{
				return val;
			}
		}
	}
}
