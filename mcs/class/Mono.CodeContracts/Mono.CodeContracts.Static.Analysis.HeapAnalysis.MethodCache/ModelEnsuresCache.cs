using System;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.MethodCache
{
	internal class ModelEnsuresCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Method, Pair<Method, IFunctionalSet<Subroutine>>>
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
}

