using System;

namespace Mono.Doc.Utils
{
	/// <summary>
	/// Thrown by AssemblyLoader.Load() in the event of a problem.
	/// </summary>
	public class AssemblyLoadException : ApplicationException
	{
		public AssemblyLoadException() : base() {}
		public AssemblyLoadException(string message) : base(message) {}
		public AssemblyLoadException(string message, Exception nested) : base(message, nested) {}
	}
}
