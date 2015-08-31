namespace Mono.Unix.Android
{
	// Another version of this class is used by the Xamarin.Android test suite
	// It is here to keep the test code #ifdef free as much as possible
	public class TestHelper
	{
		public static bool CanUseRealTimeSignals ()
		{
			return true;
		}
	}
}