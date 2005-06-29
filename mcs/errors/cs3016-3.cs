// cs3016-3.cs: Arrays as attribute arguments are not CLS-compliant
// Line: 12

using System;
[assembly:CLSCompliant(true)]

public class CLSAttribute: Attribute {
        public CLSAttribute() {}
        public CLSAttribute(string[] array) {}
}

[CLSAttribute(new string[] { "", "" })]
public interface ITest {
}