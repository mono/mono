// cs3002.cs: Argument type 'CLSClass.Test1()' is not CLS-compliant
// Line: 11

using System;
[assembly:CLSCompliant(true)]

[CLSCompliant(false)]
public interface I {}

public class C {}

public class CLSClass {
        public I Test1() { return null; } 
	public C Test2() { return null; }
}
