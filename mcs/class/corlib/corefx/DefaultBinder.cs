using System.Reflection;

namespace System {
	partial class DefaultBinder {
		internal static bool CompareMethodSig (MethodBase m1, MethodBase m2)
		{
			ParameterInfo[] params1 = m1.GetParametersNoCopy ();
			ParameterInfo[] params2 = m2.GetParametersNoCopy ();

			if (params1.Length != params2.Length)
				return false;

			int numParams = params1.Length;
			for (int i = 0; i < numParams; i++) {
				if (params1 [i].ParameterType != params2 [i].ParameterType)
					return false;
			}

			return true;
		}

        // Given a set of methods that match the base criteria, select a method based
        // upon an array of types.  This method should return null if no method matchs
        // the criteria.
        public sealed override MethodBase SelectMethod (BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            int i;
            int j;

            Type[] realTypes = new Type [types.Length];
            for (i = 0; i < types.Length; i++)
            {
                realTypes[i] = types[i].UnderlyingSystemType;
                if (!(realTypes[i].IsRuntimeImplemented() || realTypes[i] is SignatureType))
                    throw new ArgumentException(SR.Arg_MustBeType, nameof(types));
            }
            types = realTypes;

            // We don't automatically jump out on exact match.
            if (match == null || match.Length == 0)
                throw new ArgumentException(SR.Arg_EmptyArray, nameof(match));

            MethodBase[] candidates = (MethodBase[])match.Clone();

            // Find all the methods that can be described by the types parameter. 
            //  Remove all of them that cannot.
            int CurIdx = 0;
            for (i = 0; i < candidates.Length; i++)
            {
                ParameterInfo[] par = candidates[i].GetParametersNoCopy();
                if (par.Length != types.Length)
                    continue;
                for (j = 0; j < types.Length; j++)
                {
                    Type pCls = par[j].ParameterType;
                    if (types[j].MatchesParameterTypeExactly(par[j]))
                        continue;
                    if (pCls == typeof(object))
                        continue;

                    Type type = types[j];
                    if (type is SignatureType signatureType)
                    {
                        if (!(candidates[i] is MethodInfo methodInfo))
                            break;
                        type = signatureType.TryResolveAgainstGenericMethod(methodInfo);
                        if (type == null)
                            break;
                    }

                    if (pCls.IsPrimitive)
                    {
                        if (!(type.UnderlyingSystemType.IsRuntimeImplemented()) ||
                            !CanChangePrimitive(type.UnderlyingSystemType, pCls.UnderlyingSystemType))
                            break;
                    }
                    else
                    {
                        if (!pCls.IsAssignableFrom(type))
                            break;
                    }
                }
                if (j == types.Length)
                    candidates[CurIdx++] = candidates[i];
            }
            if (CurIdx == 0)
                return null;
            if (CurIdx == 1)
                return candidates[0];

            // Walk all of the methods looking the most specific method to invoke
            int currentMin = 0;
            bool ambig = false;
            int[] paramOrder = new int[types.Length];
            for (i = 0; i < types.Length; i++)
                paramOrder[i] = i;
            for (i = 1; i < CurIdx; i++)
            {
                int newMin = FindMostSpecificMethod(candidates[currentMin], paramOrder, null, candidates[i], paramOrder, null, types, null);
                if (newMin == 0)
                    ambig = true;
                else
                {
                    if (newMin == 2)
                    {
                        currentMin = i;
                        ambig = false;
                        currentMin = i;
                    }
                }
            }
            if (ambig)
                throw new AmbiguousMatchException(SR.Arg_AmbiguousMatchException);
            return candidates[currentMin];
        }

        // CanChangePrimitive
        // This will determine if the source can be converted to the target type
        private static bool CanChangePrimitive(Type source, Type target)
        {
            return CanPrimitiveWiden(source, target);
        }

        // CanChangePrimitiveObjectToType
        private static bool CanChangePrimitiveObjectToType(object source, Type type)
        {
            return CanPrimitiveWiden(source.GetType(), type);
        }

        private static bool CanPrimitiveWiden(Type source, Type target)
        {
            Primitives widerCodes = _primitiveConversions[(int)(Type.GetTypeCode(source))];
            Primitives targetCode = (Primitives)(1 << (int)(Type.GetTypeCode(target)));

            return 0 != (widerCodes & targetCode);
        }

        [Flags]
        private enum Primitives
        {
            Boolean = 1 << (int)TypeCode.Boolean,
            Char = 1 << (int)TypeCode.Char,
            SByte = 1 << (int)TypeCode.SByte,
            Byte = 1 << (int)TypeCode.Byte,
            Int16 = 1 << (int)TypeCode.Int16,
            UInt16 = 1 << (int)TypeCode.UInt16,
            Int32 = 1 << (int)TypeCode.Int32,
            UInt32 = 1 << (int)TypeCode.UInt32,
            Int64 = 1 << (int)TypeCode.Int64,
            UInt64 = 1 << (int)TypeCode.UInt64,
            Single = 1 << (int)TypeCode.Single,
            Double = 1 << (int)TypeCode.Double,
            Decimal = 1 << (int)TypeCode.Decimal,
            DateTime = 1 << (int)TypeCode.DateTime,
            String = 1 << (int)TypeCode.String,
        }

        private static Primitives[] _primitiveConversions = new Primitives[]
        {
                /* Empty    */  0, // not primitive
                /* Object   */  0, // not primitive
                /* DBNull   */  0, // not exposed.
                /* Boolean  */  Primitives.Boolean,
                /* Char     */  Primitives.Char    | Primitives.UInt16 | Primitives.UInt32 | Primitives.Int32  | Primitives.UInt64 | Primitives.Int64  | Primitives.Single |  Primitives.Double,
                /* SByte    */  Primitives.SByte   | Primitives.Int16  | Primitives.Int32  | Primitives.Int64  | Primitives.Single | Primitives.Double,
                /* Byte     */  Primitives.Byte    | Primitives.Char   | Primitives.UInt16 | Primitives.Int16  | Primitives.UInt32 | Primitives.Int32  | Primitives.UInt64 |  Primitives.Int64 |  Primitives.Single |  Primitives.Double,
                /* Int16    */  Primitives.Int16   | Primitives.Int32  | Primitives.Int64  | Primitives.Single | Primitives.Double,
                /* UInt16   */  Primitives.UInt16  | Primitives.UInt32 | Primitives.Int32  | Primitives.UInt64 | Primitives.Int64  | Primitives.Single | Primitives.Double,
                /* Int32    */  Primitives.Int32   | Primitives.Int64  | Primitives.Single | Primitives.Double,
                /* UInt32   */  Primitives.UInt32  | Primitives.UInt64 | Primitives.Int64  | Primitives.Single | Primitives.Double,
                /* Int64    */  Primitives.Int64   | Primitives.Single | Primitives.Double,
                /* UInt64   */  Primitives.UInt64  | Primitives.Single | Primitives.Double,
                /* Single   */  Primitives.Single  | Primitives.Double,
                /* Double   */  Primitives.Double,
                /* Decimal  */  Primitives.Decimal,
                /* DateTime */  Primitives.DateTime,
                /* [Unused] */  0,
                /* String   */  Primitives.String,
        };        		
	}
}