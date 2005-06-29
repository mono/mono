// cs0674.cs: Do not use `System.ParamArrayAttribute'. Use the `params' keyword instead
// Line: 8

using System;

public class X
{
        public void Error ([ParamArray] int args)
        {
        }
       
}
