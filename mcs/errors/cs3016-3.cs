// cs3016.cs: Arrays as attribute arguments is not CLS-compliant
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