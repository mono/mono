
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.6.1055.0
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly methodimpl
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module overideinterface.exe // GUID = {256DB25B-2D9B-4A06-B00B-B49832669885}


  .class interface public auto ansi abstract I1
  {

    // method line 1
    .method public virtual hidebysig newslot 
           instance default int32 M1 ()  cil managed 
    {
        // Method begins at RVA 0x2050
	// Code size 8 (0x8)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4.s 0x64
	IL_0003:  stloc.0 
	IL_0004:  br.s IL_0006

	IL_0006:  ldloc.0 
	IL_0007:  ret 
    } // end of method I1::M1

  } // end of class I1

  .class interface public auto ansi abstract I2
  	implements I1  {

    // method line 2
    .method private final virtual hidebysig 
           instance default int32 I1.M1 ()  cil managed 
    {
        // Method begins at RVA 0x2064
	.override class I1::M1
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4 200
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method I2::I1.M1

  } // end of class I2

  .class interface public auto ansi abstract I3
  	implements I1  {

    // method line 3
    .method private final virtual hidebysig 
           instance default int32 I1.M1 ()  cil managed 
    {
        // Method begins at RVA 0x207c
	.override class I1::M1
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4 300
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method I3::I1.M1

  } // end of class I3

  .class interface public auto ansi abstract I4
  	implements I1  {

    // method line 4
    .method private final virtual hidebysig 
           instance default int32 I1.M1 ()  cil managed 
    {
        // Method begins at RVA 0x2094
	.override class I1::M1
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4 400
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method I4::I1.M1

  } // end of class I4

  .class private auto ansi beforefieldinit Test10
  	extends [mscorlib]System.Object
  	implements I1, I2, I3, I4  {

    // method line 5
    .method public final virtual hidebysig newslot 
           instance default int32 M1 ()  cil managed 
    {
        // Method begins at RVA 0x20ac
	// Code size 7 (0x7)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4.0 
	IL_0002:  stloc.0 
	IL_0003:  br.s IL_0005

	IL_0005:  ldloc.0 
	IL_0006:  ret 
    } // end of method Test10::M1

    // method line 6
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x20bf
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  nop 
	IL_0007:  ret 
    } // end of method Test10::.ctor

  } // end of class Test10

  .class interface public auto ansi abstract I1t
  {

    // method line 7
    .method public virtual hidebysig newslot 
           instance default int32 M1t ()  cil managed 
    {
        // Method begins at RVA 0x20c8
	// Code size 8 (0x8)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4.s 0x64
	IL_0003:  stloc.0 
	IL_0004:  br.s IL_0006

	IL_0006:  ldloc.0 
	IL_0007:  ret 
    } // end of method I1t::M1t

  } // end of class I1t

  .class interface public auto ansi abstract I2t
  	implements I1t  {

    // method line 8
    .method private final virtual hidebysig 
           instance default int32 I1t.M1t ()  cil managed 
    {
        // Method begins at RVA 0x20dc
	.override class I1t::M1t
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4 200
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method I2t::I1t.M1t

  } // end of class I2t

  .class interface public auto ansi abstract I3t
  	implements I1t  {

    // method line 9
    .method private final virtual hidebysig 
           instance default int32 I1t.M1t ()  cil managed 
    {
        // Method begins at RVA 0x20f4
	.override class I1t::M1t
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4 300
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method I3t::I1t.M1t

  } // end of class I3t

  .class interface public auto ansi abstract I4t
  	implements I1t  {

    // method line 10
    .method private final virtual hidebysig 
           instance default int32 I1t.M1t ()  cil managed 
    {
        // Method begins at RVA 0x210c
	.override class I1t::M1t
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4 400
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method I4t::I1t.M1t

  } // end of class I4t

  .class private auto ansi beforefieldinit Test10t
  	extends [mscorlib]System.Object
  	implements I1t, I2t, I3t, I4t  {

    // method line 11
    .method private final virtual hidebysig newslot 
           instance default int32 I1t.M1t ()  cil managed 
    {
        // Method begins at RVA 0x2124
	.override class I1t::M1t
	// Code size 7 (0x7)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4.0 
	IL_0002:  stloc.0 
	IL_0003:  br.s IL_0005

	IL_0005:  ldloc.0 
	IL_0006:  ret 
    } // end of method Test10t::I1t.M1t

    // method line 12
    .method public hidebysig 
           instance default int32 M1t ()  cil managed 
    {
        // Method begins at RVA 0x2138
	// Code size 8 (0x8)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4.s 0x0a
	IL_0003:  stloc.0 
	IL_0004:  br.s IL_0006

	IL_0006:  ldloc.0 
	IL_0007:  ret 
    } // end of method Test10t::M1t

    // method line 13
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x214c
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  nop 
	IL_0007:  ret 
    } // end of method Test10t::.ctor

  } // end of class Test10t

  .class interface public auto ansi abstract IName
  {

    // method line 14
    .method public virtual hidebysig newslot abstract specialname 
           instance default string get_Name ()  cil managed 
    {
        // Method begins at RVA 0x0
    } // end of method IName::get_Name

	.property instance string Name ()
	{
		.get instance default string IName::get_Name () 
	}
  } // end of class IName

  .class interface public auto ansi abstract IOther`1<T>
  {

    // method line 15
    .method public virtual hidebysig newslot abstract specialname 
           instance default !T get_other ()  cil managed 
    {
        // Method begins at RVA 0x0
    } // end of method IOther`1::get_other

    // method line 16
    .method public virtual hidebysig newslot abstract specialname 
           instance default string get_Name ()  cil managed 
    {
        // Method begins at RVA 0x0
    } // end of method IOther`1::get_Name

	.property instance string Name ()
	{
		.get instance default string IOther`1::get_Name () 
	}
  } // end of class IOther`1

  .class public auto ansi beforefieldinit Name1
  	extends [mscorlib]System.Object
  	implements IName  {

    // method line 17
    .method public hidebysig specialname 
           instance default string get_Name ()  cil managed 
    {
        // Method begins at RVA 0x2158
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		string	V_0)
	IL_0000:  nop 
	IL_0001:  ldstr "ClassName"
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method Name1::get_Name

    // method line 18
    .method private final virtual hidebysig newslot specialname 
           instance default string IName.get_Name ()  cil managed 
    {
        // Method begins at RVA 0x2170
	.override class IName::get_Name
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		string	V_0)
	IL_0000:  nop 
	IL_0001:  ldstr "InterfaceName"
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method Name1::IName.get_Name

    // method line 19
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2187
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  nop 
	IL_0007:  ret 
    } // end of method Name1::.ctor

	.property instance string Name ()
	{
		.get instance default string Name1::get_Name () 
	}
	.property instance string IName.Name ()
	{
		.get instance default string Name1::IName.get_Name () 
	}
  } // end of class Name1

  .class public auto ansi beforefieldinit Name2
  	extends [mscorlib]System.Object
  	implements IName, class IOther`1<int32>  {

    // method line 20
    .method public final virtual hidebysig newslot specialname 
           instance default string get_Name ()  cil managed 
    {
        // Method begins at RVA 0x2190
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		string	V_0)
	IL_0000:  nop 
	IL_0001:  ldstr "ClassName"
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method Name2::get_Name

    // method line 21
    .method private final virtual hidebysig newslot specialname 
           instance default string IName.get_Name ()  cil managed 
    {
        // Method begins at RVA 0x21a8
	.override class IName::get_Name
	// Code size 11 (0xb)
	.maxstack 1
	.locals init (
		string	V_0)
	IL_0000:  nop 
	IL_0001:  ldstr "InterfaceName"
	IL_0006:  stloc.0 
	IL_0007:  br.s IL_0009

	IL_0009:  ldloc.0 
	IL_000a:  ret 
    } // end of method Name2::IName.get_Name

    // method line 22
    .method public final virtual hidebysig newslot specialname 
           instance default int32 get_other ()  cil managed 
    {
        // Method begins at RVA 0x21c0
	// Code size 8 (0x8)
	.maxstack 1
	.locals init (
		int32	V_0)
	IL_0000:  nop 
	IL_0001:  ldc.i4.s 0x2b
	IL_0003:  stloc.0 
	IL_0004:  br.s IL_0006

	IL_0006:  ldloc.0 
	IL_0007:  ret 
    } // end of method Name2::get_other

    // method line 23
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x21d4
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  nop 
	IL_0007:  ret 
    } // end of method Name2::.ctor

	.property instance string Name ()
	{
		.get instance default string Name2::get_Name () 
	}
	.property instance string IName.Name ()
	{
		.get instance default string Name2::IName.get_Name () 
	}
	.property instance int32 other ()
	{
		.get instance default int32 Name2::get_other () 
	}
  } // end of class Name2

  .class private auto ansi beforefieldinit Program
  	extends [mscorlib]System.Object
  {

    // method line 24
    .method private static hidebysig 
           default int32 Main (string[] args)  cil managed 
    {
        // Method begins at RVA 0x21e0
	.entrypoint
	// Code size 199 (0xc7)
	.maxstack 2
	.locals init (
		class Name1	V_0,
		class Name2	V_1,
		class IName	V_2,
		class IName	V_3,
		class I1	V_4,
		class I1t	V_5,
		bool	V_6,
		int32	V_7,
		bool	V_8,
		bool	V_9,
		bool	V_10,
		bool	V_11)
	IL_0000:  nop 
	IL_0001:  newobj instance void class Name1::'.ctor'()
	IL_0006:  stloc.0 
	IL_0007:  newobj instance void class Name2::'.ctor'()
	IL_000c:  stloc.1 
	IL_000d:  ldloc.0 
	IL_000e:  stloc.2 
	IL_000f:  ldloc.1 
	IL_0010:  stloc.3 
	IL_0011:  ldloc.2 
	IL_0012:  callvirt instance string class IName::get_Name()
	IL_0017:  ldstr "InterfaceName"
	IL_001c:  callvirt instance bool string::Equals(string)
	IL_0021:  ldc.i4.0 
	IL_0022:  ceq 
	IL_0024:  stloc.s 6
	IL_0026:  ldloc.s 6
	IL_0028:  brfalse.s IL_0033

	IL_002a:  ldc.i4.s 0x0a
	IL_002c:  stloc.s 7
	IL_002e:  br IL_00c4

	IL_0033:  ldloc.3 
	IL_0034:  callvirt instance string class IName::get_Name()
	IL_0039:  ldstr "InterfaceName"
	IL_003e:  callvirt instance bool string::Equals(string)
	IL_0043:  ldc.i4.0 
	IL_0044:  ceq 
	IL_0046:  stloc.s 8
	IL_0048:  ldloc.s 8
	IL_004a:  brfalse.s IL_0052

	IL_004c:  ldc.i4.s 0x14
	IL_004e:  stloc.s 7
	IL_0050:  br.s IL_00c4

	IL_0052:  ldloc.0 
	IL_0053:  callvirt instance string class Name1::get_Name()
	IL_0058:  ldstr "ClassName"
	IL_005d:  callvirt instance bool string::Equals(string)
	IL_0062:  ldc.i4.0 
	IL_0063:  ceq 
	IL_0065:  stloc.s 9
	IL_0067:  ldloc.s 9
	IL_0069:  brfalse.s IL_0071

	IL_006b:  ldc.i4.s 0x1e
	IL_006d:  stloc.s 7
	IL_006f:  br.s IL_00c4

	IL_0071:  ldloc.1 
	IL_0072:  callvirt instance string class Name2::get_Name()
	IL_0077:  ldstr "ClassName"
	IL_007c:  callvirt instance bool string::Equals(string)
	IL_0081:  ldc.i4.0 
	IL_0082:  ceq 
	IL_0084:  stloc.s 10
	IL_0086:  ldloc.s 10
	IL_0088:  brfalse.s IL_0090

	IL_008a:  ldc.i4.s 0x28
	IL_008c:  stloc.s 7
	IL_008e:  br.s IL_00c4

	IL_0090:  newobj instance void class Test10::'.ctor'()
	IL_0095:  stloc.s 4
	IL_0097:  ldloc.s 4
	IL_0099:  callvirt instance int32 class I1::M1()
	IL_009e:  ldc.i4.0 
	IL_009f:  cgt.un 
	IL_00a1:  stloc.s 11
	IL_00a3:  ldloc.s 11
	IL_00a5:  brfalse.s IL_00b2

	IL_00a7:  ldloc.s 4
	IL_00a9:  callvirt instance int32 class I1::M1()
	IL_00ae:  stloc.s 7
	IL_00b0:  br.s IL_00c4

	IL_00b2:  newobj instance void class Test10t::'.ctor'()
	IL_00b7:  stloc.s 5
	IL_00b9:  ldloc.s 5
	IL_00bb:  callvirt instance int32 class I1t::M1t()
	IL_00c0:  stloc.s 7
	IL_00c2:  br.s IL_00c4

	IL_00c4:  ldloc.s 7
	IL_00c6:  ret 
    } // end of method Program::Main

    // method line 25
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x22b3
	// Code size 8 (0x8)
	.maxstack 8
	IL_0000:  ldarg.0 
	IL_0001:  call instance void object::'.ctor'()
	IL_0006:  nop 
	IL_0007:  ret 
    } // end of method Program::.ctor

  } // end of class Program

/*
using System;

public interface I1
{
    int M1()
    {
        return 100;
    }
}

public interface I2 : I1 { int I1.M1() { return 200; } }

public interface I3 : I1 { int I1.M1() { return 300; } }

public interface I4 : I1 { int I1.M1() { return 400; } }

class Test10 : I1, I2, I3, I4 {
    //void I1.M1() { System.Console.WriteLine("I1.I1.M1"); }
    public int M1() { return 0; }
}

public interface I1t
{
    int M1t()
    {
        return 100;
    }
}

public interface I2t : I1t { int I1t.M1t() { return 200; } }

public interface I3t : I1t { int I1t.M1t() { return 300; } }

public interface I4t : I1t { int I1t.M1t() { return 400; } }

class Test10t : I1t, I2t, I3t, I4t
{
    int I1t.M1t() { return 0; }
    public int M1t() { return 10; }
}


public interface IName
{
	string Name { get; }
}
public interface IOther<T>
{
	T other { get; }
	string Name { get; }
}
public class Name1 : IName
{
	public string Name { get { return "ClassName"; } }
	string IName.Name { get { return "InterfaceName"; } }
}
public class Name2 : IName, IOther<int>
{
	public string Name { get { return "ClassName"; } }
	string IName.Name { get { return "InterfaceName"; } }
	public int other { get { return 43; } }
}

class Program
{
    static int Main(string[] args)
    {
        var name1 = new Name1();
        var name2 = new Name2();
        IName iName1 = name1;
        IName iName2 = name2;
        if (!iName1.Name.Equals("InterfaceName"))
            return 10;
        if (!iName2.Name.Equals("InterfaceName"))
            return 20;
        if (!name1.Name.Equals("ClassName"))
            return 30;
        if (!name2.Name.Equals("ClassName"))
            return 40;

        I1 var = new Test10();
        if (var.M1() != 0)
            return var.M1();

        I1t var2 = new Test10t();
        return var2.M1t();
    }
}

*/
