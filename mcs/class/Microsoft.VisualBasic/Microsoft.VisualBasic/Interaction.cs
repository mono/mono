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

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
//using Windows.Drawing;
//using System.Windows.Forms;

using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Interaction {
		// Declarations
		// Constructors
		// Properties
		// Methods
		//[MonoTODO]
		public static System.Int32 Shell (System.String Pathname, 
						  [System.Runtime.InteropServices.Optional] 
						  [System.ComponentModel.DefaultValue(2)] Microsoft.VisualBasic.AppWinStyle Style, 
						  [System.Runtime.InteropServices.Optional] 
						  [System.ComponentModel.DefaultValue(false)] System.Boolean Wait, 
						  [System.Runtime.InteropServices.Optional] 
						  [System.ComponentModel.DefaultValue(-1)] System.Int32 Timeout)
		{ 
			Process prcs = new Process();

			ProcessWindowStyle PWinStyle = 0;
			switch (Style){
			case AppWinStyle.Hide:
				PWinStyle = ProcessWindowStyle.Hidden;
				break;
			case AppWinStyle.NormalFocus:
				PWinStyle = ProcessWindowStyle.Normal;
				break;
			case AppWinStyle.MinimizedFocus:
				PWinStyle = ProcessWindowStyle.Minimized;
				break;
			case AppWinStyle.MaximizedFocus:
				PWinStyle = ProcessWindowStyle.Maximized;
				break;
			case AppWinStyle.NormalNoFocus:
				PWinStyle = ProcessWindowStyle.Normal; //ToDo: no focus is not set
				break;
			case AppWinStyle.MinimizedNoFocus:
				PWinStyle = ProcessWindowStyle.Minimized; //ToDo: no focus is not set
				break;
			}

			prcs.StartInfo.FileName = Pathname;
			prcs.StartInfo.WindowStyle = PWinStyle;

			try	
			{
				if(prcs.Start()) 
				{
					if (Wait)
					{
						if (Timeout == -1)
							prcs.WaitForExit();
						else
							prcs.WaitForExit(Timeout);
					}
					return prcs.Id;
				}
				else
					return 0;
			}
			catch (System.ComponentModel.Win32Exception e){
				throw new System.IO.FileNotFoundException (
									   Utils.GetResourceString(53));
			}
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
			
		[MonoTODO]
		public static System.String InputBox (System.String Prompt, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String Title, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue("")] System.String DefaultResponse, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 XPos, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(-1)] System.Int32 YPos)
		{ 
			throw new NotImplementedException ();
		}
			
		public static System.Object IIf (System.Boolean Expression, System.Object TruePart, System.Object FalsePart)
		{
			return Expression ? TruePart : FalsePart;
		}
			
		public static System.String Partition (System.Int64 number, System.Int64 start, System.Int64 stop, System.Int64 interval)
		{ 
			String stopStr = "";
			String startStr = "";
			long startNumber = 0;
			int spacesCount = 0;
			long endNumber = 0;

			if (start < 0)
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_InvalidValue1", "Start"));
			if (stop <= start)
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_InvalidValue1", "Stop"));
			if (interval < 1)
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_InvalidValue1", "Interval"));

			if (number < start)
				endNumber = start - 1;
			else {
				if (number > stop)
					startNumber = stop + 1;
				else {
					if (interval == 1){
						startNumber = number;
						endNumber = number;
					}
					else {
						endNumber = start-1;
						while (endNumber < number)
							endNumber += interval;
						startNumber = endNumber - interval + 1;

						if (endNumber > stop)
							endNumber = stop;
						if (startNumber < start)
							startNumber = start;
					}
				}
			}
			
			startStr = startNumber.ToString();
			stopStr = endNumber.ToString();

			if (stopStr.Length  > startStr.Length)
				spacesCount = stopStr.Length;
			else
				spacesCount = startStr.Length;
	
			return startStr.PadLeft(spacesCount) + ":" + stopStr.PadRight(spacesCount);
		}
			
		public static System.Object Switch (params System.Object[] VarExpr)
		{ 
			int counter;
			int index;

			if (VarExpr == null)
				return null;

			counter = VarExpr.Length;
			index = 0;

			if (counter % 2 != 0)
				throw new ArgumentException(
							    Utils.GetResourceString("Argument_InvalidValue1", "VarExpr"));

			do {
				if((bool)VarExpr[index])
					return VarExpr[index + 1];
				index += 2;
				counter = counter - 2;
			}
			while (counter > 0);

			return null;
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
		

		public static Object CallByName (Object objRef, String name, CallType userCallType, Object[] args)
		{
			Object retVal = null;
			Type[] argsType = null;


			if(args != null && args.Length != 0) {
				argsType = new Type[args.Length];

				for(int i = 0; i < args.Length; i++) 
					argsType[i] = args[i].GetType();
			}

			Type objType = objRef.GetType();
        
			try
			{
				MethodInfo methodInfo = null;

				if(userCallType == CallType.Method) {
					Console.WriteLine("Method");
					methodInfo = objType.GetMethod(name, argsType);
				}
				else if(userCallType == CallType.Get) {
					Console.WriteLine("GetMethod");
					methodInfo = objType.GetProperty(name).GetGetMethod();
				}
				else if(userCallType == CallType.Set) {

					Console.WriteLine("SetMethod");
					methodInfo = objType.GetProperty(name).GetSetMethod();
				}

				return methodInfo.Invoke(objRef, args);

			}
			catch (Exception exp)
			{
				throw new ArgumentException();
			}

		}


		public static System.Object Choose (System.Double Index, System.Object[] Choice)
		{ 
			int i;

			i = (int) Math.Round(Conversion.Fix(Index) - 1.0);
			if(Choice.Rank != 1) 
				throw new ArgumentException(Utils.GetResourceString("Argument_RankEQOne1", "Choice"));
        
			if(i < 0 || i > Choice.GetUpperBound(0)) 
				return null;
			else
				return Choice[i];
		}


		public static System.String Environ (System.Int32 Expression)
		{ 
			int index = 0;
			Exception e;

			//		Console.WriteLine("Coming Here"+Expression);

			IDictionary envVars = Environment.GetEnvironmentVariables();

			foreach(DictionaryEntry de in envVars) {
				if(++index == Expression) {
					if( (object) de.Value == null)
						return "";
					else
						return String.Concat(de.Key, "=" , de.Value);
				}
			}
			//		Console.WriteLine("Exiting the loop");

			return "";

		}
			
		public static System.String Environ (System.String Expression)
		{ 
			Exception e;
			if (Expression == null) {
				e = ExceptionUtils.VbMakeExceptionEx(5, Utils.GetResourceString("Argument_InvalidValue1", Expression));
				throw e;
			}
			
			string var = Environment.GetEnvironmentVariable (Expression);
			return var != null ? var : "";
		}

		public static void Beep ()
		{ 
			Console.WriteLine("\a");
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
		public static Microsoft.VisualBasic.MsgBoxResult MsgBox (System.Object Prompt, 
									 [System.Runtime.InteropServices.Optional] 
									 [System.ComponentModel.DefaultValue(0)] 
									 Microsoft.VisualBasic.MsgBoxStyle Buttons, 
									 [System.Runtime.InteropServices.Optional] 
									 [System.ComponentModel.DefaultValue(null)] System.Object Title)
		{ 
			throw new NotImplementedException ();
			/*	//MessageButtons msgBoxButtons = 0;
				MessageBoxIcon msgBoxIcon = 0;
				MessageBoxDefaultButton msgBoxDefaultButton = 0;
				MessageBoxOptions msgBoxOptions = 0;
			

				int IconsMask = MsgBoxStyle.Critical | MsgBoxStyle.Question | MsgBoxStyle.Exclamation | MsgBoxStyle.Information;

				int ButtonsMask = MsgBoxStyle.OKOnly |MsgBoxStyle.OKCancel | MsgBoxStyle.AbortRetryIgnore |
				MsgBoxStyle.YesNoCancel |
				MsgBoxStyle.YesNo | MsgBoxStyle.RetryCancel;

				int DefaultButtonMask = MsgBoxStyle.DeafultButton1 | MsgBoxStyle.DefaultButton2 | 
				MsgBoxStyle.DefaultButton3;

				int OptionsMask =  MsgBoxStyle.MsgBoxRight | MsgBoxStyle.MsgBoxRtlReading;


				switch(Buttons & IconMask) {
				case MsgBoxStyle.OKOnly:
				msgBoxButtons = MessageBoxButtons.OK;
				break;

				case MsgBoxStyle.OKCancel:
				msgBoxButtons = MessageBoxButtons.OK;
				break;

				case MsgBoxStyle.AbortRetryIgnore:
				msgBoxButtons = MessageBoxButtons.OKCancel;
				break;

				case MsgBoxStyle.YesNoCancel:
				msgBoxButtons = MessageBoxButtons.YesNoCancel;
				break;

				case MsgBoxStyle.YesNo:
				msgBoxButtons = MessageBoxButtons.YesNo;
				break;

				case MsgBoxStyle.RetryCancel:
				msgBoxButtons = MessageBoxButtons.RetryCancel;
				break;

				default:
				// handle error
				break;
				}



				switch(Buttons & IconMask) {

				case MsgBoxStyle.Critical:
				msgBoxIcon = MessageBoxIcon.Error;
				break;

				case MsgBoxStyle.Question:
				msgBoxIcon = MessageBoxIcon.Question;
				break;

				case MsgBoxStyle.Exclamation:
				msgBoxIcon = MessageBoxIcon.Exclamation;
				break;

				case MsgBoxStyle.Information:
				msgBoxIcon = MessageBoxIcon.Information;
				break;

				default:
				// handle error
				break;
				}

				switch(Buttons & DefaultButtonMask) {
				case MsgBoxStyle.DefaultButton1:
				msgBoxDefaultButton = MessageBoxDefaultButton.Button1;
				break;
				case MsgBoxStyle.DefaultButton2:
				msgBoxDefaultButton = MessageBoxDefaultButton.Button2;
				break;
				case MsgBoxStyle.DefaultButton3:
				msgBoxDefaultButton = MessageBoxDefaultButton.Button3;
				break;
				default:
				//handle error
				break;
				}

				switch(Buttons & OptionsMask) {
				default:
				break;
				

			
				}	*/	
		}
	}
}

