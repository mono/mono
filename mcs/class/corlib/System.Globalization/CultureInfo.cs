//
// System.Globalization.CultureInfo.cs
//
// Authors:
// Miguel de Icaza (miguel@ximian.com)
// Dick Porter (dick@ximian.com)
// Marek Safar (marek.safar@gmail.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Globalization
{
	[System.Runtime.InteropServices.ComVisible (true)]
	[Serializable]
	[StructLayout (LayoutKind.Sequential)]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		static volatile CultureInfo invariant_culture_info = new CultureInfo (InvariantCultureId, false, true);
		static object shared_table_lock = new object ();
		internal static int BootstrapCultureID;

#pragma warning disable 169, 649
		bool m_isReadOnly;
		int  cultureID;
		[NonSerialized]
		int parent_lcid;
		[NonSerialized]
		int datetime_index;
		[NonSerialized]
		int number_index;
		[NonSerialized]
		int default_calendar_type;
		bool m_useUserOverride;
		[NonSerialized]
		volatile NumberFormatInfo numInfo;
		volatile DateTimeFormatInfo dateTimeInfo;
		volatile TextInfo textInfo;
		private string m_name;
		
		[NonSerialized]
		private string englishname;
		[NonSerialized]
		private string nativename;
		[NonSerialized]
		private string iso3lang;
		[NonSerialized]
		private string iso2lang;
		[NonSerialized]
		private string win3lang;
		[NonSerialized]
		private string territory;
		[NonSerialized]
		string[] native_calendar_names;

		volatile CompareInfo compareInfo;
		[NonSerialized]
		private unsafe readonly void *textinfo_data;

		int m_dataItem;		// MS.NET serializes this.
#pragma warning restore 169, 649

		Calendar calendar;

		[NonSerialized]
		CultureInfo parent_culture;

		// Deserialized instances will set this to false
		[NonSerialized]
		bool constructed;

		[NonSerialized]
		// Used by Thread.set_CurrentCulture
		internal byte[] cached_serialized_form;
		
		const int InvariantCultureId = 0x7F;
		const int CalendarTypeBits = 8;

		const string MSG_READONLY = "This instance is read only";
		
		public static CultureInfo InvariantCulture {
			get {
				return invariant_culture_info;
			}
		}

		public static CultureInfo CurrentCulture {
			get {
				return Thread.CurrentThread.CurrentCulture;
			}
		}

		public static CultureInfo CurrentUICulture { 
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
				return(m_name);
			}
		}

		public virtual string NativeName {
			get {
				if (!constructed) Construct ();
				return nativename;
			}
		}

		internal string NativeCalendarName {
			get {
				if (!constructed) Construct ();
				return native_calendar_names[(default_calendar_type >> CalendarTypeBits) - 1];
			}
		}
		
		public virtual Calendar Calendar {
			get {
				if (calendar == null) {
					if (!constructed) Construct ();
					calendar = CreateCalendar (default_calendar_type);
				}

				return calendar;
			}
		}

		[MonoLimitation ("Optional calendars are not supported only default calendar is returned")]
		public virtual Calendar[] OptionalCalendars {
			get {
				return new[] { Calendar };
			}
		}

		public virtual CultureInfo Parent
		{
			get {
				if (parent_culture == null) {
					if (!constructed)
						Construct ();
					if (parent_lcid == cultureID) {
						//
						// Parent lcid is same but culture info is not for legacy zh culture
						//
						if (parent_lcid == 0x7C04 && EnglishName.EndsWith (" Legacy", StringComparison.Ordinal))
							return parent_culture = new CultureInfo ("zh-Hant");

						return null;
					}
					
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

		public virtual string ThreeLetterISOLanguageName {
			get {
				if (!constructed) Construct ();
				return iso3lang;
			}
		}

		public virtual string ThreeLetterWindowsLanguageName
		{
			get {
				if (!constructed) Construct ();
				return(win3lang);
			}
		}

		public virtual string TwoLetterISOLanguageName {
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

		public override int GetHashCode ()
		{
			return cultureID.GetHashCode ();
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

		public virtual bool IsNeutralCulture {
			get {
				if (cultureID == InvariantCultureId)
					return false;

				if (!constructed) Construct ();
				return territory == null;
			}
		}

		internal void CheckNeutral ()
		{
#if !NET_4_0
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

		public virtual DateTimeFormatInfo DateTimeFormat {
			get {
				if (dateTimeInfo != null)
					return dateTimeInfo;

				if (!constructed) Construct ();
				CheckNeutral ();

				// TODO: Have to lock because construct_datetime_format is not atomic
				lock (this) {
					if (cultureID == InvariantCultureId && m_isReadOnly)
						dateTimeInfo = DateTimeFormatInfo.InvariantInfo;
					else if (dateTimeInfo == null) {
						dateTimeInfo = new DateTimeFormatInfo (this, m_isReadOnly);
						if (cultureID != InvariantCultureId)
							construct_datetime_format ();
					}
				}

				return dateTimeInfo;
			}

			set {
				if (!constructed) Construct ();
				if (m_isReadOnly) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("DateTimeFormat");
				
				dateTimeInfo = value;
			}
		}

		public virtual string DisplayName {
			get {
				// Mono is not localized and will always return english name regardless of OS locale
				return EnglishName;
			}
		}

		public virtual string EnglishName {
			get {
				if (!constructed) Construct ();
				return englishname;
			}
		}

		public static CultureInfo InstalledUICulture
		{
			get { return GetCultureInfo (BootstrapCultureID); }
		}

		public bool IsReadOnly {
			get {
				return m_isReadOnly;
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

//		[MethodImplAttribute (MethodImplOptions.InternalCall)]
//		private extern static bool construct_internal_locale_from_specific_name (CultureInfo ci, string name);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static bool construct_internal_locale_from_current_locale (CultureInfo ci);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static CultureInfo [] internal_get_cultures (bool neutral, bool specific, bool installed);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_datetime_format ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_number_format ();

		private void ConstructInvariant (bool read_only)
		{
			cultureID = InvariantCultureId;

			/* NumberFormatInfo defaults to the invariant data */
			numInfo=NumberFormatInfo.InvariantInfo;

			if (!read_only) {
				numInfo = (NumberFormatInfo) numInfo.Clone ();
			}

			textInfo = CreateTextInfo (read_only);

			m_name=String.Empty;
			englishname=
			nativename="Invariant Language (Invariant Country)";
			iso3lang="IVL";
			iso2lang="iv";
			win3lang="IVL";
			default_calendar_type = 1 << CalendarTypeBits;
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

			if (!construct_internal_locale_from_lcid (culture)) {
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

			if (!construct_internal_locale_from_name (name.ToLowerInvariant ())) {
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
		static Dictionary<int, CultureInfo> shared_by_number;
		static Dictionary<string, CultureInfo> shared_by_name;
		
		static void insert_into_shared_tables (CultureInfo c)
		{
			if (shared_by_number == null){
				shared_by_number = new Dictionary<int, CultureInfo> ();
				shared_by_name = new Dictionary<string, CultureInfo> ();
			}
			shared_by_number [c.cultureID] = c;
			shared_by_name [c.m_name] = c;
		}
		
		public static CultureInfo GetCultureInfo (int culture)
		{
			CultureInfo c;
			
			lock (shared_table_lock){
				if (shared_by_number != null) {
					if (shared_by_number.TryGetValue (culture, out c))
						return c;
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
					if (shared_by_name.TryGetValue (name, out c))
						return c;
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

		public static CultureInfo CreateSpecificCulture (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				return InvariantCulture;

			CultureInfo ci = null;
			try {
				ci = new CultureInfo (name);
			} catch (Exception) {
				// TODO: Use construct_internal_locale_from_name when it's not bound to constructor instead
				// of try-catch
				int idx = name.IndexOf ('-');
				if (idx > 0) {
					try {
						ci = new CultureInfo (name.Substring (0, idx));
					} catch {
					}
				}
				
				if (ci == null)
					throw;
			}

			if (!ci.IsNeutralCulture)
				return ci;

			return CreateSpecificCultureFromNeutral (ci.Name);
		}

		//
		// Creates specific culture from neutral culture. Used by CreateSpecificCulture
		// only but using separate method we can delay switch underlying Dictionary
		// initialization
		//
		static CultureInfo CreateSpecificCultureFromNeutral (string name)
		{
			int id;

			//
			// For neutral cultures find predefined default specific culture
			//
			// Use managed switch because we need this for only some cultures
			// and the method is not used frequently
			//
			// TODO: We could optimize for cultures with single specific culture 
			//
			switch (name.ToLowerInvariant ()) {
			case "af": id = 1078; break;
			case "am": id = 1118; break;
			case "ar": id = 1025; break;
			case "arn": id = 1146; break;
			case "as": id = 1101; break;
			case "az": id = 1068; break;
			case "az-Cyrl": id = 2092; break;
			case "az-Latn": id = 1068; break;
			case "ba": id = 1133; break;
			case "be": id = 1059; break;
			case "bg": id = 1026; break;
			case "bn": id = 1093; break;
			case "bo": id = 1105; break;
			case "br": id = 1150; break;
			case "bs": id = 5146; break;
			case "bs-Cyrl": id = 8218; break;
			case "bs-Latn": id = 5146; break;
			case "ca": id = 1027; break;
			case "co": id = 1155; break;
			case "cs": id = 1029; break;
			case "cy": id = 1106; break;
			case "da": id = 1030; break;
			case "de": id = 1031; break;
			case "dsb": id = 2094; break;
			case "dv": id = 1125; break;
			case "el": id = 1032; break;
			case "en": id = 1033; break;
			case "es": id = 3082; break;
			case "et": id = 1061; break;
			case "eu": id = 1069; break;
			case "fa": id = 1065; break;
			case "fi": id = 1035; break;
			case "fil": id = 1124; break;
			case "fo": id = 1080; break;
			case "fr": id = 1036; break;
			case "fy": id = 1122; break;
			case "ga": id = 2108; break;
			case "gd": id = 1169; break;
			case "gl": id = 1110; break;
			case "gsw": id = 1156; break;
			case "gu": id = 1095; break;
			case "ha": id = 1128; break;
			case "ha-Latn": id = 1128; break;
			case "he": id = 1037; break;
			case "hi": id = 1081; break;
			case "hr": id = 1050; break;
			case "hsb": id = 1070; break;
			case "hu": id = 1038; break;
			case "hy": id = 1067; break;
			case "id": id = 1057; break;
			case "ig": id = 1136; break;
			case "ii": id = 1144; break;
			case "is": id = 1039; break;
			case "it": id = 1040; break;
			case "iu": id = 2141; break;
			case "iu-Cans": id = 1117; break;
			case "iu-Latn": id = 2141; break;
			case "ja": id = 1041; break;
			case "ka": id = 1079; break;
			case "kk": id = 1087; break;
			case "kl": id = 1135; break;
			case "km": id = 1107; break;
			case "kn": id = 1099; break;
			case "ko": id = 1042; break;
			case "kok": id = 1111; break;
			case "ky": id = 1088; break;
			case "lb": id = 1134; break;
			case "lo": id = 1108; break;
			case "lt": id = 1063; break;
			case "lv": id = 1062; break;
			case "mi": id = 1153; break;
			case "mk": id = 1071; break;
			case "ml": id = 1100; break;
			case "mn": id = 1104; break;
			case "mn-Cyrl": id = 1104; break;
			case "mn-Mong": id = 2128; break;
			case "moh": id = 1148; break;
			case "mr": id = 1102; break;
			case "ms": id = 1086; break;
			case "mt": id = 1082; break;
			case "nb": id = 1044; break;
			case "ne": id = 1121; break;
			case "nl": id = 1043; break;
			case "nn": id = 2068; break;
			case "no": id = 1044; break;
			case "nso": id = 1132; break;
			case "oc": id = 1154; break;
			case "or": id = 1096; break;
			case "pa": id = 1094; break;
			case "pl": id = 1045; break;
			case "prs": id = 1164; break;
			case "ps": id = 1123; break;
			case "pt": id = 1046; break;
			case "qut": id = 1158; break;
			case "quz": id = 1131; break;
			case "rm": id = 1047; break;
			case "ro": id = 1048; break;
			case "ru": id = 1049; break;
			case "rw": id = 1159; break;
			case "sa": id = 1103; break;
			case "sah": id = 1157; break;
			case "se": id = 1083; break;
			case "si": id = 1115; break;
			case "sk": id = 1051; break;
			case "sl": id = 1060; break;
			case "sma": id = 7227; break;
			case "smj": id = 5179; break;
			case "smn": id = 9275; break;
			case "sms": id = 8251; break;
			case "sq": id = 1052; break;
			case "sr": id = 9242; break;
			case "sr-Cyrl": id = 10266; break;
			case "sr-Latn": id = 9242; break;
			case "sv": id = 1053; break;
			case "sw": id = 1089; break;
			case "syr": id = 1114; break;
			case "ta": id = 1097; break;
			case "te": id = 1098; break;
			case "tg": id = 1064; break;
			case "tg-Cyrl": id = 1064; break;
			case "th": id = 1054; break;
			case "tk": id = 1090; break;
			case "tn": id = 1074; break;
			case "tr": id = 1055; break;
			case "tt": id = 1092; break;
			case "tzm": id = 2143; break;
			case "tzm-Latn": id = 2143; break;
			case "ug": id = 1152; break;
			case "uk": id = 1058; break;
			case "ur": id = 1056; break;
			case "uz": id = 1091; break;
			case "uz-Cyrl": id = 2115; break;
			case "uz-Latn": id = 1091; break;
			case "vi": id = 1066; break;
			case "wo": id = 1160; break;
			case "xh": id = 1076; break;
			case "yo": id = 1130; break;
			case "zh": id = 2052; break;
			case "zh-CHS": case "zh-Hans":
				id = 2052; break;
			case "zh-CHT": case "zh-Hant":
				id = 3076; break;
			case "zu": id = 1077; break;
			default:
				throw new NotImplementedException ("Mapping for neutral culture " + name);
			}

			return new CultureInfo (id);
		}

		static Calendar CreateCalendar (int calendarType)
		{
			string name = null;
			switch (calendarType >> CalendarTypeBits) {
			case 1:
				GregorianCalendarTypes greg_type;
				greg_type = (GregorianCalendarTypes) (calendarType & 0xFF);
				return new GregorianCalendar (greg_type);
			case 2:
				name = "System.Globalization.ThaiBuddhistCalendar";
				break;
			case 3:
				name = "System.Globalization.UmAlQuraCalendar";
				break;
			case 4:
				name = "System.Globalization.HijriCalendar";
				break;
			default:
				throw new NotImplementedException ("Unknown calendar type: " + calendarType);
			}

			Type type = Type.GetType (name, false);
			if (type == null)
				throw new NotSupportedException ("Calendar not found, if the linker is enabled make sure to preserve this type: " + name);
			return (Calendar) Activator.CreateInstance (type);
		}
		
#if NET_4_5
		[MonoTODO]
		public static CultureInfo DefaultThreadCurrentCulture {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public static CultureInfo DefaultThreadCurrentUICulture {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif
	}
}
