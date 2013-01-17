using System;

namespace ProtectedSetter
{
	public abstract class BaseClass
	{
		public abstract string Name { get; internal set;}
	}

	public class DerivedClass : BaseClass
	{
		
		public override String Name
		{
			get {
				return null;
			}
			internal set {
			}
		}
		
		public static void Main () {}
	}
}