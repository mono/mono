//
// System.Globalization.CultureInfo Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2005 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System.IO;
using System;
using System.Globalization;

namespace MonoTests.System.Globalization
{
	[TestFixture]
	public class CultureInfoTest : Assertion
	{
		[Test]
		public void GetAllCulturesInvariant () // bug #72081
		{
			CultureInfo invariant = CultureInfo.InvariantCulture;
			CultureInfo [] infos = CultureInfo.GetCultures (CultureTypes.AllCultures);
			foreach (CultureInfo culture in infos) {
				if (culture.Equals (invariant))
					return;
			}

			Assert (true);
		}
	}
}

