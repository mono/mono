using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System {
	partial class RuntimeType {
		private const int GenericParameterCountAny = -1;

		protected override MethodInfo GetMethodImpl (
			String name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv,
			Type [] types, ParameterModifier [] modifiers)
		{
			return GetMethodImplCommon (name, GenericParameterCountAny, bindingAttr, binder, callConv, types, modifiers);
		}

		protected override MethodInfo GetMethodImpl (
			String name, int genericParameterCount, BindingFlags bindingAttr, Binder binder, CallingConventions callConv,
			Type [] types, ParameterModifier [] modifiers)
		{
			return GetMethodImplCommon (name, genericParameterCount, bindingAttr, binder, callConv, types, modifiers);
		}

		private MethodInfo GetMethodImplCommon (
			String name, int genericParameterCount, BindingFlags bindingAttr, Binder binder, CallingConventions callConv,
			Type [] types, ParameterModifier [] modifiers)
		{
			ListBuilder<MethodInfo> candidates = GetMethodCandidates (name, genericParameterCount, bindingAttr, callConv, types, false);

			if (candidates.Count == 0)
				return null;

			if (types == null || types.Length == 0) {
				MethodInfo firstCandidate = candidates[0];

				if (candidates.Count == 1) {
					return firstCandidate;
				} else if (types == null) {
					for (int j = 1; j < candidates.Count; j++) {
						MethodInfo methodInfo = candidates [j];
						if (!System.DefaultBinder.CompareMethodSig (methodInfo, firstCandidate))
							throw new AmbiguousMatchException(SR.Arg_AmbiguousMatchException);
					}

					// All the methods have the exact same name and sig so return the most derived one.
					return System.DefaultBinder.FindMostDerivedNewSlotMeth(candidates.ToArray(), candidates.Count) as MethodInfo;
				}
			}

			if (binder == null)
				binder = DefaultBinder;

			return binder.SelectMethod (bindingAttr, candidates.ToArray(), types, modifiers) as MethodInfo;
		}

		private ListBuilder<MethodInfo> GetMethodCandidates(
			String name, int genericParameterCount, BindingFlags bindingAttr, CallingConventions callConv,
			Type[] types, bool allowPrefixLookup)
		{
			bool prefixLookup, ignoreCase;
			MemberListType listType;
			RuntimeType.FilterHelper(bindingAttr, ref name, allowPrefixLookup, out prefixLookup, out ignoreCase, out listType);

#if MONO
			RuntimeMethodInfo[] cache = GetMethodsByName (name, bindingAttr, listType, this);
#else
			RuntimeMethodInfo[] cache = Cache.GetMethodList(listType, name);
#endif

			ListBuilder<MethodInfo> candidates = new ListBuilder<MethodInfo>(cache.Length);
			for (int i = 0; i < cache.Length; i++)
			{
				RuntimeMethodInfo methodInfo = cache[i];
				if (genericParameterCount != GenericParameterCountAny && genericParameterCount != methodInfo.GenericParameterCount)
					continue;

				if (FilterApplyMethodInfo(methodInfo, bindingAttr, callConv, types) &&
					(!prefixLookup || RuntimeType.FilterApplyPrefixLookup(methodInfo, name, ignoreCase)))
				{
					candidates.Add(methodInfo);
				}
			}

			return candidates;
		}
	}
}