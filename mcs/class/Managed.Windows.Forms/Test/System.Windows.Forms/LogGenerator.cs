using System;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Logger
{
	class LogGenerator
	{
		[STAThread()]
#if INDEVENV
		static int Main2(string[] args)
#else
		static int Main(string[] args)
#endif
		{
			Type type = null;
			System.Text.StringBuilder code = new StringBuilder ();
			bool is_2_0 = false;
			
			try {
				//if (args.Length >= 1 && args [0].ToLower () == "all") {
				//        if (args.Length == 1) {
				//                GenerateAll ();
				//                return 0;
				//        } else if (args.Length == 2) {
				//                GenerateAll (args [1], false);
				//                return 0;
				//        }
				//}
				
				if (args.Length != 2 && args.Length != 3) {
					Console.WriteLine("Must supply at least two arguments: ");
					Console.WriteLine("\t Type to log ('all' to log all overrides and events for all types in System.Windows.Forms.dll)");
					Console.WriteLine("\t What to log [overrides|events|overridesevents]");
					Console.WriteLine("\t [output filename]");
					return 1;
				}
				
				Assembly a = typeof(System.Windows.Forms.Control).Assembly;
				type = a.GetType (args [0]);
				is_2_0 = a.FullName.IndexOf ("2.0") >= 0;
				
				if (type == null)
					throw new Exception (String.Format("Type '{0}' not found.", args[0]));

				code.Append ("// Automatically generated for assembly: " + a.FullName + Environment.NewLine);
				code.Append ("// To regenerate:" + Environment.NewLine);
				code.Append ("// " + (is_2_0 ? "gmcs" : "mcs") + " -r:System.Windows.Forms.dll LogGenerator.cs && mono LogGenerator.exe " + type.FullName + " " + args [1] + " " + (args.Length > 2 ? args [2] : " outfile.cs") + Environment.NewLine);

				if (is_2_0) {
					code.Append ("#if NET_2_0" + Environment.NewLine);
				} else {
					code.Append ("#if !NET_2_0" + Environment.NewLine);
				}
					
				if (args[1] == "overrides" || args[1] == "overridesevents")
				{
					code.Append (override_logger.GenerateLog (type));
				}
				if (args[1] == "events" || args[1] == "overridesevents")
				{
					code.Append (event_logger.GenerateLog (type));
				}

				code.Append ("#endif" + Environment.NewLine);

				if (args.Length > 2) {
					using (System.IO.StreamWriter writer = new System.IO.StreamWriter(args[2], false))
					{
						writer.Write(code);
					}
				} else {
					Console.WriteLine(code);
				}

				return 0;
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				return 1;
			}
		}
	
	}

	class override_logger
	{
		public static string GenerateLog (Type type)
		{
			StringBuilder members = new StringBuilder ();

			string code =
@"
#region {0}OverrideLogger
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.Remoting;
#if NET_2_0
using System.Windows.Forms.Layout;
#endif
using System.Text;

namespace MonoTests.System.Windows.Forms 
{{
	public class {0}OverrideLogger : {2}{0}
	{{
		public StringBuilder Log = new StringBuilder ();
		void ShowLocation(string message)
		{{
			Log.Append (message + Environment.NewLine);
			//Console.WriteLine(DateTime.Now.ToLongTimeString() + "" {2}{0}."" + message);
		}}
		{1}
	}}
#endregion
}}
";

			string method_impl =
@"
		{1} override {2} {0}({3})
		{{
			{4};
			{5};
		}}
";

			string property_impl =
@"
		{1} override {2} {0}
		{{{3}{4}}}

";

			string get_impl =
				@"
			get {{
				{1};
				return base.{0};
			}}
";

			string set_impl =
				@"
			set {{
				{1};
				base.{0} = value;
			}}
";


			foreach (MemberInfo member in type.GetMembers (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
				switch (member.MemberType) {
				case MemberTypes.Constructor:
				case MemberTypes.Event:
				case MemberTypes.Field:
				case MemberTypes.NestedType:
				case MemberTypes.TypeInfo:
				case MemberTypes.Custom:
					continue;
				case MemberTypes.Property:
				case MemberTypes.Method:
					break;
				default:
					continue;
				}

				MethodInfo method = member as MethodInfo;
				PropertyInfo property = member as PropertyInfo;
				string returnType;
				string access;
				string parameters;
				string message = "";
				string membercode;
				string basecall;

				if (method != null) {
					if (!getData (method, out returnType, out access, out parameters, ref message, out basecall, false))
						continue;
					membercode = string.Format (method_impl, method.Name, access, returnType, parameters, message, basecall);

				} else {
					string getstr = "";
					string setstr = "";

					MethodInfo get = (property.CanRead ? property.GetGetMethod () : null);

					if (get == null)
						continue;

					if (!getData (get, out returnType, out access, out parameters, ref message, out basecall, true))
						continue;

					getstr = string.Format (get_impl, property.Name, message);
					setstr = string.Format (set_impl, property.Name, message);

					if (!property.CanRead)
						getstr = "";
					if (!property.CanWrite)
						setstr = "";

					membercode = string.Format (property_impl, property.Name, access, returnType, getstr, setstr);
				}

				members.Append (membercode + "\n");
			}
			code = String.Format (code, type.Name, members.ToString (), "");

			return code;
		}

		static bool getData (MethodInfo method, out string returnType, out string access, out string parameters, ref string message, out string basecall, bool allow_specialname)
		{
			returnType = "";
			access = "";
			parameters = "";
			message = "";
			basecall = "";

			if (method.IsPrivate)
				return false;
			if (method.IsAssembly)
				return false;
				
			if (!method.IsVirtual)
				return false;
			if (method.IsFinal)
				return false;
			if (method.IsSpecialName && !allow_specialname)
				return false;
			//if (method.Name.StartsWith("get_"))
			//	return false;
			//if (method.Name.StartsWith("set_"))
			//	return false;

			if (method.Name == "Finalize")
				return false;
			if (method.Name == "GetLifetimeService")
				return false;

			returnType = method.ReturnType.FullName.Replace ("+", ".");
			returnType = method.ReturnType.Name;//.Replace ("+", ".");
			if (returnType == "Void")
				returnType = "void";

			if (method.IsPublic)
				access = "public";
			else if (method.IsFamilyOrAssembly)
				access = "protected";
			else if (method.IsFamily)
				access = "protected";
			else
				access = "?";

			string msgParams = "";
			string baseParams = "";
			string formatParams = "";
			ParameterInfo [] ps = method.GetParameters ();
			parameters = "";
			for (int i = 0; i < ps.Length; i++) {
				ParameterInfo param = ps [i];

				if (parameters != "") {
					parameters += ", ";
					msgParams += ", ";
					formatParams += ", ";
					baseParams += ", ";
				}
				
				string parameterType;
				
				if (param.ParameterType.IsByRef) {
					parameterType = param.ParameterType.GetElementType ().Name + " ";//sparam.ParameterType.FullName.Replace ("+", ".") + " ";
					baseParams += "ref ";
					parameters += "ref ";
				} else {
					parameterType = param.ParameterType.Name + " ";
				}
								
				parameters += parameterType;

				string name;
				if (param.Name != null && param.Name != "")
					name = param.Name;
				else
					name = "parameter" + (i + 1).ToString ();

				parameters += param.Name + " ";
				msgParams += param.Name;
				baseParams += param.Name;
				formatParams += param.Name + "=<{" + i.ToString () + "}>";
			}
			
			if (!method.IsAbstract) {
				basecall = "base." + method.Name + "(" + baseParams + ");";
				if (returnType != "void")
					basecall = "return " + basecall;
			}
			if (msgParams != "")
				msgParams = ", " + msgParams;
			message = "ShowLocation (string.Format(\"" + method.Name + " (" + formatParams + ") \"" + msgParams + "))";

			return true;
		}
	}

	class event_logger
	{
		public static string GenerateLog (Type type)
		{
			StringBuilder adders = new StringBuilder ();
			StringBuilder handlers = new StringBuilder ();

			string code =
@"
#region {0}EventLogger
using System;
using System.Reflection;
using System.Diagnostics;

namespace MonoTests.System.Windows.Forms 
{{
	public class {0}EventLogger
	{{
		private {3}{0} _obj;

		public {0}EventLogger({3}{0} obj)
		{{
			_obj = obj;
{1}
		}}

		void ShowLocation()
		{{
			MethodBase method = new StackFrame (1, true).GetMethod ();
			Console.WriteLine (DateTime.Now.ToLongTimeString () + "" {0}."" + method.Name.Replace (""_ctrl_"", """"));
		}}
		{2}
	}}
}}
#endregion
";

			string method =
@"
		void _obj_{0} ({1} sender, {2} e)
		{{
			ShowLocation ();
		}}
";

			foreach (EventInfo ev in type.GetEvents ()) {
				string handler;
				string adder;

				ParameterInfo [] ps = ev.EventHandlerType.GetMethod ("Invoke").GetParameters ();
				handler = string.Format (method, ev.Name, ps [0].ParameterType.Name, ps [1].ParameterType.Name);
				adder = "\t\t_obj." + ev.Name + " += new " + ev.EventHandlerType.Name + " (_obj_" + ev.Name + ");";

				adders.Append (adder + Environment.NewLine);
				handlers.Append (handler);
			}
			code = String.Format (code, type.Name, adders.ToString (), handlers.ToString (), "");//type.Namespace + ".");

			return code;
		}


	}
}
