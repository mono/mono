//
// Constants.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class Constants {
		// Declarations
		public const System.Int32 vbObjectError = (System.Int32)(-2147221504);
		public const System.String vbCrLf = "\n\r";
		public const System.String vbNewLine = "\n\r";
		public const System.String vbCr = "\n";
		public const System.String vbLf = "\r";
		public const System.String vbBack = "\b";
		public const System.String vbFormFeed = "\f";
		public const System.String vbTab = "\t";
		public const System.String vbVerticalTab = "\v";
		public const System.String vbNullChar = "\0";
		public const System.String vbNullString = (System.String)null;
		public const Microsoft.VisualBasic.AppWinStyle vbHide = (Microsoft.VisualBasic.AppWinStyle)(0);
		public const Microsoft.VisualBasic.AppWinStyle vbNormalFocus = (Microsoft.VisualBasic.AppWinStyle)(1);
		public const Microsoft.VisualBasic.AppWinStyle vbMinimizedFocus = (Microsoft.VisualBasic.AppWinStyle)(2);
		public const Microsoft.VisualBasic.AppWinStyle vbMaximizedFocus = (Microsoft.VisualBasic.AppWinStyle)(3);
		public const Microsoft.VisualBasic.AppWinStyle vbNormalNoFocus = (Microsoft.VisualBasic.AppWinStyle)(4);
		public const Microsoft.VisualBasic.AppWinStyle vbMinimizedNoFocus = (Microsoft.VisualBasic.AppWinStyle)(6);
		public const Microsoft.VisualBasic.CallType vbMethod = (Microsoft.VisualBasic.CallType)(1);
		public const Microsoft.VisualBasic.CallType vbGet = (Microsoft.VisualBasic.CallType)(2);
		public const Microsoft.VisualBasic.CallType vbLet = (Microsoft.VisualBasic.CallType)(4);
		public const Microsoft.VisualBasic.CallType vbSet = (Microsoft.VisualBasic.CallType)(8);
		public const Microsoft.VisualBasic.CompareMethod vbBinaryCompare = (Microsoft.VisualBasic.CompareMethod)(0);
		public const Microsoft.VisualBasic.CompareMethod vbTextCompare = (Microsoft.VisualBasic.CompareMethod)(1);
		public const Microsoft.VisualBasic.DateFormat vbGeneralDate = (Microsoft.VisualBasic.DateFormat)(0);
		public const Microsoft.VisualBasic.DateFormat vbLongDate = (Microsoft.VisualBasic.DateFormat)(1);
		public const Microsoft.VisualBasic.DateFormat vbShortDate = (Microsoft.VisualBasic.DateFormat)(2);
		public const Microsoft.VisualBasic.DateFormat vbLongTime = (Microsoft.VisualBasic.DateFormat)(3);
		public const Microsoft.VisualBasic.DateFormat vbShortTime = (Microsoft.VisualBasic.DateFormat)(4);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbUseSystemDayOfWeek = (Microsoft.VisualBasic.FirstDayOfWeek)(0);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbSunday = (Microsoft.VisualBasic.FirstDayOfWeek)(1);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbMonday = (Microsoft.VisualBasic.FirstDayOfWeek)(2);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbTuesday = (Microsoft.VisualBasic.FirstDayOfWeek)(3);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbWednesday = (Microsoft.VisualBasic.FirstDayOfWeek)(4);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbThursday = (Microsoft.VisualBasic.FirstDayOfWeek)(5);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbFriday = (Microsoft.VisualBasic.FirstDayOfWeek)(6);
		public const Microsoft.VisualBasic.FirstDayOfWeek vbSaturday = (Microsoft.VisualBasic.FirstDayOfWeek)(7);
		public const Microsoft.VisualBasic.FileAttribute vbNormal = (Microsoft.VisualBasic.FileAttribute)(0);
		public const Microsoft.VisualBasic.FileAttribute vbReadOnly = (Microsoft.VisualBasic.FileAttribute)(1);
		public const Microsoft.VisualBasic.FileAttribute vbHidden = (Microsoft.VisualBasic.FileAttribute)(2);
		public const Microsoft.VisualBasic.FileAttribute vbSystem = (Microsoft.VisualBasic.FileAttribute)(4);
		public const Microsoft.VisualBasic.FileAttribute vbVolume = (Microsoft.VisualBasic.FileAttribute)(8);
		public const Microsoft.VisualBasic.FileAttribute vbDirectory = (Microsoft.VisualBasic.FileAttribute)(16);
		public const Microsoft.VisualBasic.FileAttribute vbArchive = (Microsoft.VisualBasic.FileAttribute)(32);
		public const Microsoft.VisualBasic.FirstWeekOfYear vbUseSystem = (Microsoft.VisualBasic.FirstWeekOfYear)(0);
		public const Microsoft.VisualBasic.FirstWeekOfYear vbFirstJan1 = (Microsoft.VisualBasic.FirstWeekOfYear)(1);
		public const Microsoft.VisualBasic.FirstWeekOfYear vbFirstFourDays = (Microsoft.VisualBasic.FirstWeekOfYear)(2);
		public const Microsoft.VisualBasic.FirstWeekOfYear vbFirstFullWeek = (Microsoft.VisualBasic.FirstWeekOfYear)(3);
		public const Microsoft.VisualBasic.VbStrConv vbUpperCase = (Microsoft.VisualBasic.VbStrConv)(1);
		public const Microsoft.VisualBasic.VbStrConv vbLowerCase = (Microsoft.VisualBasic.VbStrConv)(2);
		public const Microsoft.VisualBasic.VbStrConv vbProperCase = (Microsoft.VisualBasic.VbStrConv)(3);
		public const Microsoft.VisualBasic.VbStrConv vbWide = (Microsoft.VisualBasic.VbStrConv)(4);
		public const Microsoft.VisualBasic.VbStrConv vbNarrow = (Microsoft.VisualBasic.VbStrConv)(8);
		public const Microsoft.VisualBasic.VbStrConv vbKatakana = (Microsoft.VisualBasic.VbStrConv)(16);
		public const Microsoft.VisualBasic.VbStrConv vbHiragana = (Microsoft.VisualBasic.VbStrConv)(32);
		public const Microsoft.VisualBasic.VbStrConv vbSimplifiedChinese = (Microsoft.VisualBasic.VbStrConv)(256);
		public const Microsoft.VisualBasic.VbStrConv vbTraditionalChinese = (Microsoft.VisualBasic.VbStrConv)(512);
		public const Microsoft.VisualBasic.VbStrConv vbLinguisticCasing = (Microsoft.VisualBasic.VbStrConv)(1024);
		public const Microsoft.VisualBasic.TriState vbUseDefault = (Microsoft.VisualBasic.TriState)(-2);
		public const Microsoft.VisualBasic.TriState vbTrue = (Microsoft.VisualBasic.TriState)(-1);
		public const Microsoft.VisualBasic.TriState vbFalse = (Microsoft.VisualBasic.TriState)(0);
		public const Microsoft.VisualBasic.VariantType vbEmpty = (Microsoft.VisualBasic.VariantType)(0);
		public const Microsoft.VisualBasic.VariantType vbNull = (Microsoft.VisualBasic.VariantType)(1);
		public const Microsoft.VisualBasic.VariantType vbInteger = (Microsoft.VisualBasic.VariantType)(3);
		public const Microsoft.VisualBasic.VariantType vbLong = (Microsoft.VisualBasic.VariantType)(20);
		public const Microsoft.VisualBasic.VariantType vbSingle = (Microsoft.VisualBasic.VariantType)(4);
		public const Microsoft.VisualBasic.VariantType vbDouble = (Microsoft.VisualBasic.VariantType)(5);
		public const Microsoft.VisualBasic.VariantType vbCurrency = (Microsoft.VisualBasic.VariantType)(6);
		public const Microsoft.VisualBasic.VariantType vbDate = (Microsoft.VisualBasic.VariantType)(7);
		public const Microsoft.VisualBasic.VariantType vbString = (Microsoft.VisualBasic.VariantType)(8);
		public const Microsoft.VisualBasic.VariantType vbObject = (Microsoft.VisualBasic.VariantType)(9);
		public const Microsoft.VisualBasic.VariantType vbBoolean = (Microsoft.VisualBasic.VariantType)(11);
		public const Microsoft.VisualBasic.VariantType vbVariant = (Microsoft.VisualBasic.VariantType)(12);
		public const Microsoft.VisualBasic.VariantType vbDecimal = (Microsoft.VisualBasic.VariantType)(14);
		public const Microsoft.VisualBasic.VariantType vbByte = (Microsoft.VisualBasic.VariantType)(17);
		public const Microsoft.VisualBasic.VariantType vbUserDefinedType = (Microsoft.VisualBasic.VariantType)(36);
		public const Microsoft.VisualBasic.VariantType vbArray = (Microsoft.VisualBasic.VariantType)(8192);
		public const Microsoft.VisualBasic.MsgBoxResult vbOK = (Microsoft.VisualBasic.MsgBoxResult)(1);
		public const Microsoft.VisualBasic.MsgBoxResult vbCancel = (Microsoft.VisualBasic.MsgBoxResult)(2);
		public const Microsoft.VisualBasic.MsgBoxResult vbAbort = (Microsoft.VisualBasic.MsgBoxResult)(3);
		public const Microsoft.VisualBasic.MsgBoxResult vbRetry = (Microsoft.VisualBasic.MsgBoxResult)(4);
		public const Microsoft.VisualBasic.MsgBoxResult vbIgnore = (Microsoft.VisualBasic.MsgBoxResult)(5);
		public const Microsoft.VisualBasic.MsgBoxResult vbYes = (Microsoft.VisualBasic.MsgBoxResult)(6);
		public const Microsoft.VisualBasic.MsgBoxResult vbNo = (Microsoft.VisualBasic.MsgBoxResult)(7);
		public const Microsoft.VisualBasic.MsgBoxStyle vbOKOnly = (Microsoft.VisualBasic.MsgBoxStyle)(0);
		public const Microsoft.VisualBasic.MsgBoxStyle vbOKCancel = (Microsoft.VisualBasic.MsgBoxStyle)(1);
		public const Microsoft.VisualBasic.MsgBoxStyle vbAbortRetryIgnore = (Microsoft.VisualBasic.MsgBoxStyle)(2);
		public const Microsoft.VisualBasic.MsgBoxStyle vbYesNoCancel = (Microsoft.VisualBasic.MsgBoxStyle)(3);
		public const Microsoft.VisualBasic.MsgBoxStyle vbYesNo = (Microsoft.VisualBasic.MsgBoxStyle)(4);
		public const Microsoft.VisualBasic.MsgBoxStyle vbRetryCancel = (Microsoft.VisualBasic.MsgBoxStyle)(5);
		public const Microsoft.VisualBasic.MsgBoxStyle vbCritical = (Microsoft.VisualBasic.MsgBoxStyle)(16);
		public const Microsoft.VisualBasic.MsgBoxStyle vbQuestion = (Microsoft.VisualBasic.MsgBoxStyle)(32);
		public const Microsoft.VisualBasic.MsgBoxStyle vbExclamation = (Microsoft.VisualBasic.MsgBoxStyle)(48);
		public const Microsoft.VisualBasic.MsgBoxStyle vbInformation = (Microsoft.VisualBasic.MsgBoxStyle)(64);
		public const Microsoft.VisualBasic.MsgBoxStyle vbDefaultButton1 = (Microsoft.VisualBasic.MsgBoxStyle)(0);
		public const Microsoft.VisualBasic.MsgBoxStyle vbDefaultButton2 = (Microsoft.VisualBasic.MsgBoxStyle)(256);
		public const Microsoft.VisualBasic.MsgBoxStyle vbDefaultButton3 = (Microsoft.VisualBasic.MsgBoxStyle)(512);
		public const Microsoft.VisualBasic.MsgBoxStyle vbApplicationModal = (Microsoft.VisualBasic.MsgBoxStyle)(0);
		public const Microsoft.VisualBasic.MsgBoxStyle vbSystemModal = (Microsoft.VisualBasic.MsgBoxStyle)(4096);
		public const Microsoft.VisualBasic.MsgBoxStyle vbMsgBoxHelp = (Microsoft.VisualBasic.MsgBoxStyle)(16384);
		public const Microsoft.VisualBasic.MsgBoxStyle vbMsgBoxRight = (Microsoft.VisualBasic.MsgBoxStyle)(524288);
		public const Microsoft.VisualBasic.MsgBoxStyle vbMsgBoxRtlReading = (Microsoft.VisualBasic.MsgBoxStyle)(1048576);
		public const Microsoft.VisualBasic.MsgBoxStyle vbMsgBoxSetForeground = (Microsoft.VisualBasic.MsgBoxStyle)(65536);
		// Constructors
		// Properties
		// Methods
		// Events
	};
}
