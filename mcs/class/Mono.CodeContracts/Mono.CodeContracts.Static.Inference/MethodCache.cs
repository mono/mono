using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;

namespace Mono.CodeContracts.Static.Inference
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
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label> requiresSubroutine = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label>(this, method, builder, precondition, FunctionalSet<Subroutine>.Empty());
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
      MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> ensuresSubroutine = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>(this, method, builder, postCondition, FunctionalSet<Subroutine>.Empty());
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

    private abstract class SubroutineFactory<Key, Data> : ICodeConsumer<Local, Parameter, Method, Field, Type, Data, Subroutine>
    {
      private readonly Dictionary<Key, Subroutine> cache = new Dictionary<Key, Subroutine>();
      protected readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodCache;

      protected IDecodeContracts<Local, Parameter, Method, Field, Type> ContractDecoder
      {
        get
        {
          return this.MethodCache.ContractDecoder;
        }
      }

      protected IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
      {
        get
        {
          return this.MethodCache.MetadataDecoder;
        }
      }

      public SubroutineFactory(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache)
      {
        this.MethodCache = methodCache;
      }

      private void ObjectInvariant()
      {
      }

      public Subroutine Get(Key key)
      {
        if (this.cache.ContainsKey(key))
          return this.cache[key];
        Subroutine subroutine = this.BuildNewSubroutine(key);
        this.cache.Add(key, subroutine);
        if (subroutine != null)
          subroutine.Initialize();
        return subroutine;
      }

      public void Install(Key key, Subroutine sr)
      {
        this.cache.ContainsKey(key);
        this.cache[key] = sr;
      }

      public bool Remove(Key key)
      {
        if (!this.cache.ContainsKey(key))
          return false;
        this.cache.Remove(key);
        return true;
      }

      protected abstract Subroutine BuildNewSubroutine(Key key);

      protected abstract Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Data data);

      public Subroutine Accept<Label>(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider, Label entryPoint, Data data)
      {
        return this.Factory<Label>(new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label>(codeProvider, this.MethodCache, entryPoint), entryPoint, data);
      }
    }

    private class EnsuresCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Method, Pair<Method, IFunctionalSet<Subroutine>>>
    {
      private Method lastMethodWeAddedInferredEnsures;
      private Set<string> lastMethodInferredEnsures;

      public EnsuresCache(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache)
        : base(methodCache)
      {
      }

      public bool AlreadyInferred(Method method, string postCondition)
      {
        if (!this.MetadataDecoder.Equal(method, this.lastMethodWeAddedInferredEnsures))
        {
          this.lastMethodWeAddedInferredEnsures = method;
          this.lastMethodInferredEnsures = new Set<string>();
        }
        return !this.lastMethodInferredEnsures.AddQ(postCondition);
      }

      protected override Subroutine BuildNewSubroutine(Method method)
      {
        if (this.ContractDecoder != null)
        {
          IFunctionalSet<Subroutine> inheritedEnsures = this.GetInheritedEnsures(method);
          if (this.ContractDecoder.HasEnsures(method))
            return this.ContractDecoder.AccessEnsures<Pair<Method, IFunctionalSet<Subroutine>>, Subroutine>(method, (ICodeConsumer<Local, Parameter, Method, Field, Type, Pair<Method, IFunctionalSet<Subroutine>>, Subroutine>) this, new Pair<Method, IFunctionalSet<Subroutine>>(method, inheritedEnsures));
          if (inheritedEnsures.Count > 0)
          {
            if (inheritedEnsures.Count > 1)
              return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Unit>(this.MethodCache, method, inheritedEnsures);
            else
              return inheritedEnsures.Any;
          }
        }
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Unit>(this.MethodCache, method, (IFunctionalSet<Subroutine>) null);
      }

      protected override Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Pair<Method, IFunctionalSet<Subroutine>> data)
      {
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>(this.MethodCache, data.One, sb, entry, data.Two);
      }

      private IFunctionalSet<Subroutine> GetInheritedEnsures(Method method)
      {
        IFunctionalSet<Subroutine> functionalSet = FunctionalSet<Subroutine>.Empty(new Converter<Subroutine, int>(Subroutine.GetKey));
        if (this.MetadataDecoder.IsVirtual(method) && this.ContractDecoder.CanInheritContracts(method))
        {
          foreach (Method method1 in this.MetadataDecoder.OverriddenAndImplementedMethods(method))
          {
            Subroutine elem = this.Get(this.MetadataDecoder.Unspecialized(method1));
            if (elem != null)
              functionalSet = functionalSet.Add(elem);
          }
        }
        return functionalSet;
      }
    }

    private class ModelEnsuresCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Method, Pair<Method, IFunctionalSet<Subroutine>>>
    {
      public ModelEnsuresCache(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache)
        : base(methodCache)
      {
      }

      protected override Subroutine BuildNewSubroutine(Method method)
      {
        if (this.ContractDecoder != null)
        {
          IFunctionalSet<Subroutine> inheritedEnsures = this.GetInheritedEnsures(method);
          if (this.ContractDecoder.HasModelEnsures(method))
            return this.ContractDecoder.AccessModelEnsures<Pair<Method, IFunctionalSet<Subroutine>>, Subroutine>(method, (ICodeConsumer<Local, Parameter, Method, Field, Type, Pair<Method, IFunctionalSet<Subroutine>>, Subroutine>) this, new Pair<Method, IFunctionalSet<Subroutine>>(method, inheritedEnsures));
          if (inheritedEnsures.Count > 0)
          {
            if (inheritedEnsures.Count > 1)
              return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.ModelEnsuresSubroutine<Unit>(this.MethodCache, method, inheritedEnsures);
            else
              return inheritedEnsures.Any;
          }
        }
        return (Subroutine) null;
      }

      protected override Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Pair<Method, IFunctionalSet<Subroutine>> data)
      {
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.ModelEnsuresSubroutine<Label>(this.MethodCache, data.One, sb, entry, data.Two);
      }

      private IFunctionalSet<Subroutine> GetInheritedEnsures(Method method)
      {
        IFunctionalSet<Subroutine> functionalSet = FunctionalSet<Subroutine>.Empty(new Converter<Subroutine, int>(Subroutine.GetKey));
        if (this.MetadataDecoder.IsVirtual(method) && this.ContractDecoder.CanInheritContracts(method))
        {
          foreach (Method method1 in this.MetadataDecoder.OverriddenAndImplementedMethods(method))
          {
            Subroutine elem = this.Get(this.MetadataDecoder.Unspecialized(method1));
            if (elem != null)
              functionalSet = functionalSet.Add(elem);
          }
        }
        return functionalSet;
      }
    }

    private class RequiresCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Method, Pair<Method, IFunctionalSet<Subroutine>>>
    {
      private ErrorHandler output;

      public RequiresCache(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, ErrorHandler output)
        : base(methodCache)
      {
        this.output = output;
      }

      protected override Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Pair<Method, IFunctionalSet<Subroutine>> data)
      {
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label>(this.MethodCache, data.One, sb, entry, data.Two);
      }

      protected override Subroutine BuildNewSubroutine(Method method)
      {
        if (this.ContractDecoder != null)
        {
          IFunctionalSet<Subroutine> inheritedRequires = this.GetInheritedRequires(method);
          if (this.ContractDecoder.HasRequires(method))
            return this.ContractDecoder.AccessRequires<Pair<Method, IFunctionalSet<Subroutine>>, Subroutine>(method, (ICodeConsumer<Local, Parameter, Method, Field, Type, Pair<Method, IFunctionalSet<Subroutine>>, Subroutine>) this, new Pair<Method, IFunctionalSet<Subroutine>>(method, inheritedRequires));
          if (inheritedRequires.Count > 0)
          {
            if (inheritedRequires.Count == 1)
              return inheritedRequires.Any;
            else
              return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Unit>(this.MethodCache, method, inheritedRequires);
          }
        }
        return (Subroutine) null;
      }

      private IFunctionalSet<Subroutine> GetInheritedRequires(Method method)
      {
        IFunctionalSet<Subroutine> functionalSet = FunctionalSet<Subroutine>.Empty(new Converter<Subroutine, int>(Subroutine.GetKey));
        if (this.MetadataDecoder.IsVirtual(method) && this.ContractDecoder.CanInheritContracts(method))
        {
          Method rootMethod;
          if (this.MetadataDecoder.TryGetRootMethod(method, out rootMethod))
          {
            Subroutine elem = this.Get(this.MetadataDecoder.Unspecialized(rootMethod));
            if (elem != null)
              functionalSet = functionalSet.Add(elem);
          }
          foreach (Method method1 in this.MetadataDecoder.ImplementedMethods(method))
          {
            this.MetadataDecoder.Unspecialized(method1);
            Subroutine elem = this.Get(this.MetadataDecoder.Unspecialized(method1));
            if (elem != null)
              functionalSet = functionalSet.Add(elem);
          }
        }
        return functionalSet;
      }
    }

    private class InvariantCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Type, Pair<Type, Subroutine>>
    {
      public InvariantCache(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache)
        : base(methodCache)
      {
      }

      protected override Subroutine BuildNewSubroutine(Type type)
      {
        if (this.ContractDecoder == null)
          return (Subroutine) null;
        Subroutine inheritedInvariant = this.GetInheritedInvariant(type);
        if (this.ContractDecoder.HasInvariant(type))
          return this.ContractDecoder.AccessInvariant<Pair<Type, Subroutine>, Subroutine>(type, (ICodeConsumer<Local, Parameter, Method, Field, Type, Pair<Type, Subroutine>, Subroutine>) this, new Pair<Type, Subroutine>(type, inheritedInvariant));
        else
          return inheritedInvariant;
      }

      protected override Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Pair<Type, Subroutine> data)
      {
        Subroutine baseInv = data.Two;
        Type associatedType = data.One;
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.InvariantSubroutine<Label>(this.MethodCache, sb, entry, baseInv, associatedType);
      }

      private Subroutine GetInheritedInvariant(Type type)
      {
        if (this.MetadataDecoder.HasBaseClass(type) && this.ContractDecoder.CanInheritContracts(type))
          return this.MethodCache.GetInvariant(this.MetadataDecoder.BaseClass(this.MetadataDecoder.Unspecialized(type)));
        else
          return (Subroutine) null;
      }
    }

    internal abstract class SubroutineBuilder<Label>
    {
      private readonly Set<Label> labelsStartingBlocks = new Set<Label>();
      private readonly Set<Label> targetLabels = new Set<Label>();
      internal readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodCache;
      private OnDemandMap<Label, Pair<Method, bool>> labelsForCallSites;
      private OnDemandMap<Label, Method> labelsForNewObjSites;
      internal readonly ICodeProvider<Label, Local, Parameter, Method, Field, Type> CodeProvider;

      internal IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> MetadataDecoder
      {
        get
        {
          return this.MethodCache.MetadataDecoder;
        }
      }

      internal IDecodeContracts<Local, Parameter, Method, Field, Type> ContractDecoder
      {
        get
        {
          return this.MethodCache.ContractDecoder;
        }
      }

      protected abstract MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> CurrentSubroutine { get; }

      protected SubroutineBuilder(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Label entry)
      {
        this.CodeProvider = codeProvider;
        this.MethodCache = methodCache;
        this.AddTargetLabel(entry);
      }

      private void ObjectInvariant()
      {
      }

      protected void AddTargetLabel(Label target)
      {
        this.AddBlockStart(target);
        this.targetLabels.Add(target);
      }

      protected void AddBlockStart(Label target)
      {
        this.labelsStartingBlocks.Add(target);
      }

      protected void Initialize(Label entry)
      {
        new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>.BlockStartGatherer(this).TraceAggregateSequentially(entry);
      }

      public bool IsBlockStart(Label label)
      {
        return this.labelsStartingBlocks.Contains(label);
      }

      internal virtual void RecordBlockInfoSameAsOtherBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> ab, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> otherblock)
      {
      }

      internal bool IsMethodCallSite(Label label, out Pair<Method, bool> methodVirtPair)
      {
        return this.labelsForCallSites.TryGetValue(label, out methodVirtPair);
      }

      internal bool IsNewObjSite(Label label, out Method constructor)
      {
        return this.labelsForNewObjSites.TryGetValue(label, out constructor);
      }

      protected MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> BuildBlocks(Label start)
      {
        return MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>.BlockBuilder.BuildBlocks(start, this);
      }

      protected virtual MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> RecordInformationForNewBlock(Label currentLabel, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> previousBlock)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> block = this.CurrentSubroutine.GetBlock(currentLabel);
        if (previousBlock != null)
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> newBlock = block;
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> previousBlock1 = previousBlock;
          if (block is MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label> && previousBlock is MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>)
          {
            MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> ab = this.CurrentSubroutine.NewBlock();
            this.RecordBlockInfoSameAsOtherBlock(ab, previousBlock);
            newBlock = ab;
            previousBlock1 = ab;
            this.CurrentSubroutine.AddSuccessor((CFGBlock) previousBlock, "fallthrough", (CFGBlock) ab);
            this.CurrentSubroutine.AddSuccessor((CFGBlock) ab, "fallthrough", (CFGBlock) block);
          }
          else
            this.CurrentSubroutine.AddSuccessor((CFGBlock) previousBlock, "fallthrough", (CFGBlock) block);
          this.InsertPostConditionEdges(previousBlock, newBlock);
          this.InsertPreConditionEdges(previousBlock1, block);
        }
        return block;
      }

      private void InsertPreConditionEdges(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> previousBlock, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> newBlock)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label> methodCallBlock = newBlock as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>;
        if (methodCallBlock == null || this.CurrentSubroutine.IsContract || this.CurrentSubroutine.IsOldValue)
          return;
        if (this.CurrentSubroutine.IsMethod)
        {
          IMethodInfo<Method> methodInfo = this.CurrentSubroutine as IMethodInfo<Method>;
          if (methodInfo != null && this.MetadataDecoder.IsConstructor(methodInfo.Method) && (this.MetadataDecoder.IsPropertySetter(methodCallBlock.CalledMethod) && this.MetadataDecoder.IsAutoPropertyMember(methodCallBlock.CalledMethod)))
            return;
        }
        string callTag = methodCallBlock.IsNewObj ? "beforeNewObj" : "beforeCall";
        this.CurrentSubroutine.AddEdgeSubroutine((CFGBlock) previousBlock, (CFGBlock) newBlock, this.MethodCache.GetRequires(methodCallBlock.CalledMethod), callTag);
      }

      private void InsertPostConditionEdges(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> previousBlock, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> newBlock)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label> methodCallBlock = previousBlock as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>;
        if (methodCallBlock == null)
          return;
        if (this.CurrentSubroutine.IsMethod)
        {
          IMethodInfo<Method> methodInfo = this.CurrentSubroutine as IMethodInfo<Method>;
          if (methodInfo != null && this.MetadataDecoder.IsConstructor(methodInfo.Method) && (this.MetadataDecoder.IsPropertyGetter(methodCallBlock.CalledMethod) && this.MetadataDecoder.IsAutoPropertyMember(methodCallBlock.CalledMethod)))
            return;
        }
        string callTag = methodCallBlock.IsNewObj ? "afterNewObj" : "afterCall";
        Subroutine ensures = this.MethodCache.GetEnsures(methodCallBlock.CalledMethod);
        this.CurrentSubroutine.AddEdgeSubroutine((CFGBlock) previousBlock, (CFGBlock) newBlock, ensures, callTag);
        Subroutine modelEnsures = this.MethodCache.GetModelEnsures(methodCallBlock.CalledMethod);
        this.CurrentSubroutine.AddEdgeSubroutine((CFGBlock) previousBlock, (CFGBlock) newBlock, modelEnsures, callTag);
      }

      internal virtual void BeginOldHook(Label current)
      {
      }

      internal virtual void EndOldHook(Label current)
      {
      }

      internal bool IsTargetLabel(Label label)
      {
        return this.targetLabels.Contains(label);
      }

      private class BlockStartGatherer : MSILVisitor<Label, Local, Parameter, Method, Field, Type, Unit, Unit, Unit, bool>, ICodeQuery<Label, Local, Parameter, Method, Field, Type, Unit, bool>, IVisitMSIL<Label, Local, Parameter, Method, Field, Type, Unit, Unit, Unit, bool>, IVisitSynthIL<Label, Method, Type, Unit, Unit, Unit, bool>, IVisitExprIL<Label, Type, Unit, Unit, Unit, bool>
      {
        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> parent;

        public BlockStartGatherer(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> parent)
        {
          this.parent = parent;
        }

        private void ObjectInvariant()
        {
        }

        public bool TraceAggregateSequentially(Label current)
        {
          bool flag1;
          bool flag2;
          do
          {
            flag1 = this.parent.CodeProvider.Decode<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>.BlockStartGatherer, Unit, bool>(current, this, Unit.Value);
            flag2 = this.parent.CodeProvider.Next(current, out current);
            if (flag2 && flag1)
              this.AddBlockStart(current);
          }
          while (flag2);
          return flag1;
        }

        private void AddBlockStart(Label target)
        {
          this.parent.AddBlockStart(target);
        }

        private void AddTargetLabel(Label target)
        {
          this.parent.AddTargetLabel(target);
        }

        protected override bool Default(Label pc, Unit data)
        {
          return false;
        }

        public override bool Branch(Label pc, Label target, bool leave, Unit data)
        {
          this.AddTargetLabel(target);
          return true;
        }

        public override bool BranchCond(Label pc, Label target, BranchOperator bop, Unit value1, Unit value2, Unit data)
        {
          this.AddTargetLabel(target);
          return true;
        }

        public override bool BranchFalse(Label pc, Label target, Unit cond, Unit data)
        {
          this.AddTargetLabel(target);
          return true;
        }

        public override bool BranchTrue(Label pc, Label target, Unit cond, Unit data)
        {
          this.AddTargetLabel(target);
          return true;
        }

        public override bool Switch(Label pc, Type type, IEnumerable<Pair<object, Label>> cases, Unit value, Unit data)
        {
          foreach (Pair<object, Label> pair in cases)
            this.AddTargetLabel(pair.Two);
          return true;
        }

        public override bool Throw(Label pc, Unit exn, Unit data)
        {
          return true;
        }

        public override bool Rethrow(Label pc, Unit data)
        {
          return true;
        }

        public override bool Endfinally(Label pc, Unit data)
        {
          return true;
        }

        public override bool Return(Label pc, Unit source, Unit data)
        {
          return true;
        }

        public bool Aggregate(Label current, Label aggregateStart, bool canBeBranchTarget, Unit data)
        {
          return this.TraceAggregateSequentially(aggregateStart);
        }

        public override bool Call<TypeList, ArgList>(Label pc, Method method, bool tail, bool virt, TypeList extraVarargs, Unit dest, ArgList args, Unit data)
        {
          return this.CallHelper(pc, method, false, virt);
        }

        public override bool ConstrainedCallvirt<TypeList, ArgList>(Label pc, Method method, bool tail, Type constraint, TypeList extraVarargs, Unit dest, ArgList args, Unit data)
        {
          return this.CallHelper(pc, method, false, true);
        }

        private bool CallHelper(Label current, Method method, bool newObj, bool isVirtual)
        {
          this.AddBlockStart(current);
          if (newObj)
            this.parent.labelsForNewObjSites[current] = method;
          else
            this.parent.labelsForCallSites[current] = new Pair<Method, bool>(method, isVirtual);
          return true;
        }

        public override bool Newobj<ArgList>(Label pc, Method ctor, Unit dest, ArgList args, Unit data)
        {
          return this.CallHelper(pc, ctor, true, false);
        }

        public override bool BeginOld(Label pc, Label matchingEnd, Unit data)
        {
          this.AddTargetLabel(pc);
          this.parent.BeginOldHook(pc);
          return false;
        }

        public override bool EndOld(Label pc, Label matchingBegin, Type type, Unit dest, Unit source, Unit data)
        {
          this.parent.EndOldHook(pc);
          return true;
        }
      }

      private class BlockBuilder : MSILVisitor<Label, Local, Parameter, Method, Field, Type, Unit, Unit, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>, bool>, ICodeQuery<Label, Local, Parameter, Method, Field, Type, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>, bool>, IVisitMSIL<Label, Local, Parameter, Method, Field, Type, Unit, Unit, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>, bool>, IVisitSynthIL<Label, Method, Type, Unit, Unit, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>, bool>, IVisitExprIL<Label, Type, Unit, Unit, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>, bool>
      {
        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> parent;
        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock;

        private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> CurrentSubroutine
        {
          get
          {
            return this.parent.CurrentSubroutine;
          }
        }

        private BlockBuilder(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> builder)
        {
          this.parent = builder;
        }

        private void ObjectInvariant()
        {
        }

        internal static MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> BuildBlocks(Label start, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> builder)
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>.BlockBuilder blockBuilder = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>.BlockBuilder(builder);
          blockBuilder.TraceAggregateSequentially(start);
          if (blockBuilder.currentBlock == null)
            return (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null;
          blockBuilder.CurrentSubroutine.AddSuccessor((CFGBlock) blockBuilder.currentBlock, "fallthrough-return", blockBuilder.CurrentSubroutine.Exit);
          blockBuilder.CurrentSubroutine.AddReturnBlock(blockBuilder.currentBlock);
          return blockBuilder.currentBlock;
        }

        private void TraceAggregateSequentially(Label currentLabel)
        {
          do
          {
            if (this.parent.IsBlockStart(currentLabel))
              this.currentBlock = this.parent.RecordInformationForNewBlock(currentLabel, this.currentBlock);
            if (this.parent.CodeProvider.Decode<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>.BlockBuilder, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>, bool>(currentLabel, this, this.currentBlock))
              this.currentBlock = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null;
          }
          while (this.parent.CodeProvider.Next(currentLabel, out currentLabel));
        }

        protected override bool Default(Label pc, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          return false;
        }

        public override bool Branch(Label pc, Label target, bool leave, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, "branch", (CFGBlock) this.CurrentSubroutine.GetTargetBlock(target));
          return true;
        }

        public override bool BranchCond(Label pc, Label target, BranchOperator bop, Unit value1, Unit value2, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          return this.HandleCondBranch(pc, target, true, currentBlock);
        }

        public override bool BranchFalse(Label pc, Label target, Unit cond, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> data)
        {
          return this.HandleCondBranch(pc, target, false, data);
        }

        public override bool BranchTrue(Label pc, Label target, Unit cond, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> data)
        {
          return this.HandleCondBranch(pc, target, true, data);
        }

        private bool HandleCondBranch(Label pc, Label target, bool trueBranch, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          string tag1 = trueBranch ? "true" : "false";
          string tag2 = trueBranch ? "false" : "true";
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.AssumeBlock<Label> assumeBlock1 = this.CurrentSubroutine.NewAssumeBlock(pc, tag1);
          this.parent.RecordBlockInfoSameAsOtherBlock((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) assumeBlock1, this.currentBlock);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, tag1, (CFGBlock) assumeBlock1);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) assumeBlock1, "fallthrough", (CFGBlock) this.CurrentSubroutine.GetTargetBlock(target));
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.AssumeBlock<Label> assumeBlock2 = this.CurrentSubroutine.NewAssumeBlock(pc, tag2);
          this.parent.RecordBlockInfoSameAsOtherBlock((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) assumeBlock2, this.currentBlock);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, tag2, (CFGBlock) assumeBlock2);
          this.currentBlock = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) assumeBlock2;
          return false;
        }

        public override bool Switch(Label pc, Type type, IEnumerable<Pair<object, Label>> cases, Unit value, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          List<object> patterns = new List<object>();
          currentBlock.Add(pc);
          foreach (Pair<object, Label> pair in cases)
          {
            patterns.Add(pair.One);
            MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SwitchCaseAssumeBlock<Label> switchCaseAssumeBlock = this.CurrentSubroutine.NewSwitchCaseAssumeBlock(pc, pair.One, type);
            this.parent.RecordBlockInfoSameAsOtherBlock((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) switchCaseAssumeBlock, this.currentBlock);
            this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, "switch", (CFGBlock) switchCaseAssumeBlock);
            this.CurrentSubroutine.AddSuccessor((CFGBlock) switchCaseAssumeBlock, "fallthrough", (CFGBlock) this.CurrentSubroutine.GetTargetBlock(pair.Two));
          }
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SwitchDefaultAssumeBlock<Label> defaultAssumeBlock = this.CurrentSubroutine.NewSwitchDefaultAssumeBlock(pc, patterns, type);
          this.parent.RecordBlockInfoSameAsOtherBlock((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) defaultAssumeBlock, this.currentBlock);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, "default", (CFGBlock) defaultAssumeBlock);
          this.currentBlock = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) defaultAssumeBlock;
          return false;
        }

        public override bool Throw(Label pc, Unit exn, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          return true;
        }

        public override bool Rethrow(Label pc, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          return true;
        }

        public override bool Endfinally(Label pc, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, "endsub", this.CurrentSubroutine.Exit);
          return true;
        }

        public override bool Return(Label pc, Unit source, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          currentBlock.Add(pc);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) currentBlock, "return", this.CurrentSubroutine.Exit);
          this.CurrentSubroutine.AddReturnBlock(currentBlock);
          return true;
        }

        public bool Aggregate(Label pc, Label nestedAggregate, bool canBeBranchTarget, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          this.TraceAggregateSequentially(nestedAggregate);
          return false;
        }

        public override bool Nop(Label pc, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> data)
        {
          return false;
        }

        public override bool EndOld(Label pc, Label matchingBegin, Type type, Unit dest, Unit source, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> data)
        {
          this.currentBlock.Add(pc);
          this.CurrentSubroutine.AddSuccessor((CFGBlock) this.currentBlock, "endold", this.CurrentSubroutine.Exit);
          return false;
        }

        public override bool Stfld(Label pc, Field field, bool @volatile, Unit obj, Unit value, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          if (this.CurrentSubroutine.IsMethod)
          {
            IMethodInfo<Method> methodInfo = (IMethodInfo<Method>) this.CurrentSubroutine;
            if (this.parent.MetadataDecoder.IsPropertySetter(methodInfo.Method))
              this.parent.MethodCache.AddModifies(methodInfo.Method, field);
          }
          this.currentBlock.Add(pc);
          return false;
        }

        public override bool Ldfld(Label pc, Field field, bool @volatile, Unit obj, Unit value, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> currentBlock)
        {
          if (this.CurrentSubroutine.IsMethod)
          {
            IMethodInfo<Method> methodInfo = (IMethodInfo<Method>) this.CurrentSubroutine;
            if (this.parent.MetadataDecoder.IsPropertyGetter(methodInfo.Method))
              this.parent.MethodCache.AddReads(methodInfo.Method, field);
          }
          this.currentBlock.Add(pc);
          return false;
        }
      }
    }

    internal class SubroutineWithHandlersBuilder<Label, Handler> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>
    {
      private FList<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>> subroutineStack;
      private OnDemandMap<Label, Stack<Handler>> tryStartList;
      private OnDemandMap<Label, Queue<Handler>> tryEndList;
      private OnDemandMap<Label, Queue<Handler>> subroutineHandlerEndList;
      private OnDemandMap<Label, Handler> handlerStartingAt;
      protected readonly Method method;
      internal readonly IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> CodeProvider;

      protected override MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> CurrentSubroutine
      {
        get
        {
          return (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>) this.subroutineStack.Head;
        }
      }

      protected MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler> CurrentSubroutineWithHandler
      {
        get
        {
          return this.subroutineStack.Head;
        }
      }

      private FList<Handler> CurrentProtectingHandlers
      {
        get
        {
          return this.CurrentSubroutineWithHandler.CurrentProtectingHandlers;
        }
        set
        {
          this.CurrentSubroutineWithHandler.CurrentProtectingHandlers = value;
        }
      }

      public SubroutineWithHandlersBuilder(IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> codeProvider, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, Label entry)
        : base((ICodeProvider<Label, Local, Parameter, Method, Field, Type>) codeProvider, methodCache, entry)
      {
        this.CodeProvider = codeProvider;
        this.method = method;
        this.ComputeTryBlockStartAndEndInfo(method);
        this.Initialize(entry);
      }

      private void ObjectInvariant()
      {
      }

      public CFGBlock BuildBlocks(Label entry, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler> subroutine)
      {
        this.subroutineStack = FList<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>>.Cons(subroutine, (FList<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>>) null);
        return (CFGBlock) base.BuildBlocks(entry);
      }

      internal override void RecordBlockInfoSameAsOtherBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> ab, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> otherblock)
      {
        FList<Handler> flist;
        if (!this.CurrentSubroutineWithHandler.ProtectingHandlers.TryGetValue((CFGBlock) otherblock, out flist))
          return;
        this.CurrentSubroutineWithHandler.ProtectingHandlers.Add((CFGBlock) ab, flist);
      }

      protected override MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> RecordInformationForNewBlock(Label currentLabel, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> previousBlock)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null;
        Queue<Handler> handlerEnd = this.GetHandlerEnd(currentLabel);
        if (handlerEnd != null)
        {
          foreach (Handler handler in handlerEnd)
          {
            this.subroutineStack.Head.Commit();
            this.subroutineStack = this.subroutineStack.Tail;
            previousBlock = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null;
          }
        }
        Queue<Handler> tryEnd = this.GetTryEnd(currentLabel);
        if (tryEnd != null)
        {
          foreach (Handler handler in tryEnd)
          {
            if (!object.Equals((object) handler, (object) this.CurrentProtectingHandlers.Head))
              throw new ApplicationException("wrong handler");
            this.CurrentProtectingHandlers = this.CurrentProtectingHandlers.Tail;
          }
        }
        Handler handler1;
        if (this.IsHandlerStart(currentLabel, out handler1))
        {
          if (this.IsFaultOrFinally(handler1))
          {
            MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler> elem = !this.CodeProvider.IsFaultHandler(handler1) ? (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.FinallySubroutine<Label, Handler>(this.MethodCache, this, currentLabel) : (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.FaultSubroutine<Label, Handler>(this.MethodCache, this, currentLabel);
            this.CurrentSubroutineWithHandler.FaultFinallySubroutines.Add(handler1, (Subroutine) elem);
            this.subroutineStack = FList<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>>.Cons(elem, this.subroutineStack);
            previousBlock = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null;
          }
          else
            blockWithLabels = this.CurrentSubroutineWithHandler.CreateCatchFilterHeader(handler1, currentLabel);
        }
        if (blockWithLabels == null)
          blockWithLabels = base.RecordInformationForNewBlock(currentLabel, previousBlock);
        Stack<Handler> tryStart = this.GetTryStart(currentLabel);
        if (tryStart != null)
        {
          foreach (Handler elem in tryStart)
            this.CurrentProtectingHandlers = FList<Handler>.Cons(elem, this.CurrentProtectingHandlers);
        }
        this.CurrentSubroutineWithHandler.ProtectingHandlers.Add((CFGBlock) blockWithLabels, this.CurrentProtectingHandlers);
        return blockWithLabels;
      }

      private void ComputeTryBlockStartAndEndInfo(Method method)
      {
        foreach (Handler handler in this.CodeProvider.TryBlocks(method))
        {
          if (this.CodeProvider.IsFilterHandler(handler))
            this.AddTargetLabel(this.CodeProvider.FilterDecisionStart(handler));
          this.AddTargetLabel(this.CodeProvider.HandlerStart(handler));
          this.AddTargetLabel(this.CodeProvider.HandlerEnd(handler));
          this.AddTryStart(handler);
          this.AddTryEnd(handler);
          this.AddHandlerEnd(handler);
          this.handlerStartingAt.Add(this.CodeProvider.HandlerStart(handler), handler);
        }
      }

      private void AddTryStart(Handler eh)
      {
        Label index = this.CodeProvider.TryStart(eh);
        Stack<Handler> stack;
        this.tryStartList.TryGetValue(index, out stack);
        if (stack == null)
        {
          stack = new Stack<Handler>();
          this.tryStartList[index] = stack;
        }
        stack.Push(eh);
        this.AddTargetLabel(index);
      }

      private void AddTryEnd(Handler eh)
      {
        Label index = this.CodeProvider.TryEnd(eh);
        Queue<Handler> queue;
        this.tryEndList.TryGetValue(index, out queue);
        if (queue == null)
        {
          queue = new Queue<Handler>();
          this.tryEndList[index] = queue;
        }
        queue.Enqueue(eh);
        this.AddTargetLabel(index);
      }

      private void AddHandlerEnd(Handler eh)
      {
        if (!this.IsFaultOrFinally(eh))
          return;
        Label index = this.CodeProvider.HandlerEnd(eh);
        Queue<Handler> queue;
        this.subroutineHandlerEndList.TryGetValue(index, out queue);
        if (queue == null)
        {
          queue = new Queue<Handler>();
          this.subroutineHandlerEndList[index] = queue;
        }
        queue.Enqueue(eh);
        this.AddTargetLabel(index);
      }

      public bool IsHandlerStart(Label label, out Handler handler)
      {
        return this.handlerStartingAt.TryGetValue(label, out handler);
      }

      public bool IsFaultOrFinally(Handler handler)
      {
        if (!this.CodeProvider.IsFaultHandler(handler))
          return this.CodeProvider.IsFinallyHandler(handler);
        else
          return true;
      }

      public Queue<Handler> GetHandlerEnd(Label label)
      {
        Queue<Handler> queue;
        this.subroutineHandlerEndList.TryGetValue(label, out queue);
        return queue;
      }

      public Queue<Handler> GetTryEnd(Label label)
      {
        Queue<Handler> queue;
        this.tryEndList.TryGetValue(label, out queue);
        return queue;
      }

      public Stack<Handler> GetTryStart(Label label)
      {
        Stack<Handler> stack;
        this.tryStartList.TryGetValue(label, out stack);
        return stack;
      }
    }

    internal class SimpleSubroutineBuilder<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>
    {
      private IMutableSet<Label> beginOldStart = (IMutableSet<Label>) new Set<Label>();
      private IMutableSet<Label> endOldStart = (IMutableSet<Label>) new Set<Label>();
      private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> currentSubroutine;
      protected MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.OldValueSubroutine<Label> currentOldSubroutine;
      protected MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockPriorToOld;

      protected override MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> CurrentSubroutine
      {
        get
        {
          if (this.currentOldSubroutine != null)
            return (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>) this.currentOldSubroutine;
          else
            return this.currentSubroutine;
        }
      }

      public SimpleSubroutineBuilder(ICodeProvider<Label, Local, Parameter, Method, Field, Type> codeProvider, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Label entry)
        : base(codeProvider, methodCache, entry)
      {
        this.Initialize(entry);
      }

      public MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> BuildBlocks(Label entry, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> subroutine)
      {
        this.currentSubroutine = subroutine;
        return base.BuildBlocks(entry);
      }

      protected override MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> RecordInformationForNewBlock(Label currentLabel, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> previousBlock)
      {
        Label label;
        if (previousBlock != null && previousBlock.HasLastLabel(out label) && this.endOldStart.Contains(label))
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.OldValueSubroutine<Label> oldValueSubroutine = this.currentOldSubroutine;
          oldValueSubroutine.Commit(previousBlock);
          this.currentOldSubroutine = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.OldValueSubroutine<Label>) null;
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels = base.RecordInformationForNewBlock(currentLabel, this.blockPriorToOld);
          this.CurrentSubroutine.AddEdgeSubroutine((CFGBlock) this.blockPriorToOld, (CFGBlock) blockWithLabels, (Subroutine) oldValueSubroutine, "old");
          return blockWithLabels;
        }
        else
        {
          if (!this.beginOldStart.Contains(currentLabel))
            return base.RecordInformationForNewBlock(currentLabel, previousBlock);
          this.currentOldSubroutine = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.OldValueSubroutine<Label>(this.MethodCache, ((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CallingContractSubroutine<Label>) this.currentSubroutine).Method, this, currentLabel);
          this.blockPriorToOld = previousBlock;
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> newBlock = base.RecordInformationForNewBlock(currentLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null);
          this.currentOldSubroutine.RegisterBeginBlock(newBlock);
          return newBlock;
        }
      }

      internal override void BeginOldHook(Label label)
      {
        this.beginOldStart.Add(label);
      }

      internal override void EndOldHook(Label label)
      {
        this.endOldStart.Add(label);
      }
    }

    internal abstract class BlockBase : CFGBlock
    {
      internal BlockBase(Subroutine container, ref int idGen)
        : base(container, ref idGen)
      {
      }

      internal abstract Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data) where Visitor : IVisitMSIL<APC, Local, Parameter, Method, Field, Type, Unit, Unit, Data, Result>;
    }

    internal class BlockWithLabels<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockBase, IEquatable<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>>
    {
      private static Label[] EmptyLabels = new Label[0];
      private readonly List<Label> Labels;
      internal readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> subroutine;

      public override int Count
      {
        get
        {
          return this.Labels.Count;
        }
      }

      static BlockWithLabels()
      {
      }

      internal BlockWithLabels(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idGen)
        : base((Subroutine) container, ref idGen)
      {
        this.Labels = new List<Label>();
        this.subroutine = container;
      }

      private void ObjectInvariant()
      {
      }

      internal void Add(Label label)
      {
        this.Labels.Add(label);
      }

      internal bool HasLastLabel(out Label label)
      {
        if (this.Labels.Count > 0)
        {
          label = this.Labels[this.Labels.Count - 1];
          return true;
        }
        else
        {
          label = default (Label);
          return false;
        }
      }

      internal virtual bool UnderlyingLabelForward(int index, out Label label)
      {
        if (index < this.Labels.Count)
        {
          label = this.Labels[index];
          return true;
        }
        else
        {
          label = default (Label);
          return false;
        }
      }

      public override string SourceAssertionCondition(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceAssertionCondition(label);
        else
          return (string) null;
      }

      public override string SourceContext(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceContext(label);
        else
          return (string) null;
      }

      public override string SourceDocument(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceDocument(label);
        else
          return (string) null;
      }

      public override int SourceStartLine(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceStartLine(label);
        else
          return 0;
      }

      public override int SourceEndLine(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceEndLine(label);
        else
          return 0;
      }

      public override int SourceStartColumn(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceStartColumn(label);
        else
          return 0;
      }

      public override int SourceEndColumn(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceEndColumn(label);
        else
          return 0;
      }

      public override int SourceStartIndex(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceStartIndex(label);
        else
          return 0;
      }

      public override int SourceLength(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.SourceLength(label);
        else
          return 0;
      }

      public override int ILOffset(APC pc)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.ILOffset(label);
        else
          return 0;
      }

      internal override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return this.subroutine.CodeProvider.Decode<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>.LabelAdapter<Data, Result, Visitor>, Data, Result>(label, new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>.LabelAdapter<Data, Result, Visitor>(visitor, pc), data);
        else
          return visitor.Nop(pc, data);
      }

      public override string ToString()
      {
        return this.Index.ToString();
      }

      public bool Equals(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> other)
      {
        return this == other;
      }

      protected struct LabelAdapter<Data, Result, Visitor> : ICodeQuery<Label, Local, Parameter, Method, Field, Type, Data, Result>, IVisitMSIL<Label, Local, Parameter, Method, Field, Type, Unit, Unit, Data, Result>, IVisitSynthIL<Label, Method, Type, Unit, Unit, Data, Result>, IVisitExprIL<Label, Type, Unit, Unit, Data, Result> where Visitor : IVisitMSIL<APC, Local, Parameter, Method, Field, Type, Unit, Unit, Data, Result>
      {
        private APC originalPC;
        private Visitor visitor;

        public LabelAdapter(Visitor visitor, APC origPC)
        {
          this.visitor = visitor;
          this.originalPC = origPC;
        }

        private APC ConvertLabel(Label pc)
        {
          return this.originalPC;
        }

        private APC ConvertMatchingBeginLabel(Label underlying)
        {
          return ((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.OldValueSubroutine<Label>) this.originalPC.Block.Subroutine).BeginOldAPC(this.originalPC.SubroutineContext);
        }

        private APC ConvertMatchingEndLabel(Label underlying)
        {
          return ((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.OldValueSubroutine<Label>) this.originalPC.Block.Subroutine).EndOldAPC(this.originalPC.SubroutineContext);
        }

        public Result Aggregate(Label pc, Label nested, bool branchTarget, Data data)
        {
          return this.visitor.Nop(this.ConvertLabel(pc), data);
        }

        public Result Assume(Label pc, string tag, Unit source, object provenance, Data data)
        {
          return this.visitor.Assume(this.ConvertLabel(pc), tag, source, provenance, data);
        }

        public Result Assert(Label pc, string tag, Unit source, object provenance, Data data)
        {
          return this.visitor.Assert(this.ConvertLabel(pc), tag, source, provenance, data);
        }

        public Result Arglist(Label pc, Unit dest, Data data)
        {
          return this.visitor.Arglist(this.ConvertLabel(pc), dest, data);
        }

        public Result Binary(Label pc, BinaryOperator op, Unit dest, Unit s1, Unit s2, Data data)
        {
          return this.visitor.Binary(this.ConvertLabel(pc), op, dest, s1, s2, data);
        }

        public Result BranchCond(Label pc, Label target, BranchOperator bop, Unit value1, Unit value2, Data data)
        {
          return this.visitor.BranchCond(this.ConvertLabel(pc), this.ConvertLabel(target), bop, value1, value2, data);
        }

        public Result BranchTrue(Label pc, Label target, Unit cond, Data data)
        {
          return this.visitor.BranchTrue(this.ConvertLabel(pc), this.ConvertLabel(target), cond, data);
        }

        public Result BranchFalse(Label pc, Label target, Unit cond, Data data)
        {
          return this.visitor.BranchFalse(this.ConvertLabel(pc), this.ConvertLabel(target), cond, data);
        }

        public Result Branch(Label pc, Label target, bool leave, Data data)
        {
          return this.visitor.Branch(this.ConvertLabel(pc), this.ConvertLabel(target), leave, data);
        }

        public Result Break(Label pc, Data data)
        {
          return this.visitor.Break(this.ConvertLabel(pc), data);
        }

        public Result Call<TypeList, ArgList>(Label pc, Method method, bool tail, bool virt, TypeList extraVarargs, Unit dest, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Unit>
        {
          return this.visitor.Call<TypeList, ArgList>(this.ConvertLabel(pc), method, tail, virt, extraVarargs, dest, args, data);
        }

        public Result Calli<TypeList, ArgList>(Label pc, Type returnType, TypeList argTypes, bool tail, bool isInstance, Unit dest, Unit fp, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Unit>
        {
          return this.visitor.Calli<TypeList, ArgList>(this.ConvertLabel(pc), returnType, argTypes, tail, isInstance, dest, fp, args, data);
        }

        public Result Ckfinite(Label pc, Unit dest, Unit source, Data data)
        {
          return this.visitor.Ckfinite(this.ConvertLabel(pc), dest, source, data);
        }

        public Result Cpblk(Label pc, bool @volatile, Unit destaddr, Unit srcaddr, Unit len, Data data)
        {
          return this.visitor.Cpblk(this.ConvertLabel(pc), @volatile, destaddr, srcaddr, len, data);
        }

        public Result Endfilter(Label pc, Unit decision, Data data)
        {
          return this.visitor.Endfilter(this.ConvertLabel(pc), decision, data);
        }

        public Result Endfinally(Label pc, Data data)
        {
          return this.visitor.Endfinally(this.ConvertLabel(pc), data);
        }

        public Result Entry(Label pc, Method method, Data data)
        {
          return this.visitor.Entry(this.ConvertLabel(pc), method, data);
        }

        public Result Initblk(Label pc, bool @volatile, Unit destaddr, Unit value, Unit len, Data data)
        {
          return this.visitor.Initblk(this.ConvertLabel(pc), @volatile, destaddr, value, len, data);
        }

        public Result Jmp(Label pc, Method method, Data data)
        {
          return this.visitor.Jmp(this.ConvertLabel(pc), method, data);
        }

        public Result Ldarg(Label pc, Parameter argument, bool isOld, Unit dest, Data data)
        {
          return this.visitor.Ldarg(this.ConvertLabel(pc), argument, isOld, dest, data);
        }

        public Result Ldarga(Label pc, Parameter argument, bool isOld, Unit dest, Data data)
        {
          return this.visitor.Ldarga(this.ConvertLabel(pc), argument, isOld, dest, data);
        }

        public Result Ldconst(Label pc, object constant, Type type, Unit dest, Data data)
        {
          return this.visitor.Ldconst(this.ConvertLabel(pc), constant, type, dest, data);
        }

        public Result Ldnull(Label pc, Unit dest, Data data)
        {
          return this.visitor.Ldnull(this.ConvertLabel(pc), dest, data);
        }

        public Result Ldftn(Label pc, Method method, Unit dest, Data data)
        {
          return this.visitor.Ldftn(this.ConvertLabel(pc), method, dest, data);
        }

        public Result Ldind(Label pc, Type type, bool @volatile, Unit dest, Unit ptr, Data data)
        {
          return this.visitor.Ldind(this.ConvertLabel(pc), type, @volatile, dest, ptr, data);
        }

        public Result Ldloc(Label pc, Local local, Unit dest, Data data)
        {
          return this.visitor.Ldloc(this.ConvertLabel(pc), local, dest, data);
        }

        public Result Ldloca(Label pc, Local local, Unit dest, Data data)
        {
          return this.visitor.Ldloca(this.ConvertLabel(pc), local, dest, data);
        }

        public Result Ldstack(Label pc, int offset, Unit dest, Unit source, bool isOld, Data data)
        {
          return this.visitor.Ldstack(this.ConvertLabel(pc), offset, dest, source, isOld, data);
        }

        public Result Ldstacka(Label pc, int offset, Unit dest, Unit source, Type type, bool isOld, Data data)
        {
          return this.visitor.Ldstacka(this.ConvertLabel(pc), offset, dest, source, type, isOld, data);
        }

        public Result Localloc(Label pc, Unit dest, Unit size, Data data)
        {
          return this.visitor.Localloc(this.ConvertLabel(pc), dest, size, data);
        }

        public Result Nop(Label pc, Data data)
        {
          return this.visitor.Nop(this.ConvertLabel(pc), data);
        }

        public Result Pop(Label pc, Unit source, Data data)
        {
          return this.visitor.Pop(this.ConvertLabel(pc), source, data);
        }

        public Result Return(Label pc, Unit source, Data data)
        {
          return this.visitor.Return(this.ConvertLabel(pc), source, data);
        }

        public Result Starg(Label pc, Parameter argument, Unit source, Data data)
        {
          return this.visitor.Starg(this.ConvertLabel(pc), argument, source, data);
        }

        public Result Stind(Label pc, Type type, bool @volatile, Unit ptr, Unit value, Data data)
        {
          return this.visitor.Stind(this.ConvertLabel(pc), type, @volatile, ptr, value, data);
        }

        public Result Stloc(Label pc, Local local, Unit source, Data data)
        {
          return this.visitor.Stloc(this.ConvertLabel(pc), local, source, data);
        }

        public Result Switch(Label pc, Type type, IEnumerable<Pair<object, Label>> cases, Unit value, Data data)
        {
          return this.visitor.Nop(this.originalPC, data);
        }

        public Result Unary(Label pc, UnaryOperator op, bool overflow, bool unsigned, Unit dest, Unit source, Data data)
        {
          return this.visitor.Unary(this.ConvertLabel(pc), op, overflow, unsigned, dest, source, data);
        }

        public Result Box(Label pc, Type type, Unit dest, Unit source, Data data)
        {
          return this.visitor.Box(this.ConvertLabel(pc), type, dest, source, data);
        }

        public Result ConstrainedCallvirt<TypeList, ArgList>(Label pc, Method method, bool tail, Type constraint, TypeList extraVarargs, Unit dest, ArgList args, Data data) where TypeList : IIndexable<Type> where ArgList : IIndexable<Unit>
        {
          return this.visitor.ConstrainedCallvirt<TypeList, ArgList>(this.ConvertLabel(pc), method, tail, constraint, extraVarargs, dest, args, data);
        }

        public Result Castclass(Label pc, Type type, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Castclass(this.ConvertLabel(pc), type, dest, obj, data);
        }

        public Result Cpobj(Label pc, Type type, Unit destptr, Unit srcptr, Data data)
        {
          return this.visitor.Cpobj(this.ConvertLabel(pc), type, destptr, srcptr, data);
        }

        public Result Initobj(Label pc, Type type, Unit ptr, Data data)
        {
          return this.visitor.Initobj(this.ConvertLabel(pc), type, ptr, data);
        }

        public Result Isinst(Label pc, Type type, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Isinst(this.ConvertLabel(pc), type, dest, obj, data);
        }

        public Result Ldelem(Label pc, Type type, Unit dest, Unit array, Unit index, Data data)
        {
          return this.visitor.Ldelem(this.ConvertLabel(pc), type, dest, array, index, data);
        }

        public Result Ldelema(Label pc, Type type, bool @readonly, Unit dest, Unit array, Unit index, Data data)
        {
          return this.visitor.Ldelema(this.ConvertLabel(pc), type, @readonly, dest, array, index, data);
        }

        public Result Ldfld(Label pc, Field field, bool @volatile, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Ldfld(this.ConvertLabel(pc), field, @volatile, dest, obj, data);
        }

        public Result Ldflda(Label pc, Field field, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Ldflda(this.ConvertLabel(pc), field, dest, obj, data);
        }

        public Result Ldlen(Label pc, Unit dest, Unit array, Data data)
        {
          return this.visitor.Ldlen(this.ConvertLabel(pc), dest, array, data);
        }

        public Result Ldsfld(Label pc, Field field, bool @volatile, Unit dest, Data data)
        {
          return this.visitor.Ldsfld(this.ConvertLabel(pc), field, @volatile, dest, data);
        }

        public Result Ldsflda(Label pc, Field field, Unit dest, Data data)
        {
          return this.visitor.Ldsflda(this.ConvertLabel(pc), field, dest, data);
        }

        public Result Ldtypetoken(Label pc, Type type, Unit dest, Data data)
        {
          return this.visitor.Ldtypetoken(this.ConvertLabel(pc), type, dest, data);
        }

        public Result Ldfieldtoken(Label pc, Field field, Unit dest, Data data)
        {
          return this.visitor.Ldfieldtoken(this.ConvertLabel(pc), field, dest, data);
        }

        public Result Ldmethodtoken(Label pc, Method method, Unit dest, Data data)
        {
          return this.visitor.Ldmethodtoken(this.ConvertLabel(pc), method, dest, data);
        }

        public Result Ldvirtftn(Label pc, Method method, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Ldvirtftn(this.ConvertLabel(pc), method, dest, obj, data);
        }

        public Result Mkrefany(Label pc, Type type, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Mkrefany(this.ConvertLabel(pc), type, dest, obj, data);
        }

        public Result Newarray<ArgList>(Label pc, Type type, Unit dest, ArgList lengths, Data data) where ArgList : IIndexable<Unit>
        {
          return this.visitor.Newarray<ArgList>(this.ConvertLabel(pc), type, dest, lengths, data);
        }

        public Result Newobj<ArgList>(Label pc, Method ctor, Unit dest, ArgList args, Data data) where ArgList : IIndexable<Unit>
        {
          return this.visitor.Newobj<ArgList>(this.ConvertLabel(pc), ctor, dest, args, data);
        }

        public Result Refanytype(Label pc, Unit dest, Unit source, Data data)
        {
          return this.visitor.Refanytype(this.ConvertLabel(pc), dest, source, data);
        }

        public Result Refanyval(Label pc, Type type, Unit dest, Unit source, Data data)
        {
          return this.visitor.Refanyval(this.ConvertLabel(pc), type, dest, source, data);
        }

        public Result Rethrow(Label pc, Data data)
        {
          return this.visitor.Rethrow(this.ConvertLabel(pc), data);
        }

        public Result Sizeof(Label pc, Type type, Unit dest, Data data)
        {
          return this.visitor.Sizeof(this.ConvertLabel(pc), type, dest, data);
        }

        public Result Stelem(Label pc, Type type, Unit array, Unit index, Unit value, Data data)
        {
          return this.visitor.Stelem(this.ConvertLabel(pc), type, array, index, value, data);
        }

        public Result Stfld(Label pc, Field field, bool @volatile, Unit obj, Unit value, Data data)
        {
          return this.visitor.Stfld(this.ConvertLabel(pc), field, @volatile, obj, value, data);
        }

        public Result Stsfld(Label pc, Field field, bool @volatile, Unit value, Data data)
        {
          return this.visitor.Stsfld(this.ConvertLabel(pc), field, @volatile, value, data);
        }

        public Result Throw(Label pc, Unit exn, Data data)
        {
          return this.visitor.Throw(this.ConvertLabel(pc), exn, data);
        }

        public Result Unbox(Label pc, Type type, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Unbox(this.ConvertLabel(pc), type, dest, obj, data);
        }

        public Result Unboxany(Label pc, Type type, Unit dest, Unit obj, Data data)
        {
          return this.visitor.Unboxany(this.ConvertLabel(pc), type, dest, obj, data);
        }

        public Result BeginOld(Label pc, Label matchingEnd, Data data)
        {
          if (this.originalPC.InsideOldManifestation)
            return this.visitor.Nop(this.ConvertLabel(pc), data);
          else
            return this.visitor.BeginOld(this.ConvertLabel(pc), this.ConvertMatchingEndLabel(matchingEnd), data);
        }

        public Result EndOld(Label pc, Label matchingBegin, Type type, Unit dest, Unit source, Data data)
        {
          return this.visitor.EndOld(this.ConvertLabel(pc), this.ConvertMatchingBeginLabel(matchingBegin), type, dest, source, data);
        }

        public Result Ldresult(Label pc, Type type, Unit dest, Unit source, Data data)
        {
          return this.visitor.Ldresult(this.ConvertLabel(pc), type, dest, source, data);
        }
      }
    }

    internal class EnsuresBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      private const int BeginOldMask = -2147483648;
      private const int EndOldMask = 1073741824;
      private const int Mask = -1073741824;
      private List<int> overridingLabels;

      internal bool UsesOverriding
      {
        get
        {
          return this.overridingLabels != null;
        }
      }

      private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> Subroutine
      {
        get
        {
          return (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>) base.Subroutine;
        }
      }

      public override int Count
      {
        get
        {
          if (this.overridingLabels != null)
            return this.overridingLabels.Count;
          else
            return base.Count;
        }
      }

      public EnsuresBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idgen)
        : base(container, ref idgen)
      {
      }

      private bool IsOriginal(int index, out int originalOffset)
      {
        if (this.overridingLabels == null)
        {
          originalOffset = index;
          return true;
        }
        else if (index < this.overridingLabels.Count && (this.overridingLabels[index] & -1073741824) == 0)
        {
          originalOffset = this.overridingLabels[index] & 1073741823;
          return true;
        }
        else
        {
          originalOffset = 0;
          return false;
        }
      }

      private bool IsBeginOld(int index, out int endOldIndex)
      {
        if (this.overridingLabels == null || index >= this.overridingLabels.Count)
        {
          endOldIndex = 0;
          return false;
        }
        else if ((this.overridingLabels[index] & int.MinValue) != 0)
        {
          endOldIndex = this.overridingLabels[index] & 1073741823;
          return true;
        }
        else
        {
          endOldIndex = 0;
          return false;
        }
      }

      private bool IsEndOld(int index, out int beginOldIndex)
      {
        if (this.overridingLabels == null || index >= this.overridingLabels.Count)
        {
          beginOldIndex = 0;
          return false;
        }
        else if ((this.overridingLabels[index] & 1073741824) != 0)
        {
          beginOldIndex = this.overridingLabels[index] & 1073741823;
          return true;
        }
        else
        {
          beginOldIndex = 0;
          return false;
        }
      }

      internal override bool UnderlyingLabelForward(int index, out Label label)
      {
        int originalOffset;
        if (this.IsOriginal(index, out originalOffset))
          return base.UnderlyingLabelForward(originalOffset, out label);
        label = default (Label);
        return false;
      }

      internal Result OriginalForwardDecode<Data, Result, Visitor>(int index, Visitor visitor, Data data) where Visitor : ICodeQuery<Label, Local, Parameter, Method, Field, Type, Data, Result>
      {
        Label label;
        if (base.UnderlyingLabelForward(index, out label))
          return this.subroutine.CodeProvider.Decode<Visitor, Data, Result>(label, visitor, data);
        else
          throw new NotImplementedException();
      }

      internal override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        Label label;
        if (this.UnderlyingLabelForward(pc.Index, out label))
          return base.ForwardDecode<Data, Result, Visitor>(pc, visitor, data);
        int endOldIndex;
        if (this.IsBeginOld(pc.Index, out endOldIndex))
        {
          CFGBlock block = this.Subroutine.InferredBeginEndBijection(pc);
          return visitor.BeginOld(pc, new APC(block, endOldIndex, pc.SubroutineContext), data);
        }
        else
        {
          int beginOldIndex;
          if (!this.IsEndOld(pc.Index, out beginOldIndex))
            return visitor.Nop(pc, data);
          Type endOldType;
          CFGBlock block = this.Subroutine.InferredBeginEndBijection(pc, out endOldType);
          return visitor.EndOld(pc, new APC(block, beginOldIndex, pc.SubroutineContext), endOldType, Unit.Value, Unit.Value, data);
        }
      }

      internal void StartOverridingLabels()
      {
        this.overridingLabels = new List<int>();
      }

      internal void BeginOld(int index)
      {
        if (this.overridingLabels == null)
        {
          this.StartOverridingLabels();
          for (int index1 = 0; index1 < index; ++index1)
            this.overridingLabels.Add(index1);
        }
        this.overridingLabels.Add(int.MinValue);
      }

      internal void AddInstruction(int index)
      {
        this.overridingLabels.Add(index);
      }

      internal void EndOld(int index, Type nextEndOldType)
      {
        this.AddInstruction(index);
        this.EndOldWithoutInstruction(nextEndOldType);
      }

      internal void EndOldWithoutInstruction(Type nextEndOldType)
      {
        int count = this.overridingLabels.Count;
        CFGBlock beginBlock;
        this.overridingLabels.Add(1073741824 | this.PatchPriorBeginOld((CFGBlock) this, count, out beginBlock));
        this.Subroutine.AddInferredOldMap(this.Index, count, beginBlock, nextEndOldType);
      }

      private int PatchPriorBeginOld(CFGBlock endBlock, int endOldIndex, out CFGBlock beginBlock)
      {
        for (int index = this == endBlock ? endOldIndex - 2 : this.Count - 1; index >= 0; --index)
        {
          int endOldIndex1;
          if (this.IsBeginOld(index, out endOldIndex1))
          {
            this.overridingLabels[index] = int.MinValue | endOldIndex;
            beginBlock = (CFGBlock) this;
            this.Subroutine.AddInferredOldMap(this.Index, index, endBlock, default (Type));
            return index;
          }
        }
        IEnumerator<CFGBlock> enumerator = this.subroutine.PredecessorBlocks((CFGBlock) this).GetEnumerator();
        if (!enumerator.MoveNext())
          throw new InvalidOperationException("missing begin_old");
        int num = MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label>.PatchPriorBeginOld(endBlock, endOldIndex, enumerator.Current, out beginBlock);
        enumerator.MoveNext();
        return num;
      }

      private static int PatchPriorBeginOld(CFGBlock endBlock, int endOldIndex, CFGBlock current, out CFGBlock beginBlock)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label> ensuresBlock = current as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label>;
        if (ensuresBlock != null)
          return ensuresBlock.PatchPriorBeginOld(endBlock, endOldIndex, out beginBlock);
        IEnumerator<CFGBlock> enumerator = current.Subroutine.PredecessorBlocks(current).GetEnumerator();
        if (!enumerator.MoveNext())
          throw new InvalidOperationException("missing begin_old");
        int num = MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label>.PatchPriorBeginOld(endBlock, endOldIndex, enumerator.Current, out beginBlock);
        enumerator.MoveNext();
        return num;
      }
    }

    internal class MethodCallBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      public readonly Method CalledMethod;
      public readonly bool Virtual;
      private readonly int parameterCount;

      internal virtual bool IsNewObj
      {
        get
        {
          return false;
        }
      }

      public MethodCallBlock(Method method, bool virtcall, IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> mdDecoder, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idgen)
        : base(container, ref idgen)
      {
        this.CalledMethod = method;
        this.Virtual = virtcall;
        this.parameterCount = mdDecoder.Parameters(method).Count;
      }

      public override bool IsMethodCallBlock<Method2>(out Method2 calledMethod, out bool isNewObj, out bool isVirtual)
      {
        if ((object) this.CalledMethod is Method2)
        {
          calledMethod = (Method2) (object) this.CalledMethod;
          isNewObj = this.IsNewObj;
          isVirtual = this.Virtual;
          return true;
        }
        else
        {
          calledMethod = default (Method2);
          isNewObj = false;
          isVirtual = false;
          return false;
        }
      }
    }

    internal class NewObjCallBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>
    {
      internal override bool IsNewObj
      {
        get
        {
          return true;
        }
      }

      public NewObjCallBlock(Method method, IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> mdDecoder, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idgen)
        : base(method, false, mdDecoder, container, ref idgen)
      {
      }
    }

    internal class AssumeBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      public readonly string Tag;
      public readonly Label BranchLabel;

      public override int Count
      {
        get
        {
          return 1;
        }
      }

      public AssumeBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, Label label, string tag, ref int idgen)
        : base(container, ref idgen)
      {
        this.Tag = tag;
        this.BranchLabel = label;
      }

      internal override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        if (pc.Index == 0)
          return visitor.Assume(pc, this.Tag, Unit.Value, (object) null, data);
        else
          return visitor.Nop(pc, data);
      }
    }

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

    internal class SwitchDefaultAssumeBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      private readonly Label SwitchLabel;
      private readonly List<object> Patterns;
      private readonly Type Type;

      public override int Count
      {
        get
        {
          return this.Patterns.Count * 4 + 1;
        }
      }

      public SwitchDefaultAssumeBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, Label label, List<object> patterns, Type type, ref int idgen)
        : base(container, ref idgen)
      {
        this.SwitchLabel = label;
        this.Patterns = patterns;
        this.Type = type;
      }

      internal override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        int index = pc.Index / 4;
        if (index < this.Patterns.Count)
        {
          switch (pc.Index % 4)
          {
            case 0:
              return visitor.Ldstack(pc, 0, Unit.Value, Unit.Value, false, data);
            case 1:
              return visitor.Ldconst(pc, this.Patterns[index], this.Type, Unit.Value, data);
            case 2:
              return visitor.Binary(pc, BinaryOperator.Cne_Un, Unit.Value, Unit.Value, Unit.Value, data);
            case 3:
              return visitor.Assume(pc, "true", Unit.Value, (object) null, data);
          }
        }
        else if (index == this.Patterns.Count && pc.Index % 4 == 0)
          return visitor.Pop(pc, Unit.Value, data);
        return visitor.Nop(pc, data);
      }
    }

    internal class EntryExitBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      public override int Count
      {
        get
        {
          return 1;
        }
      }

      public EntryExitBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idGen)
        : base(container, ref idGen)
      {
      }
    }

    internal class EntryBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EntryExitBlock<Label>
    {
      public EntryBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idGen)
        : base(container, ref idGen)
      {
      }

      internal override Result ForwardDecode<Data, Result, Visitor>(APC pc, Visitor visitor, Data data)
      {
        if (pc.Index != 0 || pc.SubroutineContext != null || !this.Subroutine.IsMethod)
          return visitor.Nop(pc, data);
        IMethodInfo<Method> methodInfo = (IMethodInfo<Method>) this.Subroutine;
        return visitor.Entry(pc, methodInfo.Method, data);
      }
    }

    internal class CatchFilterEntryBlock<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>
    {
      public CatchFilterEntryBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label> container, ref int idgen)
        : base(container, ref idgen)
      {
      }
    }

    internal abstract class SubroutineBase<Label> : Subroutine, IGraph<CFGBlock, Unit>, IStackInfo, IEdgeSubroutineAdaptor
    {
      protected Dictionary<Label, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>> BlockStart = new Dictionary<Label, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>>();
      private readonly List<Pair<CFGBlock, Pair<string, CFGBlock>>> successors = new List<Pair<CFGBlock, Pair<string, CFGBlock>>>();
      private const int UnusedBlockIndex = 32757;
      private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> entry;
      private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> exit;
      private readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CatchFilterEntryBlock<Label> exceptionExit;
      protected readonly Label startLabel;
      private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> entryAfterRequires;
      protected int blockIdGenerator;
      protected readonly MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> MethodCache;
      protected MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> Builder;
      internal readonly ICodeProvider<Label, Local, Parameter, Method, Field, Type> CodeProvider;
      protected CFGBlock[] blocks;
      protected OnDemandMap<Pair<CFGBlock, CFGBlock>, FList<Pair<string, Subroutine>>> edgeSubroutines;
      private DepthFirst.Visitor<CFGBlock, Unit> edgeInfo;
      private EdgeMap<string> successorEdges;
      private EdgeMap<string> predecessorEdges;

      public override CFGBlock Entry
      {
        get
        {
          return (CFGBlock) this.entry;
        }
      }

      public override CFGBlock EntryAfterRequires
      {
        get
        {
          if (this.entryAfterRequires != null)
            return (CFGBlock) this.entryAfterRequires;
          else
            return this.Entry;
        }
      }

      public override CFGBlock Exit
      {
        get
        {
          return (CFGBlock) this.exit;
        }
      }

      public override CFGBlock ExceptionExit
      {
        get
        {
          return (CFGBlock) this.exceptionExit;
        }
      }

      public override bool HasReturnValue
      {
        get
        {
          return false;
        }
      }

      public override bool HasContextDependentStackDepth
      {
        get
        {
          return true;
        }
      }

      public override int BlockCount
      {
        get
        {
          return this.blocks.Length;
        }
      }

      public override IEnumerable<CFGBlock> Blocks
      {
        get
        {
          return (IEnumerable<CFGBlock>) this.blocks;
        }
      }

      public override string Name
      {
        get
        {
          return "SR" + this.Id.ToString();
        }
      }

      internal override DepthFirst.Visitor<CFGBlock, Unit> EdgeInfo
      {
        get
        {
          return this.edgeInfo;
        }
      }

      public override int StackDelta
      {
        get
        {
          return 0;
        }
      }

      internal override EdgeMap<string> SuccessorEdges
      {
        get
        {
          return this.successorEdges;
        }
      }

      internal override EdgeMap<string> PredecessorEdges
      {
        get
        {
          if (this.predecessorEdges == null)
            this.predecessorEdges = this.SuccessorEdges.ReversedEdges();
          return this.predecessorEdges;
        }
      }

      IEnumerable<CFGBlock> IGraph<CFGBlock, Unit>.Nodes
      {
        get
        {
          return (IEnumerable<CFGBlock>) this.blocks;
        }
      }

      protected SubroutineBase(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache)
      {
        this.MethodCache = methodCache;
        this.entry = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EntryBlock<Label>(this, ref this.blockIdGenerator);
        this.exit = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EntryExitBlock<Label>(this, ref this.blockIdGenerator);
        this.exceptionExit = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CatchFilterEntryBlock<Label>(this, ref this.blockIdGenerator);
      }

      protected SubroutineBase(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Label startLabel, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label> builder)
        : this(methodCache)
      {
        this.startLabel = startLabel;
        this.Builder = builder;
        this.CodeProvider = builder.CodeProvider;
        this.entryAfterRequires = this.GetTargetBlock(startLabel);
        this.AddSuccessor((CFGBlock) this.entry, "entry", (CFGBlock) this.entryAfterRequires);
      }

      private void ObjectInvariant()
      {
      }

      internal MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.AssumeBlock<Label> NewAssumeBlock(Label current, string tag)
      {
        return new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.AssumeBlock<Label>(this, current, tag, ref this.blockIdGenerator);
      }

      internal MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SwitchCaseAssumeBlock<Label> NewSwitchCaseAssumeBlock(Label current, object pattern, Type type)
      {
        return new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SwitchCaseAssumeBlock<Label>(this, current, pattern, type, ref this.blockIdGenerator);
      }

      internal MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SwitchDefaultAssumeBlock<Label> NewSwitchDefaultAssumeBlock(Label current, List<object> patterns, Type type)
      {
        return new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SwitchDefaultAssumeBlock<Label>(this, current, patterns, type, ref this.blockIdGenerator);
      }

      internal virtual MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> NewBlock()
      {
        return new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>(this, ref this.blockIdGenerator);
      }

      internal abstract override void Initialize();

      internal virtual void Commit()
      {
        this.PostProcessBlocks();
      }

      internal void AddSuccessor(CFGBlock from, string tag, CFGBlock target)
      {
        this.AddNormalControlFlowEdge(this.successors, from, tag, target);
      }

      internal MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> GetTargetBlock(Label label)
      {
        return this.GetBlock(label);
      }

      internal MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> GetBlock(Label label)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels;
        if (!this.BlockStart.TryGetValue(label, out blockWithLabels))
        {
          Pair<Method, bool> methodVirtPair;
          Method constructor;
          blockWithLabels = !this.Builder.IsMethodCallSite(label, out methodVirtPair) ? (!this.Builder.IsNewObjSite(label, out constructor) ? this.NewBlock() : (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.NewObjCallBlock<Label>(constructor, this.MethodCache.MetadataDecoder, this, ref this.blockIdGenerator)) : (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>(methodVirtPair.One, methodVirtPair.Two, this.MethodCache.MetadataDecoder, this, ref this.blockIdGenerator);
          if (this.Builder.IsTargetLabel(label))
            this.BlockStart.Add(label, blockWithLabels);
        }
        return blockWithLabels;
      }

      public override sealed void AddEdgeSubroutine(CFGBlock from, CFGBlock to, Subroutine subroutine, string callTag)
      {
        if (subroutine == null)
          return;
        Pair<CFGBlock, CFGBlock> key = new Pair<CFGBlock, CFGBlock>(from, to);
        FList<Pair<string, Subroutine>> tail;
        this.edgeSubroutines.TryGetValue(key, out tail);
        this.edgeSubroutines[key] = FList<Pair<string, Subroutine>>.Cons(new Pair<string, Subroutine>(callTag, subroutine), tail);
      }

      FList<Pair<string, Subroutine>> IEdgeSubroutineAdaptor.GetOrdinaryEdgeSubroutinesInternal(CFGBlock from, CFGBlock to, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        FList<Pair<string, Subroutine>> list;
        this.edgeSubroutines.TryGetValue(new Pair<CFGBlock, CFGBlock>(from, to), out list);
        if (list != null && context != null)
          list = FList.Filter<Pair<string, Subroutine>>(list, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>.FilterRecursiveContracts(to, context));
        return list;
      }

      public override FList<Pair<string, Subroutine>> GetOrdinaryEdgeSubroutines(CFGBlock from, CFGBlock to, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        APC apc = new APC(to, 0, context);
        CallAdaption.Push<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>>(this);
        try
        {
          FList<Pair<string, Subroutine>> flist = CallAdaption.Dispatch<IEdgeSubroutineAdaptor>((IEdgeSubroutineAdaptor) this).GetOrdinaryEdgeSubroutinesInternal(from, to, context);
          if (apc.InsideContract)
          {
            if (context != null && flist != null)
            {
              IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> decodeMetaData = this.MethodCache.MetadataDecoder;
              Method calledMethod1;
              bool isNewObj1;
              bool isVirtual1;
              if (from.IsMethodCallBlock<Method>(out calledMethod1, out isNewObj1, out isVirtual1) && isVirtual1 && CallAdaption.Dispatch<IStackInfo>((IStackInfo) this).IsCallOnThis(new APC(from, 0, (FList<STuple<CFGBlock, CFGBlock, string>>) null)))
              {
                Type ype = decodeMetaData.DeclaringType(calledMethod1);
                do
                {
                  if (context.Head.Three.StartsWith("inherited") || context.Head.Three.StartsWith("extra") || context.Head.Three.StartsWith("old"))
                  {
                    context = context.Tail;
                  }
                  else
                  {
                    Method calledMethod2;
                    bool isNewObj2;
                    bool isVirtual2;
                    if (context.Head.Three.StartsWith("after") && context.Head.One.IsMethodCallBlock<Method>(out calledMethod2, out isNewObj2, out isVirtual2))
                    {
                      Type sub = decodeMetaData.DeclaringType(calledMethod2);
                      if (decodeMetaData.DerivesFromIgnoringTypeArguments(sub, ype))
                        ype = sub;
                      if (!CallAdaption.Dispatch<IStackInfo>((IStackInfo) this).IsCallOnThis(new APC(context.Head.One, 0, (FList<STuple<CFGBlock, CFGBlock, string>>) null)))
                        break;
                    }
                    else if (context.Head.Three.StartsWith("before") && context.Head.Two.IsMethodCallBlock<Method>(out calledMethod2, out isNewObj2, out isVirtual2))
                    {
                      Type sub = decodeMetaData.DeclaringType(calledMethod2);
                      if (decodeMetaData.DerivesFromIgnoringTypeArguments(sub, ype))
                        ype = sub;
                      if (!CallAdaption.Dispatch<IStackInfo>((IStackInfo) this).IsCallOnThis(new APC(context.Head.Two, 0, (FList<STuple<CFGBlock, CFGBlock, string>>) null)))
                        break;
                    }
                    else if (context.Head.Three == "exit")
                    {
                      IMethodInfo<Method> methodInfo = context.Head.One.Subroutine as IMethodInfo<Method>;
                      if (methodInfo != null)
                      {
                        Type sub = decodeMetaData.DeclaringType(methodInfo.Method);
                        if (decodeMetaData.DerivesFromIgnoringTypeArguments(sub, ype))
                        {
                          ype = sub;
                          break;
                        }
                        else
                          break;
                      }
                      else
                        break;
                    }
                    else
                    {
                      if (!(context.Head.Three == "entry"))
                        return flist;
                      IMethodInfo<Method> methodInfo = context.Head.One.Subroutine as IMethodInfo<Method>;
                      if (methodInfo != null)
                      {
                        Type sub = decodeMetaData.DeclaringType(methodInfo.Method);
                        if (decodeMetaData.DerivesFromIgnoringTypeArguments(sub, ype))
                        {
                          ype = sub;
                          break;
                        }
                        else
                          break;
                      }
                      else
                        break;
                    }
                    context = context.Tail;
                  }
                }
                while (context != null);
                Method implementingMethod;
                if (!decodeMetaData.Equal(ype, decodeMetaData.DeclaringType(calledMethod1)) && decodeMetaData.TryGetImplementingMethod(ype, calledMethod1, out implementingMethod))
                  flist = this.SpecializeEnsures(flist, this.MethodCache.GetEnsures(calledMethod1), this.MethodCache.GetEnsures(implementingMethod));
              }
            }
          }
          else
          {
            IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> mdDecoder = this.MethodCache.MetadataDecoder;
            Method calledMethod;
            bool isNewObj;
            bool isVirtual;
            if (from.IsMethodCallBlock<Method>(out calledMethod, out isNewObj, out isVirtual))
            {
              if (CallAdaption.Dispatch<IStackInfo>((IStackInfo) this).IsCallOnThis(new APC(from, 0, (FList<STuple<CFGBlock, CFGBlock, string>>) null)))
              {
                IMethodInfo<Method> im = from.Subroutine as IMethodInfo<Method>;
                if (im != null)
                {
                  Type bestType = mdDecoder.DeclaringType(im.Method);
                  Method implementingMethod;
                  if (isVirtual && mdDecoder.TryGetImplementingMethod(bestType, calledMethod, out implementingMethod))
                    flist = this.SpecializeEnsures(flist, this.MethodCache.GetEnsures(calledMethod), this.MethodCache.GetEnsures(implementingMethod));
                  flist = this.InsertInvariant(from, flist, mdDecoder, calledMethod, im, ref bestType, context);
                }
              }
              else
              {
                Method method;
                if (this.MethodCache.IsMonitorWaitOrExit(calledMethod) && apc.TryGetContainingMethod<Method>(out method))
                {
                  Subroutine invariant = this.MethodCache.GetInvariant(mdDecoder.DeclaringType(method));
                  if (invariant != null)
                    flist = FList.Cons<Pair<string, Subroutine>>(flist, new Pair<string, Subroutine>("entry", invariant));
                }
              }
            }
          }
          return flist;
        }
        finally
        {
          CallAdaption.Pop<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>>(this);
        }
      }

      private FList<Pair<string, Subroutine>> InsertInvariant(CFGBlock from, FList<Pair<string, Subroutine>> result, IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> mdDecoder, Method calledMethod, IMethodInfo<Method> im, ref Type bestType, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        if (this.MethodCache.MetadataDecoder.IsPropertySetter(calledMethod) && (this.MethodCache.MetadataDecoder.IsAutoPropertyMember(calledMethod) || this.WithinConstructor(from, context)))
          return result;
        if (mdDecoder.IsConstructor(calledMethod))
          bestType = mdDecoder.DeclaringType(calledMethod);
        Subroutine invariant = this.MethodCache.GetInvariant(bestType);
        if (invariant != null)
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label> methodCallBlock = from as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>;
          if (methodCallBlock != null)
          {
            string first = methodCallBlock.IsNewObj ? "afterNewObj" : "afterCall";
            result = FList.Cons<Pair<string, Subroutine>>(result, new Pair<string, Subroutine>(first, invariant));
          }
        }
        return result;
      }

      private bool WithinConstructor(CFGBlock from, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        return new APC(from, 0, context).InsideConstructor;
      }

      private FList<Pair<string, Subroutine>> SpecializeEnsures(FList<Pair<string, Subroutine>> subs, Subroutine toReplace, Subroutine specializedEnsures)
      {
        return FList.Map<Pair<string, Subroutine>, Pair<string, Subroutine>>(subs, (Converter<Pair<string, Subroutine>, Pair<string, Subroutine>>) (pair => new Pair<string, Subroutine>(pair.One, this.SpecializeEnsures(pair.Two, toReplace, specializedEnsures))), (FList<Pair<string, Subroutine>>) null);
      }

      private Subroutine SpecializeEnsures(Subroutine sub, Subroutine toReplace, Subroutine specializedEnsures)
      {
        IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> decodeMetaData = this.MethodCache.MetadataDecoder;
        if (sub == toReplace)
          return specializedEnsures;
        else
          return sub;
      }

      bool IStackInfo.IsCallOnThis(APC pc)
      {
        return false;
      }

      private static Predicate<Pair<string, Subroutine>> FilterRecursiveContracts(CFGBlock from, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        return (Predicate<Pair<string, Subroutine>>) (candidatePair =>
        {
          Subroutine local_0 = candidatePair.Two;
          if (!local_0.IsContract)
            return true;
          if (local_0 == from.Subroutine)
            return false;
          for (FList<STuple<CFGBlock, CFGBlock, string>> local_1 = context; local_1 != null; local_1 = local_1.Tail)
          {
            if (local_0 == local_1.Head.One.Subroutine)
              return false;
          }
          return true;
        });
      }

      internal virtual void AddReturnBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> block)
      {
      }

      public override IEnumerable<CFGBlock> SuccessorBlocks(CFGBlock block)
      {
        foreach (Pair<string, CFGBlock> pair in (IEnumerable<Pair<string, CFGBlock>>) this.SuccessorEdges[block])
          yield return pair.Two;
      }

      public override IEnumerable<CFGBlock> PredecessorBlocks(CFGBlock block)
      {
        foreach (Pair<string, CFGBlock> pair in (IEnumerable<Pair<string, CFGBlock>>) this.PredecessorEdges[block])
          yield return pair.Two;
      }

      public override bool IsCatchFilterHeader(CFGBlock block)
      {
        return block is MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CatchFilterEntryBlock<Label>;
      }

      public override bool IsSubroutineStart(CFGBlock block)
      {
        return block == this.entry;
      }

      public override bool IsSubroutineEnd(CFGBlock block)
      {
        if (block != this.exit)
          return block == this.exceptionExit;
        else
          return true;
      }

      public override bool IsJoinPoint(CFGBlock block)
      {
        if (this.IsCatchFilterHeader(block) || this.IsSubroutineStart(block) || this.IsSubroutineEnd(block))
          return true;
        else
          return this.PredecessorEdges[block].Count > 1;
      }

      public override bool IsSplitPoint(CFGBlock block)
      {
        if (this.IsSubroutineStart(block) || this.IsSubroutineEnd(block))
          return true;
        else
          return this.SuccessorEdges[block].Count > 1;
      }

      internal string SourceContext(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return string.Format("{0}({1},{2})", (object) this.CodeProvider.SourceDocument(label), (object) this.CodeProvider.SourceStartLine(label), (object) this.CodeProvider.SourceStartColumn(label));
        else
          return (string) null;
      }

      internal string SourceDocument(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceDocument(label);
        else
          return (string) null;
      }

      internal int SourceStartLine(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceStartLine(label);
        else
          return 0;
      }

      internal int SourceEndLine(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceEndLine(label);
        else
          return 0;
      }

      internal int SourceStartColumn(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceStartColumn(label);
        else
          return 0;
      }

      internal int SourceEndColumn(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceEndColumn(label);
        else
          return 0;
      }

      internal int SourceStartIndex(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceStartIndex(label);
        else
          return 0;
      }

      internal int SourceLength(Label label)
      {
        if (this.CodeProvider.HasSourceContext(label))
          return this.CodeProvider.SourceLength(label);
        else
          return 0;
      }

      internal int ILOffset(Label label)
      {
        return this.CodeProvider.ILOffset(label);
      }

      private void AddNormalControlFlowEdge(List<Pair<CFGBlock, Pair<string, CFGBlock>>> succs, CFGBlock from, string tag, CFGBlock to)
      {
        succs.Add(new Pair<CFGBlock, Pair<string, CFGBlock>>(from, new Pair<string, CFGBlock>(tag, to)));
      }

      internal override bool HasSingleSuccessor(APC ppoint, out APC next, DConsCache consCache)
      {
        if (ppoint.Index < ppoint.Block.Count)
        {
          next = new APC(ppoint.Block, ppoint.Index + 1, ppoint.SubroutineContext);
          return true;
        }
        else if (this.IsSubroutineEnd(ppoint.Block))
        {
          if (ppoint.SubroutineContext == null)
          {
            next = APC.Dummy;
            return false;
          }
          else
          {
            next = this.ComputeSubroutineContinuation(ppoint, consCache);
            return true;
          }
        }
        else
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels1 = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) null;
          foreach (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels2 in ppoint.Block.Subroutine.SuccessorBlocks(ppoint.Block))
          {
            if (blockWithLabels1 == null)
            {
              blockWithLabels1 = blockWithLabels2;
            }
            else
            {
              next = APC.Dummy;
              return false;
            }
          }
          if (blockWithLabels1 != null)
          {
            next = this.ComputeTargetFinallyContext(ppoint, (CFGBlock) blockWithLabels1, consCache);
            return true;
          }
          else
          {
            next = APC.Dummy;
            return false;
          }
        }
      }

      internal override bool HasSinglePredecessor(APC ppoint, out APC singlePredecessor, DConsCache consCache)
      {
        if (ppoint.Index > 0)
        {
          singlePredecessor = new APC(ppoint.Block, ppoint.Index - 1, ppoint.SubroutineContext);
          return true;
        }
        else if (this.IsSubroutineStart(ppoint.Block))
        {
          if (ppoint.SubroutineContext == null)
          {
            singlePredecessor = APC.Dummy;
            return false;
          }
          else
          {
            bool hasSinglePred;
            singlePredecessor = this.ComputeSubroutinePreContinuation(ppoint, out hasSinglePred, consCache);
            return hasSinglePred;
          }
        }
        else
        {
          CFGBlock cfgBlock1 = (CFGBlock) null;
          foreach (CFGBlock cfgBlock2 in ppoint.Block.Subroutine.PredecessorBlocks(ppoint.Block))
          {
            if (cfgBlock1 != null)
            {
              singlePredecessor = APC.Dummy;
              return false;
            }
            else
              cfgBlock1 = cfgBlock2;
          }
          if (cfgBlock1 == null)
          {
            singlePredecessor = APC.Dummy;
            return false;
          }
          else
          {
            FList<Pair<string, Subroutine>> flist = this.EdgeSubroutinesOuterToInner(cfgBlock1, ppoint.Block, ppoint.SubroutineContext);
            if (flist == null)
            {
              singlePredecessor = APC.ForEnd(cfgBlock1, ppoint.SubroutineContext);
              return true;
            }
            else
            {
              FList<STuple<CFGBlock, CFGBlock, string>> subroutineContext = consCache(new STuple<CFGBlock, CFGBlock, string>(cfgBlock1, ppoint.Block, flist.Head.One), ppoint.SubroutineContext);
              Subroutine subroutine = flist.Head.Two;
              singlePredecessor = APC.ForEnd(subroutine.Exit, subroutineContext);
              return true;
            }
          }
        }
      }

      internal override APC PredecessorPCPriorToRequires(APC pc, DConsCache consCache)
      {
        if (pc.Index != 0)
          return pc;
        CFGBlock block = pc.Block;
        Method calledMethod;
        bool isNewObj;
        bool isVirtual;
        if (block.IsMethodCallBlock<Method>(out calledMethod, out isNewObj, out isVirtual))
        {
          EnumerableIndexable<CFGBlock> enumerableIndexable = IndexableExtensions.AsIndexable<CFGBlock>(this.PredecessorBlocks(block), 1);
          if (enumerableIndexable.Count == 1)
            return APC.ForEnd(enumerableIndexable[0], pc.SubroutineContext);
        }
        return pc;
      }

      internal override IEnumerable<APC> Successors(APC ppoint, DConsCache consCache)
      {
        APC singleNext;
        if (this.HasSingleSuccessor(ppoint, out singleNext, consCache))
        {
          yield return singleNext;
        }
        else
        {
          int successors = 0;
          foreach (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels in ppoint.Block.Subroutine.SuccessorBlocks(ppoint.Block))
          {
            ++successors;
            yield return ppoint.Block.Subroutine.ComputeTargetFinallyContext(ppoint, (CFGBlock) blockWithLabels, consCache);
          }
        }
      }

      internal override IEnumerable<APC> Predecessors(APC ppoint, DConsCache consCache)
      {
        if (ppoint.Index > 0)
          yield return new APC(ppoint.Block, ppoint.Index - 1, ppoint.SubroutineContext);
        else if (this.IsSubroutineStart(ppoint.Block))
        {
          if (ppoint.SubroutineContext != null)
          {
            foreach (APC apc in this.ComputeSubroutinePreContinuation(ppoint, consCache))
              yield return apc;
          }
        }
        else
        {
          foreach (CFGBlock cfgBlock in ppoint.Block.Subroutine.PredecessorBlocks(ppoint.Block))
          {
            FList<Pair<string, Subroutine>> diffs = this.EdgeSubroutinesOuterToInner(cfgBlock, ppoint.Block, ppoint.SubroutineContext);
            if (diffs == null)
            {
              yield return APC.ForEnd(cfgBlock, ppoint.SubroutineContext);
            }
            else
            {
              FList<STuple<CFGBlock, CFGBlock, string>> newFinallyContext = consCache(new STuple<CFGBlock, CFGBlock, string>(cfgBlock, ppoint.Block, diffs.Head.One), ppoint.SubroutineContext);
              Subroutine subroutine = diffs.Head.Two;
              yield return APC.ForEnd(subroutine.Exit, newFinallyContext);
            }
          }
        }
      }

      private IEnumerable<APC> ComputeSubroutinePreContinuation(APC ppoint, DConsCache consCache)
      {
        STuple<CFGBlock, CFGBlock, string> edge = ppoint.SubroutineContext.Head;
        bool isHandlerEdge;
        FList<Pair<string, Subroutine>> diffs = base.EdgeSubroutinesOuterToInner(edge.One, edge.Two, out isHandlerEdge, ppoint.SubroutineContext.Tail);
        while (diffs.Head.Two != this)
          diffs = diffs.Tail;
        if (diffs.Tail == null)
        {
          if (isHandlerEdge)
          {
            for (int index = 0; index == 0 || index < edge.One.Count; ++index)
              yield return new APC(edge.One, index, ppoint.SubroutineContext.Tail);
          }
          else
            yield return APC.ForEnd(edge.One, ppoint.SubroutineContext.Tail);
        }
        else
        {
          Subroutine nextSubroutine = diffs.Tail.Head.Two;
          yield return APC.ForEnd(nextSubroutine.Exit, consCache(new STuple<CFGBlock, CFGBlock, string>(edge.One, edge.Two, diffs.Tail.Head.One), ppoint.SubroutineContext.Tail));
        }
      }

      private APC ComputeSubroutinePreContinuation(APC ppoint, out bool hasSinglePred, DConsCache consCache)
      {
        STuple<CFGBlock, CFGBlock, string> head = ppoint.SubroutineContext.Head;
        bool isExceptionHandlerEdge;
        FList<Pair<string, Subroutine>> flist = base.EdgeSubroutinesOuterToInner(head.One, head.Two, out isExceptionHandlerEdge, ppoint.SubroutineContext.Tail);
        while (flist.Head.Two != this)
          flist = flist.Tail;
        if (flist.Tail == null)
        {
          if (isExceptionHandlerEdge && head.One.Count > 1)
          {
            hasSinglePred = false;
            return APC.Dummy;
          }
          else
          {
            hasSinglePred = true;
            return APC.ForEnd(head.One, ppoint.SubroutineContext.Tail);
          }
        }
        else
        {
          Subroutine subroutine = flist.Tail.Head.Two;
          hasSinglePred = true;
          return APC.ForEnd(subroutine.Exit, consCache(new STuple<CFGBlock, CFGBlock, string>(head.One, head.Two, flist.Tail.Head.One), ppoint.SubroutineContext.Tail));
        }
      }

      private APC ComputeSubroutineContinuation(APC ppoint, DConsCache consCache)
      {
        STuple<CFGBlock, CFGBlock, string> head = ppoint.SubroutineContext.Head;
        FList<Pair<string, Subroutine>> flist = this.EdgeSubroutinesOuterToInner(head.One, head.Two, ppoint.SubroutineContext.Tail);
        if (flist.Head.Two == this)
          return new APC(head.Two, 0, ppoint.SubroutineContext.Tail);
        while (flist.Tail.Head.Two != this)
          flist = flist.Tail;
        return new APC(flist.Head.Two.Entry, 0, consCache(new STuple<CFGBlock, CFGBlock, string>(head.One, head.Two, flist.Head.One), ppoint.SubroutineContext.Tail));
      }

      private FList<Pair<string, Subroutine>> EdgeSubroutinesOuterToInner(CFGBlock current, CFGBlock succ, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        bool isExceptionHandlerEdge;
        return base.EdgeSubroutinesOuterToInner(current, succ, out isExceptionHandlerEdge, context);
      }

      public override FList<Pair<string, Subroutine>> EdgeSubroutinesOuterToInner(CFGBlock current, CFGBlock succ, out bool isExceptionHandlerEdge, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        if (current.Subroutine != this)
          return current.Subroutine.EdgeSubroutinesOuterToInner(current, succ, out isExceptionHandlerEdge, context);
        bool flag = this.IsCatchFilterHeader(succ);
        isExceptionHandlerEdge = flag;
        return this.GetOrdinaryEdgeSubroutines(current, succ, context);
      }

      internal override APC ComputeTargetFinallyContext(APC ppoint, CFGBlock succ, DConsCache consCache)
      {
        FList<Pair<string, Subroutine>> l = this.EdgeSubroutinesOuterToInner(ppoint.Block, succ, ppoint.SubroutineContext);
        if (l == null)
          return new APC(succ, 0, ppoint.SubroutineContext);
        while (FList.Length<Pair<string, Subroutine>>(l) > 1)
          l = l.Tail;
        return new APC(l.Head.Two.Entry, 0, consCache(new STuple<CFGBlock, CFGBlock, string>(ppoint.Block, succ, l.Head.One), ppoint.SubroutineContext));
      }

      internal override IEnumerable<CFGBlock> ExceptionHandlers<Data, Type2>(CFGBlock ppoint, Subroutine innerSubroutine, Data data, IHandlerFilter<Type2, Data> handlerPredicateArg)
      {
        yield return (CFGBlock) this.exceptionExit;
      }

      private bool IsEdgeUsed(Pair<CFGBlock, Pair<string, CFGBlock>> edge)
      {
        return edge.One.Index != 32757;
      }

      protected void PostProcessBlocks()
      {
        Stack<CFGBlock> blockStack = new Stack<CFGBlock>();
        this.successorEdges = new EdgeMap<string>(this.successors);
        this.edgeInfo = new DepthFirst.Visitor<CFGBlock, Unit>((IGraph<CFGBlock, Unit>) this, (Predicate<CFGBlock>) null, (Action<CFGBlock>) (block => blockStack.Push(block)), (EdgeVisitor<CFGBlock, Unit>) null);
        this.edgeInfo.VisitSubGraphNonRecursive((CFGBlock) this.exceptionExit);
        this.edgeInfo.VisitSubGraphNonRecursive((CFGBlock) this.exit);
        this.edgeInfo.VisitSubGraphNonRecursive((CFGBlock) this.entry);
        foreach (Pair<CFGBlock, Pair<string, CFGBlock>> pair in this.successorEdges)
        {
          int idGen = 32757;
          pair.One.Renumber(ref idGen);
        }
        int idGen1 = 0;
        foreach (CFGBlock cfgBlock in blockStack)
          cfgBlock.Renumber(ref idGen1);
        this.SuccessorEdges.Filter(new Predicate<Pair<CFGBlock, Pair<string, CFGBlock>>>(this.IsEdgeUsed));
        this.predecessorEdges = this.SuccessorEdges.ReversedEdges();
        int finishTime = 0;
        DepthFirst.Visitor<CFGBlock, string> visitor = new DepthFirst.Visitor<CFGBlock, string>((IGraph<CFGBlock, string>) this.predecessorEdges, (Predicate<CFGBlock>) null, (Action<CFGBlock>) (block => block.SetReversePostOrderIndex(finishTime++)), (EdgeVisitor<CFGBlock, string>) null);
        visitor.VisitSubGraphNonRecursive((CFGBlock) this.exit);
        foreach (CFGBlock node in blockStack)
          visitor.VisitSubGraphNonRecursive(node);
        this.SuccessorEdges.ReSort();
        this.blocks = blockStack.ToArray();
        this.BlockStart = (Dictionary<Label, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>>) null;
        this.Builder = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>) null;
      }

      bool IGraph<CFGBlock, Unit>.Contains(CFGBlock node)
      {
        for (int index = 0; index < this.blocks.Length; ++index)
        {
          if (this.blocks[index] == node)
            return true;
        }
        return false;
      }

      public virtual IEnumerable<Pair<Unit, CFGBlock>> Successors(CFGBlock node)
      {
        foreach (Pair<string, CFGBlock> pair in (IEnumerable<Pair<string, CFGBlock>>) this.SuccessorEdges[node])
          yield return new Pair<Unit, CFGBlock>(Unit.Value, pair.Two);
        if (node != this.exceptionExit)
          yield return new Pair<Unit, CFGBlock>(Unit.Value, (CFGBlock) this.exceptionExit);
      }

      public override void Print(TextWriter tw, ILPrinter<APC> ilPrinter, BlockInfoPrinter<APC> blockInfoPrinter, Func<CFGBlock, IEnumerable<FList<STuple<CFGBlock, CFGBlock, string>>>> contextLookup, FList<STuple<CFGBlock, CFGBlock, string>> context, IMutableSet<Pair<Subroutine, FList<STuple<CFGBlock, CFGBlock, string>>>> printed)
      {
        Pair<Subroutine, FList<STuple<CFGBlock, CFGBlock, string>>> element = new Pair<Subroutine, FList<STuple<CFGBlock, CFGBlock, string>>>((Subroutine) this, context);
        if (printed.Contains(element))
          return;
        printed.Add(element);
        IMutableSet<Subroutine> subs = (IMutableSet<Subroutine>) new Set<Subroutine>();
        IMethodInfo<Method> methodInfo = this as IMethodInfo<Method>;
        string str1 = methodInfo != null ? string.Format("({0})", (object) this.MethodCache.MetadataDecoder.FullName(methodInfo.Method)) : (string) null;
        if (context == null)
          tw.WriteLine("Subroutine SR{0} {1} {2}", (object) this.Id, (object) this.Kind, (object) str1);
        else
          tw.WriteLine("Subroutine SR{0} {1} {2} {3}", (object) this.Id, (object) this.Kind, (object) new APC(this.Entry, 0, context), (object) str1);
        tw.WriteLine("-----------------");
        foreach (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> block in this.Blocks)
        {
          tw.Write("Block {0} ({1})", (object) block.Index, (object) block.ReversePostOrderIndex);
          if (this.EdgeInfo.DepthFirstInfo((CFGBlock) block).TargetOfBackEdge)
            tw.WriteLine(" (target of backedge)");
          else if (this.IsJoinPoint((CFGBlock) block))
            tw.WriteLine(" (join point)");
          else
            tw.WriteLine();
          tw.Write("  Predecessors: ");
          foreach (Pair<string, CFGBlock> pair in (IEnumerable<Pair<string, CFGBlock>>) block.Subroutine.PredecessorEdges[(CFGBlock) block])
            tw.Write("({0}, {1}) ", (object) pair.One, (object) pair.Two.Index);
          tw.WriteLine();
          this.PrintHandlers(tw, block);
          tw.WriteLine("  Code:");
          foreach (APC label in block.APCs(context))
          {
            string str2 = label.PrimarySourceContext();
            if (str2 != null)
              tw.WriteLine(str2);
            ilPrinter(label, "    ", tw);
          }
          tw.Write("  Successors: ");
          foreach (Pair<string, CFGBlock> pair in (IEnumerable<Pair<string, CFGBlock>>) this.SuccessorEdges[(CFGBlock) block])
          {
            tw.Write("({0},{1}", (object) pair.One, (object) pair.Two.Index);
            if (this.EdgeInfo.IsBackEdge((CFGBlock) block, Unit.Value, pair.Two))
              tw.Write(" BE");
            for (FList<Pair<string, Subroutine>> flist = this.GetOrdinaryEdgeSubroutines((CFGBlock) block, pair.Two, context); flist != null; flist = flist.Tail)
            {
              subs.Add(flist.Head.Two);
              tw.Write(" SR{0}({1})", (object) flist.Head.Two.Id, (object) flist.Head.One);
            }
            tw.Write(") ");
          }
          tw.WriteLine();
          if (blockInfoPrinter != null)
            blockInfoPrinter(new APC((CFGBlock) block, block.Last.Index, context), "  ", tw);
          tw.WriteLine();
        }
        this.PrintReferencedSubroutines(tw, subs, ilPrinter, blockInfoPrinter, contextLookup, context, printed);
      }

      protected virtual void PrintReferencedSubroutines(TextWriter tw, IMutableSet<Subroutine> subs, ILPrinter<APC> ilPrinter, BlockInfoPrinter<APC> blockInfoPrinter, Func<CFGBlock, IEnumerable<FList<STuple<CFGBlock, CFGBlock, string>>>> contextLookup, FList<STuple<CFGBlock, CFGBlock, string>> context, IMutableSet<Pair<Subroutine, FList<STuple<CFGBlock, CFGBlock, string>>>> printed)
      {
        foreach (Subroutine subroutine in (IEnumerable<Subroutine>) subs)
        {
          if (contextLookup == null)
          {
            subroutine.Print(tw, ilPrinter, blockInfoPrinter, contextLookup, (FList<STuple<CFGBlock, CFGBlock, string>>) null, printed);
          }
          else
          {
            foreach (FList<STuple<CFGBlock, CFGBlock, string>> context1 in contextLookup(subroutine.Entry))
              subroutine.Print(tw, ilPrinter, blockInfoPrinter, contextLookup, context1, printed);
          }
        }
      }

      protected virtual void PrintHandlers(TextWriter tw, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> block)
      {
        tw.Write("  Handlers: ");
        if (block != this.exceptionExit)
          tw.Write("{0} ", (object) this.exceptionExit.Index);
        tw.WriteLine();
      }

      internal override IEnumerable<Subroutine> UsedSubroutines(IMutableSet<int> alreadyFound)
      {
        foreach (FList<Pair<string, Subroutine>> flist in this.edgeSubroutines.Values)
        {
          for (FList<Pair<string, Subroutine>> list = flist; list != null; list = list.Tail)
          {
            Subroutine sub = list.Head.Two;
            if (!alreadyFound.Contains(sub.Id))
            {
              alreadyFound.Add(sub.Id);
              yield return sub;
            }
          }
        }
      }

      internal virtual string SourceAssertionCondition(Label label)
      {
        return this.CodeProvider.SourceAssertionCondition(label);
      }
    }

    internal abstract class SubroutineWithHandlers<Label, Handler> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>
    {
      internal FList<Handler> CurrentProtectingHandlers = FList<Handler>.Empty;
      protected OnDemandMap<Handler, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>> FilterCodeBlocks;
      protected OnDemandMap<Handler, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>> CatchFilterHeaders;
      internal OnDemandMap<Handler, Subroutine> FaultFinallySubroutines;
      internal OnDemandMap<CFGBlock, FList<Handler>> ProtectingHandlers;

      protected IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler> CodeProvider
      {
        get
        {
          return (IMethodCodeProvider<Label, Local, Parameter, Method, Field, Type, Handler>) this.CodeProvider;
        }
      }

      internal SubroutineWithHandlers(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache)
        : base(methodCache)
      {
      }

      internal SubroutineWithHandlers(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler> builder, Label entry)
        : base(methodCache, entry, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>) builder)
      {
      }

      private bool IsFault(Handler handler)
      {
        return this.CodeProvider.IsFaultHandler(handler);
      }

      private FList<Handler> ProtectingHandlersList(CFGBlock block)
      {
        FList<Handler> flist;
        this.ProtectingHandlers.TryGetValue(block, out flist);
        return flist;
      }

      private IEnumerable<Handler> GetProtectingHandlers(CFGBlock block)
      {
        return FList.GetEnumerable<Handler>(this.ProtectingHandlersList(block));
      }

      internal MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> CreateCatchFilterHeader(Handler handler, Label label)
      {
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels;
        if (!this.BlockStart.TryGetValue(label, out blockWithLabels))
        {
          blockWithLabels = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CatchFilterEntryBlock<Label>((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>) this, ref this.blockIdGenerator);
          this.CatchFilterHeaders.Add(handler, blockWithLabels);
          this.BlockStart.Add(label, blockWithLabels);
          if (this.CodeProvider.IsFilterHandler(handler))
          {
            MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> targetBlock = this.GetTargetBlock(this.CodeProvider.FilterDecisionStart(handler));
            this.FilterCodeBlocks.Add(handler, targetBlock);
          }
        }
        return blockWithLabels;
      }

      internal override IEnumerable<Subroutine> UsedSubroutines(IMutableSet<int> alreadySeen)
      {
        foreach (Subroutine subroutine in this.FaultFinallySubroutines.Values)
          yield return subroutine;
        foreach (Subroutine subroutine in this.BaseUsedSubroutines(alreadySeen))
          yield return subroutine;
      }

      private IEnumerable<Subroutine> BaseUsedSubroutines(IMutableSet<int> alreadySeen)
      {
        return base.UsedSubroutines(alreadySeen);
      }

      public override FList<Pair<string, Subroutine>> EdgeSubroutinesOuterToInner(CFGBlock current, CFGBlock succ, out bool isExceptionHandlerEdge, FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        if (current.Subroutine != this)
          return current.Subroutine.EdgeSubroutinesOuterToInner(current, succ, out isExceptionHandlerEdge, context);
        FList<Handler> l1 = this.ProtectingHandlersList(current);
        FList<Handler> l2 = this.ProtectingHandlersList(succ);
        bool flag = this.IsCatchFilterHeader(succ);
        isExceptionHandlerEdge = flag;
        FList<Pair<string, Subroutine>> tail = this.GetOrdinaryEdgeSubroutines(current, succ, context);
        while (l1 != l2)
        {
          if (FList.Length<Handler>(l1) >= FList.Length<Handler>(l2))
          {
            Handler head = l1.Head;
            if (this.IsFaultOrFinally(head) && (!this.IsFault(head) || flag))
              tail = FList<Pair<string, Subroutine>>.Cons(new Pair<string, Subroutine>("finally", this.FaultFinallySubroutines[head]), tail);
            l1 = l1.Tail;
          }
          else
            l2 = l2.Tail;
        }
        return tail;
      }

      public bool IsFaultOrFinally(Handler handler)
      {
        if (!this.CodeProvider.IsFaultHandler(handler))
          return this.CodeProvider.IsFinallyHandler(handler);
        else
          return true;
      }

      internal override IEnumerable<CFGBlock> ExceptionHandlers<Data, Type2>(CFGBlock ppoint, Subroutine innerSubroutine, Data data, IHandlerFilter<Type2, Data> handlerPredicateArg)
      {
        IHandlerFilter<Type, Data> handlerPredicate = (IHandlerFilter<Type, Data>) handlerPredicateArg;
        FList<Handler> protectingHandlers = this.ProtectingHandlersList(ppoint);
        if (innerSubroutine != null && innerSubroutine.IsFaultFinally)
        {
          for (; protectingHandlers != null; protectingHandlers = protectingHandlers.Tail)
          {
            if (this.IsFaultOrFinally(protectingHandlers.Head) && this.FaultFinallySubroutines[protectingHandlers.Head] == innerSubroutine)
            {
              protectingHandlers = protectingHandlers.Tail;
              break;
            }
          }
        }
        for (; protectingHandlers != null; protectingHandlers = protectingHandlers.Tail)
        {
          Handler handler = protectingHandlers.Head;
          if (!this.IsFaultOrFinally(handler))
          {
            if (handlerPredicate != null)
            {
              bool stopPropagation = false;
              if (this.CodeProvider.IsCatchHandler(handler))
              {
                if (handlerPredicate.Catch(data, this.CodeProvider.CatchType(handler), out stopPropagation))
                  yield return (CFGBlock) this.CatchFilterHeaders[handler];
              }
              else if (handlerPredicate.Filter(data, new APC((CFGBlock) this.FilterCodeBlocks[handler], 0, (FList<STuple<CFGBlock, CFGBlock, string>>) null), out stopPropagation))
                yield return (CFGBlock) this.CatchFilterHeaders[handler];
              if (stopPropagation)
                yield break;
            }
            else
              yield return (CFGBlock) this.CatchFilterHeaders[handler];
            if (this.CodeProvider.IsCatchAllHandler(handler))
              yield break;
          }
        }
        yield return this.ExceptionExit;
      }

      public override IEnumerable<Pair<Unit, CFGBlock>> Successors(CFGBlock node)
      {
        foreach (Pair<string, CFGBlock> pair in (IEnumerable<Pair<string, CFGBlock>>) this.SuccessorEdges[node])
          yield return new Pair<Unit, CFGBlock>(Unit.Value, pair.Two);
        foreach (Handler handler in this.GetProtectingHandlers(node))
        {
          if (!this.IsFaultOrFinally(handler))
            yield return new Pair<Unit, CFGBlock>(Unit.Value, (CFGBlock) this.CatchFilterHeaders[handler]);
        }
        if (node != this.ExceptionExit)
          yield return new Pair<Unit, CFGBlock>(Unit.Value, this.ExceptionExit);
      }

      protected override void PrintReferencedSubroutines(TextWriter tw, IMutableSet<Subroutine> subs, ILPrinter<APC> ilPrinter, BlockInfoPrinter<APC> blockInfoPrinter, Func<CFGBlock, IEnumerable<FList<STuple<CFGBlock, CFGBlock, string>>>> contextLookup, FList<STuple<CFGBlock, CFGBlock, string>> context, IMutableSet<Pair<Subroutine, FList<STuple<CFGBlock, CFGBlock, string>>>> printed)
      {
        foreach (Subroutine subroutine in this.FaultFinallySubroutines.Values)
        {
          if (contextLookup == null)
          {
            subroutine.Print(tw, ilPrinter, blockInfoPrinter, contextLookup, context, printed);
          }
          else
          {
            foreach (FList<STuple<CFGBlock, CFGBlock, string>> context1 in contextLookup(subroutine.Entry))
              subroutine.Print(tw, ilPrinter, blockInfoPrinter, contextLookup, context1, printed);
          }
        }
        base.PrintReferencedSubroutines(tw, subs, ilPrinter, blockInfoPrinter, contextLookup, context, printed);
      }

      protected override void PrintHandlers(TextWriter tw, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> block)
      {
        tw.Write("  Handlers: ");
        foreach (Handler handler in this.GetProtectingHandlers((CFGBlock) block))
        {
          if (this.IsFaultOrFinally(handler))
          {
            Subroutine subroutine = this.FaultFinallySubroutines[handler];
            tw.Write("SR{0} ", (object) subroutine.Id);
          }
          else
          {
            MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels = this.CatchFilterHeaders[handler];
            tw.Write("{0} ", (object) blockWithLabels.Index);
          }
        }
        if (block != this.ExceptionExit)
          tw.Write("{0} ", (object) this.ExceptionExit.Index);
        tw.WriteLine();
      }
    }

    internal class MethodSubroutine<Label, Handler> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>, IMethodInfo<Method>
    {
      private Set<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>> blocksEndingInReturn;
      private Method method;

      public override string Kind
      {
        get
        {
          return "method";
        }
      }

      public override bool HasReturnValue
      {
        get
        {
          return !this.MethodCache.MetadataDecoder.IsVoidMethod(this.method);
        }
      }

      public override bool IsMethod
      {
        get
        {
          return true;
        }
      }

      public override bool IsConstructor
      {
        get
        {
          return this.MethodCache.MetadataDecoder.IsConstructor(this.method);
        }
      }

      internal override bool IsCompilerGenerated
      {
        get
        {
          return this.MethodCache.MetadataDecoder.IsCompilerGenerated(this.method);
        }
      }

      public override string Name
      {
        get
        {
          return this.MethodCache.MetadataDecoder.FullName(this.method);
        }
      }

      public Method Method
      {
        get
        {
          return this.method;
        }
      }

      internal MethodSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method)
        : base(methodCache)
      {
        this.method = method;
      }

      public MethodSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler> builder, Label startLabel)
        : base(methodCache, builder, startLabel)
      {
        this.method = method;
        builder.BuildBlocks(startLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>) this);
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> targetBlock = this.GetTargetBlock(startLabel);
        this.Commit();
        Type type = this.MethodCache.MetadataDecoder.DeclaringType(method);
        Subroutine invariant = this.MethodCache.GetInvariant(type);
        if (invariant != null && !this.MethodCache.MetadataDecoder.IsConstructor(method) && !this.MethodCache.MetadataDecoder.IsStatic(method))
        {
          this.AddEdgeSubroutine(this.Entry, (CFGBlock) targetBlock, invariant, "entry");
          Subroutine requires = this.MethodCache.GetRequires(method);
          if (requires != null)
          {
            this.AddEdgeSubroutine(this.Entry, (CFGBlock) targetBlock, requires, "entry");
            this.AddEdgeSubroutine(this.Entry, (CFGBlock) targetBlock, this.MethodCache.GetRedundantInvariant(invariant, type), "entry");
          }
        }
        else
          this.AddEdgeSubroutine(this.Entry, (CFGBlock) targetBlock, this.MethodCache.GetRequires(method), "entry");
        if (this.blocksEndingInReturn == null)
          return;
        Subroutine ensures = this.MethodCache.GetEnsures(method);
        foreach (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> blockWithLabels in this.blocksEndingInReturn)
        {
          if (!this.MethodCache.MetadataDecoder.IsStatic(method) && !this.MethodCache.ContractDecoder.IsPure(method) && (!this.MethodCache.MetadataDecoder.IsFinalizer(method) && !this.MethodCache.MetadataDecoder.IsDispose(method)))
            this.AddEdgeSubroutine((CFGBlock) blockWithLabels, this.Exit, invariant, "exit");
          this.AddEdgeSubroutine((CFGBlock) blockWithLabels, this.Exit, ensures, "exit");
        }
        if (ensures != null)
        {
          foreach (Subroutine oldEval in ensures.UsedSubroutines())
          {
            if (oldEval.IsOldValue)
              this.AddEdgeSubroutine(this.Entry, (CFGBlock) targetBlock, Predefined.OldEvalPopSubroutine<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>(this.MethodCache, oldEval), "oldmanifest");
          }
        }
        this.blocksEndingInReturn = (Set<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>>) null;
      }

      internal override void Initialize()
      {
      }

      internal override void AddReturnBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> block)
      {
        if (this.blocksEndingInReturn == null)
          this.blocksEndingInReturn = new Set<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>>();
        this.blocksEndingInReturn.Add(block);
        base.AddReturnBlock(block);
      }
    }

    internal abstract class FaultFinallySubroutineBase<Label, Handler> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlers<Label, Handler>
    {
      public override bool HasContextDependentStackDepth
      {
        get
        {
          return false;
        }
      }

      public override bool IsFaultFinally
      {
        get
        {
          return true;
        }
      }

      protected FaultFinallySubroutineBase(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler> builder, Label startLabel)
        : base(methodCache, builder, startLabel)
      {
      }

      internal override void Initialize()
      {
      }
    }

    internal class FaultSubroutine<Label, Handler> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.FaultFinallySubroutineBase<Label, Handler>
    {
      public override string Kind
      {
        get
        {
          return "fault";
        }
      }

      public FaultSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler> builder, Label startLabel)
        : base(methodCache, builder, startLabel)
      {
      }
    }

    internal class FinallySubroutine<Label, Handler> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.FaultFinallySubroutineBase<Label, Handler>
    {
      public override string Kind
      {
        get
        {
          return "finally";
        }
      }

      public FinallySubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineWithHandlersBuilder<Label, Handler> builder, Label startLabel)
        : base(methodCache, builder, startLabel)
      {
      }
    }

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

    internal class RequiresSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CallingContractSubroutine<Label>, IEquatable<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label>>
    {
      public override string Kind
      {
        get
        {
          return "requires";
        }
      }

      public override bool IsRequires
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

      public RequiresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel, IFunctionalSet<Subroutine> inherited)
        : base(methodCache, method, builder, startLabel)
      {
        this.AddBaseRequires((CFGBlock) this.GetTargetBlock(startLabel), inherited);
      }

      public RequiresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, IFunctionalSet<Subroutine> inherited)
        : base(methodCache, method)
      {
        this.AddSuccessor(this.Entry, "entry", this.Exit);
        this.AddBaseRequires(this.Exit, inherited);
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

      private void AddBaseRequires(CFGBlock targetOfEntry, IFunctionalSet<Subroutine> inherited)
      {
        foreach (Subroutine subroutine in inherited.Elements)
          this.AddEdgeSubroutine(this.Entry, targetOfEntry, subroutine, "inherited");
      }

      public bool Equals(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label> that)
      {
        return this.Id == that.Id;
      }
    }

    internal class SimpleSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>
    {
      private int stackDelta;

      public override int StackDelta
      {
        get
        {
          return this.stackDelta;
        }
      }

      public override string Kind
      {
        get
        {
          return "simple";
        }
      }

      public SimpleSubroutine(int stackDelta, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Label startLabel, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder)
        : base(methodCache, startLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBuilder<Label>) builder)
      {
        this.stackDelta = stackDelta;
        builder.BuildBlocks(startLabel, (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>) this);
        this.Commit();
      }

      internal override void Initialize()
      {
      }
    }

    internal class OldValueSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CallingContractSubroutine<Label>
    {
      private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> beginOldBlock;
      private MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> endOldBlock;

      public override string Kind
      {
        get
        {
          return "old";
        }
      }

      public override int StackDelta
      {
        get
        {
          return 1;
        }
      }

      public override bool IsOldValue
      {
        get
        {
          return true;
        }
      }

      public OldValueSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel)
        : base(methodCache, method, builder, startLabel)
      {
      }

      internal override void Initialize()
      {
      }

      internal void Commit(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> endOldBlock)
      {
        this.endOldBlock = endOldBlock;
        base.Commit();
      }

      internal void RegisterBeginBlock(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> newBlock)
      {
        this.beginOldBlock = newBlock;
      }

      internal APC BeginOldAPC(FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        return new APC((CFGBlock) this.beginOldBlock, 0, context);
      }

      internal APC EndOldAPC(FList<STuple<CFGBlock, CFGBlock, string>> context)
      {
        return new APC((CFGBlock) this.endOldBlock, this.endOldBlock.Count - 1, context);
      }
    }

    internal class EnsuresSubroutine<Label> : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.CallingContractSubroutine<Label>, IEquatable<MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>>
    {
      private OnDemandMap<int, Pair<CFGBlock, Type>> inferredOldLabelReverseMap;

      public override string Kind
      {
        get
        {
          return "ensures";
        }
      }

      public override bool IsEnsures
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

      public EnsuresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel, IFunctionalSet<Subroutine> inherited)
        : base(methodCache, method, builder, startLabel)
      {
        this.AddBaseEnsures(this.Entry, (CFGBlock) this.GetTargetBlock(startLabel), inherited);
      }

      public EnsuresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, IFunctionalSet<Subroutine> inherited)
        : base(methodCache, method)
      {
        this.AddSuccessor(this.Entry, "entry", this.Exit);
        this.AddBaseEnsures(this.Entry, this.Exit, inherited);
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

      internal override MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label> NewBlock()
      {
        return (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.BlockWithLabels<Label>) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label>((MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineBase<Label>) this, ref this.blockIdGenerator);
      }

      private void AddBaseEnsures(CFGBlock fromBlock, CFGBlock toBlock, IFunctionalSet<Subroutine> inherited)
      {
        if (inherited == null)
          return;
        foreach (Subroutine subroutine in inherited.Elements)
          this.AddEdgeSubroutine(fromBlock, toBlock, subroutine, "inherited");
      }

      internal static int GetKey(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> rs)
      {
        return rs.Id;
      }

      public bool Equals(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label> that)
      {
        return this.Id == that.Id;
      }

      internal override void Commit()
      {
        base.Commit();
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.CommitScanState visitor = new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.CommitScanState(this);
        MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label> priorBlock = (MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label>) null;
        foreach (CFGBlock block1 in this.Blocks)
        {
          MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label> block2 = block1 as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresBlock<Label>;
          if (block2 != null)
          {
            priorBlock = block2;
            int count = block2.Count;
            visitor.StartBlock(block2);
            for (int index = 0; index < count; ++index)
            {
              if (block2.OriginalForwardDecode<int, bool, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.CommitScanState>(index, visitor, index))
                block2.AddInstruction(index);
            }
          }
          else
            visitor.HandlePotentialCallBlock(block1 as MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.MethodCallBlock<Label>, priorBlock);
          foreach (CFGBlock succ in this.SuccessorBlocks(block1))
            visitor.SetStartState(succ);
        }
      }

      internal void AddInferredOldMap(int blockIndex, int instructionIndex, CFGBlock otherBlock, Type endOldType)
      {
        this.inferredOldLabelReverseMap.Add(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.OverlayInstructionKey(blockIndex, instructionIndex), new Pair<CFGBlock, Type>(otherBlock, endOldType));
      }

      private static int OverlayInstructionKey(int blockIndex, int instruction)
      {
        return (instruction << 16) + blockIndex;
      }

      internal CFGBlock InferredBeginEndBijection(APC pc)
      {
        Type endOldType;
        return this.InferredBeginEndBijection(pc, out endOldType);
      }

      internal CFGBlock InferredBeginEndBijection(APC pc, out Type endOldType)
      {
        Pair<CFGBlock, Type> pair;
        if (!this.inferredOldLabelReverseMap.TryGetValue(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>.OverlayInstructionKey(pc.Block.Index, pc.Index), out pair))
          throw new ApplicationException("Fatal bug in ensures CFG begin/end old map");
        endOldType = pair.Two;
        return pair.One;
      }

      private enum ScanState
      {
        OutsideOld,
        InsideOld,
        InsertingOld,
        InsertingOldAfterCall,
      }

      private class CommitScanState : MSILVisitor<Label, Local, Parameter, Method, Field, Type, Unit, Unit, int, bool>, ICodeQuery<Label, Local, Parameter, Method, Field, Type, int, bool>, IVisitMSIL<Label, Local, Parameter, Method, Field, Type, Unit, Unit, int, bool>, IVisitSynthIL<Label, Method, Type, Unit, Unit, int, bool>, IVisitExprIL<Label, Type, Unit, Unit, int, bool>
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

      public ModelEnsuresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> builder, Label startLabel, IFunctionalSet<Subroutine> inherited)
        : base(methodCache, method, builder, startLabel, inherited)
      {
      }

      public ModelEnsuresSubroutine(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, Method method, IFunctionalSet<Subroutine> inherited)
        : base(methodCache, method, inherited)
      {
      }
    }

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
}
