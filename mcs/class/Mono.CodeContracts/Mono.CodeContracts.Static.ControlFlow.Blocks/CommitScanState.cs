using System;
using Mono.CodeContracts.Static.ControlFlow.Subroutines;

namespace Mono.CodeContracts.Static.ControlFlow.Blocks
{
	  internal class CommitScanState : MSILVisitor<Label, Local, Parameter, Method, Field, Type, Unit, Unit, int, bool>, ICodeQuery<Label, Local, Parameter, Method, Field, Type, int, bool>, IVisitMSIL<Label, Local, Parameter, Method, Field, Type, Unit, Unit, int, bool>, IVisitSynthIL<Label, Method, Type, Unit, Unit, int, bool>, IVisitExprIL<Label, Type, Unit, Unit, int, bool>
      {
        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState state;
        private Type nextEndOldType;
        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label> currentBlock;
        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> parent;
        private OnDemandMap<CFGBlock, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState> blockStartState;

        public CommitScanState(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> parent)
        {
          this.parent = parent;
          this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
        }

        public void StartBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label> block)
        {
          if (!this.blockStartState.TryGetValue((CFGBlock) block, out this.state))
          {
            this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
            this.blockStartState.Add((CFGBlock) block, this.state);
          }
          if (this.state == 2)
            block.StartOverridingLabels();
          if (this.state == 3)
          {
            block.StartOverridingLabels();
            block.EndOldWithoutInstruction(this.nextEndOldType);
            this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
          }
          this.currentBlock = block;
        }

        public void SetStartState(CFGBlock succ)
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState scanState;
          if (this.blockStartState.TryGetValue(succ, out scanState))
            return;
          this.blockStartState.Add(succ, this.state);
        }

        protected override bool Default(Label pc, int index)
        {
          if (this.state != 2)
            return this.currentBlock.UsesOverriding;
          this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
          this.currentBlock.EndOldWithoutInstruction(this.parent.MethodCache.MetadataDecoder.ManagedPointer(this.nextEndOldType));
          return true;
        }

        public bool Aggregate(Label pc, Label aggStart, bool branchTarget, int data)
        {
          return this.Nop(pc, data);
        }

        public override bool Nop(Label pc, int data)
        {
          return this.currentBlock.UsesOverriding;
        }

        public override bool BeginOld(Label pc, Label matchingEnd, int index)
        {
          this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 1;
          return this.currentBlock.UsesOverriding;
        }

        public override bool EndOld(Label pc, Label matchingBegin, Type type, Unit dest, Unit source, int index)
        {
          this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
          return this.currentBlock.UsesOverriding;
        }

        public override bool Ldarga(Label pc, Parameter argument, bool dummyIsOld, Unit dest, int index)
        {
          // ISSUE: unable to decompile the method.
        }

        public override bool Ldind(Label pc, Type type, bool @volatile, Unit dest, Unit ptr, int index)
        {
          if (this.state != 2)
            return this.currentBlock.UsesOverriding;
          this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
          this.currentBlock.EndOld(index, this.nextEndOldType);
          return false;
        }

        public override bool Ldfld(Label pc, Field field, bool @volatile, Unit dest, Unit obj, int index)
        {
          if (this.state != 2)
            return this.currentBlock.UsesOverriding;
          this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
          this.currentBlock.EndOld(index, this.parent.MethodCache.MetadataDecoder.FieldType(field));
          return false;
        }

        public override bool Ldflda(Label pc, Field field, Unit dest, Unit obj, int data)
        {
          if (this.state == 2)
            this.nextEndOldType = this.parent.MethodCache.MetadataDecoder.FieldType(field);
          return this.currentBlock.UsesOverriding;
        }

        public override bool Call<TypeList, ArgList>(Label pc, Method method, bool tail, bool virt, TypeList extraVarargs, Unit dest, ArgList args, int index)
        {
          return false;
        }

        internal void HandlePotentialCallBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label> block, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label> priorBlock)
        {
          if (block == null || this.state != 2)
            return;
          int count = this.parent.MethodCache.MetadataDecoder.Parameters(block.CalledMethod).Count;
          if (!this.parent.MethodCache.MetadataDecoder.IsStatic(block.CalledMethod))
            ++count;
          if (count > 1)
          {
            this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 0;
            Type nextEndOldType = this.parent.MethodCache.MetadataDecoder.ManagedPointer(this.nextEndOldType);
            priorBlock.EndOldWithoutInstruction(nextEndOldType);
          }
          else
          {
            this.state = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.ScanState) 3;
            this.nextEndOldType = this.parent.MethodCache.MetadataDecoder.ReturnType(block.CalledMethod);
          }
        }
      }
}

