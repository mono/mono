// help building System.Data without win32-specific p/invokes

using System;
using System.Diagnostics;

internal static partial class Bid {
    internal enum ApiGroup : uint {
		Pooling     = 0x00001000,
		Correlation = 0x00040000,
	}
	
    internal static bool AdvancedOn {
        get { return false; }
    }

    internal static bool IsOn (ApiGroup flag)
	{
        return false;
    }

	[Conditional ("BID")]
	internal static void Trace (params object[] a)
	{
	}

	[Conditional ("BID")]
	internal static void PoolerTrace (params object[] a)
	{
	}

	// out method can't be conditional
	internal static void ScopeEnter (out IntPtr p, params object[] a)
	{
		p = IntPtr.Zero;
	}
	
	[Conditional ("BID")]
	internal static void ScopeLeave (ref IntPtr a)
	{
	}
	
	// out method can't be conditional
	internal static void PoolerScopeEnter (out IntPtr p, string a, System.Int32 b)
	{
		p = IntPtr.Zero;
	}
}

[ConditionalAttribute ("CODE_ANALYSIS")]
[AttributeUsage (AttributeTargets.Method)]
internal sealed class BidMethodAttribute : Attribute {
}

[ConditionalAttribute ("CODE_ANALYSIS")]
[AttributeUsage (AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple=true)]
internal sealed class BidArgumentTypeAttribute : Attribute {
	internal BidArgumentTypeAttribute (Type bidArgumentType)
	{
	}
}

namespace System.Data.Common {

	internal static class UnsafeNativeMethods {
		
		// note: likely unreachable code - as this never worked on mono
		internal static int lstrlenW (IntPtr p)
		{
			throw new NotImplementedException ();
		}
		
		static internal int CreateWellKnownSid (int sidType, byte[] domainSid, byte[] resultSid, ref uint resultSidLength )
		{
			return -1;
		}
		
		static internal bool CheckTokenMembership (IntPtr tokenHandle, byte[] sidToCheck, out bool isMember)
		{
			isMember = false;
			return false;
		}
		
		static internal bool GetTokenInformation (IntPtr tokenHandle, uint token_class, IntPtr tokenStruct, uint tokenInformationLength, ref uint tokenString)
		{
			return false;
		}
		
		static internal bool ConvertSidToStringSidW (IntPtr sid, out IntPtr stringSid)
		{
			stringSid = IntPtr.Zero;
			return false;
		}
	}
}
