//
// cdeclCallback.il
//
// Author: 
//		Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/2003 Ximian, Inc.
.assembly extern mscorlib
{
}
.assembly cdeclCallback
{
}

.namespace cdeclCallback
{
  .class public auto ansi beforefieldinit cdeclRedirector
         extends [mscorlib]System.Object
  {
    .class auto ansi sealed nested public MethodVoidIntPtr
           extends [mscorlib]System.MulticastDelegate
    {
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor(object 'object',
                                   native int 'method') runtime managed
      {
      } // end of method MethodVoidIntPtr::.ctor

      .method public hidebysig virtual instance void modopt([mscorlib]System.Runtime.CompilerServices.CallConvCdecl) 
              Invoke(native int param) runtime managed
      {
      } // end of method MethodVoidIntPtr::Invoke

      .method public hidebysig newslot virtual 
              instance class [mscorlib]System.IAsyncResult 
              BeginInvoke(native int param,
                          class [mscorlib]System.AsyncCallback callback,
                          object 'object') runtime managed
      {
      } // end of method MethodVoidIntPtr::BeginInvoke

      .method public hidebysig newslot virtual 
              instance void  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
      {
      } // end of method MethodVoidIntPtr::EndInvoke

    } // end of class MethodVoidIntPtr

    .class auto ansi sealed nested public MethodIntIntPtr
           extends [mscorlib]System.MulticastDelegate
    {
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor(object 'object',
                                   native int 'method') runtime managed
      {
      } // end of method MethodIntIntPtr::.ctor

      .method public hidebysig virtual instance int32 modopt([mscorlib]System.Runtime.CompilerServices.CallConvCdecl) 
              Invoke(native int param) runtime managed
      {
      } // end of method MethodIntIntPtr::Invoke

      .method public hidebysig newslot virtual 
              instance class [mscorlib]System.IAsyncResult 
              BeginInvoke(native int param,
                          class [mscorlib]System.AsyncCallback callback,
                          object 'object') runtime managed
      {
      } // end of method MethodIntIntPtr::BeginInvoke

      .method public hidebysig newslot virtual 
              instance int32  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
      {
      } // end of method MethodIntIntPtr::EndInvoke

    } // end of class MethodIntIntPtr

    .class auto ansi sealed nested public MethodVoidIntPtrInt
           extends [mscorlib]System.MulticastDelegate
    {
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor(object 'object',
                                   native int 'method') runtime managed
      {
      } // end of method MethodVoidIntPtrInt::.ctor

      .method public hidebysig virtual instance void modopt([mscorlib]System.Runtime.CompilerServices.CallConvCdecl) 
              Invoke(native int param,
                     int32 param1) runtime managed
      {
      } // end of method MethodVoidIntPtrInt::Invoke

      .method public hidebysig newslot virtual 
              instance class [mscorlib]System.IAsyncResult 
              BeginInvoke(native int param,
                          int32 param1,
                          class [mscorlib]System.AsyncCallback callback,
                          object 'object') runtime managed
      {
      } // end of method MethodVoidIntPtrInt::BeginInvoke

      .method public hidebysig newslot virtual 
              instance void  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
      {
      } // end of method MethodVoidIntPtrInt::EndInvoke

    } // end of class MethodVoidIntPtrInt

    .class auto ansi sealed nested public MethodIntIntPtrInt
           extends [mscorlib]System.MulticastDelegate
    {
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor(object 'object',
                                   native int 'method') runtime managed
      {
      } // end of method MethodIntIntPtrInt::.ctor

      .method public hidebysig virtual instance int32 modopt([mscorlib]System.Runtime.CompilerServices.CallConvCdecl) 
              Invoke(native int param,
                     int32 param1) runtime managed
      {
      } // end of method MethodIntIntPtrInt::Invoke

      .method public hidebysig newslot virtual 
              instance class [mscorlib]System.IAsyncResult 
              BeginInvoke(native int param,
                          int32 param1,
                          class [mscorlib]System.AsyncCallback callback,
                          object 'object') runtime managed
      {
      } // end of method MethodIntIntPtrInt::BeginInvoke

      .method public hidebysig newslot virtual 
              instance int32  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
      {
      } // end of method MethodIntIntPtrInt::EndInvoke

    } // end of class MethodIntIntPtrInt

    .class auto ansi sealed nested public MethodVoidIntPtrIntPtr
           extends [mscorlib]System.MulticastDelegate
    {
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor(object 'object',
                                   native int 'method') runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtr::.ctor

      .method public hidebysig virtual instance void modopt([mscorlib]System.Runtime.CompilerServices.CallConvCdecl) 
              Invoke(native int param,
                     native int param1) runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtr::Invoke

      .method public hidebysig newslot virtual 
              instance class [mscorlib]System.IAsyncResult 
              BeginInvoke(native int param,
                          native int param1,
                          class [mscorlib]System.AsyncCallback callback,
                          object 'object') runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtr::BeginInvoke

      .method public hidebysig newslot virtual 
              instance int32  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtr::EndInvoke
    } // end of class MethodVoidIntPtrIntPtr

    .class auto ansi sealed nested public MethodVoidIntPtrIntPtrInt
           extends [mscorlib]System.MulticastDelegate
    {
      .method public hidebysig specialname rtspecialname 
              instance void  .ctor(object 'object',
                                   native int 'method') runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtrInt::.ctor

      .method public hidebysig virtual instance void modopt([mscorlib]System.Runtime.CompilerServices.CallConvCdecl) 
              Invoke(native int param,
		     native int param1,
                     int32 param2) runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtrInt::Invoke

      .method public hidebysig newslot virtual 
              instance class [mscorlib]System.IAsyncResult 
              BeginInvoke(native int param,
			  native int param1,
                          int32 param2,
                          class [mscorlib]System.AsyncCallback callback,
                          object 'object') runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtrInt::BeginInvoke

      .method public hidebysig newslot virtual 
              instance int32  EndInvoke(class [mscorlib]System.IAsyncResult result) runtime managed
      {
      } // end of method MethodVoidIntPtrIntPtrInt::EndInvoke


    } // end of class MethodVoidIntPtrIntPtrInt
  } // end of class cdeclRedirector
}
