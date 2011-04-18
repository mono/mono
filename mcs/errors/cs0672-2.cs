// CS0672: Member `C.Method()' overrides obsolete member `BaseClass.Method()'. Add the Obsolete attribute to `C.Method()'
// Line: 14
// Compiler options: -warnaserror

using System;

class BaseClass {
        [Obsolete]
        protected virtual void Method () {}
}

class C: BaseClass
{
        protected override void Method () {}
}