//
// The minimal mscorlib implementation
//

namespace System
{
	public partial class Object {}

	public partial struct Byte {}
	public partial struct Int16 {}
	public partial struct Int32 {}
	public partial struct Int64 {}
	public partial struct Single {}
	public partial struct Double{}
	public partial struct Char {}
	public partial struct Boolean {}
	public partial struct SByte {}
	public partial struct UInt16 {}
	public partial struct UInt32 {}
	public partial struct UInt64 {}
	public partial struct IntPtr {}
	public partial struct UIntPtr {}
	public partial struct Decimal { }
	public partial class String { }
	public partial class Delegate {}
	public partial class MulticastDelegate {}
	public partial class Array {}
	public partial class Exception {}
	public partial class Type {}
	public partial class ValueType {}
	public partial class Enum {}
	public partial class Attribute {}
	public partial struct Void {}
	public partial class ParamArrayAttribute {}
	public partial class DefaultMemberAttribute {}
	public partial struct RuntimeTypeHandle {}
	public partial struct RuntimeFieldHandle {}

	public partial interface IDisposable {}

	public delegate void Action ();
}
	
namespace System.Runtime.InteropServices
{
	public partial class OutAttribute {}
}
		

namespace System.Collections
{
	public partial interface IEnumerable {}
	public partial interface IEnumerator {}
}

namespace System.Reflection
{
	public partial class DefaultMemberAttribute {}
}

namespace System.Runtime.CompilerServices
{
	public class ExtensionAttribute : Attribute {}
}
