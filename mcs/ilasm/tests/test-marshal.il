.assembly extern mscorlib
{
}
.assembly 'test-marshal'
{
}


  .class private auto ansi beforefieldinit test
  	extends [mscorlib]System.Object
  {
    .field  private  marshal (int32[]) int32 a

    .method private hidebysig 
           instance default void marshal (as any) xyz ()  cil managed 
    {
	.maxstack 8
	IL_0000:  ret 
    }

    .method private hidebysig 
           instance default object marshal (interface) abc (class test 'assembly', string marshal (bool) str, string str2, int32 i4)  cil managed 
    {
	.maxstack 8
	IL_0000:  ldnull 
	IL_0001:  ret 
    }

    .method private hidebysig 
           instance default void marshal (as any) def (class test 'assembly', string str, string str2, int32 marshal (bool) i4)  cil managed 
    {
	.maxstack 8
	IL_0000:  ret 
    }

    .method public static hidebysig pinvokeimpl ("foo" as "foo" winapi )
           default void foo (string marshal (vbbyrefstr) s)  cil managed preservesig 
    {
    } // end of method C::foo

  }

