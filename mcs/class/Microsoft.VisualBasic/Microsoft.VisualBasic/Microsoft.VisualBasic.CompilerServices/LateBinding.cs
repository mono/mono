//
// LateBinding.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Marco Ridoni    (marco.ridoni@virgilio.it)
//
// (C) 2002 Chris J Breisch
// (C) 2003 Marco Ridoni
//

using System;
using System.Reflection;
using Microsoft.VisualBasic;

namespace Microsoft.VisualBasic.CompilerServices
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute]
	sealed public class LateBinding {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		[System.Diagnostics.DebuggerHiddenAttribute] 
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		public static System.Object LateGet (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames, System.Boolean[] CopyBack) { throw new NotImplementedException (); }
		[MonoTODO]
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSetComplex (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames, System.Boolean OptimisticSet, System.Boolean RValueBase) { throw new NotImplementedException (); }
		[MonoTODO]
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static void LateSet (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames) { throw new NotImplementedException (); }
		[MonoTODO]
		[System.Diagnostics.DebuggerStepThroughAttribute] 
		[System.Diagnostics.DebuggerHiddenAttribute] 
		public static System.Object LateIndexGet (System.Object o, System.Object[] args, System.String[] paramnames)
		{
			Type objType;
			Object binderState = null;
	
			if (o == null || args == null)
				throw new ArgumentException();
	
			objType = o.GetType();
			if (objType.IsArray) {
				Array a = (Array) o;
				int[] idxs = new int[args.Length];
				Array.Copy (args, idxs, args.Length);
	
				return a.GetValue(idxs);
			}
			else
			{
				MemberInfo[] defaultMembers = objType.GetDefaultMembers();
				if (defaultMembers == null)
					throw new Exception();  // FIXME: Which exception should we throw?
					
				// We try to find a default method/property/field we can invoke/use
				VBBinder MyBinder = new VBBinder();
				BindingFlags bindingFlags = BindingFlags.IgnoreCase |
						BindingFlags.Instance |
						BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.GetProperty |
						BindingFlags.GetField |
						BindingFlags.InvokeMethod;
	
				MethodBase[] mb = new MethodBase[defaultMembers.Length];
				try {
					for (int x = 0; x < defaultMembers.Length; x++)
						if (defaultMembers[x].MemberType == MemberTypes.Property)
							mb[x] = ((PropertyInfo) defaultMembers[x]).GetGetMethod();
						else
							mb[x] = (MethodBase) defaultMembers[x];
				} catch (Exception e) {	}
	
				MethodBase TheMethod = MyBinder.BindToMethod (bindingFlags,
										mb,
										ref args,
										null,
										null,
										paramnames,
										out binderState);
				if (TheMethod == null)
					throw new TargetInvocationException(new ArgumentNullException());
				
				return TheMethod.Invoke (o, args);		
			}
		}
		[MonoTODO]
		[System.Diagnostics.DebuggerHiddenAttribute]
		[System.Diagnostics.DebuggerStepThroughAttribute]
		public static void LateIndexSetComplex (System.Object o, System.Object[] args, System.String[] paramnames, System.Boolean OptimisticSet, System.Boolean RValueBase) { throw new NotImplementedException (); }
		[MonoTODO]
		[System.Diagnostics.DebuggerStepThroughAttribute]
		[System.Diagnostics.DebuggerHiddenAttribute]
		public static void LateIndexSet (System.Object o, System.Object[] args, System.String[] paramnames) 
		{
			Type objType;
			Object binderState = null;
			Object myValue;

			if (o == null || args == null)
				throw new ArgumentException();
	
			myValue = args[args.Length - 1];
			objType = o.GetType();
			if (objType.IsArray) {
				Array a = (Array) o;
				int[] idxs = new int[args.Length - 1];
				Array.Copy (args, idxs, args.Length -1);
				a.SetValue(myValue, idxs);
			}
			else
			{
				MemberInfo[] defaultMembers = objType.GetDefaultMembers();
				if (defaultMembers == null)
					throw new Exception();  // FIXME: Which exception should we throw?
									
				// We try to find a default method/property/field we can invoke/use
				VBBinder MyBinder = new VBBinder();
				BindingFlags bindingFlags = BindingFlags.IgnoreCase |
						BindingFlags.Instance |
						BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.GetProperty |
						BindingFlags.GetField |
						BindingFlags.InvokeMethod;

				MethodBase[] mb = new MethodBase[defaultMembers.Length];
				try {
					for (int x = 0; x < defaultMembers.Length; x++)
						if (defaultMembers[x].MemberType == MemberTypes.Property)
							mb[x] = ((PropertyInfo) defaultMembers[x]).GetSetMethod();
						else
							mb[x] = (MethodBase) defaultMembers[x];
				} catch (Exception e) {	}
	
				MethodBase TheMethod = MyBinder.BindToMethod (bindingFlags,
										mb,
										ref args,
										null,
										null,
										paramnames,
										out binderState);
				if (TheMethod == null)
					throw new TargetInvocationException(new ArgumentNullException());
				
				TheMethod.Invoke (o, args);	
			}	
		}
		[MonoTODO]
		[System.Diagnostics.DebuggerStepThroughAttribute]
		[System.Diagnostics.DebuggerHiddenAttribute]
		public static void LateCall (System.Object o, System.Type objType, System.String name, System.Object[] args, System.String[] paramnames, System.Boolean[] CopyBack) { throw new NotImplementedException (); }
		// Events
	};
}
