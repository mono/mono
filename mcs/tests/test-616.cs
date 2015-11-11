// Compiler options: -nostdlib -t:library -noconfig

//
// Tests compiler mscorlib bootstrap
//

namespace System
{
	public class Object
	{
		object value_with_no_base;
	}
		
	public struct Byte {}
	public struct Int16 {}
	public struct Int32 {}
	public struct Int64 {}
	public struct Single {}
	public struct Double{}
	public struct Char {}
	public struct Boolean {}
	public struct SByte {}
	public struct UInt16 {}
	public struct UInt32 {}
	public struct UInt64 {}
	public struct IntPtr {}
	public struct UIntPtr {}
	public class String { }
	public class Delegate {}
	public class MulticastDelegate {}
	public class Array {}
	public class Exception {}
	public partial class Type {}
	public class ValueType {}
	public class Enum {}
	public class Attribute {}
	public struct Void {}
	public class ParamArrayAttribute {}
	public class DefaultMemberAttribute {}
	public struct RuntimeTypeHandle {}
	public struct RuntimeFieldHandle {}
		
	public interface IDisposable {}

	public struct Decimal {

		private int flags;

		public Decimal(int[] bits) {
			flags = 0;
			SetBits(bits);
		}

		public Decimal (int i)
		{
			flags = 0;
		}

		private void SetBits(int[] bits) {
		}
	}

	partial class Type
	{
		public static bool operator == (Type left, Type right)
		{
			return false;
		}

		public static bool operator != (Type left, Type right)
		{
			return true;
		}

		void Foo ()
		{
			Decimal d = 0;
			var d2 = d;
		}
	}	
}
	
namespace System.Runtime.InteropServices
{
	public class OutAttribute {}
}
		

namespace System.Collections
{
	public interface IEnumerable {}
	public interface IEnumerator {}
}

namespace System.Reflection
{
	public class DefaultMemberAttribute {}
}
