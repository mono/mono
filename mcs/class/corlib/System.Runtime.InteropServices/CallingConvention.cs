// CallingConvention.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.Runtime.InteropServices {


	/// <summary>
	/// <para>Indicates the calling convention used by a method located in an unmanaged shared library.</para>
	/// </summary>
	/// <remarks>
	/// <para>The values of this enumeration are used to specify the calling conventions required
	///  to call unmanaged methods implemented in shared libraries.</para>
	/// <para>
	/// <block subset="none" type="note">Implementers
	///  should map the semantics of specified calling conventions onto the calling conventions
	///  of the host OS.</block>
	/// </para>
	/// <para>
	/// <block subset="none" type="note">For additional information on shared
	///  libraries and an example of the use of the <see cref="T:System.Runtime.InteropServices.CallingConvention" /> enumeration, see the <see cref="T:System.Runtime.InteropServices.DllImportAttribute" /> class overview.</block>
	/// </para>
	/// </remarks>
	public enum CallingConvention {

		/// <summary><para>Indicates that the <see langword="cdecl " />
		/// calling convention is appropriate for a method call.</para><para>For example, on a Windows platform the <see cref="F:System.Runtime.InteropServices.CallingConvention.Cdecl" /> convention produces the
		/// following behavior:</para><list type="table"><listheader><term>Element</term><description>Behavior</description></listheader><item><term> Argument-passing order</term><description>Right to left.</description></item><item><term> Stack-maintenance responsibility</term><description>Calling function pops the arguments from the
		///  stack.</description></item></list><para><block subset="none" type="note">This is the default
		///  calling convention for functions compiled with 32-bit C and C++ compilers.</block></para></summary>
		Cdecl = 2,

		/// <summary><para>Indicates that the <see langword="stdcall " />
		/// calling convention is appropriate for a
		/// method.</para><para>For example, on a Windows platform the <see cref="F:System.Runtime.InteropServices.CallingConvention.StdCall" /> convention produces the
		/// following behavior:</para><list type="table"><listheader><term>Element</term><description>Behavior</description></listheader><item><term> Argument-passing order</term><description>Right to left.</description></item><item><term> Stack-maintenance responsibility</term><description>Called function pops its own arguments from the
		///  stack.</description></item></list></summary>
		StdCall = 3,

		/// <summary><para>Indicates that the <see langword="thiscall" /> calling convention should be used for a
		///  method. This convention is similar to the <see cref="F:System.Runtime.InteropServices.CallingConvention.Cdecl" /> calling convention,
		///  except that the last element that the caller pushes the stack is the
		/// <see langword="this" /> 
		/// pointer.</para><para>For example, on a Windows platform the <see cref="F:System.Runtime.InteropServices.CallingConvention.ThisCall" /> convention
		/// produces the following behavior:</para><list type="table"><listheader><term>Element</term><description>Behavior</description></listheader><item><term> Argument-passing order</term><description>Right to left.</description></item><item><term> Stack-maintenance responsibility</term><description>Calling function pops the arguments from the
		///  stack.</description></item><item><term><see langword="this" /> pointer</term><description>Pushed last onto the stack.</description></item></list><para><block subset="none" type="note">The <see langword="thiscall" /> calling convention is the default
		/// calling convention used by C++ member functions that are not called with
		/// a variable number of arguments.</block></para></summary>
		ThisCall = 4,

		/// <summary><para>Indicates that the <see langword="fastcall " />
		/// calling convention is appropriate for a method
		/// call. </para><para><block subset="none" type="note">On a Windows platform this convention
		///  indicates that arguments to functions are to be passed in registers, whenever
		///  possible.</block></para></summary>
		FastCall = 5,
	} // CallingConvention

} // System.Runtime.InteropServices
