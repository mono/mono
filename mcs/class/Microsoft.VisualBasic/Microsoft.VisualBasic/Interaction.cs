//
// Interaction.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//   Joerg Rosenkranz (JoergR@voelcker.com)
//
// (C) 2002 Chris J Breisch
// (C) 2004 Joerg Rosenkranz
//
using System;

namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Interaction {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static System.Int32 Shell (System.String Pathname, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(2)] Microsoft.VisualBasic.AppWinStyle Style, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(false)] System.Boolean Wait, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 Timeout)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static void AppActivate (System.Int32 ProcessId)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static void AppActivate (System.String Title)
		{ 
			throw new NotImplementedException ();
		}
			
		public static System.String Command ()
		{ 
			string [] args = Environment.GetCommandLineArgs ();

			if (args != null && args.Length > 1) {
				return string.Join (" ", args, 1, args.Length - 1);
			} else {
				return "";
			}
		}
			
		[MonoTODO]
		public static System.String Environ (System.Int32 Expression)
		{ 
			throw new NotImplementedException ();
		}
			
		public static System.String Environ (System.String Expression)
		{ 
			if (Expression == null)
				return "";
			
			string var = Environment.GetEnvironmentVariable (Expression);
			return var != null ? var : "";
		}
			
		[MonoTODO]
		public static void Beep ()
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.String InputBox (System.String Prompt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String Title, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String DefaultResponse, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 XPos, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 YPos)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static Microsoft.VisualBasic.MsgBoxResult MsgBox (System.Object Prompt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(0)] Microsoft.VisualBasic.MsgBoxStyle Buttons, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.Object Title)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.Object CallByName (System.Object ObjectRef, System.String ProcName, Microsoft.VisualBasic.CallType UseCallType, params System.Object[] Args)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.Object Choose (System.Double Index, params System.Object[] Choice)
		{ 
			throw new NotImplementedException ();
		}
			
		public static System.Object IIf (System.Boolean Expression, System.Object TruePart, System.Object FalsePart)
		{
			return Expression ? TruePart : FalsePart;
		}
			
		[MonoTODO]
		public static System.String Partition (System.Int64 Number, System.Int64 Start, System.Int64 Stop, System.Int64 Interval)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.Object Switch (params System.Object[] VarExpr)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static void DeleteSetting (System.String AppName, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Section, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Key)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.String[,] GetAllSettings (System.String AppName, System.String Section)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.String GetSetting (System.String AppName, System.String Section, System.String Key, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String Default)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static void SaveSetting (System.String AppName, System.String Section, System.String Key, System.String Setting)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.Object CreateObject (System.String ProgId, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String ServerName)
		{ 
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public static System.Object GetObject ([System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String PathName, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(null)] System.String Class)
		{ 
			throw new NotImplementedException ();
		}
		
		// Events
	};
}
