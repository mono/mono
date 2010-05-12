using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;

using ApplicationPreStartMethods;

namespace ApplicationPreStartMethods.Tests
{
	public class PreStartMethods
	{
		public void PublicInstanceMethod (string param)
		{
		}

		public static void PublicStaticMethod ()
		{
			throw new InvalidOperationException ("test");
		}

		public static void PublicStaticMethod (string val)
		{
		}

		internal void InternalInstanceMethod ()
		{
		}

		static internal void InternalStaticMethod ()
		{
		}

		void PrivateInstanceMethod ()
		{
		}

		static void PrivateStaticMethod ()
		{
		}
	}
}