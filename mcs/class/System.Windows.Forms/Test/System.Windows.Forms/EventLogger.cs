//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//      Rolf Bjarne Kvinge  (RKvinge@novell.com)
//

using System;
using System.Text;
using System.IO;
using NUnit.Framework;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace MonoTests.System.Windows.Forms
{
	public class EventLogger
	{
		public class EventLog : ArrayList
		{
			public bool PrintAdds = false;
			
			new public int Add (object obj)
			{
				if (PrintAdds)
					Console.WriteLine ("{1} EventLog: {0}", obj, DateTime.Now.ToLongTimeString ());
				return base.Add (obj);
			}
		}
	
		private EventLog log;
		private object instance;

		public bool PrintAdds {
			get { return log.PrintAdds; }
			set { log.PrintAdds = value; }
		}

		// Tests if all the names in Names are in log with the order given in Names.
		public bool ContainsEventsOrdered (params string [] Names) 
		{
			if (Names.Length == 0)
				return true;
		
			int n = 0;
			for (int i = 0; i < log.Count; i++) {
				if ((string) log [i] == Names [n]) {
					n++;
					if (n == Names.Length)
						return true;
				}
			}
			
			if (n == Names.Length) {
				return true;
			} else {
				Console.WriteLine ("ContainsEventsOrdered: logged events '" + EventsJoined () + "' didn't match correct events '" + string.Join (";", Names) + "'");
				return false;
			}
		}
		
		public int CountEvents (string Name)
		{
			int count = 0;
			foreach (string str in log) {
				if (Name.Equals (str)) {
					count++;	
				}
			}
			return count;
		}
		
		public bool EventRaised (string Name) 
		{
			return log.Contains (Name);
		}
		
		public int EventsRaised {
			get {
				return log.Count;
			}
		}

		public string EventsJoined ()
		{
			return EventsJoined (";");
		}
		
		public string EventsJoined (string separator)
		{
			return string.Join (";", ToArray ());
		}
		
		public void Clear ()
		{
			log.Clear ();
		}
		
		public string [] ToArray ()
		{
			string [] result = new string [log.Count];
			log.CopyTo (result);
			return result;
		}
		
		public EventLogger (object item)
		{
			if (item == null) {
				throw new ArgumentNullException ("item");
			}

			log = new EventLog ();
			
			Type itemType = item.GetType ();
			AssemblyName name = new AssemblyName ();
			name.Name = "EventLoggerAssembly";
			AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (name, AssemblyBuilderAccess.RunAndSave);
			ModuleBuilder module = assembly.DefineDynamicModule ("EventLoggerAssembly", "EventLoggerAssembly.dll");
			
			Type ListType = log.GetType ();
			
			TypeBuilder logType = module.DefineType ("Logger");
			FieldBuilder logField = logType.DefineField ("log", ListType, FieldAttributes.Public);
			ConstructorBuilder logCtor = logType.DefineConstructor (MethodAttributes.Public, CallingConventions.HasThis, new Type [] {ListType, itemType});
			logCtor.DefineParameter (1, ParameterAttributes.None, "test");
			logCtor.DefineParameter (2, ParameterAttributes.None, "obj");
			ILGenerator logIL = logCtor.GetILGenerator ();

			logIL.Emit (OpCodes.Ldarg_0);
			logIL.Emit (OpCodes.Call, typeof (object).GetConstructor (Type.EmptyTypes));

			logIL.Emit (OpCodes.Ldarg_0);
			logIL.Emit (OpCodes.Ldarg_1);
			logIL.Emit (OpCodes.Stfld, logField);

			
			foreach (EventInfo Event in itemType.GetEvents ()) {
				ILGenerator il;

				MethodInfo invoke = Event.EventHandlerType.GetMethod ("Invoke");
				MethodBuilder method = logType.DefineMethod (Event.Name, MethodAttributes.Public, null, new Type [] { invoke.GetParameters () [0].ParameterType, invoke.GetParameters () [1].ParameterType });
				method.DefineParameter (1, ParameterAttributes.None, "test");
				method.DefineParameter (2, ParameterAttributes.None, "test2");
				il = method.GetILGenerator ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldfld, logField);
				il.Emit (OpCodes.Ldstr, Event.Name);
				il.Emit (OpCodes.Callvirt, ListType.GetMethod ("Add"));
				il.Emit (OpCodes.Pop);
				il.Emit (OpCodes.Ret);
				
				logIL.Emit (OpCodes.Ldarg_2);
				logIL.Emit (OpCodes.Ldarg_0);
				logIL.Emit (OpCodes.Dup);
				logIL.Emit (OpCodes.Ldvirtftn, method);
				logIL.Emit (OpCodes.Newobj, Event.EventHandlerType.GetConstructor (new Type [] {typeof(object), typeof(IntPtr)}));
				logIL.Emit (OpCodes.Call, Event.GetAddMethod ());
			}

			logIL.Emit (OpCodes.Ret);		
			Type builtLogType = logType.CreateType ();
			
			instance = builtLogType.GetConstructors () [0].Invoke (new object [] { log, item });
			TestHelper.RemoveWarning (instance);
			
			//assembly.Save ("EventLoggerAssembly.dll");
		}
	}
}
