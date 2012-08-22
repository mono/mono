using System;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.MethodCache
{
	public class RequiresCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Method, Pair<Method, IImmutableSet<Subroutine>>>
    {
      private ErrorHandler output;

      public RequiresCache(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly> methodCache, ErrorHandler output)
        : base(methodCache)
      {
        this.output = output;
      }

      protected override Subroutine Factory<Label>(MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SimpleSubroutineBuilder<Label> sb, Label entry, Pair<Method, IImmutableSet<Subroutine>> data)
      {
        return (Subroutine) new MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.RequiresSubroutine<Label>(this.MethodCache, data.One, sb, entry, data.Two);
      }

      protected override Subroutine BuildNewSubroutine(Method method)
      {
        if (this.ContractDecoder != null)
        {
          IImmutableSet<Subroutine> inheritedRequires = this.GetInheritedRequires(method);
          if (this.ContractDecoder.HasRequires(method))
            return this.ContractDecoder.AccessRequires<Pair<Method, IImmutableSet<Subroutine>>, Subroutine>(method, (ICodeConsumer<Local, Parameter, Method, Field, Type, Pair<Method, IImmutableSet<Subroutine>>, Subroutine>) this, new Pair<Method, IImmutableSet<Subroutine>>(method, inheritedRequires));
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

      private IImmutableSet<Subroutine> GetInheritedRequires(Method method)
      {
        IImmutableSet<Subroutine> functionalSet = ImmutableSet<Subroutine>.Empty(new Converter<Subroutine, int>(Subroutine.GetKey));
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
}

