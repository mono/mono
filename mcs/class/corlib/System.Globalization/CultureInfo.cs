//
// System.Globalization.CultureInfo
//
// Miguel de Icaza (miguel@ximian.com)
// Dick Porter (dick@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
	[Serializable]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		static CultureInfo invariant_culture_info;
		bool is_read_only;
		int  lcid, parent_lcid, specific_lcid;
		int datetime_index, number_index;
		bool use_user_override;
		NumberFormatInfo number_format;
		DateTimeFormatInfo datetime_format;
		TextInfo textinfo;

		private string name;
		private string displayname;
		private string englishname;
		private string nativename;
		private string iso3lang;
		private string iso2lang;
		private string icu_name;
		private string win3lang;
		private CompareInfo compareinfo;
		
		private static readonly string MSG_READONLY = "This instance is read only";
		
		static public CultureInfo InvariantCulture {
			get {
				if (invariant_culture_info == null) {
					lock (typeof (CultureInfo)) {
						if (invariant_culture_info == null) {
							invariant_culture_info = new CultureInfo (0x7f, false);
							invariant_culture_info.is_read_only = true;
						}
					}
				}
				
				return(invariant_culture_info);
			}
		}

		public static CultureInfo CreateSpecificCulture (string name)
		{
			if (name == null) {
				throw new ArgumentNullException ("name");
			}

			if (name == String.Empty)
				return InvariantCulture;

			CultureInfo ci = new CultureInfo ();
			if (!construct_internal_locale_from_specific_name (ci, name))
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
			if (!construct_internal_locale_from_current_locale (ci))
				ci = InvariantCulture;
			return ci;
		}

		internal static CultureInfo ConstructCurrentUICulture ()
		{
			return ConstructCurrentCulture ();
		}

		public virtual int LCID {
			get {
				return lcid;
			}
		}

		public virtual string Name {
			get {
				return(name);
			}
		}

		public virtual string NativeName
		{
			get {
				return(nativename);
			}
		}
		

		[MonoTODO]
		public virtual Calendar Calendar
		{
			get { return null; }
		}

		[MonoTODO]
		public virtual Calendar[] OptionalCalendars
		{
			get {
				return(null);
			}
		}

		public virtual CultureInfo Parent
		{
			get {
				if (parent_lcid == lcid)
					return null;
				return new CultureInfo (parent_lcid);
			}
		}

		public virtual TextInfo TextInfo
		{
			get {
				if (textinfo == null) {
					lock (this) {
						if(textinfo == null) {
							textinfo = new TextInfo (lcid);
						}
					}
				}
				
				return(textinfo);
			}
		}

		public virtual string ThreeLetterISOLanguageName
		{
			get {
				return(iso3lang);
			}
		}

		public virtual string ThreeLetterWindowsLanguageName
		{
			get {
				return(win3lang);
			}
		}

		public virtual string TwoLetterISOLanguageName
		{
			get {
				return(iso2lang);
			}
		}

		public bool UseUserOverride
		{
			get {
				return use_user_override;
			}
		}

		internal string IcuName {
			get {
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
			CultureInfo ci=(CultureInfo)MemberwiseClone ();
			ci.is_read_only=false;
			return(ci);
		}

		public override bool Equals (object value)
		{
			CultureInfo b = value as CultureInfo;
			
			if (b != null)
				return b.lcid == lcid;
			return false;
		}

		public static CultureInfo[] GetCultures(CultureTypes types)
		{
			bool neutral=((types & CultureTypes.NeutralCultures)!=0);
			bool specific=((types & CultureTypes.SpecificCultures)!=0);
			bool installed=((types & CultureTypes.InstalledWin32Cultures)!=0);  // TODO

			return internal_get_cultures (neutral, specific, installed);
		}

		public override int GetHashCode()
		{
			return lcid;
		}

		public static CultureInfo ReadOnly(CultureInfo ci)
		{
			if(ci==null) {
				throw new ArgumentNullException("ci");
			}

			if(ci.is_read_only) {
				return(ci);
			} else {
				CultureInfo new_ci=(CultureInfo)ci.Clone ();
				new_ci.is_read_only=true;
				return(new_ci);
			}
		}

		public override string ToString()
		{
			return(name);
		}
		
		public virtual CompareInfo CompareInfo
		{
			get {
				if(compareinfo==null) {
					lock (this) {
						if(compareinfo==null) {
							compareinfo=new CompareInfo (this);
						}
					}
				}
				
				return(compareinfo);
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
				return ((lcid & 0xff00) == 0 || specific_lcid == 0);
			}
		}

		public virtual NumberFormatInfo NumberFormat {
			get {
				if (number_format == null){
					lock (this){
						if (number_format == null) {
							number_format = new NumberFormatInfo ();
							construct_number_format ();
						}
					}
				}

				return number_format;
			}

			set {
				if (is_read_only) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("NumberFormat");
				
				number_format = value;
			}
		}

		public virtual DateTimeFormatInfo DateTimeFormat
		{
			get 
			{
				if (datetime_format == null)
				{
					lock (this)
					{
						if (datetime_format == null) {
							datetime_format = new DateTimeFormatInfo();
							construct_datetime_format ();
						}
					}
				}

				return datetime_format;
			}

			set 
			{
				if (is_read_only) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("DateTimeFormat");
				
				datetime_format = value;
			}
		}

		public virtual string DisplayName
		{
			get {
				return(displayname);
			}
		}

		public virtual string EnglishName
		{
			get {
				return(englishname);
			}
		}

		[MonoTODO]
		public static CultureInfo InstalledUICulture
		{
			get {
				return(null);
			}
		}

		public bool IsReadOnly 
		{
			get {
				return(is_read_only);
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_internal_locale (string locale);

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

		private void ConstructInvariant (bool use_user_override)
		{
			is_read_only=false;
			lcid=0x7f;
			this.use_user_override=use_user_override;

			/* NumberFormatInfo defaults to the invariant data */
			number_format=new NumberFormatInfo ();
			
			/* DateTimeFormatInfo defaults to the invariant data */
			datetime_format=new DateTimeFormatInfo ();

			textinfo=new TextInfo ();

			name="";
			displayname="Invariant Language (Invariant Country)";
			englishname="Invariant Language (Invariant Country)";
			nativename="Invariant Language (Invariant Country)";
			iso3lang="IVL";
			iso2lang="iv";
			icu_name="en_US_POSIX";
			win3lang="IVL";
		}
		
		public CultureInfo (int culture, bool use_user_override)
		{
			if (culture < 0)
				throw new ArgumentOutOfRangeException ("culture");

			if(culture==0x007f) {
				/* Short circuit the invariant culture */
				ConstructInvariant (use_user_override);
				return;
			}

			if (!construct_internal_locale_from_lcid (culture))
				throw new ArgumentException ("Culture name " + name +
						" is not supported.", "name");
		}

		public CultureInfo (int culture) : this (culture, false) {}
		
		public CultureInfo (string name, bool use_user_override)
		{
			if (name == null)
				throw new ArgumentNullException ();

			if(name=="") {
				/* Short circuit the invariant culture */
				ConstructInvariant (use_user_override);
				return;
			}

			if (!construct_internal_locale_from_name (name.ToLower ()))
				throw new ArgumentException ("Culture name " + name +
						" is not supported.", "name");
		}

		public CultureInfo (string name) : this (name, false) {}

		// This is used when creating by specific name and creating by
		// current locale so we can initialize the object without
		// doing any member initialization
		private CultureInfo () { } 
	}
}
