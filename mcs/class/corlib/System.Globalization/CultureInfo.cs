//
// System.Globalization.CultureInfo.cs
//
// Miguel de Icaza (miguel@ximian.com)
// Dick Porter (dick@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc. (http://www.ximian.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Globalization
{
	[System.Runtime.InteropServices.ComVisible (true)]
	[Serializable]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		static volatile CultureInfo invariant_culture_info;
		static object shared_table_lock = new object ();
		internal static int BootstrapCultureID;

		const int NumOptionalCalendars = 5;
		const int GregorianTypeMask = 0x00FFFFFF;
		const int CalendarTypeBits = 24;

#pragma warning disable 169, 649
		bool m_isReadOnly;
		int  cultureID;
		[NonSerialized]
		int parent_lcid;
		[NonSerialized]
		int specific_lcid;
		[NonSerialized]
		int datetime_index;
		[NonSerialized]
		int number_index;
		bool m_useUserOverride;
		[NonSerialized]
		volatile NumberFormatInfo numInfo;
		volatile DateTimeFormatInfo dateTimeInfo;
		volatile TextInfo textInfo;
		private string m_name;
		
		[NonSerialized]
		private string displayname;
		[NonSerialized]
		private string englishname;
		[NonSerialized]
		private string nativename;
		[NonSerialized]
		private string iso3lang;
		[NonSerialized]
		private string iso2lang;
		[NonSerialized]
		private string icu_name;
		[NonSerialized]
		private string win3lang;
		[NonSerialized]
		private string territory;
		volatile CompareInfo compareInfo;
		[NonSerialized]
		private unsafe readonly int *calendar_data;
		[NonSerialized]
		private unsafe readonly void *textinfo_data;
		[NonSerialized]
		private Calendar [] optional_calendars;
		[NonSerialized]
		CultureInfo parent_culture;

		int m_dataItem;		// MS.NET serializes this.
		Calendar calendar;	// MS.NET serializes this.
#pragma warning restore 169, 649

		// Deserialized instances will set this to false
		[NonSerialized]
		bool constructed;

		[NonSerialized]
		// Used by Thread.set_CurrentCulture
		internal byte[] cached_serialized_form;
		
		const int InvariantCultureId = 0x7F;

		private static readonly string MSG_READONLY = "This instance is read only";
		
		static public CultureInfo InvariantCulture {
			get {
				return invariant_culture_info;
			}
		}

		static CultureInfo ()
		{
			invariant_culture_info = new CultureInfo (InvariantCultureId, false, true);
		}
		
		public static CultureInfo CreateSpecificCulture (string name)
		{
			if (name == null) {
				throw new ArgumentNullException ("name");
			}

			if (name == String.Empty)
				return InvariantCulture;

			CultureInfo ci = new CultureInfo ();
			if (!ConstructInternalLocaleFromSpecificName (ci, name.ToLowerInvariant ()))
				throw new ArgumentException ("Culture name " + name +
						" is not supported.", name);

			return ci;
		}

		public static CultureInfo CurrentCulture 
		{
			get {
				return Thread.CurrentThread.CurrentCulture;
			}
		}

		public static CultureInfo CurrentUICulture 
		{
			get {
				return Thread.CurrentThread.CurrentUICulture;
			}
		}

		internal static CultureInfo ConstructCurrentCulture ()
		{
			CultureInfo ci = new CultureInfo ();
			if (!ConstructInternalLocaleFromCurrentLocale (ci))
				ci = InvariantCulture;
			BootstrapCultureID = ci.cultureID;
			return ci;
		}

		internal static CultureInfo ConstructCurrentUICulture ()
		{
			return ConstructCurrentCulture ();
		}

		// it is used for RegionInfo.
		internal string Territory {
			get { return territory; }
		}

#if !NET_2_1
		// FIXME: It is implemented, but would be hell slow.
		[ComVisible (false)]
		public CultureTypes CultureTypes {
			get {
				CultureTypes ret = (CultureTypes) 0;
				foreach (CultureTypes v in Enum.GetValues (typeof (CultureTypes)))
					if (Array.IndexOf (GetCultures (v), this) >= 0)
						ret |= v;
				return ret;
			}
		}

		[ComVisible (false)]
		public CultureInfo GetConsoleFallbackUICulture ()
		{
			// as documented in MSDN ...
			switch (Name) {
			case "ar": case "ar-BH": case "ar-EG": case "ar-IQ":
			case "ar-JO": case "ar-KW": case "ar-LB": case "ar-LY":
			case "ar-QA": case "ar-SA": case "ar-SY": case "ar-AE":
			case "ar-YE":
			case "dv": case "dv-MV":
			case "fa": case "fa-IR":
			case "gu": case "gu-IN":
			case "he": case "he-IL":
			case "hi": case "hi-IN":
			case "kn": case "kn-IN":
			case "kok": case "kok-IN":
			case "mr": case "mr-IN":
			case "pa": case "pa-IN":
			case "sa": case "sa-IN":
			case "syr": case "syr-SY":
			case "ta": case "ta-IN":
			case "te": case "te-IN":
			case "th": case "th-TH":
			case "ur": case "ur-PK":
			case "vi": case "vi-VN":
				return GetCultureInfo ("en");
			case "ar-DZ": case "ar-MA": case "ar-TN":
				return GetCultureInfo ("fr");
			}
			return (CultureTypes & CultureTypes.WindowsOnlyCultures) != 0 ? CultureInfo.InvariantCulture : this;
		}

		[ComVisible (false)]
		public string IetfLanguageTag {
			// There could be more consistent way to implement
			// it, but right now it works just fine with this...
			get {
				switch (Name) {
				case "zh-CHS":
					return "zh-Hans";
				case "zh-CHT":
					return "zh-Hant";
				default:
					return Name;
				}
			}
		}

		// For specific cultures it basically returns LCID.
		// For neutral cultures it is mapped to the default(?) specific
		// culture, where the LCID of the specific culture seems to be
		// n + 1024 by default. zh-CHS is the only exception which is 
		// mapped to 2052, not 1028 (zh-CHT is mapped to 1028 instead).
		// There are very few exceptions, here I simply list them here.
		// It is Windows-specific property anyways, so no worthy of
		// trying to do some complex things with locale-builder.
		[ComVisible (false)]
		public virtual int KeyboardLayoutId {
			get {
				switch (LCID) {
				case 4: // zh-CHS (neutral)
					return 2052;
				case 1034: // es-ES Spanish 2
					return 3082;
				case 31748: // zh-CHT (neutral)
					return 1028;
				case 31770: // sr (neutral)
					return 2074;
				default:
					return LCID < 1024 ? LCID + 1024 : LCID;
				}
			}
		}
#endif

		public virtual int LCID {
			get {
				return cultureID;
			}
		}

		public virtual string Name {
			get {
#if MOONLIGHT
				if (m_name == "zh-CHS")
					return "zh-Hans";
				if (m_name == "zh-CHT")
					return "zh-Hant";
#endif
				return(m_name);
			}
		}

		public virtual string NativeName
		{
			get {
				if (!constructed) Construct ();
				return(nativename);
			}
		}
		
		public virtual Calendar Calendar
		{
			get { return DateTimeFormat.Calendar; }
		}

		public virtual Calendar[] OptionalCalendars
		{
			get {
				if (optional_calendars == null) {
					lock (this) {
						if (optional_calendars == null)
							ConstructCalendars ();
					}
				}
				return optional_calendars;
			}
		}

		public virtual CultureInfo Parent
		{
			get {
				if (parent_culture == null) {
					if (!constructed)
						Construct ();
					if (parent_lcid == cultureID)
						return null;
					
					if (parent_lcid == InvariantCultureId)
						parent_culture = InvariantCulture;
					else if (cultureID == InvariantCultureId)
						parent_culture = this;
					else
						parent_culture = new CultureInfo (parent_lcid);
				}
				return parent_culture;
			}
		}

		public virtual TextInfo TextInfo
		{
			get {
				if (textInfo == null) {
					if (!constructed) Construct ();
					lock (this) {
						if(textInfo == null) {
							textInfo = CreateTextInfo (m_isReadOnly);
						}
					}
				}
				
				return(textInfo);
			}
		}

		public virtual string ThreeLetterISOLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(iso3lang);
			}
		}

		public virtual string ThreeLetterWindowsLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(win3lang);
			}
		}

		public virtual string TwoLetterISOLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(iso2lang);
			}
		}

		public bool UseUserOverride
		{
			get {
				return m_useUserOverride;
			}
		}

		internal string IcuName {
			get {
				if (!constructed) Construct ();
				return icu_name;
			}
		}

		public void ClearCachedData()
		{
			Thread.CurrentThread.CurrentCulture = null;
			Thread.CurrentThread.CurrentUICulture = null;
		}

		public virtual object Clone()
		{
			if (!constructed) Construct ();
			CultureInfo ci=(CultureInfo)MemberwiseClone ();
			ci.m_isReadOnly=false;
			ci.cached_serialized_form=null;
			if (!IsNeutralCulture) {
				ci.NumberFormat = (NumberFormatInfo)NumberFormat.Clone ();
				ci.DateTimeFormat = (DateTimeFormatInfo)DateTimeFormat.Clone ();
			}
			return(ci);
		}

		public override bool Equals (object value)
		{
			CultureInfo b = value as CultureInfo;
			
			if (b != null)
				return b.cultureID == cultureID;
			return false;
		}

#if !MOONLIGHT
		public static CultureInfo[] GetCultures(CultureTypes types)
		{
			bool neutral=((types & CultureTypes.NeutralCultures)!=0);
			bool specific=((types & CultureTypes.SpecificCultures)!=0);
			bool installed=((types & CultureTypes.InstalledWin32Cultures)!=0);  // TODO

			CultureInfo [] infos = internal_get_cultures (neutral, specific, installed);
			// The runtime returns a NULL in the first position of the array when
			// 'neutral' is true. We fill it in with a clone of InvariantCulture
			// since it must not be read-only
			if (neutral && infos.Length > 0 && infos [0] == null) {
				infos [0] = (CultureInfo) InvariantCulture.Clone ();
			}

			return infos;
		}
#endif

		public override int GetHashCode()
		{
			return cultureID;
		}

		public static CultureInfo ReadOnly(CultureInfo ci)
		{
			if(ci==null) {
				throw new ArgumentNullException("ci");
			}

			if(ci.m_isReadOnly) {
				return(ci);
			} else {
				CultureInfo new_ci=(CultureInfo)ci.Clone ();
				new_ci.m_isReadOnly=true;
				if (new_ci.numInfo != null)
					new_ci.numInfo = NumberFormatInfo.ReadOnly (new_ci.numInfo);
				if (new_ci.dateTimeInfo != null)
					new_ci.dateTimeInfo = DateTimeFormatInfo.ReadOnly (new_ci.dateTimeInfo);
				// TextInfo doesn't have a ReadOnly method in 1.1...
				if (new_ci.textInfo != null)
					new_ci.textInfo = TextInfo.ReadOnly (new_ci.textInfo);
				return(new_ci);
			}
		}

		public override string ToString()
		{
			return(m_name);
		}
		
		public virtual CompareInfo CompareInfo
		{
			get {
				if(compareInfo==null) {
					if (!constructed)
						Construct ();

					lock (this) {
						if(compareInfo==null) {
							compareInfo=new CompareInfo (this);
						}
					}
				}
				
				return(compareInfo);
			}
		}

		internal static bool IsIDNeutralCulture (int lcid)
		{
			bool ret;
			if (!internal_is_lcid_neutral (lcid, out ret))
				throw new ArgumentException (String.Format ("Culture id 0x{:x4} is not supported.", lcid));
				
			return ret;
		}

		public virtual bool IsNeutralCulture {
			get {
				if (!constructed) Construct ();
				if (cultureID == InvariantCultureId)
					return false;

				return ((cultureID & 0xff00) == 0 || specific_lcid == 0);
			}
		}

		internal void CheckNeutral ()
		{
#if !MOONLIGHT && !NET_4_0
			if (IsNeutralCulture) {
				throw new NotSupportedException ("Culture \"" + m_name + "\" is " +
						"a neutral culture. It can not be used in formatting " +
						"and parsing and therefore cannot be set as the thread's " +
						"current culture.");
			}
#endif
		}

		public virtual NumberFormatInfo NumberFormat {
			get {
				if (!constructed) Construct ();
				CheckNeutral ();
				if (numInfo == null){
					lock (this){
						if (numInfo == null) {
							numInfo = new NumberFormatInfo (m_isReadOnly);
							construct_number_format ();
						}
					}
				}

				return numInfo;
			}

			set {
				if (!constructed) Construct ();
				if (m_isReadOnly) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("NumberFormat");
				
				numInfo = value;
			}
		}

		public virtual DateTimeFormatInfo DateTimeFormat
		{
			get 
			{
				if (!constructed) Construct ();
				CheckNeutral ();
				if (dateTimeInfo == null)
				{
					lock (this)
					{
						if (dateTimeInfo == null) {
							dateTimeInfo = new DateTimeFormatInfo(m_isReadOnly);
							construct_datetime_format ();
							if (optional_calendars != null)
								dateTimeInfo.Calendar = optional_calendars [0];
						}
					}
				}

				return dateTimeInfo;
			}

			set 
			{
				if (!constructed) Construct ();
				if (m_isReadOnly) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("DateTimeFormat");
				
				dateTimeInfo = value;
			}
		}

		public virtual string DisplayName
		{
			get {
				if (!constructed) Construct ();
				return(displayname);
			}
		}

		public virtual string EnglishName
		{
			get {
				if (!constructed) Construct ();
				return(englishname);
			}
		}

		public static CultureInfo InstalledUICulture
		{
			get { return GetCultureInfo (BootstrapCultureID); }
		}
		public bool IsReadOnly 
		{
			get {
				return(m_isReadOnly);
			}
		}
		

		// 
		// IFormatProvider implementation
		//
		public virtual object GetFormat( Type formatType )
		{
			object format = null;

			if ( formatType == typeof(NumberFormatInfo) )
				format = NumberFormat;
			else if ( formatType == typeof(DateTimeFormatInfo) )
				format = DateTimeFormat;
			
			return format;
		}
		
		void Construct ()
		{
			construct_internal_locale_from_lcid (cultureID);
			constructed = true;
		}

		bool ConstructInternalLocaleFromName (string locale)
		{
			// It is sort of hack to get those new pseudo-alias
			// culture names that are not supported in good old
			// Windows.
#if MOONLIGHT
			if (locale == "zh-chs" || locale == "zh-cht")
				return false;
#endif
			switch (locale) {
			case "zh-hans":
				locale = "zh-chs";
				break;
			case "zh-hant":
				locale = "zh-cht";
				break;
			}

			if (!construct_internal_locale_from_name (locale))
				return false;
			return true;
		}

		bool ConstructInternalLocaleFromLcid (int lcid)
		{
			if (!construct_internal_locale_from_lcid (lcid))
				return false;
			return true;
		}

		static bool ConstructInternalLocaleFromSpecificName (CultureInfo ci, string name)
		{
			if (!construct_internal_locale_from_specific_name (ci, name))
				return false;
			return true;
		}

		static bool ConstructInternalLocaleFromCurrentLocale (CultureInfo ci)
		{
			if (!construct_internal_locale_from_current_locale (ci))
				return false;
			return true;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_locale_from_lcid (int lcid);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool construct_internal_locale_from_name (string name);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool construct_internal_locale_from_specific_name (CultureInfo ci,
				string name);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool construct_internal_locale_from_current_locale (CultureInfo ci);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static CultureInfo [] internal_get_cultures (bool neutral, bool specific, bool installed);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_datetime_format ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_number_format ();

		// Returns false if the culture can not be found, sets is_neutral if it is
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool internal_is_lcid_neutral (int lcid, out bool is_neutral);

		private void ConstructInvariant (bool read_only)
		{
			cultureID = InvariantCultureId;

			/* NumberFormatInfo defaults to the invariant data */
			numInfo=NumberFormatInfo.InvariantInfo;
			/* DateTimeFormatInfo defaults to the invariant data */
			dateTimeInfo=DateTimeFormatInfo.InvariantInfo;

			if (!read_only) {
				numInfo = (NumberFormatInfo) numInfo.Clone ();
				dateTimeInfo = (DateTimeFormatInfo) dateTimeInfo.Clone ();
			}

			textInfo = CreateTextInfo (read_only);

			m_name=String.Empty;
			displayname=
			englishname=
			nativename="Invariant Language (Invariant Country)";
			iso3lang="IVL";
			iso2lang="iv";
			icu_name="en_US_POSIX";
			win3lang="IVL";
		}

		private unsafe TextInfo CreateTextInfo (bool readOnly)
		{
			return new TextInfo (this, cultureID, this.textinfo_data, readOnly);
		}

		public CultureInfo (int culture) : this (culture, true) {}

		public CultureInfo (int culture, bool useUserOverride) :
			this (culture, useUserOverride, false) {}

		private CultureInfo (int culture, bool useUserOverride, bool read_only)
		{
			if (culture < 0)
				throw new ArgumentOutOfRangeException ("culture", "Positive "
					+ "number required.");

			constructed = true;
			m_isReadOnly = read_only;
			m_useUserOverride = useUserOverride;

			if (culture == InvariantCultureId) {
				/* Short circuit the invariant culture */
				ConstructInvariant (read_only);
				return;
			}

			if (!ConstructInternalLocaleFromLcid (culture)) {
#if NET_4_0
				throw new CultureNotFoundException ("culture", 
					String.Format ("Culture ID {0} (0x{0:X4}) is not a " +
							"supported culture.", culture));
#else
				throw new ArgumentException (
					String.Format ("Culture ID {0} (0x{0:X4}) is not a " +
							"supported culture.", culture), "culture");
#endif
			}
		}

		public CultureInfo (string name) : this (name, true) {}

		public CultureInfo (string name, bool useUserOverride) :
			this (name, useUserOverride, false) {}

		private CultureInfo (string name, bool useUserOverride, bool read_only)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			constructed = true;
			m_isReadOnly = read_only;
			m_useUserOverride = useUserOverride;

			if (name.Length == 0) {
				/* Short circuit the invariant culture */
				ConstructInvariant (read_only);
				return;
			}

			if (!ConstructInternalLocaleFromName (name.ToLowerInvariant ())) {
#if NET_4_0
				throw new CultureNotFoundException ("name",
						"Culture name " + name + " is not supported.");
#else
				throw new ArgumentException ("Culture name " + name +
						" is not supported.", "name");
#endif
			}
		}

		// This is used when creating by specific name and creating by
		// current locale so we can initialize the object without
		// doing any member initialization
		private CultureInfo () { constructed = true; }
		static Hashtable shared_by_number, shared_by_name;
		
		static void insert_into_shared_tables (CultureInfo c)
		{
			if (shared_by_number == null){
				shared_by_number = new Hashtable ();
				shared_by_name = new Hashtable ();
			}
			shared_by_number [c.cultureID] = c;
			shared_by_name [c.m_name] = c;
		}
		
		public static CultureInfo GetCultureInfo (int culture)
		{
			CultureInfo c;
			
			lock (shared_table_lock){
				if (shared_by_number != null){
					c = shared_by_number [culture] as CultureInfo;

					if (c != null)
						return (CultureInfo) c;
				}
				c = new CultureInfo (culture, false, true);
				insert_into_shared_tables (c);
				return c;
			}
		}

		public static CultureInfo GetCultureInfo (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			CultureInfo c;
			lock (shared_table_lock){
				if (shared_by_name != null){
					c = shared_by_name [name] as CultureInfo;

					if (c != null)
						return (CultureInfo) c;
				}
				c = new CultureInfo (name, false, true);
				insert_into_shared_tables (c);
				return c;
			}
		}

		[MonoTODO ("Currently it ignores the altName parameter")]
		public static CultureInfo GetCultureInfo (string name, string altName) {
			if (name == null)
				throw new ArgumentNullException ("null");
			if (altName == null)
				throw new ArgumentNullException ("null");

			return GetCultureInfo (name);
		}

		public static CultureInfo GetCultureInfoByIetfLanguageTag (string name)
		{
			// There could be more consistent way to implement
			// it, but right now it works just fine with this...
			switch (name) {
			case "zh-Hans":
				return GetCultureInfo ("zh-CHS");
			case "zh-Hant":
				return GetCultureInfo ("zh-CHT");
			default:
				return GetCultureInfo (name);
			}
		}

		// used in runtime (icall.c) to construct CultureInfo for
		// AssemblyName of assemblies
		internal static CultureInfo CreateCulture (string name, bool reference)
		{
			bool read_only;
			bool use_user_override;

			bool invariant = name.Length == 0;
			if (reference) {
				use_user_override = invariant ? false : true;
				read_only = false;
			} else {
				read_only = false;
				use_user_override = invariant ? false : true;
			}

			return new CultureInfo (name, use_user_override, read_only);
		}

		unsafe internal void ConstructCalendars ()
		{
			if (calendar_data == null) {
				optional_calendars = new Calendar [] {new GregorianCalendar (GregorianCalendarTypes.Localized)};
				return;
			}

			optional_calendars = new Calendar [NumOptionalCalendars];

			for (int i=0; i<NumOptionalCalendars; i++) {
				Calendar cal = null;
				int caldata = *(calendar_data + i);
				int caltype = (caldata >> CalendarTypeBits);
				switch (caltype) {
				case 0:
					GregorianCalendarTypes greg_type;
					greg_type = (GregorianCalendarTypes) (caldata & GregorianTypeMask);
					cal = new GregorianCalendar (greg_type);
					break;
				case 1:
					cal = new HijriCalendar ();
					break;
				case 2:
					cal = new ThaiBuddhistCalendar ();
					break;
				default:
					throw new Exception ("invalid calendar type:  " + caldata);
				}
				optional_calendars [i] = cal;
			}
		}
	}
}
