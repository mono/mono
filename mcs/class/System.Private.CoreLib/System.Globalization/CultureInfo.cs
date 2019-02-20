namespace System.Globalization
{
	partial class CultureInfo
	{
#region referencesources legacy
		internal static CultureInfo ConstructCurrentCulture () => throw new NotImplementedException ();

		internal static CultureInfo ConstructCurrentUICulture () => throw new NotImplementedException ();

		internal static CultureInfo UserDefaultCulture {
			get {
				throw new NotImplementedException ();
			}
		}
#endregion
	}
}