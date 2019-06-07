#if !FULL_AOT_RUNTIME
using System.Reflection.Emit;
#endif

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Reflection
{
	[Serializable]
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

		internal virtual int get_next_table_index (object obj, int table, int count) {
#if !FULL_AOT_RUNTIME
			if (this is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)this;
				return mb.get_next_table_index (obj, table, count);
			}
			if (this is ConstructorBuilder) {
				ConstructorBuilder mb = (ConstructorBuilder)this;
				return mb.get_next_table_index (obj, table, count);
			}
#endif
			throw new Exception ("Method is not a builder method");
		}

		internal virtual string FormatNameAndSig (bool serialization)
		{
			// Serialization uses ToString to resolve MethodInfo overloads.
			StringBuilder sbName = new StringBuilder (Name);

			sbName.Append ("(");
			sbName.Append (ConstructParameters (GetParameterTypes (), CallingConvention, serialization));
			sbName.Append (")");

			return sbName.ToString ();
		}

		internal virtual Type[] GetParameterTypes ()
		{
			ParameterInfo[] paramInfo = GetParametersNoCopy ();

			Type[] parameterTypes = new Type [paramInfo.Length];
			for (int i = 0; i < paramInfo.Length; i++)
				parameterTypes [i] = paramInfo [i].ParameterType;

			return parameterTypes;
		}

		internal virtual ParameterInfo[] GetParametersNoCopy () => GetParameters ();

		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle)
		{
			if (handle.IsNullHandle ())
				throw new ArgumentException (Environment.GetResourceString("Argument_InvalidHandle"));

#if MONO
			MethodBase m = RuntimeMethodInfo.GetMethodFromHandleInternalType (handle.Value, IntPtr.Zero);
			if (m == null)
				throw new ArgumentException ("The handle is invalid.");
#else
			MethodBase m = RuntimeType.GetMethodBase (handle.GetMethodInfo ());
#endif

			Type declaringType = m.DeclaringType;
			if (declaringType != null && declaringType.IsGenericType)
				throw new ArgumentException (String.Format (
					CultureInfo.CurrentCulture, Environment.GetResourceString ("Argument_MethodDeclaringTypeGeneric"), 
					m, declaringType.GetGenericTypeDefinition ()));
 
			return m;
		}

		[System.Runtime.InteropServices.ComVisible(false)]
		public static MethodBase GetMethodFromHandle (RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
		{
			if (handle.IsNullHandle ())
				throw new ArgumentException (Environment.GetResourceString("Argument_InvalidHandle"));
#if MONO
			MethodBase m = RuntimeMethodInfo.GetMethodFromHandleInternalType (handle.Value, declaringType.Value);
			if (m == null)
				throw new ArgumentException ("The handle is invalid.");
			return m;
#else
			return RuntimeType.GetMethodBase (declaringType.GetRuntimeType (), handle.GetMethodInfo ());
#endif
		}

		internal static string ConstructParameters (Type[] parameterTypes, CallingConventions callingConvention, bool serialization)
		{
			StringBuilder sbParamList = new StringBuilder ();
			string comma = "";

			for (int i = 0; i < parameterTypes.Length; i++) {
				Type t = parameterTypes [i];

				sbParamList.Append (comma);

				string typeName = t.FormatTypeName (serialization);

				// Legacy: Why use "ByRef" for by ref parameters? What language is this? 
				// VB uses "ByRef" but it should precede (not follow) the parameter name.
				// Why don't we just use "&"?
				if (t.IsByRef && !serialization) {
					sbParamList.Append (typeName.TrimEnd (new char[] { '&' }));
					sbParamList.Append (" ByRef");
				} else {
					sbParamList.Append (typeName);
				}

				comma = ", ";
			}

			if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs) {
				sbParamList.Append (comma);
				sbParamList.Append ("...");
			}

			return sbParamList.ToString ();
		}

#if MONO
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static MethodBase GetCurrentMethod ();
#else
		[System.Security.DynamicSecurityMethod] // Specify DynamicSecurityMethod attribute to prevent inlining of the caller.
		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static MethodBase GetCurrentMethod ()
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return RuntimeMethodInfo.InternalGetCurrentMethod (ref stackMark);
		}
#endif
	}
}
