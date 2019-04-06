using System.Reflection;

namespace System
{
	partial class DefaultBinder
	{
		// InvokeUtil::PrimitiveAttributes from coreclr
		static readonly int[] PrimitiveAttributes = {
			0x00,                     // ELEMENT_TYPE_END
			0x00,                     // ELEMENT_TYPE_VOID
			0x0004,    // ELEMENT_TYPE_BOOLEAN
			0x3F88,    // ELEMENT_TYPE_CHAR (W = U2, CHAR, I4, U4, I8, U8, R4, R8) (U2 == Char)
			0x3550,    // ELEMENT_TYPE_I1   (W = I1, I2, I4, I8, R4, R8)
			0x3FE8,    // ELEMENT_TYPE_U1   (W = CHAR, U1, I2, U2, I4, U4, I8, U8, R4, R8)
			0x3540,    // ELEMENT_TYPE_I2   (W = I2, I4, I8, R4, R8)
			0x3F88,    // ELEMENT_TYPE_U2   (W = U2, CHAR, I4, U4, I8, U8, R4, R8)
			0x3500,    // ELEMENT_TYPE_I4   (W = I4, I8, R4, R8)
			0x3E00,    // ELEMENT_TYPE_U4   (W = U4, I8, R4, R8)
			0x3400,    // ELEMENT_TYPE_I8   (W = I8, R4, R8)
			0x3800,    // ELEMENT_TYPE_U8   (W = U8, R4, R8)
			0x3000,    // ELEMENT_TYPE_R4   (W = R4, R8)
			0x2000,    // ELEMENT_TYPE_R8   (W = R8)
		};

		static bool IsPrimitiveType (CorElementType type) {
			return (type >= CorElementType.ELEMENT_TYPE_VOID && type <= CorElementType.ELEMENT_TYPE_R8) || type == CorElementType.ELEMENT_TYPE_I || type == CorElementType.ELEMENT_TYPE_U;
		}

		internal static bool CanChangePrimitive (Type source, Type target)
		{
			var src = RuntimeTypeHandle.GetCorElementType ((RuntimeType)source);
			var dst = RuntimeTypeHandle.GetCorElementType ((RuntimeType)target);

			if (!IsPrimitiveType (dst) || !IsPrimitiveType (src))
				return false;
			// This handles I/U
			if (src == dst)
				return true;
			if (src > CorElementType.ELEMENT_TYPE_R8)
				return false;
			return ((1 << (int)dst) & PrimitiveAttributes [(int)src]) != 0;
		}

		private static bool CanChangePrimitiveObjectToType(object source, Type type)
		{
			if (source == null)
				return true;

			if (source is Enum)
				return CanChangePrimitive (Enum.GetUnderlyingType (source.GetType ()), type);
			else
				return CanChangePrimitive (source.GetType (), type);
		}
	}
}