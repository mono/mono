/**
 * Namespace: System.Web.Util
 * Class:     WebEqualComparer
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Globalization;
using System.Collections;

namespace System.Web.Util
{
	public class WebEqualComparer : IComparer
	{
		private static IComparer defC;

		public WebEqualComparer()
		{
		}

		public static IComparer Default
		{
			get
			{
				if(defC == null)
				{
					defC = new WebEqualComparer();
				}
				return defC;
			}
		}

		/// <summary>
		/// To compare two strings
		/// </summary>
		/// <remarks>
		/// Cannot apply String.Compare(..) since I am at web
		/// </remarks>
		int IComparer.Compare(object left, object right)
		{
			string leftStr, rightStr;
			leftStr = null;
			rightStr = null;
			if(left is string)
			{
				leftStr = (string)left;
			}
			if(right is string)
			{
				rightStr = (string)right;
			}

			if(leftStr==null || rightStr==null)
			{
				throw new ArgumentException();
			}

			int ll = leftStr.Length;
			int lr = rightStr.Length;
			if(ll==0 && lr==0)
			{
				return 0;
			}

			if(ll==0 || lr==0)
			{
				return ( (ll > 0) ? 1 : -1);
			}

			char cl,cr;
			int i=0;
			for(i=0; i < leftStr.Length; i++)
			{
				if(i==lr)
				{
					return 1;
				}
				cl = leftStr[i];
				cr = leftStr[i];
				if(cl==cr)
				{
					continue;
				}
				UnicodeCategory ucl = Char.GetUnicodeCategory(cl);
				UnicodeCategory ucr = Char.GetUnicodeCategory(cr);
				if(ucl==ucr)
				{
					return ( (cl > cr) ? 1 : -1 );
				}
				cl = Char.ToLower(cl);
				cr = Char.ToLower(cr);
				if(cl!=cr)
				{
					return ( (cl > cr) ? 1 : -1);
				}
			}
			return ( (i==lr) ? 0 : -1 );
		}
	}
}
