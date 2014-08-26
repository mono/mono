.assembly extern 'cs0012-24-missing'
{
}

.assembly extern mscorlib
{
}

.assembly 'CS0012-24-lib'
{
}

.module 'cs0012-24-lib.dll'

  .class public auto ansi beforefieldinit A
  	extends ['cs0012-24-missing']A0
  {

    .method public hidebysig specialname rtspecialname
           instance default void '.ctor' ()  cil managed
    {
	   ldarg.0
	   call instance void class ['cs0012-24-missing']A0::'.ctor'()
	   ret
    }

    .method public static hidebysig
           default void Test ()  cil managed
    {
    ret
    }

  }