/**
 * Namespace: System.Web.UI
 * Class:     Pair
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Collections;
using System.Collections.Specialized;

namespace System.Web.UI
{
	public class Pair
	{
		public object First;
		public object Second;
		
		public Pair(object first, object second)
		{
			First  = first;
			Second = second;
		}
		
		public Pair()
		{
		}
	}
}
