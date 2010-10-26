using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CoreClr.Tools
{
	public class MethodMap
	{
		readonly Dictionary<MethodDefinition, HashSet<MethodDefinition>> _callers = new Dictionary<MethodDefinition, HashSet<MethodDefinition>>();
		readonly Dictionary<MethodDefinition, HashSet<MethodDefinition>> _methodsOverridingMe = new Dictionary<MethodDefinition, HashSet<MethodDefinition>>();
		readonly Dictionary<MethodDefinition, HashSet<MethodDefinition>> _methodsIOverride = new Dictionary<MethodDefinition, HashSet<MethodDefinition>>();
		private IEnumerable<MethodToMethodCall> _ignoredCalls = new List<MethodToMethodCall>();

		public MethodMap(IEnumerable<AssemblyDefinition> assemblies, IEnumerable<MethodToMethodCall> ignoredCalls)
		{
			_ignoredCalls = ignoredCalls;
			foreach (var assembly in assemblies)
				ProcessAssembly(assembly);
		}
        public MethodMap(TypeDefinition type)
        {
                ProcessType(type);
        }

		public IEnumerable<MethodDefinition> CallersOf(MethodDefinition m)
		{
			return ExistingSetOf(_callers, m);
		}

		public IEnumerable<MethodDefinition> GetMethodsOverriding(MethodDefinition m)
		{
			return ExistingSetOf(_methodsOverridingMe, m);
		}

		public IEnumerable<MethodDefinition> GetMethodsOverriddenBy(MethodDefinition m)
		{
			return ExistingSetOf(_methodsIOverride, m);
		}

        public IEnumerable<MethodDefinition> GetEntireMethodEnheritanceGraph(MethodDefinition method)
        {
            return GetEntireMethodEnheritanceGraph(method, new List<MethodDefinition>());
        }

        private IEnumerable<MethodDefinition> GetEntireMethodEnheritanceGraph(MethodDefinition method, List<MethodDefinition> alreadyProcessed)
        {
            foreach (var m in GetMethodImplementingAndBaseMethodsOf(method).Where(m1 => !alreadyProcessed.Contains(m1)))
            {
                alreadyProcessed.Add(m);
                yield return m;
                foreach (var m2 in GetEntireMethodEnheritanceGraph(m, alreadyProcessed))
                    yield return m2;
            }

        }

        private IEnumerable<MethodDefinition> GetMethodImplementingAndBaseMethodsOf(MethodDefinition method)
        {
            foreach (var m in GetMethodsOverriddenBy(method))
                yield return m;
            foreach (var m in GetMethodsOverriding(method))
                yield return m;
        }



		void ProcessMethod(MethodDefinition m)
		{
			ProcessCallees(m);
			ProcessBaseMethodDefinitions(m);
		}

		private void ProcessBaseMethodDefinitions(MethodDefinition @override)
		{
            if (@override.IsConstructor || !@override.IsVirtual)
                return;

            foreach (MethodReference overriddenRef in @override.Overrides)
            {
                var baseDefinition = overriddenRef.Resolve();
                RegisterOverride(@override, baseDefinition);
            }

		    foreach (var t in TypeChainOf(@override.DeclaringType))
            {
                var baseDefinition = FindMethodIn(@override, t);
                if (baseDefinition == null)
                    continue;
                RegisterOverride(@override, baseDefinition);
            }
		}

	    private void RegisterOverride(MethodDefinition @override, MethodDefinition baseDefinition)
	    {
	        SetFor(_methodsOverridingMe, baseDefinition).Add(@override);
	        SetFor(_methodsIOverride, @override).Add(baseDefinition);
	    }

	    private IEnumerable<TypeDefinition> TypeChainOf(TypeDefinition declaringType)
	    {
	        var current = declaringType;
            while (current != null)
            {
                foreach (var t in BaseTypeAndInterfacesOf(current).Select(t => Resolve(t)).Where(t => t != null))
                    yield return t;

                current = Resolve(current.BaseType);
            }
	    }

	    private IEnumerable<TypeReference> BaseTypeAndInterfacesOf(TypeDefinition typeDefinition)
	    {
	        yield return typeDefinition.BaseType;
            foreach (TypeReference itf in typeDefinition.Interfaces)
                yield return itf;
	    }

	    private void ProcessCallees(MethodDefinition caller)
		{
			if (!caller.HasBody)
				return;

			foreach (Instruction ins in caller.Body.Instructions)
			{
				var mr = (ins.Operand as MethodReference);
				if (mr == null)
					continue;

				var callee = mr.Resolve();

				if (callee == null)
				{
					// this can occurs for some generated types, like Int[,] where the compiler generates a few methods
					continue;
				}
				if (ShouldIgnore(caller, callee)) continue;

				CallerSetFor(callee).Add(caller);
			}
		}

		private bool ShouldIgnore(MethodDefinition caller, MethodDefinition callee)
		{
			return _ignoredCalls.Contains(new MethodToMethodCall(caller,callee));
		}

		private static IEnumerable<TypeDefinition> BaseTypesOf(TypeDefinition declaringType)
	    {   
            var current = Resolve(declaringType.BaseType);
            while (current != null)
            {
                yield return current;
                current = Resolve(current.BaseType);
            }
	    }

	    private static TypeDefinition Resolve(TypeReference typeRef)
	    {
            if (typeRef==null) return null;
	        return typeRef.Resolve();
	    }

	    private static MethodDefinition FindMethodIn(MethodDefinition method, TypeDefinition typeDef)
	    {
            return typeDef.Methods.Cast<MethodDefinition>().FirstOrDefault(bm => MethodDefinitionComparator.Compare(method, bm));
	    }

		private static IEnumerable<MethodDefinition> ExistingSetOf(Dictionary<MethodDefinition, HashSet<MethodDefinition>> map, MethodDefinition m)
		{
			HashSet<MethodDefinition> resultingSet;
			if (map.TryGetValue(m, out resultingSet))
				return resultingSet;
			return NoMethods;
		}

		private HashSet<MethodDefinition> CallerSetFor(MethodDefinition m)
		{
			return SetFor(_callers, m);
		}

		private static HashSet<MethodDefinition> SetFor(Dictionary<MethodDefinition, HashSet<MethodDefinition>> map, MethodDefinition m)
		{
			HashSet<MethodDefinition> set;
			if (map.TryGetValue(m, out set))
				return set;

			set = new HashSet<MethodDefinition>();
			map.Add(m, set);
			return set;
		}

		void ProcessType(TypeDefinition type)
		{
			if (type.HasConstructors)
				foreach (MethodDefinition ctor in type.Constructors)
					ProcessMethod(ctor);

			if (type.HasMethods)
				foreach (MethodDefinition method in type.Methods)
					ProcessMethod(method);

            if (type.HasNestedTypes)
                foreach(TypeDefinition nested in type.NestedTypes)
                    ProcessType(nested);
		}

		void ProcessAssembly(AssemblyDefinition assembly)
		{	
			foreach (ModuleDefinition module in assembly.Modules)
				foreach (TypeDefinition type in module.Types)
					ProcessType(type);
		}
		
		static readonly MethodDefinition[] NoMethods = new MethodDefinition[0];
	}
}