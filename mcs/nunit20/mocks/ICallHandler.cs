using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The ICallHandler interface dispatches calls to methods or
	/// other objects implementing the ICall interface.
	/// </summary>
	public interface ICallHandler
	{		
		/// <summary>
		/// Simulate a method call on the mocked object.
		/// </summary>
		/// <param name="methodName">The name of the method</param>
		/// <param name="args">Arguments for this call</param>
		/// <returns>Previously specified object or null</returns>
		object Call( string methodName, params object[] args );
	}
}
