 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
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
namespace Microsoft.VisualBasic.CompilerServices {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class ExceptionUtils {

		public const int E_NOTIMPL = -2147467263;
		public const int E_NOINTERFACE = -2147467262;
		public const int E_ABORT = -2147467260;
		public const int DISP_E_UNKNOWNINTERFACE = -2147352575;
		public const int DISP_E_MEMBERNOTFOUND = -2147352573;
		public const int DISP_E_PARAMNOTFOUND = -2147352572;
		public const int DISP_E_TYPEMISMATCH = -2147352571;
		public const int DISP_E_UNKNOWNNAME = -2147352570;
		public const int DISP_E_NONAMEDARGS = -2147352569;
		public const int DISP_E_BADVARTYPE = -2147352568;
		public const int DISP_E_OVERFLOW = -2147352566;
		public const int DISP_E_BADINDEX = -2147352565;
		public const int DISP_E_UNKNOWNLCID = -2147352564;
		public const int DISP_E_ARRAYISLOCKED = -2147352563;
		public const int DISP_E_BADPARAMCOUNT = -2147352562;
		public const int DISP_E_PARAMNOTOPTIONAL = -2147352561;
		public const int DISP_E_NOTACOLLECTION = -2147352559;
		public const int DISP_E_DIVBYZERO = -2147352558;
		public const int TYPE_E_BUFFERTOOSMALL = -2147319786;
		public const int TYPE_E_INVDATAREAD = -2147319784;
		public const int TYPE_E_UNSUPFORMAT = -2147319783;
		public const int TYPE_E_REGISTRYACCESS = -2147319780;
		public const int TYPE_E_LIBNOTREGISTERED = -2147319779;
		public const int TYPE_E_UNDEFINEDTYPE = -2147319769;
		public const int TYPE_E_QUALIFIEDNAMEDISALLOWED = -2147319768;
		public const int TYPE_E_INVALIDSTATE = -2147319767;
		public const int TYPE_E_WRONGTYPEKIND = -2147319766;
		public const int TYPE_E_ELEMENTNOTFOUND = -2147319765;
		public const int TYPE_E_AMBIGUOUSNAME = -2147319764;
		public const int TYPE_E_NAMECONFLICT = -2147319763;
		public const int TYPE_E_UNKNOWNLCID = -2147319762;
		public const int TYPE_E_DLLFUNCTIONNOTFOUND = -2147319761;
		public const int TYPE_E_BADMODULEKIND = -2147317571;
		public const int TYPE_E_SIZETOOBIG = -2147317563;
		public const int TYPE_E_TYPEMISMATCH = -2147316576;
		public const int TYPE_E_OUTOFBOUNDS = -2147316575;
		public const int TYPE_E_IOERROR = -2147316574;
		public const int TYPE_E_CANTCREATETMPFILE = -2147316573;
		public const int TYPE_E_CANTLOADLIBRARY = -2147312566;
		public const int TYPE_E_INCONSISTENTPROPFUNCS = -2147312509;
		public const int TYPE_E_CIRCULARTYPE = -2147312508;
		public const int STG_E_INVALIDFUNCTION = -2147287039;
		public const int STG_E_FILENOTFOUND = -2147287038;
		public const int STG_E_PATHNOTFOUND = -2147287037;
		public const int STG_E_TOOMANYOPENFILES = -2147287036;
		public const int STG_E_ACCESSDENIED = -2147287035;
		public const int STG_E_INVALIDHANDLE = -2147287034;
		public const int STG_E_INSUFFICIENTMEMORY = -2147287032;
		public const int STG_E_NOMOREFILES = -2147287022;
		public const int STG_E_DISKISWRITEPROTECTED = -2147287021;
		public const int STG_E_SEEKERROR = -2147287015;
		public const int STG_E_WRITEFAULT = -2147287011;
		public const int STG_E_READFAULT = -2147287010;
		public const int STG_E_SHAREVIOLATION = -2147287008;
		public const int STG_E_LOCKVIOLATION = -2147287007;
		public const int STG_E_FILEALREADYEXISTS = -2147286960;
		public const int STG_E_MEDIUMFULL = -2147286928;
		public const int STG_E_INVALIDHEADER = -2147286789;
		public const int STG_E_INVALIDNAME = -2147286788;
		public const int STG_E_UNKNOWN = -2147286787;
		public const int STG_E_UNIMPLEMENTEDFUNCTION = -2147286786;
		public const int STG_E_INUSE = -2147286784;
		public const int STG_E_NOTCURRENT = -2147286783;
		public const int STG_E_REVERTED = -2147286782;
		public const int STG_E_CANTSAVE = -2147286781;
		public const int STG_E_OLDFORMAT = -2147286780;
		public const int STG_E_OLDDLL = -2147286779;
		public const int STG_E_SHAREREQUIRED = -2147286778;
		public const int STG_E_NOTFILEBASEDSTORAGE = -2147286777;
		public const int STG_E_EXTANTMARSHALLINGS = -2147286776;
		public const int CLASS_E_NOTLICENSED = -2147221230;
		public const int REGDB_E_CLASSNOTREG = -2147221164;
		public const int MK_E_UNAVAILABLE = -2147221021;
		public const int MK_E_INVALIDEXTENSION = -2147221018;
		public const int MK_E_CANTOPENFILE = -2147221014;
		public const int CO_E_CLASSSTRING = -2147221005;
		public const int CO_E_APPNOTFOUND = -2147221003;
		public const int CO_E_APPDIDNTREG = -2147220994;
		public const int E_ACCESSDENIED = -2147024891;
		public const int E_OUTOFMEMORY = -2147024882;
		public const int E_INVALIDARG = -2147024809;
		public const int CO_E_SERVER_EXEC_FAILURE = -2146959355;

		//TODO: uncomment and convert
		//    private static HashMap _dotNetToVBMap = new HashMap();
		//    
		//    private static HashMap _vbToDotNetClassMap = new HashMap();
		//    
		//    static
		//    { 
		//        //initializing the hash map that maps vb exception number to .NET exception
		//        //classes
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.ReturnWOGoSub),InvalidOperationException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.ResumeWOErr),InvalidOperationException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.CantUseNull),InvalidOperationException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DoesntImplementICollection),InvalidOperationException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.IllegalFuncCall),ArgumentException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.NamedArgsNotSupported),ArgumentException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.NamedParamNotFound),ArgumentException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.ParameterNotOptional),ArgumentException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.OLENoPropOrMethod),MissingMemberException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.Overflow),OverflowException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.OutOfMemory),OutOfMemoryException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.OutOfStrSpace),OutOfMemoryException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.OutOfBounds),IndexOutOfRangeException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DivByZero),DivideByZeroException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.TypeMismatch),InvalidCastException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.OutOfStack),StackOverflowException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DLLLoadErr),TypeLoadException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.FileNotFound),FileNotFoundException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.EndOfFile),EndOfStreamException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.BadFileNameOrNumber),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.BadFileMode),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.IOError),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.FileAlreadyExists),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.FileAlreadyOpen),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.BadRecordLen),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DiskFull),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.BadRecordNum),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.TooManyFiles),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DevUnavailable),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.PermissionDenied),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DiskNotReady),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.DifferentDrive),IOException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.PathFileAccess),IOException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.PathNotFound),FileNotFoundException.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.OLEFileNotFound),FileNotFoundException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.ObjNotSet),NullReferenceException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.PropertyNotFound),MissingFieldException.class);
		//        
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.CantCreateObject),system.Exception.class);
		//        _vbToDotNetClassMap.put(new Integer(vbErrors.ServerNotFound),system.Exception.class);
		//        
		//        //initializing the hash map that maps .NET exception number to VB exception
		//         //number      
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.E_NOTIMPL),new Integer(vbErrors.NotYetImplemented));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.E_NOINTERFACE),new Integer(vbErrors.OLENotSupported));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.E_ABORT),new Integer(vbErrors.Abort));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_UNKNOWNINTERFACE),new Integer(vbErrors.OLENoPropOrMethod));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_MEMBERNOTFOUND),new Integer(vbErrors.OLENoPropOrMethod));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_PARAMNOTFOUND),new Integer(vbErrors.NamedParamNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_TYPEMISMATCH),new Integer(vbErrors.TypeMismatch));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_UNKNOWNNAME),new Integer(vbErrors.OLENoPropOrMethod));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_NONAMEDARGS),new Integer(vbErrors.NamedArgsNotSupported));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_BADVARTYPE),new Integer(vbErrors.InvalidTypeLibVariable));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_OVERFLOW),new Integer(vbErrors.Overflow));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_BADINDEX),new Integer(vbErrors.OutOfBounds));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_UNKNOWNLCID),new Integer(vbErrors.LocaleSettingNotSupported));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_ARRAYISLOCKED),new Integer(vbErrors.ArrayLocked));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_BADPARAMCOUNT),new Integer(vbErrors.FuncArityMismatch));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_NOTACOLLECTION),new Integer(vbErrors.NotEnum));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.DISP_E_DIVBYZERO),new Integer(vbErrors.DivByZero));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_BUFFERTOOSMALL),new Integer(vbErrors.BufferTooSmall));
		//        _dotNetToVBMap.put(new Integer(-2147319785),new Integer(vbErrors.IdentNotMember));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_INVDATAREAD),new Integer(vbErrors.InvDataRead));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_UNSUPFORMAT),new Integer(vbErrors.UnsupFormat));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_REGISTRYACCESS),new Integer(vbErrors.RegistryAccess));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_LIBNOTREGISTERED),new Integer(vbErrors.LibNotRegistered));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_UNDEFINEDTYPE),new Integer(vbErrors.UndefinedType));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_QUALIFIEDNAMEDISALLOWED),new Integer(vbErrors.QualifiedNameDisallowed));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_INVALIDSTATE),new Integer(vbErrors.InvalidState));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_WRONGTYPEKIND),new Integer(vbErrors.WrongTypeKind));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_ELEMENTNOTFOUND),new Integer(vbErrors.ElementNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_AMBIGUOUSNAME),new Integer(vbErrors.AmbiguousName));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_NAMECONFLICT),new Integer(vbErrors.ModNameConflict));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_UNKNOWNLCID),new Integer(vbErrors.UnknownLcid));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_DLLFUNCTIONNOTFOUND),new Integer(vbErrors.InvalidDllFunctionName));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_BADMODULEKIND),new Integer(vbErrors.BadModuleKind));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_SIZETOOBIG),new Integer(vbErrors.SizeTooBig));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_TYPEMISMATCH),new Integer(vbErrors.TypeMismatch));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_OUTOFBOUNDS),new Integer(vbErrors.OutOfBounds));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_IOERROR),new Integer(vbErrors.TypeMismatch));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_CANTCREATETMPFILE),new Integer(vbErrors.CantCreateTmpFile));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_CANTLOADLIBRARY),new Integer(vbErrors.DLLLoadErr));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_INCONSISTENTPROPFUNCS),new Integer(vbErrors.InconsistentPropFuncs));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.TYPE_E_CIRCULARTYPE),new Integer(vbErrors.CircularType));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_INVALIDFUNCTION),new Integer(vbErrors.BadFunctionId));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_FILENOTFOUND),new Integer(vbErrors.FileNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_PATHNOTFOUND),new Integer(vbErrors.PathNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_TOOMANYOPENFILES),new Integer(vbErrors.TooManyFiles));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_ACCESSDENIED),new Integer(vbErrors.PermissionDenied));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_INVALIDHANDLE),new Integer(vbErrors.ReadFault));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_INSUFFICIENTMEMORY),new Integer(vbErrors.OutOfMemory));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_NOMOREFILES),new Integer(vbErrors.TooManyFiles));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_DISKISWRITEPROTECTED),new Integer(vbErrors.PermissionDenied));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_SEEKERROR),new Integer(vbErrors.SeekErr));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_WRITEFAULT),new Integer(vbErrors.ReadFault));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_READFAULT),new Integer(vbErrors.ReadFault));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_SHAREVIOLATION),new Integer(vbErrors.PathFileAccess));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_LOCKVIOLATION),new Integer(vbErrors.PermissionDenied));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_FILEALREADYEXISTS),new Integer(vbErrors.PathFileAccess));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_MEDIUMFULL),new Integer(vbErrors.DiskFull));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_INVALIDHEADER),new Integer(vbErrors.InvDataRead));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_INVALIDNAME),new Integer(vbErrors.FileNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_UNKNOWN),new Integer(vbErrors.InvDataRead));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_UNIMPLEMENTEDFUNCTION),new Integer(vbErrors.NotYetImplemented));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_INUSE),new Integer(vbErrors.PermissionDenied));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_NOTCURRENT),new Integer(vbErrors.PermissionDenied));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_REVERTED),new Integer(vbErrors.WriteFault));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_CANTSAVE),new Integer(vbErrors.IOError));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_OLDFORMAT),new Integer(vbErrors.UnsupFormat));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_OLDDLL),new Integer(vbErrors.UnsupFormat));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_SHAREREQUIRED),new Integer(32789));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_NOTFILEBASEDSTORAGE),new Integer(vbErrors.UnsupFormat));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.STG_E_EXTANTMARSHALLINGS),new Integer(vbErrors.UnsupFormat));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.CLASS_E_NOTLICENSED),new Integer(vbErrors.CantCreateObject));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.REGDB_E_CLASSNOTREG),new Integer(vbErrors.CantCreateObject));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.MK_E_UNAVAILABLE),new Integer(vbErrors.CantCreateObject));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.MK_E_INVALIDEXTENSION),new Integer(vbErrors.OLEFileNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.MK_E_CANTOPENFILE),new Integer(vbErrors.OLEFileNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.CO_E_CLASSSTRING),new Integer(vbErrors.CantCreateObject));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.CO_E_APPNOTFOUND),new Integer(vbErrors.CantCreateObject));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.CO_E_APPDIDNTREG),new Integer(vbErrors.CantCreateObject));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.E_ACCESSDENIED),new Integer(vbErrors.PermissionDenied));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.E_OUTOFMEMORY),new Integer(vbErrors.OutOfMemory));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.E_INVALIDARG),new Integer(vbErrors.IllegalFuncCall));
		//        _dotNetToVBMap.put(new Integer(-2147023174),new Integer(vbErrors.ServerNotFound));
		//        _dotNetToVBMap.put(new Integer(ExceptionUtils.CO_E_SERVER_EXEC_FAILURE),new Integer(vbErrors.CantCreateObject));
		//
		//        //new .NET values 
		//        _dotNetToVBMap.put(new Integer(-2146233080),new Integer(vbErrors.OutOfBounds));//IndexOutOfRangeException
		//        _dotNetToVBMap.put(new Integer(-2146233065),new Integer(vbErrors.OutOfBounds));//RankException
		//        _dotNetToVBMap.put(new Integer(-2147467261),new Integer(vbErrors.ObjNotSet));//NullPointerException
		//        _dotNetToVBMap.put(new Integer(-2146233066),new Integer(vbErrors.Overflow));//OverflowException
		//        _dotNetToVBMap.put(new Integer(-2146233048),new Integer(vbErrors.Overflow));//NotFiniteNumberException
		//        _dotNetToVBMap.put(new Integer(-2146233067),new Integer(vbErrors.TypeMismatch));//NotSupportedException
		//        _dotNetToVBMap.put(new Integer(-2146233070),new Integer(vbErrors.OLENoPropOrMethod));//MissingMemberException
		//        _dotNetToVBMap.put(new Integer(-2146233053),new Integer(vbErrors.InvalidDllFunctionName));//EntryPointNotFoundException
		//        _dotNetToVBMap.put(new Integer(-2146233054),new Integer(vbErrors.CantCreateObject));//TypeLoadException
		//        _dotNetToVBMap.put(new Integer(-2146233033),new Integer(vbErrors.TypeMismatch));//FormatException
		//        _dotNetToVBMap.put(new Integer(-2147024893),new Integer(vbErrors.PathNotFound));//DirectoryNotFoundException
		//        _dotNetToVBMap.put(new Integer(-2146232800),new Integer(vbErrors.IOError));//IOException
		//        _dotNetToVBMap.put(new Integer(-2147024894),new Integer(vbErrors.FileNotFound));//FileNotFoundException     
		//    }
    
    
		public static int getVBFromDotNet(int number) {
			return (int)-2147467263;
			//return (int)_dotNetToVBMap.get(number);//uncomment and convert
		}
    
		public static int fromDotNetToVB(int number) {
		
			if (number > 0)
				return 0;

			if ((number & 536805376) == 655360)
				return number & 65535;

			//TODO:  uncomment and convert
			//Integer vbNum =  (Integer)_dotNetToVBMap.get(new Integer(number)); //Mainsoft code  

			// if (vbNum != null)
			//     return vbNum.intValue();
			// else
			return number;

		}

		//TODO:convert
		/**
			 * This method returns relevant .NET exception according to given number, 
			 * with given description as message.
			 * @param number
			 * @param description
			 * @param VBDefinedError
			 * @return java.lang.Exception
			 */
		//TODO: is it correct to replace Mainsoft's ClrBoolean with bool?
		//public static Exception BuildException(int number, String description, ClrBoolean VBDefinedError) {
		public static Exception BuildException(int number, string description, bool VBDefinedError) {
			if (number != 0) {
				//TODO:convert
				//VBDefinedError.setValue(ClrBoolean.True);
				//Class exceptionClass = (Class)_vbToDotNetClassMap.get(new Integer(number));
				//if (exceptionClass != null)
				//{
				//    try
				//    {
				//        Constructor ctor = exceptionClass.getConstructor(new Class[] {string.class});
				//        return (Exception)ctor.newInstance(new object[]{description});                    
				//    }
				//    catch (Exception e)
				//    {
				//        //Here should be Tracing !!!
				//        System.out.println("Failed to initiate exception in ExceptionUtils.BuildException with number =" + number);
				//        e.printStackTrace();
				//    }
				//}
		
				//VBDefinedError.setValue(ClrBoolean.False);
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
		
		public static void ThrowException1(int hr, string param1) {
			string str = "";
			//TODO: delete or convert???

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
		public static Exception VbMakeException(int hr) {

			string str = "";

			//TODO:convert
			//if (hr > 0 && hr <= 65535) {
			//	str = VBUtils.GetResourceString(hr);
			//	if (str == null)
			//		str  = VBUtils.GetResourceString(95);
			//}    
			str = "VB error #95 ";
			return VbMakeExceptionEx(hr, str);
		}

		public static Exception VbMakeException(Exception ex, int hr) {
			//TODO: convert
			//Information.Err().SetUnmappedError(hr);
			return ex;
		}

		/**
		 * This method returns .NET exception that relevant for the given number.
		 * @param Number
		 * @param sMsg
		 * @return java.lang.Exception
		 */
		//TODO: convert
		public static Exception VbMakeExceptionEx(int number, string msg) {
			//ClrBoolean bool = new ClrBoolean();
			bool Bool = false; //new ClrBoolean();
			Exception exp;
		
			exp = ExceptionUtils.BuildException(number, msg, Bool);
				//TODO: convert
				// if (bool.getValue() == ClrBoolean.True)
				//     Information.Err().SetUnmappedError(number);
				return exp;
		}

	}
}
