//
// Constants.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
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
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic {
	[StandardModule] 
	sealed public class Constants {
		private Constants () {}

		// Declarations
		public const System.Int32 vbObjectError = (System.Int32)(-2147221504);
		public const System.String vbCrLf = "\r\n";
		public const System.String vbNewLine = "\r\n";
		public const System.String vbCr = "\r";
		public const System.String vbLf = "\n";
		public const System.String vbBack = "\b";
		public const System.String vbFormFeed = "\f";
		public const System.String vbTab = "\t";
		public const System.String vbVerticalTab = "\v";
		public const System.String vbNullChar = "\0";
		public const System.String vbNullString = "";

		public const AppWinStyle vbHide = AppWinStyle.Hide;
		public const AppWinStyle vbNormalFocus = AppWinStyle.NormalFocus;
		public const AppWinStyle vbMinimizedFocus = AppWinStyle.MinimizedFocus;
		public const AppWinStyle vbMaximizedFocus = AppWinStyle.MaximizedFocus;
		public const AppWinStyle vbNormalNoFocus = AppWinStyle.NormalNoFocus;
		public const AppWinStyle vbMinimizedNoFocus = AppWinStyle.MinimizedNoFocus;

		public const CallType vbMethod = CallType.Method;
		public const CallType vbGet = CallType.Get;
		public const CallType vbLet = CallType.Let;
		public const CallType vbSet = CallType.Set;

		public const CompareMethod vbBinaryCompare = CompareMethod.Binary;
		public const CompareMethod vbTextCompare = CompareMethod.Text;

		public const DateFormat vbGeneralDate = DateFormat.GeneralDate;
		public const DateFormat vbLongDate = DateFormat.LongDate;
		public const DateFormat vbShortDate = DateFormat.ShortDate;
		public const DateFormat vbLongTime = DateFormat.LongTime;
		public const DateFormat vbShortTime = DateFormat.ShortTime;

		public const FirstDayOfWeek vbUseSystemDayOfWeek = FirstDayOfWeek.System;
		public const FirstDayOfWeek vbSunday = FirstDayOfWeek.Sunday;
		public const FirstDayOfWeek vbMonday = FirstDayOfWeek.Monday;
		public const FirstDayOfWeek vbTuesday = FirstDayOfWeek.Tuesday;
		public const FirstDayOfWeek vbWednesday = FirstDayOfWeek.Wednesday;
		public const FirstDayOfWeek vbThursday = FirstDayOfWeek.Thursday;
		public const FirstDayOfWeek vbFriday = FirstDayOfWeek.Friday;
		public const FirstDayOfWeek vbSaturday = FirstDayOfWeek.Saturday;

		public const FileAttribute vbNormal = FileAttribute.Normal;
		public const FileAttribute vbReadOnly = FileAttribute.ReadOnly;
		public const FileAttribute vbHidden = FileAttribute.Hidden;
		public const FileAttribute vbSystem = FileAttribute.System;
		public const FileAttribute vbVolume = FileAttribute.Volume;
		public const FileAttribute vbDirectory = FileAttribute.Directory;
		public const FileAttribute vbArchive = FileAttribute.Archive;

		public const FirstWeekOfYear vbUseSystem = FirstWeekOfYear.System;
		public const FirstWeekOfYear vbFirstJan1 = FirstWeekOfYear.Jan1;
		public const FirstWeekOfYear vbFirstFourDays = FirstWeekOfYear.FirstFourDays;
		public const FirstWeekOfYear vbFirstFullWeek = FirstWeekOfYear.FirstFullWeek;

		public const VbStrConv vbUpperCase = VbStrConv.UpperCase;
		public const VbStrConv vbLowerCase = VbStrConv.LowerCase;
		public const VbStrConv vbProperCase = VbStrConv.ProperCase;
		public const VbStrConv vbWide = VbStrConv.Wide;
		public const VbStrConv vbNarrow = VbStrConv.Narrow;
		public const VbStrConv vbKatakana = VbStrConv.Katakana;
		public const VbStrConv vbHiragana = VbStrConv.Hiragana;
		public const VbStrConv vbSimplifiedChinese = VbStrConv.SimplifiedChinese;
		public const VbStrConv vbTraditionalChinese = VbStrConv.TraditionalChinese;
		public const VbStrConv vbLinguisticCasing = VbStrConv.LinguisticCasing;

		public const TriState vbUseDefault = TriState.UseDefault;
		public const TriState vbTrue = TriState.True;
		public const TriState vbFalse = TriState.False;

		public const VariantType vbEmpty = VariantType.Empty;
		public const VariantType vbNull = VariantType.Null;
		public const VariantType vbInteger = VariantType.Integer;
		public const VariantType vbLong = VariantType.Long;
		public const VariantType vbSingle = VariantType.Single;
		public const VariantType vbDouble = VariantType.Double;
		public const VariantType vbCurrency = VariantType.Currency;
		public const VariantType vbDate = VariantType.Date;
		public const VariantType vbString = VariantType.String;
		public const VariantType vbObject = VariantType.Object;
		public const VariantType vbBoolean = VariantType.Boolean;
		public const VariantType vbVariant = VariantType.Variant;
		public const VariantType vbDecimal = VariantType.Decimal;
		public const VariantType vbByte = VariantType.Byte;
		public const VariantType vbUserDefinedType = VariantType.UserDefinedType;
		public const VariantType vbArray = VariantType.Array;

		public const MsgBoxResult vbOK = MsgBoxResult.OK;
		public const MsgBoxResult vbCancel = MsgBoxResult.Cancel;
		public const MsgBoxResult vbAbort = MsgBoxResult.Abort;
		public const MsgBoxResult vbRetry = MsgBoxResult.Retry;
		public const MsgBoxResult vbIgnore = MsgBoxResult.Ignore;
		public const MsgBoxResult vbYes = MsgBoxResult.Yes;
		public const MsgBoxResult vbNo = MsgBoxResult.No;

		public const MsgBoxStyle vbOKOnly = MsgBoxStyle.OKOnly;
		public const MsgBoxStyle vbOKCancel = MsgBoxStyle.OKCancel;
		public const MsgBoxStyle vbAbortRetryIgnore = MsgBoxStyle.AbortRetryIgnore;
		public const MsgBoxStyle vbYesNoCancel = MsgBoxStyle.YesNoCancel;
		public const MsgBoxStyle vbYesNo = MsgBoxStyle.YesNo;
		public const MsgBoxStyle vbRetryCancel = MsgBoxStyle.RetryCancel;
		public const MsgBoxStyle vbCritical = MsgBoxStyle.Critical;
		public const MsgBoxStyle vbQuestion = MsgBoxStyle.Question;
		public const MsgBoxStyle vbExclamation = MsgBoxStyle.Exclamation;
		public const MsgBoxStyle vbInformation = MsgBoxStyle.Information;
		public const MsgBoxStyle vbDefaultButton1 = MsgBoxStyle.DefaultButton1;
		public const MsgBoxStyle vbDefaultButton2 = MsgBoxStyle.DefaultButton2;
		public const MsgBoxStyle vbDefaultButton3 = MsgBoxStyle.DefaultButton3;
		public const MsgBoxStyle vbApplicationModal = MsgBoxStyle.ApplicationModal;
		public const MsgBoxStyle vbSystemModal = MsgBoxStyle.SystemModal;
		public const MsgBoxStyle vbMsgBoxHelp = MsgBoxStyle.MsgBoxHelp;
		public const MsgBoxStyle vbMsgBoxRight = MsgBoxStyle.MsgBoxRight;
		public const MsgBoxStyle vbMsgBoxRtlReading = MsgBoxStyle.MsgBoxRtlReading;
		public const MsgBoxStyle vbMsgBoxSetForeground = MsgBoxStyle.MsgBoxSetForeground;

		// Constructors
		// Properties
		// Methods
		// Events
	};
}
