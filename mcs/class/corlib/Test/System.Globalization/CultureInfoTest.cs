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
using System.Threading;

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

			Assert ("InvariantCulture not found in the array from GetCultures()", false);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TrySetNeutralCultureNotInvariant ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("ar");
		}

		[Test]
		// make sure that all CultureInfo holds non-null calendars.
		public void OptionalCalendars ()
		{
			foreach (CultureInfo ci in CultureInfo.GetCultures (
				CultureTypes.AllCultures))
				AssertNotNull (String.Format ("{0} {1}",
					ci.LCID, ci.Name), ci.OptionalCalendars);
		}

		[Test]
		public void CloneNeutral () // bug #77347
		{
			CultureInfo culture = new CultureInfo ("en");
			CultureInfo cultureClone = culture.Clone () as CultureInfo;
			Assert (culture.Equals (cultureClone));
		}
	}
}

