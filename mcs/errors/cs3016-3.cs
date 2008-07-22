// CS3016: Arrays as attribute arguments are not CLS-compliant
// Line: 13
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

public class CLSAttribute: Attribute {
        public CLSAttribute() {}
        public CLSAttribute(string[] array) {}
}

[CLSAttribute(new string[] { "", "" })]
public interface ITest {
}
