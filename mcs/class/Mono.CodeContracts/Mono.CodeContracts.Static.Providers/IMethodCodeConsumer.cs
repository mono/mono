using System;

namespace Mono.CodeContracts.Static.Providers
{
  interface IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Data, Result>
  {
    Result Accept<Label, Handler>(IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> codeProvider, Label entryPoint, Method method, Data data);
  }
}


