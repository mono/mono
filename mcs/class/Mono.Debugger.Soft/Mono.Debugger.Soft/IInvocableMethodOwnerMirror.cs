namespace Mono.Debugger.Soft
{
	/// <summary>
	/// Interface to mark a mirror that supports method invocation
	/// </summary>
	public interface IInvocableMethodOwnerMirror : IMirror
	{
		/// <summary>
		/// Value that will be passed as 'this' reference when invoking a method (can be null e.g. for static methods)
		/// </summary>
		/// <returns>'this' reference</returns>
		Value GetThisObject ();

		/// <summary>
		/// Make some additional processing of invocation result. See implementation in <see cref="StructMirror"/>
		/// </summary>
		/// <param name="result"></param>
		void ProcessResult (IInvokeResult result);
	}
}