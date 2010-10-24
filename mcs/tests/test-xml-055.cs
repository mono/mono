// Compiler options: -doc:xml-055.xml -warnaserror

namespace NAnt.Core.Filters
{
	/// <summary>
	/// Represent a chain of NAnt filters that can be applied to a 'Task'.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///   <item>
	///       <description><see cref="NAnt.Core.Tasks.CopyTask"/></description>
	///   </item>
	/// </list>
	/// </remarks>
	public class FilterChain
	{
		static void Main ()
		{
		}
	}
}

namespace NAnt.Core.Tasks
{
	/// <summary>
	/// Copies a file or set of files to a new file or directory.
	/// </summary>
	public class CopyTask { }
}
