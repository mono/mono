using System;

namespace NUnit.Core
{
	/// <summary>
	/// All objects which are marshalled by reference
	/// and whose lifetime is manually controlled by
	/// the app, should derive from this class rather
	/// than MarshalByRefObject.
	/// 
	/// This includes the remote test domain objects
	/// which are accessed by the client and those
	/// client objects which are called back by the
	/// remote test domain.
	/// 
	/// Objects in this category that already inherit
	/// from some other class (e.g. from TextWriter)
	/// which in turn inherits from MarshalByRef object 
	/// should override InitializeLifetimeService to 
	/// return null to obtain the same effect.
	/// </summary>
	public class LongLivingMarshalByRefObject : MarshalByRefObject
	{
		public override Object InitializeLifetimeService()
		{
			return null;
		}
	}
}
