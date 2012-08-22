using System;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.MethodCache
{
	internal class EnsuresCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Method, Pair<Method, IImmutableSet<Subroutine>>>
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
          IImmutableSet<Subroutine> inheritedEnsures = this.GetInheritedEnsures(method);
          if (this.ContractDecoder.HasEnsures(method))
            return this.ContractDecoder.AccessEnsures<Pair<Method, IImmutableSet<Subroutine>>, Subroutine>(method, (ICodeConsumer<Local, Parameter, Method, Field, Type, Pair<Method, IImmutableSet<Subroutine>>, Subroutine>) this, new Pair<Method, IImmutableSet<Subroutine>>(method, inheritedEnsures));
          if (inheritedEnsures.Count > 0)
          {
            if (inheritedEnsures.Count > 1)
              return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Unit>(this.MethodCache, method, inheritedEnsures);
            else
              return inheritedEnsures.Any;
          }
        }
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Unit>(this.MethodCache, method, (IImmutableSet<Subroutine>) null);
      }

      protected override Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Pair<Method, IImmutableSet<Subroutine>> data)
      {
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.EnsuresSubroutine<Label>(this.MethodCache, data.One, sb, entry, data.Two);
      }

      private IImmutableSet<Subroutine> GetInheritedEnsures(Method method)
      {
        IImmutableSet<Subroutine> functionalSet = ImmutableSet<Subroutine>.Empty(new Converter<Subroutine, int>(Subroutine.GetKey));
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
}

