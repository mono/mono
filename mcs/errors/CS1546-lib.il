.assembly extern mscorlib { }

.assembly 'cs1546-lib' { }

.namespace Test {

.class interface public abstract auto ansi import ITopic
{
      .property string Title(int32)
      {
            .get instance string Test.ITopic::get_Title(int32, int32)
      }
	  
	.method public hidebysig newslot specialname abstract virtual instance string get_Title(int32, int32) runtime managed internalcall
	{
	}
}
}
