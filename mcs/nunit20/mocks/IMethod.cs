using System;

namespace NUnit.Mocks
{
	/// <summary>
	/// The IMethod interface represents an method or other named object that 
	/// is both callable and self-verifying.
	/// </summary>
	public interface IMethod : IVerify, ICall
	{
		/// <summary>
		/// The name of the object
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Tell the object to expect a certain call.
		/// </summary>
		/// <param name="call"></param>
		void Expect( ICall call );
	}
}
