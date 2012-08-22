using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines
{
	internal abstract class CallingContractSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>, IMethodInfo<Method>
    {
      private Method methodWithThisContract;
      protected MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> Builder;

      public Method Method
      {
        get
        {
          return this.methodWithThisContract;
        }
      }

      public CallingContractSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method)
        : base(methodCache)
      {
        this.methodWithThisContract = method;
      }

      public CallingContractSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel)
        : base(methodCache, startLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>) builder)
      {
        this.Builder = builder;
        this.methodWithThisContract = method;
      }
    }
}

