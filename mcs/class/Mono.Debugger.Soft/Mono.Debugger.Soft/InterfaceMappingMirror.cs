using System;
using System.Collections.Generic;
using System.Reflection;
using C = Mono.Cecil;
using Mono.Cecil.Metadata;

namespace Mono.Debugger.Soft
{
	public class InterfaceMappingMirror : Mirror {

		internal InterfaceMappingMirror (VirtualMachine vm, TypeMirror target, TypeMirror iface, MethodMirror[] iface_methods, MethodMirror[] target_methods) : base (vm, 0) {
			TargetType = target;
			InterfaceType = iface;
			InterfaceMethods = iface_methods;
			TargetMethods = target_methods;
		}

		public MethodMirror[] InterfaceMethods;

		public TypeMirror InterfaceType;

		public MethodMirror[] TargetMethods;

		public TypeMirror TargetType;
	}
}
