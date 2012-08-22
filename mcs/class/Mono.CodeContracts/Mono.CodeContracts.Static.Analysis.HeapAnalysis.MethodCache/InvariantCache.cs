using System;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.MethodCache
{
	public class InvariantCache : MethodCache<Local, Parameter, Type, Method, Field, Property, Event, Attribute, Assembly>.SubroutineFactory<Type, Pair<Type, Subroutine>>
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
}

