// cs0672.cs: Member 'B.Test()' overrides obsolete member 'A.Test(). Add the Obsolete attribute to 'B.Test()
// Line: 14

using System;

public class A
{
        [Obsolete ("Causes an error", true)]
        public virtual void Test () {}
}

public class B: A
{
        public override void Test () {}
}

