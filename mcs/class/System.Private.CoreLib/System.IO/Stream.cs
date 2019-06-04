namespace System.IO
{
	partial class Stream
	{
		bool HasOverriddenBeginEndRead () => true;

		bool HasOverriddenBeginEndWrite () => true;
	}
}
