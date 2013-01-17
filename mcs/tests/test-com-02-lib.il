.assembly extern mscorlib { }

.assembly 'test-com-02-lib' { }

.module 'test-com-02-lib.dll'

.namespace Test {

.class interface public abstract auto ansi import ITopic
{
      .property string Title(int32)
      {
            .get instance string Test.ITopic::get_Title(int32, int32)
			.set instance string Test.ITopic::set_Title(int32, int32, bool, string)
      }
	  
	.method public hidebysig newslot specialname abstract virtual instance string get_Title(int32, int32) runtime managed internalcall
	{
	}
	
	.method public hidebysig newslot specialname abstract virtual instance string set_Title(int32, int32, bool, string) runtime managed internalcall
	{
	}
	
}
}
