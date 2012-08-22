using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines
{
	internal class ModelEnsuresSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>
    {
      public override bool IsModelEnsures
      {
        get
        {
          return true;
        }
      }

      public override string Kind
      {
        get
        {
          return "model-ensures";
        }
      }

      public ModelEnsuresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel, IImmutableSet<Subroutine> inherited)
        : base(methodCache, method, builder, startLabel, inherited)
      {
      }

      public ModelEnsuresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, IImmutableSet<Subroutine> inherited)
        : base(methodCache, method, inherited)
      {
      }
    }
}

