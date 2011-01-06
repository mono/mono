.assembly extern mscorlib
{
}

.assembly extern 'CS0012-lib-missing'
{
}

.assembly 'CS0012-10-lib'
{
  .hash algorithm 0x00008004
  .ver 0:0:0:0
}

.module 'CS0012-10-lib.dll'

.class public auto ansi sealed D`1<(I) T>
       extends [mscorlib]System.MulticastDelegate
{
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(object 'object',
                               native int 'method') runtime managed
  {
  }

  .method public hidebysig newslot virtual 
          instance !T  Invoke() runtime managed
  {
  }

  .method public hidebysig newslot virtual 
          instance class [mscorlib]System.IAsyncResult 
          BeginInvoke(class [mscorlib]System.AsyncCallback callback,
                      object 'object') runtime managed
  {
  }

  .method public hidebysig newslot virtual 
          instance !T  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
  {
  }

}

.class interface public abstract auto ansi I
       implements ['CS0012-lib-missing']I1
{
}