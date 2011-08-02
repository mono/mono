// CS0672: Member `B.Test(string)' overrides obsolete member `A.Test(string)'. Add the Obsolete attribute to `B.Test(string)'
// Line: 15
// Compiler options: -warnaserror

using System;

public class A
{
        [Obsolete ("Causes an error", true)]
        public virtual void Test (string arg) {}
}

public class B: A
{
        public override void Test (string arg) {}
}

