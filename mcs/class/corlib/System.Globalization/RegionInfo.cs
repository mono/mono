//
// System.Globalization.RegionInfo.cs
//
// Authors:
//	Atsushi Enomoto  <atsushi@ximian.com>
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Globalization
{
	[System.Runtime.InteropServices.ComVisible(true)]
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public partial class RegionInfo
	{
		static RegionInfo currentRegion;

		public static RegionInfo CurrentRegion {
			get {
				var region = currentRegion;
				if (region == null) {
					CultureInfo ci = CultureInfo.CurrentCulture;
					if (ci != null) {
						region = new RegionInfo (ci);
					} else {
#if MONOTOUCH
						region = CreateFromNSLocale ();
#endif
					}

					if (Interlocked.CompareExchange (ref currentRegion, region, null) != null)
						region = currentRegion;
				}

				return region;
			}
		}
		
		// the following (instance) fields must be _first_ and stay synchronized
		// with the mono's MonoRegionInfo defined in mono/metadata/object-internals.h
#pragma warning disable 649
		int regionId;
		string iso2Name;
		string iso3Name;
		string win3Name;
		string englishName;
		string nativeName;
		string currencySymbol;
		string isoCurrencySymbol;
		string currencyEnglishName;
		string currencyNativeName;
#pragma warning restore 649
		
		public RegionInfo (int culture)
		{
			if (!GetByTerritory (CultureInfo.GetCultureInfo (culture)))
				throw new ArgumentException (
					String.Format ("Region ID {0} (0x{0:X4}) is not a supported region.", culture), "culture");
		}

		public RegionInfo (string name)
		{
			if (name == null)
				throw new ArgumentNullException ();

			if (construct_internal_region_from_name (name.ToUpperInvariant ())) {
				return;
			}
			if (!GetByTerritory (CultureInfo.GetCultureInfo (name)))
				throw new ArgumentException (String.Format ("Region name {0} is not supported.", name), "name");
		}

		RegionInfo (CultureInfo ci)
		{
			if (ci.LCID == CultureInfo.InvariantCultureId) {
				regionId = 244;
				iso2Name = "IV";
				iso3Name = "ivc";
				win3Name = "IVC";
				nativeName = englishName = "Invariant Country";
				currencySymbol = "\u00A4";
				isoCurrencySymbol ="XDR";
				currencyEnglishName = currencyNativeName = "International Monetary Fund";
				return;
			}

			if (ci.Territory == null)
				throw new NotImplementedException ("Neutral region info");

			construct_internal_region_from_name (ci.Territory.ToUpperInvariant ());
		}

		bool GetByTerritory (CultureInfo ci)
		{
			if (ci == null)
				throw new Exception ("INTERNAL ERROR: should not happen.");
			if (ci.IsNeutralCulture || ci.Territory == null)
				return false;

			return construct_internal_region_from_name (ci.Territory.ToUpperInvariant ());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_region_from_name (string name);

		[System.Runtime.InteropServices.ComVisible(false)]
		public virtual string CurrencyEnglishName {
			get { return currencyEnglishName; }
		}

		public virtual string CurrencySymbol {
			get { return currencySymbol; }
		}

		[MonoTODO ("DisplayName currently only returns the EnglishName")]
		public virtual string DisplayName {
			get { return englishName; }
		}

		public virtual string EnglishName {
			get { return englishName; }
		}

		[System.Runtime.InteropServices.ComVisible(false)]
		public virtual int GeoId {
			get { return regionId; }
		}

		public virtual bool IsMetric {
			get {
				switch (iso2Name) {
				case "US":
				case "UK":
					return false;
				default:
					return true;
				}
			}
		}

		public virtual string ISOCurrencySymbol {
			get { return isoCurrencySymbol; }
		}

		[ComVisible(false)]
		public virtual string NativeName {
			get { return nativeName; }
		}

		[ComVisible(false)]
		public virtual string CurrencyNativeName {
			get { return currencyNativeName; }
		}

		public virtual string Name {
			get { return iso2Name; }
		}

		public virtual string ThreeLetterISORegionName {
			get { return iso3Name; }
		}

		public virtual string ThreeLetterWindowsRegionName {
			get { return win3Name; }
		}
		
		public virtual string TwoLetterISORegionName {
			get { return iso2Name; }
		}

		public override bool Equals (object value)
		{
			RegionInfo other = value as RegionInfo;
			return other != null && Name == other.Name;
		}

		public override int GetHashCode ()
		{
			return Name.GetHashCode ();
		}

		public override string ToString ()
		{
			return Name;
		}

		internal static void ClearCachedData ()
		{
			currentRegion = null;
		}
	}
}
