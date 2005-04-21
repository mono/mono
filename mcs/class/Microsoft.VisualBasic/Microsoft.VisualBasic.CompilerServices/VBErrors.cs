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


public class VBErrors 
{
	public const int None = 0;
	public const int ReturnWOGoSub = 3;
	public const int IllegalFuncCall = 5;
	public const int Overflow = 6;
	public const int OutOfMemory = 7;
	public const int OutOfBounds = 9;
	public const int ArrayLocked = 10;
	public const int DivByZero = 11;
	public const int TypeMismatch = 13;
	public const int OutOfStrSpace = 14;
	public const int ExprTooComplex = 16;
	public const int CantContinue = 17;
	public const int UserInterrupt = 18;
	public const int ResumeWOErr = 20;
	public const int OutOfStack = 28;
	public const int UNDONE = 29;
	public const int UndefinedProc = 35;
	public const int TooManyClients = 47;
	public const int DLLLoadErr = 48;
	public const int DLLBadCallingConv = 49;
	public const int InternalError = 51;
	public const int BadFileNameOrNumber = 52;
	public const int FileNotFound = 53;
	public const int BadFileMode = 54;
	public const int FileAlreadyOpen = 55;
	public const int IOError = 57;
	public const int FileAlreadyExists = 58;
	public const int BadRecordLen = 59;
	public const int DiskFull = 61;
	public const int EndOfFile = 62;
	public const int BadRecordNum = 63;
	public const int TooManyFiles = 67;
	public const int DevUnavailable = 68;
	public const int PermissionDenied = 70;
	public const int DiskNotReady = 71;
	public const int DifferentDrive = 74;
	public const int PathFileAccess = 75;
	public const int PathNotFound = 76;
	public const int ObjNotSet = 91;
	public const int IllegalFor = 92;
	public const int BadPatStr = 93;
	public const int CantUseNull = 94;
	public const int UserDefined = 95;
	public const int AdviseLimit = 96;
	public const int BadCallToFriendFunction = 97;
	public const int CantPassPrivateObject = 98;
	public const int DLLCallException = 99;
	public const int DoesntImplementICollection = 100;
	public const int Abort = 287;
	public const int InvalidFileFormat = 321;
	public const int CantCreateTmpFile = 322;
	public const int InvalidResourceFormat = 325;
	public const int InvalidPropertyValue = 380;
	public const int InvalidPropertyArrayIndex = 381;
	public const int SetNotSupportedAtRuntime = 382;
	public const int SetNotSupported = 383;
	public const int NeedPropertyArrayIndex = 385;
	public const int SetNotPermitted = 387;
	public const int GetNotSupportedAtRuntime = 393;
	public const int GetNotSupported = 394;
	public const int PropertyNotFound = 422;
	public const int NoSuchControlOrProperty = 423;
	public const int NotObject = 424;
	public const int CantCreateObject = 429;
	public const int OLENotSupported = 430;
	public const int OLEFileNotFound = 432;
	public const int OLENoPropOrMethod = 438;
	public const int OLEAutomationError = 440;
	public const int LostTLB = 442;
	public const int OLENoDefault = 443;
	public const int ActionNotSupported = 445;
	public const int NamedArgsNotSupported = 446;
	public const int LocaleSettingNotSupported = 447;
	public const int NamedParamNotFound = 448;
	public const int ParameterNotOptional = 449;
	public const int FuncArityMismatch = 450;
	public const int NotEnum = 451;
	public const int InvalidOrdinal = 452;
	public const int InvalidDllFunctionName = 453;
	public const int CodeResourceNotFound = 454;
	public const int CodeResourceLockError = 455;
	public const int DuplicateKey = 457;
	public const int InvalidTypeLibVariable = 458;
	public const int ObjDoesNotSupportEvents = 459;
	public const int InvalidClipboardFormat = 460;
	public const int IdentNotMember = 461;
	public const int ServerNotFound = 462;
	public const int ObjNotRegistered = 463;
	public const int InvalidPicture = 481;
	public const int PrinterError = 482;
	public const int CantSaveFileToTemp = 735;
	public const int SearchTextNotFound = 744;
	public const int ReplacementsTooLong = 746;
	public const int LastTrappable = 746;
	public const int NotYetImplemented = 32768;
	public const int SeekErr = 32771;
	public const int ReadFault = 32772;
	public const int WriteFault = 32773;
	public const int BadFunctionId = 32774;
	public const int FileLockViolation = 32775;
	public const int ShareRequired = 32789;
	public const int BufferTooSmall = 32790;
	public const int InvDataRead = 32792;
	public const int UnsupFormat = 32793;
	public const int RegistryAccess = 32796;
	public const int LibNotRegistered = 32797;
	public const int Usage = 32799;
	public const int UndefinedType = 32807;
	public const int QualifiedNameDisallowed = 32808;
	public const int InvalidState = 32809;
	public const int WrongTypeKind = 32810;
	public const int ElementNotFound = 32811;
	public const int AmbiguousName = 32812;
	public const int ModNameConflict = 32813;
	public const int UnknownLcid = 32814;
	public const int BadModuleKind = 35005;
	public const int NoContainingLib = 35009;
	public const int BadTypeId = 35010;
	public const int BadLibId = 35011;
	public const int Eof = 35012;
	public const int SizeTooBig = 35013;
	public const int ExpectedFuncNotModule = 35015;
	public const int ExpectedFuncNotRecord = 35016;
	public const int ExpectedFuncNotProject = 35017;
	public const int ExpectedFuncNotVar = 35018;
	public const int ExpectedTypeNotProj = 35019;
	public const int UnsuitableFuncPropMatch = 35020;
	public const int BrokenLibRef = 35021;
	public const int UnsupportedTypeLibFeature = 35022;
	public const int ModuleAsType = 35024;
	public const int InvalidTypeInfoKind = 35025;
	public const int InvalidTypeLibFunction = 35026;
	public const int OperationNotAllowedInDll = 40035;
	public const int CompileError = 40036;
	public const int CantEvalWatch = 40037;
	public const int MissingVbaTypeLib = 40038;
	public const int UserReset = 40040;
	public const int MissingEndBrack = 40041;
	public const int IncorrectTypeChar = 40042;
	public const int InvalidNumLit = 40043;
	public const int IllegalChar = 40044;
	public const int IdTooLong = 40045;
	public const int StatementTooComplex = 40046;
	public const int ExpectedTokens = 40047;
	public const int InconsistentPropFuncs = 40067;
	public const int CircularType = 40068;
	public const int FileNotFoundWithName = 40243;
	public const int CantFindDllEntryPoint = 59201;
}
