using System;
using Mono.CodeContracts.Static.ControlFlow.Subroutines;

namespace Mono.CodeContracts.Static.ControlFlow.Blocks
{
	internal class SwitchCaseAssumeBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      private readonly Label SwitchLabel;
      private readonly object Pattern;
      private readonly Type Type;

      public override int Count
      {
        get
        {
          return 3;
        }
      }

      public SwitchCaseAssumeBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, Label switchLabel, object pattern, Type type, ref int idgen)
        : base(container, ref idgen)
      {
        this.SwitchLabel = switchLabel;
        this.Pattern = pattern;
        this.Type = type;
      }

      internal override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        if (pc.Index == 0)
          return visitor.Ldconst(pc, this.Pattern, this.Type, Unit.Value, data);
        if (pc.Index == 1)
          return visitor.Binary(pc, BinaryOperator.Ceq, Unit.Value, Unit.Value, Unit.Value, data);
        if (pc.Index == 2)
          return visitor.Assume(pc, "true", Unit.Value, (object) null, data);
        else
          return visitor.Nop(pc, data);
      }
    }
}

