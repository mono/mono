using System;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines
{
	internal class InvariantSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>, ITypeInfo<Type>
    {
      private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> Builder;
      private Type associatedType;

      public override string Kind
      {
        get
        {
          return "invariant";
        }
      }

      public override bool IsInvariant
      {
        get
        {
          return true;
        }
      }

      public override bool IsContract
      {
        get
        {
          return true;
        }
      }

      Type ITypeInfo<Type>.AssociatedType
      {
        get
        {
          return this.associatedType;
        }
      }

      public InvariantSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel, Subroutine baseInv, Type associatedType)
        : base(methodCache, startLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>) builder)
      {
        this.Builder = builder;
        this.associatedType = associatedType;
        this.AddBaseInvariant(this.Entry, (CFGBlock) this.GetTargetBlock(startLabel), baseInv);
      }

      public InvariantSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Subroutine inherited, Type associatedType)
        : base(methodCache)
      {
        this.AddSuccessor(this.Entry, "entry", this.Exit);
        this.AddBaseInvariant(this.Entry, this.Exit, inherited);
        this.Commit();
      }

      internal override void Initialize()
      {
        if (this.Builder == null)
          return;
        this.Builder.BuildBlocks(this.startLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>) this);
        this.Commit();
        this.Builder = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label>) null;
      }

      private void AddBaseInvariant(CFGBlock fromBlock, CFGBlock toBlock, Subroutine inherited)
      {
        this.AddEdgeSubroutine(fromBlock, toBlock, inherited, "inherited");
      }
    }
}

