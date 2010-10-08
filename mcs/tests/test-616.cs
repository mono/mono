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
	public struct Decimal { }
	public class String { }
	public class Delegate {}
	public class MulticastDelegate {}
	public class Array {}
	public class Exception {}
	public class Type {}
	public class ValueType {}
	public class Enum {}
	public class Attribute {}
	public struct Void {}
	public class ParamArrayAttribute {}
	public class DefaultMemberAttribute {}
	public struct RuntimeTypeHandle {}
	public struct RuntimeFieldHandle {}
		
	public interface IDisposable {}
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

