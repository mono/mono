 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */
/**
 * This class allows to map VB6 exception number to the .NET exception.
 */
using System;
using System.Collections;
using System.Reflection;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.VisualBasic.CompilerServices {
	[StandardModule, StructLayout(LayoutKind.Auto), EditorBrowsable(EditorBrowsableState.Never)] 
	sealed public class ExceptionUtils {
		private ExceptionUtils () {}

		internal const int E_NOTIMPL = -2147467263;
		internal const int E_NOINTERFACE = -2147467262;
		internal const int E_ABORT = -2147467260;
		internal const int DISP_E_UNKNOWNINTERFACE = -2147352575;
		internal const int DISP_E_MEMBERNOTFOUND = -2147352573;
		internal const int DISP_E_PARAMNOTFOUND = -2147352572;
		internal const int DISP_E_TYPEMISMATCH = -2147352571;
		internal const int DISP_E_UNKNOWNNAME = -2147352570;
		internal const int DISP_E_NONAMEDARGS = -2147352569;
		internal const int DISP_E_BADVARTYPE = -2147352568;
		internal const int DISP_E_OVERFLOW = -2147352566;
		internal const int DISP_E_BADINDEX = -2147352565;
		internal const int DISP_E_UNKNOWNLCID = -2147352564;
		internal const int DISP_E_ARRAYISLOCKED = -2147352563;
		internal const int DISP_E_BADPARAMCOUNT = -2147352562;
		internal const int DISP_E_PARAMNOTOPTIONAL = -2147352561;
		internal const int DISP_E_NOTACOLLECTION = -2147352559;
		internal const int DISP_E_DIVBYZERO = -2147352558;
		internal const int TYPE_E_BUFFERTOOSMALL = -2147319786;
		internal const int TYPE_E_INVDATAREAD = -2147319784;
		internal const int TYPE_E_UNSUPFORMAT = -2147319783;
		internal const int TYPE_E_REGISTRYACCESS = -2147319780;
		internal const int TYPE_E_LIBNOTREGISTERED = -2147319779;
		internal const int TYPE_E_UNDEFINEDTYPE = -2147319769;
		internal const int TYPE_E_QUALIFIEDNAMEDISALLOWED = -2147319768;
		internal const int TYPE_E_INVALIDSTATE = -2147319767;
		internal const int TYPE_E_WRONGTYPEKIND = -2147319766;
		internal const int TYPE_E_ELEMENTNOTFOUND = -2147319765;
		internal const int TYPE_E_AMBIGUOUSNAME = -2147319764;
		internal const int TYPE_E_NAMECONFLICT = -2147319763;
		internal const int TYPE_E_UNKNOWNLCID = -2147319762;
		internal const int TYPE_E_DLLFUNCTIONNOTFOUND = -2147319761;
		internal const int TYPE_E_BADMODULEKIND = -2147317571;
		internal const int TYPE_E_SIZETOOBIG = -2147317563;
		internal const int TYPE_E_TYPEMISMATCH = -2147316576;
		internal const int TYPE_E_OUTOFBOUNDS = -2147316575;
		internal const int TYPE_E_IOERROR = -2147316574;
		internal const int TYPE_E_CANTCREATETMPFILE = -2147316573;
		internal const int TYPE_E_CANTLOADLIBRARY = -2147312566;
		internal const int TYPE_E_INCONSISTENTPROPFUNCS = -2147312509;
		internal const int TYPE_E_CIRCULARTYPE = -2147312508;
		internal const int STG_E_INVALIDFUNCTION = -2147287039;
		internal const int STG_E_FILENOTFOUND = -2147287038;
		internal const int STG_E_PATHNOTFOUND = -2147287037;
		internal const int STG_E_TOOMANYOPENFILES = -2147287036;
		internal const int STG_E_ACCESSDENIED = -2147287035;
		internal const int STG_E_INVALIDHANDLE = -2147287034;
		internal const int STG_E_INSUFFICIENTMEMORY = -2147287032;
		internal const int STG_E_NOMOREFILES = -2147287022;
		internal const int STG_E_DISKISWRITEPROTECTED = -2147287021;
		internal const int STG_E_SEEKERROR = -2147287015;
		internal const int STG_E_WRITEFAULT = -2147287011;
		internal const int STG_E_READFAULT = -2147287010;
		internal const int STG_E_SHAREVIOLATION = -2147287008;
		internal const int STG_E_LOCKVIOLATION = -2147287007;
		internal const int STG_E_FILEALREADYEXISTS = -2147286960;
		internal const int STG_E_MEDIUMFULL = -2147286928;
		internal const int STG_E_INVALIDHEADER = -2147286789;
		internal const int STG_E_INVALIDNAME = -2147286788;
		internal const int STG_E_UNKNOWN = -2147286787;
		internal const int STG_E_UNIMPLEMENTEDFUNCTION = -2147286786;
		internal const int STG_E_INUSE = -2147286784;
		internal const int STG_E_NOTCURRENT = -2147286783;
		internal const int STG_E_REVERTED = -2147286782;
		internal const int STG_E_CANTSAVE = -2147286781;
		internal const int STG_E_OLDFORMAT = -2147286780;
		internal const int STG_E_OLDDLL = -2147286779;
		internal const int STG_E_SHAREREQUIRED = -2147286778;
		internal const int STG_E_NOTFILEBASEDSTORAGE = -2147286777;
		internal const int STG_E_EXTANTMARSHALLINGS = -2147286776;
		internal const int CLASS_E_NOTLICENSED = -2147221230;
		internal const int REGDB_E_CLASSNOTREG = -2147221164;
		internal const int MK_E_UNAVAILABLE = -2147221021;
		internal const int MK_E_INVALIDEXTENSION = -2147221018;
		internal const int MK_E_CANTOPENFILE = -2147221014;
		internal const int CO_E_CLASSSTRING = -2147221005;
		internal const int CO_E_APPNOTFOUND = -2147221003;
		internal const int CO_E_APPDIDNTREG = -2147220994;
		internal const int E_ACCESSDENIED = -2147024891;
		internal const int E_OUTOFMEMORY = -2147024882;
		internal const int E_INVALIDARG = -2147024809;
		internal const int CO_E_SERVER_EXEC_FAILURE = -2146959355;

		private static Hashtable _dotNetToVBMap = new Hashtable();
		private static Hashtable _vbToDotNetClassMap = new Hashtable();


		static ExceptionUtils() {
			// initializing the hash map that maps vb exception number to .NET exception classes
			_vbToDotNetClassMap.Add(VBErrors.ReturnWOGoSub, typeof(InvalidOperationException));
			_vbToDotNetClassMap.Add(VBErrors.ResumeWOErr, typeof(InvalidOperationException));
			_vbToDotNetClassMap.Add(VBErrors.CantUseNull, typeof(InvalidOperationException));
			_vbToDotNetClassMap.Add(VBErrors.DoesntImplementICollection, typeof(InvalidOperationException));
        
			_vbToDotNetClassMap.Add(VBErrors.IllegalFuncCall, typeof(ArgumentException));
			_vbToDotNetClassMap.Add(VBErrors.NamedArgsNotSupported, typeof(ArgumentException));
			_vbToDotNetClassMap.Add(VBErrors.NamedParamNotFound, typeof(ArgumentException));
			_vbToDotNetClassMap.Add(VBErrors.ParameterNotOptional, typeof(ArgumentException));
        
			_vbToDotNetClassMap.Add(VBErrors.OLENoPropOrMethod, typeof(MissingMemberException));
        
			_vbToDotNetClassMap.Add(VBErrors.Overflow, typeof(OverflowException));
        
			_vbToDotNetClassMap.Add(VBErrors.OutOfMemory, typeof(OutOfMemoryException));
			_vbToDotNetClassMap.Add(VBErrors.OutOfStrSpace, typeof(OutOfMemoryException));
        
			_vbToDotNetClassMap.Add(VBErrors.OutOfBounds, typeof(IndexOutOfRangeException));
        
			_vbToDotNetClassMap.Add(VBErrors.DivByZero, typeof(DivideByZeroException));
        
			_vbToDotNetClassMap.Add(VBErrors.TypeMismatch, typeof(InvalidCastException));
        
			_vbToDotNetClassMap.Add(VBErrors.OutOfStack, typeof(StackOverflowException));
        
			_vbToDotNetClassMap.Add(VBErrors.DLLLoadErr, typeof(TypeLoadException));
        
			_vbToDotNetClassMap.Add(VBErrors.FileNotFound, typeof(FileNotFoundException));
        
			_vbToDotNetClassMap.Add(VBErrors.EndOfFile, typeof(EndOfStreamException));
        
			_vbToDotNetClassMap.Add(VBErrors.BadFileNameOrNumber, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.BadFileMode, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.IOError, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.FileAlreadyExists, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.FileAlreadyOpen, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.BadRecordLen, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.DiskFull, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.BadRecordNum, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.TooManyFiles, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.DevUnavailable, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.PermissionDenied, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.DiskNotReady, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.DifferentDrive, typeof(IOException));
			_vbToDotNetClassMap.Add(VBErrors.PathFileAccess, typeof(IOException));
        
			_vbToDotNetClassMap.Add(VBErrors.PathNotFound, typeof(FileNotFoundException));
			_vbToDotNetClassMap.Add(VBErrors.OLEFileNotFound, typeof(FileNotFoundException));
        
			_vbToDotNetClassMap.Add(VBErrors.ObjNotSet, typeof(NullReferenceException));
        
			_vbToDotNetClassMap.Add(VBErrors.PropertyNotFound, typeof(MissingFieldException));
        
			_vbToDotNetClassMap.Add(VBErrors.CantCreateObject, typeof(Exception));
			_vbToDotNetClassMap.Add(VBErrors.ServerNotFound, typeof(Exception));

			// initializing the hash map that maps .NET exception number to VB exception number
			_dotNetToVBMap.Add(ExceptionUtils.E_NOTIMPL,VBErrors.NotYetImplemented);
			_dotNetToVBMap.Add(ExceptionUtils.E_NOINTERFACE,VBErrors.OLENotSupported);
			_dotNetToVBMap.Add(ExceptionUtils.E_ABORT,VBErrors.Abort);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_UNKNOWNINTERFACE,VBErrors.OLENoPropOrMethod);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_MEMBERNOTFOUND,VBErrors.OLENoPropOrMethod);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_PARAMNOTFOUND,VBErrors.NamedParamNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_TYPEMISMATCH,VBErrors.TypeMismatch);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_UNKNOWNNAME,VBErrors.OLENoPropOrMethod);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_NONAMEDARGS,VBErrors.NamedArgsNotSupported);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_BADVARTYPE,VBErrors.InvalidTypeLibVariable);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_OVERFLOW,VBErrors.Overflow);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_BADINDEX,VBErrors.OutOfBounds);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_UNKNOWNLCID,VBErrors.LocaleSettingNotSupported);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_ARRAYISLOCKED,VBErrors.ArrayLocked);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_BADPARAMCOUNT,VBErrors.FuncArityMismatch);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_NOTACOLLECTION,VBErrors.NotEnum);
			_dotNetToVBMap.Add(ExceptionUtils.DISP_E_DIVBYZERO,VBErrors.DivByZero);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_BUFFERTOOSMALL,VBErrors.BufferTooSmall);
			_dotNetToVBMap.Add(-2147319785,VBErrors.IdentNotMember);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_INVDATAREAD,VBErrors.InvDataRead);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_UNSUPFORMAT,VBErrors.UnsupFormat);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_REGISTRYACCESS,VBErrors.RegistryAccess);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_LIBNOTREGISTERED,VBErrors.LibNotRegistered);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_UNDEFINEDTYPE,VBErrors.UndefinedType);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_QUALIFIEDNAMEDISALLOWED,VBErrors.QualifiedNameDisallowed);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_INVALIDSTATE,VBErrors.InvalidState);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_WRONGTYPEKIND,VBErrors.WrongTypeKind);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_ELEMENTNOTFOUND,VBErrors.ElementNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_AMBIGUOUSNAME,VBErrors.AmbiguousName);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_NAMECONFLICT,VBErrors.ModNameConflict);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_UNKNOWNLCID,VBErrors.UnknownLcid);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_DLLFUNCTIONNOTFOUND,VBErrors.InvalidDllFunctionName);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_BADMODULEKIND,VBErrors.BadModuleKind);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_SIZETOOBIG,VBErrors.SizeTooBig);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_TYPEMISMATCH,VBErrors.TypeMismatch);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_OUTOFBOUNDS,VBErrors.OutOfBounds);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_IOERROR,VBErrors.TypeMismatch);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_CANTCREATETMPFILE,VBErrors.CantCreateTmpFile);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_CANTLOADLIBRARY,VBErrors.DLLLoadErr);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_INCONSISTENTPROPFUNCS,VBErrors.InconsistentPropFuncs);
			_dotNetToVBMap.Add(ExceptionUtils.TYPE_E_CIRCULARTYPE,VBErrors.CircularType);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_INVALIDFUNCTION,VBErrors.BadFunctionId);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_FILENOTFOUND,VBErrors.FileNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_PATHNOTFOUND,VBErrors.PathNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_TOOMANYOPENFILES,VBErrors.TooManyFiles);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_ACCESSDENIED,VBErrors.PermissionDenied);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_INVALIDHANDLE,VBErrors.ReadFault);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_INSUFFICIENTMEMORY,VBErrors.OutOfMemory);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_NOMOREFILES,VBErrors.TooManyFiles);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_DISKISWRITEPROTECTED,VBErrors.PermissionDenied);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_SEEKERROR,VBErrors.SeekErr);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_WRITEFAULT,VBErrors.ReadFault);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_READFAULT,VBErrors.ReadFault);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_SHAREVIOLATION,VBErrors.PathFileAccess);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_LOCKVIOLATION,VBErrors.PermissionDenied);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_FILEALREADYEXISTS,VBErrors.PathFileAccess);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_MEDIUMFULL,VBErrors.DiskFull);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_INVALIDHEADER,VBErrors.InvDataRead);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_INVALIDNAME,VBErrors.FileNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_UNKNOWN,VBErrors.InvDataRead);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_UNIMPLEMENTEDFUNCTION,VBErrors.NotYetImplemented);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_INUSE,VBErrors.PermissionDenied);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_NOTCURRENT,VBErrors.PermissionDenied);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_REVERTED,VBErrors.WriteFault);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_CANTSAVE,VBErrors.IOError);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_OLDFORMAT,VBErrors.UnsupFormat);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_OLDDLL,VBErrors.UnsupFormat);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_SHAREREQUIRED,32789);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_NOTFILEBASEDSTORAGE,VBErrors.UnsupFormat);
			_dotNetToVBMap.Add(ExceptionUtils.STG_E_EXTANTMARSHALLINGS,VBErrors.UnsupFormat);
			_dotNetToVBMap.Add(ExceptionUtils.CLASS_E_NOTLICENSED,VBErrors.CantCreateObject);
			_dotNetToVBMap.Add(ExceptionUtils.REGDB_E_CLASSNOTREG,VBErrors.CantCreateObject);
			_dotNetToVBMap.Add(ExceptionUtils.MK_E_UNAVAILABLE,VBErrors.CantCreateObject);
			_dotNetToVBMap.Add(ExceptionUtils.MK_E_INVALIDEXTENSION,VBErrors.OLEFileNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.MK_E_CANTOPENFILE,VBErrors.OLEFileNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.CO_E_CLASSSTRING,VBErrors.CantCreateObject);
			_dotNetToVBMap.Add(ExceptionUtils.CO_E_APPNOTFOUND,VBErrors.CantCreateObject);
			_dotNetToVBMap.Add(ExceptionUtils.CO_E_APPDIDNTREG,VBErrors.CantCreateObject);
			_dotNetToVBMap.Add(ExceptionUtils.E_ACCESSDENIED,VBErrors.PermissionDenied);
			_dotNetToVBMap.Add(ExceptionUtils.E_OUTOFMEMORY,VBErrors.OutOfMemory);
			_dotNetToVBMap.Add(ExceptionUtils.E_INVALIDARG,VBErrors.IllegalFuncCall);
			_dotNetToVBMap.Add(-2147023174,VBErrors.ServerNotFound);
			_dotNetToVBMap.Add(ExceptionUtils.CO_E_SERVER_EXEC_FAILURE,VBErrors.CantCreateObject);

			// new .NET values
			_dotNetToVBMap.Add(-2146233080,VBErrors.OutOfBounds);//IndexOutOfRangeException
			_dotNetToVBMap.Add(-2146233065,VBErrors.OutOfBounds);//RankException
			_dotNetToVBMap.Add(-2147467261,VBErrors.ObjNotSet);//NullPointerException
			_dotNetToVBMap.Add(-2146233066,VBErrors.Overflow);//OverflowException
			_dotNetToVBMap.Add(-2146233048,VBErrors.Overflow);//NotFiniteNumberException
			_dotNetToVBMap.Add(-2146233067,VBErrors.TypeMismatch);//NotSupportedException
			_dotNetToVBMap.Add(-2146233070,VBErrors.OLENoPropOrMethod);//MissingMemberException
			_dotNetToVBMap.Add(-2146233053,VBErrors.InvalidDllFunctionName);//EntryPointNotFoundException
			_dotNetToVBMap.Add(-2146233054,VBErrors.CantCreateObject);//TypeLoadException
			_dotNetToVBMap.Add(-2146233033,VBErrors.TypeMismatch);//FormatException
			_dotNetToVBMap.Add(-2147024893,VBErrors.PathNotFound);//DirectoryNotFoundException
			_dotNetToVBMap.Add(-2146232800,VBErrors.IOError);//IOException
			_dotNetToVBMap.Add(-2147024894,VBErrors.FileNotFound);//FileNotFoundException     
		}

		internal static int getVBFromDotNet(int number) 
		{
			return (int) _dotNetToVBMap[number];
		}

		internal static int fromDotNetToVB(int number) 
		{
			if (number > 0)
				return 0;

			if ((number & 536805376) == 655360)
				return number & 65535;


			if(_dotNetToVBMap.Contains (number))
				return (int) _dotNetToVBMap[number];
			else
				return number;
		}

		internal static Exception BuildException(int number, string description, ref bool VBDefinedError) 
		{
			if (number != 0) {
				VBDefinedError = true;

				Type exceptionType = (Type) _vbToDotNetClassMap[number];

				if (exceptionType != null)
				{
					try
					{
						ConstructorInfo ci = exceptionType.GetConstructor(new Type [] {typeof(string)});
						return (Exception)ci.Invoke(new object[]{description});                    
					}
					catch (Exception e)
					{
						//Here should be Tracing !!!
						// Console.WriteLine("Failed to initiate exception in ExceptionUtils.BuildException with number =" + number);
						// e.printStackTrace();
					}
				}
	
				VBDefinedError = false;
				return new Exception(description);
			}
			return null;
		}




		/**
		 * This method builds description string from resource file, replace given parameter
		 * in the description string and throws .NET exception according to the given number
		 * @param hr number of VB exception
		 * @param param1 parameter of message string
		 */
		
		internal static void ThrowException1(int hr, string param1) {
			//TODO: delete or convert???
			//string str = "";

			//			if (hr > 0 && hr <= 65535)
			//			
			//				//str = VBUtils.GetResourceString(hr, param1);
			//
			//				throw VbMakeExceptionEx(hr, str);
		}

		/**
		 * This method retrives exception message from the resource file and returns
		 * .NET exception that relevent to given number.
		 * @param hr
		 * @return java.lang.Exception
		 */
		internal static Exception VbMakeException(int hr) 
		{
			string str = "";

			if (hr > 0 && hr <= 65535) {
				str = VBUtils.GetResourceString(hr);
				if (str == null)
					str  = VBUtils.GetResourceString(95);
			}    

			return VbMakeExceptionEx(hr, str);
		}

		internal static Exception VbMakeException(Exception ex, int hr)
		{
			Information.Err().SetUnmappedError(hr);
			return ex;
		}

		/**
		 * This method returns .NET exception that relevant for the given number.
		 * @param Number
		 * @param sMsg
		 * @return java.lang.Exception
		 */

		internal static Exception VbMakeExceptionEx(int number, string msg) {
			bool Bool = false; 
			Exception exp;
		
			exp = ExceptionUtils.BuildException(number, msg, ref Bool);
			if(Bool == true)
				Information.Err().SetUnmappedError(number);

			return exp;
		}

	}
}
