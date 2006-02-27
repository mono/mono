.assembly extern mscorlib
{
}
.assembly 'test-safearray'
{
}

  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {

    .method private hidebysig 
           instance default void ret_safearray_int (int32 marshal (safearray int) a1)  cil managed 
    {
	.maxstack 8
	IL_0000:  ret 
    }

    .method private hidebysig 
           instance default void ret_safearray (int32 marshal (safearray ) a2)  cil managed 
    {
	.maxstack 8
	IL_0000:  ret 
    } 

  } 

