#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif

using System.Runtime.CompilerServices;

namespace System.Reflection
{
	partial class MethodBase
	{
		//
		// This is a quick version for our own use. We should override
		// it where possible so that it does not allocate an array.
		// They cannot be abstract otherwise we break public contract
		//
		internal virtual ParameterInfo[] GetParametersInternal ()
		{
			// Override me
			return GetParameters ();
		}

		internal virtual int GetParametersCount ()
		{
			// Override me
			return GetParametersInternal ().Length;
		}

		internal virtual Type GetParameterType (int pos)
		{
			throw new NotImplementedException ();
		}

		internal virtual int get_next_table_index (object obj, int table, bool inc) {
#if !FULL_AOT_RUNTIME
			if (this is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)this;
				return mb.get_next_table_index (obj, table, inc);
			}
			if (this is ConstructorBuilder) {
				ConstructorBuilder mb = (ConstructorBuilder)this;
				return mb.get_next_table_index (obj, table, inc);
			}
#endif
			throw new Exception ("Method is not a builder method");
		}

		internal static MethodBase GetMethodFromHandleNoGenericCheck (RuntimeMethodHandle handle)
		{
			return GetMethodFromHandleInternalType_native (handle.Value, IntPtr.Zero, false);
		}

		internal static MethodBase GetMethodFromHandleNoGenericCheck (RuntimeMethodHandle handle, RuntimeTypeHandle reflectedType)
		{
			return GetMethodFromHandleInternalType_native (handle.Value, reflectedType.Value, false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static MethodBody GetMethodBodyInternal (IntPtr handle);

		internal static MethodBody GetMethodBody (IntPtr handle) 
		{
			return GetMethodBodyInternal (handle);
		}

		static MethodBase GetMethodFromHandleInternalType (IntPtr method_handle, IntPtr type_handle) {
			return GetMethodFromHandleInternalType_native (method_handle, type_handle, true);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static MethodBase GetMethodFromHandleInternalType_native (IntPtr method_handle, IntPtr type_handle, bool genericCheck);

	}
}
