// System.Globalization.CultureInfo
//
// Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. 2001 (http://www.ximian.com)

using System.Threading;

namespace System.Globalization
{
	public class CultureInfo
	{
		static CultureInfo invariant_culture_info;
		bool is_read_only;
		int  lcid;
		bool use_user_override;
		NumberFormatInfo number_format;
		DateTimeFormatInfo datetime_format;

		private static readonly string MSG_READONLY = "This instance is read only";
		
		// <summary>
		//   Returns the Invariant Culture Information ("iv")
		// </summary>
		static public CultureInfo InvariantCulture {
			get {
				if (invariant_culture_info != null)
					return invariant_culture_info;
				
				invariant_culture_info = new CultureInfo (0x07f, false);
				invariant_culture_info.is_read_only = true;
				
				return invariant_culture_info;
			}
		}

		// <summary>
		//   Creates a CultureInfo for a specific ID
		// </summary>
		public static CultureInfo CreateSpecificCulture (string name)
		{
			switch (name){
			case "iv":
				return InvariantCulture;

			default:
				throw new ArgumentException ("CreateSpecificCultureName");
			}
		}

		/// <summary>
		/// CultureInfo instance that represents the culture used by the current thread
		/// </summary>
		public static CultureInfo CurrentCulture 
		{
			get 
			{
				return Thread.CurrentThread.CurrentCulture;
			}
			
			set 
			{
				Thread.CurrentThread.CurrentCulture = value;
			}
		}

		/// <summary>
		/// CultureInfo instance that represents the current culture used by the ResourceManager to look up culture-specific resources at run time
		/// </summary>
		public static CultureInfo CurrentUICulture 
		{
			get 
			{
				return Thread.CurrentThread.CurrentUICulture;
			}
			
			set 
			{
				Thread.CurrentThread.CurrentUICulture =	value;
			}
		}


		public virtual int LCID {
			get {
				return lcid;
			}
		}

		// <summary>
		//   Gets the string-encoded name of the culture
		// </summary>
		public virtual string Name {
			get {
				switch (lcid){
				case 0x007f:
					return "iv";
				}
				throw new Exception ("Miss constructed object for LCID: " + lcid);
			}
		}

		// <summary>
		//   Returns whether the current culture is neutral (neutral cultures
		//   only specify a language, not a country.
		// </summary>
		public virtual bool IsNeutralCulture {
			get {
				return (lcid & 0xff00) == 0;
			}
		}
		// <summary>
		//   Returns the NumberFormat for the current lcid
		// </summary>
		public virtual NumberFormatInfo NumberFormat {
			get {
				if (number_format == null){
					lock (this){
						if (number_format == null)
							number_format = new NumberFormatInfo (lcid);
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
						if (datetime_format == null)
							datetime_format = new DateTimeFormatInfo(); //FIXME: create correct localized DateTimeFormat
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

		
		public CultureInfo (int code, bool use_user_override)
		{
			switch (lcid){
			case 0x007f: // iv    Invariant
			case 0x0036: // af    Afrikaans
			case 0x0436: // af-ZA Afrikaans - South Africa
			case 0x001c: // sq    Albanian
			case 0x041c: // sq-AL Albanian  - Albania
			case 0x0001: // ar    Arabic
			case 0x1401: // ar-DZ Arabic    - Algeria
			case 0x3c01: // ar-BH Arabic    - Barhain
			case 0x0c01: // ar-EG Arabic    - Egypt
			case 0x0801: // ar-IQ Arabic    - Iraq
			case 0x2c01: // ar-JO Arabic    - Jordan
			case 0x3401: // ar-KQ Arabic    - Kuwait
			case 0x3001: // ar-LB Arabic    - Lebanon
			case 0x1001: // ar-LY Arabic    - Libya
			case 0x1801: // ar-MA Arabic    - Morocco
			case 0x2001: // ar-OM Arabic    - Oman
			case 0x4001: // ar-QA Arabic    - Qatar
			case 0x0401: // ar-SA Arabic    - Saudi Arabia
			case 0x2801: // ar-SY Arabic    - Syria
			case 0x1c01: // ar-TN Arabic    - Tunisia
			case 0x3801: // ar-AE Arabic    - United Arab Emirates
			case 0x2401: // ar-YE Arabic    - Yemen
				lcid = code;
				this.use_user_override = use_user_override;
				break;

			default:
				throw new ArgumentException ("CultureInfoCode");
			}
		}
	}
}
