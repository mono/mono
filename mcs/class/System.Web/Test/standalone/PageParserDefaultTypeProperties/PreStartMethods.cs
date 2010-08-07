using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;

namespace PageParserDefaultTypeProperties
{
	public class MyHttpApplication : HttpApplication
	{ }

	public class MyPage : Page
	{}

	public class MyPageParserFilter : PageParserFilter
	{
		public override bool AllowBaseType (Type baseType)
		{
			return true;
		}

		public override bool AllowCode {
			get {
				return true;
			}
		}

		public override bool AllowControl (Type controlType, ControlBuilder builder)
		{
			return true;
		}

		public override bool AllowServerSideInclude (string includeVirtualPath)
		{
			return true;
		}

		public override bool AllowVirtualReference (string referenceVirtualPath, VirtualReferenceType referenceType)
		{
			return true;
		}

		public override CompilationMode GetCompilationMode (CompilationMode current)
		{
			return current;
		}

		public override int NumberOfControlsAllowed {
			get {
				return 1000;
			}
		}

		public override int NumberOfDirectDependenciesAllowed {
			get {
				return 1000;
			}
		}

		public override int TotalNumberOfDependenciesAllowed {
			get {
				return 1000;
			}
		}
	}

	public class MyUserControl : UserControl
	{ }

	public class PreStartMethods
	{
		static readonly Dictionary <int, List <Type>> tests = new Dictionary<int, List<Type>> {
			{1, new List <Type> { typeof (MyHttpApplication), typeof (MyPage), typeof (MyPageParserFilter), typeof (MyUserControl)}},
			{2, new List <Type> { typeof (string), typeof (string), typeof (string), typeof (string)}},
			{3, new List <Type> { null, null, null, null}}
		};

		static int currentTestNumber;
		
		public static List<string> Info {
			get;
			private set;
		}

		static PreStartMethods ()
		{
			Info = new List<string> ();
			AppDomain.CurrentDomain.SetData ("TestRunData", Info);
		}

		static bool DetermineTestNumber ()
		{
			object o = AppDomain.CurrentDomain.GetData ("TestNumber");
			if (o == null || !(o is int)) {
#if DEBUG
				currentTestNumber = 2;
				return true;
#else
				return false;
#endif
			}

			currentTestNumber = (int) o;
			return true;
		}
		public static void PreStartMethod ()
		{
			currentTestNumber = -1;
			if (!DetermineTestNumber ()) {
				Log ("PreStartMethod: test number cannot be determined.");
				return;
			}

			List <Type> data;
			if (tests.TryGetValue (currentTestNumber, out data))
				AssignValues (data);
			else
				Log ("PreStartMethod: unknown test number {0}", currentTestNumber);
		}

		static void AssignValues (List <Type> types)
		{
			try {
				PageParser.DefaultApplicationBaseType = types [0];
				Log ("DefaultApplicationBaseType: set");
			} catch (Exception ex) {
				Log ("DefaultApplicationBaseType: exception '{0}' thrown.", ex.GetType ());
			}

			try {
				PageParser.DefaultPageBaseType = types [1];
				Log ("DefaultPageBaseType: set");
			} catch (Exception ex) {
				Log ("DefaultPageBaseType: exception '{0}' thrown.", ex.GetType ());
			}

			try {
				PageParser.DefaultPageParserFilterType= types [2];
				Log ("DefaultPageParserFilterType: set");
			} catch (Exception ex) {
				Log ("DefaultPageParserFilterType: exception '{0}' thrown.", ex.GetType ());
			}

			try {
				PageParser.DefaultUserControlBaseType = types [3];
				Log ("DefaultUserControlBaseType: set");
			} catch (Exception ex) {
				Log ("DefaultUserControlBaseType: exception '{0}' thrown.", ex.GetType ());
			}
		}

		static void Log (string format, params object [] parms)
		{
			string formatted;
			if (parms != null && parms.Length > 0)
				formatted = String.Format (format, parms);
			else
				formatted = format;
			
			Info.Add (String.Format ("{0}: {1}", currentTestNumber, formatted));
		}
	}
}