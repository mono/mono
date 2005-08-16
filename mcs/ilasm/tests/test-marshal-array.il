.assembly extern mscorlib
{
}
.assembly 'test-marshal-array.il'
{
}

  .class public auto ansi beforefieldinit Tests
  	extends [mscorlib]System.Object
  {

    .method public virtual  hidebysig 
           instance default int32 Invoke ([in] string[] marshal (lpstr[+ 3]) arr_ad1)  runtime managed 
    {
    }

    .method public virtual  hidebysig 
           instance default int32 Invoke2([in] string[] marshal (lpstr[2]) arr_ad2)  runtime managed 
    {
    }
  
    .method public virtual  hidebysig 
           instance default int32 Invoke3 ([in] string[] marshal (lpstr[2 + 3]) arr_ad3)  runtime managed 
    {
    }

     .method public virtual  hidebysig 
           instance default int32 Invoke4 ([in] string[] marshal (lpstr[]) arr_ad4)  runtime managed 
    {
    }
  }

