using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis
{
  internal class MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> : IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Unit, Subroutine>
  {
    private static IEnumerable<Field> emptyFields = (IEnumerable<Field>) new Field[0];
    private static IEnumerable<Method> emptyMethods = (IEnumerable<Method>) new Method[0];
    private readonly Dictionary<Method, ControlFlow<Method, Type>> methodCache = new Dictionary<Method, ControlFlow<Method, Type>>();
    private readonly Dictionary<Method, Set<Field>> methodModifies = new Dictionary<Method, Set<Field>>();
    private readonly Dictionary<Method, Set<Field>> methodReads = new Dictionary<Method, Set<Field>>();
    private readonly Dictionary<Field, Set<Method>> propertyReads = new Dictionary<Field, Set<Method>>();
    private readonly Dictionary<Subroutine, Subroutine> redundantInvariants = new Dictionary<Subroutine, Subroutine>();
    public readonly IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder;
    public readonly IDecodeContracts<Local, Parameter, Method, Field, Type> ContractDecoder;
    private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.InvariantCache invariantCache;
    private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresCache requiresCache;
    private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresCache ensuresCache;
    private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.ModelEnsuresCache modelEnsuresCache;

    static MethodCache()
    {
    }

    public MethodCache(IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> mdDecoder, IDecodeContracts<Local, Parameter, Method, Field, Type> contractDecoder, ErrorHandler output)
    {
      this.MetadataDecoder = mdDecoder;
      this.ContractDecoder = contractDecoder;
      this.requiresCache = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresCache(this, output);
      this.ensuresCache = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresCache(this);
      this.modelEnsuresCache = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.ModelEnsuresCache(this);
      this.invariantCache = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.InvariantCache(this);
    }

    private void ObjectInvariant()
    {
    }

    public ICFG GetCFG(Method method)
    {
      if (this.methodCache.ContainsKey(method))
        return (ICFG) this.methodCache[method];
      if (this.MetadataDecoder.HasBody(method))
        return (ICFG) new ControlFlow<Method, Type>(this.MetadataDecoder.AccessMethodBody<Unit, Subroutine>(method, (IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Unit, Subroutine>) this, Unit.Value), (object) this);
      else
        throw new InvalidOperationException("Method has no body");
    }

    internal Subroutine GetRequires(Method method)
    {
      method = this.MetadataDecoder.Unspecialized(method);
      return this.requiresCache.Get(method);
    }

    internal Subroutine GetEnsures(Method method)
    {
      method = this.MetadataDecoder.Unspecialized(method);
      return this.ensuresCache.Get(method);
    }

    internal Subroutine GetModelEnsures(Method method)
    {
      method = this.MetadataDecoder.Unspecialized(method);
      return this.modelEnsuresCache.Get(method);
    }

    public Subroutine GetInvariant(Type type)
    {
      type = this.MetadataDecoder.Unspecialized(type);
      return this.invariantCache.Get(type);
    }

    public Subroutine GetRedundantInvariant(Subroutine existingInvariant, Type type)
    {
      Subroutine subroutine1;
      if (this.redundantInvariants.TryGetValue(existingInvariant, out subroutine1))
        return subroutine1;
      Subroutine subroutine2 = (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.InvariantSubroutine<Unit>(this, existingInvariant, type);
      this.redundantInvariants.Add(existingInvariant, subroutine2);
      return subroutine2;
    }

    internal Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data) where Visitor : IVisitMSIL<APC, Local, Parameter, Method, Field, Type, Unit, Unit, Data, Result>
    {
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockBase blockBase = pc.Block as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockBase;
      if (blockBase != null)
        return blockBase.ForwardDecode<Data, Result, Visitor>(pc, visitor, data);
      else
        return visitor.Nop(pc, data);
    }

    Subroutine IMethodCodeConsumer<Local, Parameter, Method, Field, Type, Unit, Subroutine>.Accept<Label, Handler>(IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> codeProvider, Label entryPoint, Method method, Unit data)
    {
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler> builder = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler>(codeProvider, this, method, entryPoint);
      return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodSubroutine<Label, Handler>(this, method, builder, entryPoint);
    }

    public bool AddPreCondition<Label>(Method method, Label precondition, ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider)
    {
      Subroutine requires = this.GetRequires(method);
      if (requires != null)
      {
        IMethodInfo<Method> methodInfo = requires as IMethodInfo<Method>;
        if (methodInfo != null && !this.MetadataDecoder.Equal(methodInfo.Method, method))
          return false;
      }
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label>(codeProvider, this, precondition);
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label> requiresSubroutine = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label>(this, method, builder, precondition, ImmutableSet<Subroutine>.Empty());
      requiresSubroutine.Initialize();
      if (requires == null)
      {
        this.requiresCache.Install(method, (Subroutine) requiresSubroutine);
        return true;
      }
      else
      {
        foreach (CFGBlock from in requires.PredecessorBlocks(requires.Exit))
          requires.AddEdgeSubroutine(from, requires.Exit, (Subroutine) requiresSubroutine, "extra");
        return true;
      }
    }

    public bool RemovePreCondition(Method method)
    {
      return this.requiresCache.Remove(method);
    }

    public bool AddPostCondition<Label>(Method method, Label postCondition, ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider)
    {
      Subroutine ensures = this.GetEnsures(method);
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label>(codeProvider, this, postCondition);
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> ensuresSubroutine = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>(this, method, builder, postCondition, ImmutableSet<Subroutine>.Empty());
      ensuresSubroutine.Initialize();
      if (ensures == null)
      {
        this.ensuresCache.Install(method, (Subroutine) ensuresSubroutine);
        return true;
      }
      else
      {
        foreach (CFGBlock from in ensures.PredecessorBlocks(ensures.Exit))
          ensures.AddEdgeSubroutine(from, ensures.Exit, (Subroutine) ensuresSubroutine, "extra");
        return true;
      }
    }

    public bool AddInvariant<Label>(Type type, Label invariant, ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider)
    {
      Subroutine invariant1 = this.GetInvariant(type);
      if (invariant1 != null)
      {
        ITypeInfo<Type> typeInfo = invariant1 as ITypeInfo<Type>;
        if (typeInfo != null && !this.MetadataDecoder.Equal(typeInfo.AssociatedType, type))
          return false;
      }
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.InvariantSubroutine<Label> invariantSubroutine = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.InvariantSubroutine<Label>(this, new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label>(codeProvider, this, invariant), invariant, invariant1, type);
      invariantSubroutine.Initialize();
      if (invariant1 == null)
      {
        this.invariantCache.Install(type, (Subroutine) invariantSubroutine);
        return true;
      }
      else
      {
        foreach (CFGBlock from in invariant1.PredecessorBlocks(invariant1.Exit))
          invariant1.AddEdgeSubroutine(from, invariant1.Exit, (Subroutine) invariantSubroutine, "extra");
        return true;
      }
    }

    internal Subroutine BuildSubroutine<Label>(int stackDelta, ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider, Label entryPoint)
    {
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label>(codeProvider, this, entryPoint);
      return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutine<Label>(stackDelta, this, entryPoint, builder);
    }

    internal IEnumerable<Field> GetModifies(Method method)
    {
      if (this.MetadataDecoder.HasBody(method))
      {
        this.GetCFG(method);
        Set<Field> set;
        if (this.methodModifies.TryGetValue(method, out set))
          return (IEnumerable<Field>) set;
      }
      return MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.emptyFields;
    }

    internal IEnumerable<Field> GetReads(Method method)
    {
      if (this.MetadataDecoder.HasBody(method))
      {
        this.GetCFG(method);
        Set<Field> set;
        if (this.methodReads.TryGetValue(method, out set))
          return (IEnumerable<Field>) set;
      }
      return MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.emptyFields;
    }

    internal IEnumerable<Method> GetAffectedGetters(Field field)
    {
      Set<Method> set;
      if (this.propertyReads.TryGetValue(field, out set))
        return (IEnumerable<Method>) set;
      else
        return MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.emptyMethods;
    }

    private Set<Field> ModifiesSet(Method method)
    {
      Set<Field> set;
      if (!this.methodModifies.TryGetValue(method, out set))
      {
        set = new Set<Field>();
        this.methodModifies.Add(method, set);
      }
      return set;
    }

    private Set<Field> ReadSet(Method method)
    {
      Set<Field> set;
      if (!this.methodReads.TryGetValue(method, out set))
      {
        set = new Set<Field>();
        this.methodReads.Add(method, set);
      }
      return set;
    }

    private Set<Method> PropertySet(Field field)
    {
      Set<Method> set;
      if (!this.propertyReads.TryGetValue(field, out set))
      {
        set = new Set<Method>();
        this.propertyReads.Add(field, set);
      }
      return set;
    }

    internal void AddModifies(Method method, Field field)
    {
      this.ModifiesSet(method).Add(field);
    }

    public bool IsMonitorWaitOrExit(Method method)
    {
      if (this.MetadataDecoder.Name(this.MetadataDecoder.DeclaringType(method)) != "Monitor")
        return false;
      string str = this.MetadataDecoder.Name(method);
      return str == "Exit" || str == "Wait";
    }

    internal void AddReads(Method method, Field field)
    {
      this.ReadSet(method).Add(field);
      this.PropertySet(field).Add(method);
    }

    public void RemoveContractsFor(Method method)
    {
      if (this.methodCache.ContainsKey(method))
        this.methodCache.Remove(method);
      if (this.methodModifies.ContainsKey(method))
        this.methodModifies.Remove(method);
      if (this.methodReads.ContainsKey(method))
        this.methodReads.Remove(method);
      this.requiresCache.Remove(method);
      this.ensuresCache.Remove(method);
      this.modelEnsuresCache.Remove(method);
    }
  }
}
