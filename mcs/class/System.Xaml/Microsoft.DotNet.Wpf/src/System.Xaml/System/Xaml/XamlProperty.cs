using System;
using System.Collections.Generic;

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml
#else
namespace System.Xaml
#endif
{
#if SILVERLIGHTXAML
    internal
#else
    public
#endif
    abstract class XamlProperty
    {
        public String Name{ get { return NameCore; } }
        abstract protected String NameCore { get; set; }

        abstract public String BoundName { get; }
        abstract public bool IsImplicit { get; }
        abstract public bool IsUnknown { get; }

        public bool IsPublic { get { return IsPublicCore; } }
        abstract protected bool IsPublicCore { get; set; }

        public bool IsBrowsable { get { return IsBrowsableCore; } }
        abstract protected bool IsBrowsableCore { get; set; }
        
        public XamlType DeclaringType { get { return DeclaringTypeCore; } }
        abstract protected XamlType DeclaringTypeCore { get; set; }

        public XamlType Type { get { return TypeCore; } }
        abstract protected XamlType TypeCore { get; set; }

        public XamlTextSyntax TextSyntax { get { return TextSyntaxCore; } }
        abstract protected XamlTextSyntax TextSyntaxCore { get; set; }

        public bool IsReadOnly { get { return IsReadOnlyCore; } }
        abstract protected bool IsReadOnlyCore { get; set; }

        public bool IsStatic { get { return IsStaticCore; } }
        abstract protected bool IsStaticCore { get; set; }

        public bool IsAttachable { get { return IsAttachableCore; } }
        abstract protected bool IsAttachableCore { get; set; }

        public bool IsEvent { get { return IsEventCore; } }
        abstract protected bool IsEventCore { get; set; }

        public bool IsDirective { get { return IsDirectiveCore; } }
        abstract protected bool IsDirectiveCore { get; set; }
        
        public XamlType TargetType { get { return TargetTypeCore; } }
        abstract protected XamlType TargetTypeCore { get; set; }  // only if Attachable

        public AllowedMemberLocation AllowedLocation { get { return AllowedLocationCore; } }
        abstract protected AllowedMemberLocation AllowedLocationCore { get; set; }
        
        public XamlProperty DependsOn { get { return DependsOnCore; } }
        abstract protected XamlProperty DependsOnCore { get; set; }
        
        public bool IsAmbient { get { return IsAmbientCore; } }
        abstract protected bool IsAmbientCore { get; set; }

        public bool IsObsolete { get { return IsObsoleteCore; } }
        abstract protected bool IsObsoleteCore { get; set; }

        abstract public IList<string> GetXamlNamespaces();
    }
}
