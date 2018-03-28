// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;


namespace System.Workflow.Activities.Rules
{
    internal abstract class Symbol
    {
        internal abstract string Name { get; }
        internal abstract CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality);
        internal abstract void RecordSymbol(ArrayList list);
    }

    // Represents a field, property, or method within "this".  (Not a nested type.)
    internal class MemberSymbol : Symbol
    {
        private MemberInfo member;

        internal MemberSymbol(MemberInfo member)
        {
            this.member = member;
        }

        internal override string Name
        {
            get { return member.Name; }
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseUnadornedMemberIdentifier(parserContext, this, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            list.Add(member);
        }
    }

    internal class NamespaceSymbol : Symbol
    {
        private string name;
        internal readonly NamespaceSymbol Parent;
        internal Dictionary<string, Symbol> NestedSymbols;
        internal readonly int Level;

        internal NamespaceSymbol(string name, NamespaceSymbol parent)
        {
            this.name = name;
            this.Parent = parent;
            this.Level = (parent == null) ? 0 : parent.Level + 1;
        }

        // For unnamed namespaces.  There is only one of these.
        internal NamespaceSymbol()
        {
        }

        internal override string Name
        {
            get { return name; }
        }

        internal NamespaceSymbol AddNamespace(string nsName)
        {
            if (NestedSymbols == null)
                NestedSymbols = new Dictionary<string, Symbol>();

            Symbol ns = null;
            if (!NestedSymbols.TryGetValue(nsName, out ns))
            {
                ns = new NamespaceSymbol(nsName, this);
                NestedSymbols.Add(nsName, ns);
            }

            return ns as NamespaceSymbol;
        }

        internal void AddType(Type type)
        {
            TypeSymbol typeSym = new TypeSymbol(type);
            string typeName = typeSym.Name;

            if (NestedSymbols == null)
                NestedSymbols = new Dictionary<string, Symbol>();

            Symbol existingSymbol = null;
            if (NestedSymbols.TryGetValue(typeName, out existingSymbol))
            {
                OverloadedTypeSymbol overloadSym = existingSymbol as OverloadedTypeSymbol;
                if (overloadSym == null)
                {
                    TypeSymbol typeSymbol = existingSymbol as TypeSymbol;
                    System.Diagnostics.Debug.Assert(typeSymbol != null);
                    overloadSym = new OverloadedTypeSymbol(typeName, typeSym, typeSymbol);
                    NestedSymbols[typeName] = overloadSym;
                }
                else
                {
                    overloadSym.AddLocalType(typeSym);
                }
            }
            else
            {
                NestedSymbols.Add(typeName, typeSym);
            }
        }

        internal Symbol FindMember(string memberName)
        {
            Symbol nestedSym = null;
            NestedSymbols.TryGetValue(memberName, out nestedSym);
            return nestedSym;
        }

        internal ArrayList GetMembers()
        {
            ArrayList members = new ArrayList(NestedSymbols.Count);
            foreach (Symbol sym in NestedSymbols.Values)
                sym.RecordSymbol(members);

            return members;
        }

        internal string GetQualifiedName()
        {
            StringBuilder sb = new StringBuilder();

            Stack<string> names = new Stack<string>();

            names.Push(Name);
            for (NamespaceSymbol currentParent = Parent; currentParent != null; currentParent = currentParent.Parent)
                names.Push(currentParent.Name);

            sb.Append(names.Pop());
            while (names.Count > 0)
            {
                sb.Append('.');
                sb.Append(names.Pop());
            }

            return sb.ToString();
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseRootNamespaceIdentifier(parserContext, this, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            // Just add the name (string) to the member list.
            list.Add(Name);
        }
    }

    internal abstract class TypeSymbolBase : Symbol
    {
        internal abstract OverloadedTypeSymbol OverloadType(TypeSymbolBase typeSymBase);
    }

    internal class TypeSymbol : TypeSymbolBase
    {
        internal readonly Type Type;
        internal readonly int GenericArgCount;
        private string name;

        internal TypeSymbol(Type type)
        {
            this.Type = type;
            this.name = type.Name;

            if (type.IsGenericType)
            {
                int tickIx = type.Name.LastIndexOf('`');
                if (tickIx > 0)
                {
                    string count = type.Name.Substring(tickIx + 1);
                    GenericArgCount = Int32.Parse(count, CultureInfo.InvariantCulture);
                    name = type.Name.Substring(0, tickIx);
                }
            }
        }

        internal override string Name
        {
            get { return name; }
        }

        internal override OverloadedTypeSymbol OverloadType(TypeSymbolBase newTypeSymBase)
        {
            OverloadedTypeSymbol newTypeOverload = newTypeSymBase as OverloadedTypeSymbol;
            if (newTypeOverload != null)
            {
                // We've encountered an overloaded type symbol over a previous simple
                // type symbol.
                return newTypeOverload.OverloadType(this);
            }
            else
            {
                // We've encountered two simple types... just create an overload for them if
                // possible.
                TypeSymbol newTypeSym = newTypeSymBase as TypeSymbol;
                if (newTypeSym != null && this.CanOverload(newTypeSym))
                    return new OverloadedTypeSymbol(name, this, newTypeSym);
            }

            return null;
        }

        internal bool CanOverload(TypeSymbol typeSym)
        {
            return typeSym.GenericArgCount != this.GenericArgCount;
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            // The root name is a type (might be generic or not).
            return parser.ParseRootTypeIdentifier(parserContext, this, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            // Add the System.Type to the member list.
            list.Add(Type);
        }
    }

    internal class OverloadedTypeSymbol : TypeSymbolBase
    {
        internal List<TypeSymbol> TypeSymbols = new List<TypeSymbol>();
        private string name;

        internal OverloadedTypeSymbol(string name, TypeSymbol typeSym1, TypeSymbol typeSym2)
        {
            this.name = name;
            AddLocalType(typeSym1);
            AddLocalType(typeSym2);
        }

        private OverloadedTypeSymbol(string name, List<TypeSymbol> typeSymbols)
        {
            this.name = name;
            this.TypeSymbols = typeSymbols;
        }

        internal override string Name
        {
            get { return name; }
        }

        // Add a local overload (within the same namespace).
        internal void AddLocalType(TypeSymbol typeSym)
        {
            // Since it's a local overload, we don't have to check whether it's ambiguous.
            TypeSymbols.Add(typeSym);
        }

        internal override OverloadedTypeSymbol OverloadType(TypeSymbolBase newTypeSymBase)
        {
            List<TypeSymbol> newOverloads = new List<TypeSymbol>();
            TypeSymbol typeSym = null;

            OverloadedTypeSymbol newTypeOverload = newTypeSymBase as OverloadedTypeSymbol;
            if (newTypeOverload != null)
            {
                newOverloads.AddRange(newTypeOverload.TypeSymbols);
            }
            else
            {
                // We've encountered a simple type... just create an overload for them if
                // possible.
                typeSym = newTypeSymBase as TypeSymbol;
                if (typeSym != null)
                    newOverloads.Add(typeSym);
            }

            // If every item in this overloaded type symbol is overloadable with the new one,
            // add to the new list all our items.
            foreach (TypeSymbol thisTypeSym in this.TypeSymbols)
            {
                foreach (TypeSymbol newTypeSym in newOverloads)
                {
                    if (!newTypeSym.CanOverload(thisTypeSym))
                        return null; // Can't overload
                }

                newOverloads.Add(thisTypeSym);
            }

            return new OverloadedTypeSymbol(name, newOverloads);
        }

        internal override CodeExpression ParseRootIdentifier(Parser parser, ParserContext parserContext, bool assignIsEquality)
        {
            return parser.ParseRootOverloadedTypeIdentifier(parserContext, this.TypeSymbols, assignIsEquality);
        }

        internal override void RecordSymbol(ArrayList list)
        {
            foreach (TypeSymbol overloadedType in TypeSymbols)
                list.Add(overloadedType.Type);
        }
    }
}
