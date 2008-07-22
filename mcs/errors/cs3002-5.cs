// CS3002: Return type of `CLSClass.Test1()' is not CLS-compliant
// Line: 14
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface I {}

public class C {}

public class CLSClass {
	public I Test1() { return null; } 
	public C Test2() { return null; }
}
