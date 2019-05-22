namespace Mono
{
	// Internal type used by Mono runtime only
	internal sealed class MonoListItem
	{
		public MonoListItem next;
		public object data;
	}
}