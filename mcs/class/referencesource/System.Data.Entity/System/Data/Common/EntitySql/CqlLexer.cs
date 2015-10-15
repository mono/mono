namespace System.Data.Common.EntitySql
{
/*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
!!                                                                            !!
!!         ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION        !!
!!                                                                            !!
!!                     DO NOT CHANGE THIS FILE BY HAND!!!!                    !!
!!                          YOU HAVE BEEN WARNED !!!!                         !!
!!                                                                            !!
!!         ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION ATTENTION        !!
!!                                                                            !!
!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
/*------------------------------------------------------------------------------
// <copyright file="CqlLexer.l" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//----------------------------------------------------------------------------*/
using System;
using System.Globalization;
using System.Data.Common.EntitySql.AST;
using System.Data.Entity;


internal  partial  class CqlLexer
{
private const int YY_BUFFER_SIZE = 512;
private const int YY_F = -1;
private const int YY_NO_STATE = -1;
private const int YY_NOT_ACCEPT = 0;
private const int YY_START = 1;
private const int YY_END = 2;
private const int YY_NO_ANCHOR = 4;
delegate CqlLexer.Token AcceptMethod();
AcceptMethod[] accept_dispatch;
private const int YY_BOL = 128;
private const int YY_EOF = 129;
private System.IO.TextReader yy_reader;
private int yy_buffer_index;
private int yy_buffer_read;
private int yy_buffer_start;
private int yy_buffer_end;
private char[] yy_buffer;
private int yychar;
private int yyline;
private bool yy_at_bol;
private int yy_lexical_state;

internal CqlLexer(System.IO.TextReader reader) : this()
  {
  if (null == reader)
    {
          throw new System.Data.EntitySqlException(EntityRes.GetString(EntityRes.ParserInputError)); 
    }
  yy_reader = reader;
  }

internal CqlLexer(System.IO.FileStream instream) : this()
  {
  if (null == instream)
    {
           throw new System.Data.EntitySqlException(EntityRes.GetString(EntityRes.ParserInputError)); 
    }
  yy_reader = new System.IO.StreamReader(instream);
  }

private CqlLexer()
  {
  yy_buffer = new char[YY_BUFFER_SIZE];
  yy_buffer_read = 0;
  yy_buffer_index = 0;
  yy_buffer_start = 0;
  yy_buffer_end = 0;
  yychar = 0;
  yyline = 0;
  yy_at_bol = true;
  yy_lexical_state = YYINITIAL;
accept_dispatch = new AcceptMethod[] 
 {
  null,
  null,
  new AcceptMethod(this.Accept_2),
  new AcceptMethod(this.Accept_3),
  new AcceptMethod(this.Accept_4),
  new AcceptMethod(this.Accept_5),
  new AcceptMethod(this.Accept_6),
  new AcceptMethod(this.Accept_7),
  new AcceptMethod(this.Accept_8),
  new AcceptMethod(this.Accept_9),
  new AcceptMethod(this.Accept_10),
  new AcceptMethod(this.Accept_11),
  new AcceptMethod(this.Accept_12),
  new AcceptMethod(this.Accept_13),
  new AcceptMethod(this.Accept_14),
  new AcceptMethod(this.Accept_15),
  new AcceptMethod(this.Accept_16),
  new AcceptMethod(this.Accept_17),
  new AcceptMethod(this.Accept_18),
  null,
  new AcceptMethod(this.Accept_20),
  new AcceptMethod(this.Accept_21),
  new AcceptMethod(this.Accept_22),
  new AcceptMethod(this.Accept_23),
  null,
  new AcceptMethod(this.Accept_25),
  new AcceptMethod(this.Accept_26),
  new AcceptMethod(this.Accept_27),
  new AcceptMethod(this.Accept_28),
  null,
  new AcceptMethod(this.Accept_30),
  new AcceptMethod(this.Accept_31),
  new AcceptMethod(this.Accept_32),
  null,
  new AcceptMethod(this.Accept_34),
  new AcceptMethod(this.Accept_35),
  null,
  new AcceptMethod(this.Accept_37),
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  null,
  new AcceptMethod(this.Accept_53),
  new AcceptMethod(this.Accept_54),
  new AcceptMethod(this.Accept_55),
  new AcceptMethod(this.Accept_56),
  new AcceptMethod(this.Accept_57),
  new AcceptMethod(this.Accept_58),
  new AcceptMethod(this.Accept_59),
  new AcceptMethod(this.Accept_60),
  new AcceptMethod(this.Accept_61),
  new AcceptMethod(this.Accept_62),
  new AcceptMethod(this.Accept_63),
  new AcceptMethod(this.Accept_64),
  new AcceptMethod(this.Accept_65),
  new AcceptMethod(this.Accept_66),
  new AcceptMethod(this.Accept_67),
  new AcceptMethod(this.Accept_68),
  new AcceptMethod(this.Accept_69),
  new AcceptMethod(this.Accept_70),
  new AcceptMethod(this.Accept_71),
  new AcceptMethod(this.Accept_72),
  new AcceptMethod(this.Accept_73),
  new AcceptMethod(this.Accept_74),
  new AcceptMethod(this.Accept_75),
  new AcceptMethod(this.Accept_76),
  new AcceptMethod(this.Accept_77),
  new AcceptMethod(this.Accept_78),
  new AcceptMethod(this.Accept_79),
  new AcceptMethod(this.Accept_80),
  new AcceptMethod(this.Accept_81),
  new AcceptMethod(this.Accept_82),
  new AcceptMethod(this.Accept_83),
  new AcceptMethod(this.Accept_84),
  };
  }

CqlLexer.Token Accept_2()
    { // begin accept action #2
{
    return HandleEscapedIdentifiers();
}
    } // end accept action #2

CqlLexer.Token Accept_3()
    { // begin accept action #3
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #3

CqlLexer.Token Accept_4()
    { // begin accept action #4
{
    AdvanceIPos();
    ResetSymbolAsIdentifierState(false);
    return null;
}
    } // end accept action #4

CqlLexer.Token Accept_5()
    { // begin accept action #5
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}
    } // end accept action #5

CqlLexer.Token Accept_6()
    { // begin accept action #6
{
    return MapPunctuator(YYText);
}
    } // end accept action #6

CqlLexer.Token Accept_7()
    { // begin accept action #7
{
    return MapOperator(YYText);
}
    } // end accept action #7

CqlLexer.Token Accept_8()
    { // begin accept action #8
{
    _lineNumber++;
    AdvanceIPos();
    ResetSymbolAsIdentifierState(false);
    return null;
}
    } // end accept action #8

CqlLexer.Token Accept_9()
    { // begin accept action #9
{
    return NewLiteralToken(YYText, LiteralKind.String);
}
    } // end accept action #9

CqlLexer.Token Accept_10()
    { // begin accept action #10
{
    return MapDoubleQuotedString(YYText);
}
    } // end accept action #10

CqlLexer.Token Accept_11()
    { // begin accept action #11
{
    return NewParameterToken(YYText);
}
    } // end accept action #11

CqlLexer.Token Accept_12()
    { // begin accept action #12
{
    return NewLiteralToken(YYText, LiteralKind.Binary);
}
    } // end accept action #12

CqlLexer.Token Accept_13()
    { // begin accept action #13
{
    _lineNumber++;
    AdvanceIPos();
    ResetSymbolAsIdentifierState(false);
    return null;
}
    } // end accept action #13

CqlLexer.Token Accept_14()
    { // begin accept action #14
{
    return NewLiteralToken(YYText, LiteralKind.Boolean);
}
    } // end accept action #14

CqlLexer.Token Accept_15()
    { // begin accept action #15
{
    return NewLiteralToken(YYText, LiteralKind.Time);
}
    } // end accept action #15

CqlLexer.Token Accept_16()
    { // begin accept action #16
{
    return NewLiteralToken(YYText, LiteralKind.Guid);
}
    } // end accept action #16

CqlLexer.Token Accept_17()
    { // begin accept action #17
{
    return NewLiteralToken(YYText, LiteralKind.DateTime);
}
    } // end accept action #17

CqlLexer.Token Accept_18()
    { // begin accept action #18
{
    return NewLiteralToken(YYText, LiteralKind.DateTimeOffset);
}
    } // end accept action #18

CqlLexer.Token Accept_20()
    { // begin accept action #20
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #20

CqlLexer.Token Accept_21()
    { // begin accept action #21
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}
    } // end accept action #21

CqlLexer.Token Accept_22()
    { // begin accept action #22
{
    return MapPunctuator(YYText);
}
    } // end accept action #22

CqlLexer.Token Accept_23()
    { // begin accept action #23
{
    return MapOperator(YYText);
}
    } // end accept action #23

CqlLexer.Token Accept_25()
    { // begin accept action #25
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #25

CqlLexer.Token Accept_26()
    { // begin accept action #26
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}
    } // end accept action #26

CqlLexer.Token Accept_27()
    { // begin accept action #27
{
    return MapPunctuator(YYText);
}
    } // end accept action #27

CqlLexer.Token Accept_28()
    { // begin accept action #28
{
    return MapOperator(YYText);
}
    } // end accept action #28

CqlLexer.Token Accept_30()
    { // begin accept action #30
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #30

CqlLexer.Token Accept_31()
    { // begin accept action #31
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}
    } // end accept action #31

CqlLexer.Token Accept_32()
    { // begin accept action #32
{
    return MapOperator(YYText);
}
    } // end accept action #32

CqlLexer.Token Accept_34()
    { // begin accept action #34
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #34

CqlLexer.Token Accept_35()
    { // begin accept action #35
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}
    } // end accept action #35

CqlLexer.Token Accept_37()
    { // begin accept action #37
{
    return NewLiteralToken(YYText, LiteralKind.Number);
}
    } // end accept action #37

CqlLexer.Token Accept_53()
    { // begin accept action #53
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #53

CqlLexer.Token Accept_54()
    { // begin accept action #54
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #54

CqlLexer.Token Accept_55()
    { // begin accept action #55
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #55

CqlLexer.Token Accept_56()
    { // begin accept action #56
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #56

CqlLexer.Token Accept_57()
    { // begin accept action #57
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #57

CqlLexer.Token Accept_58()
    { // begin accept action #58
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #58

CqlLexer.Token Accept_59()
    { // begin accept action #59
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #59

CqlLexer.Token Accept_60()
    { // begin accept action #60
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #60

CqlLexer.Token Accept_61()
    { // begin accept action #61
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #61

CqlLexer.Token Accept_62()
    { // begin accept action #62
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #62

CqlLexer.Token Accept_63()
    { // begin accept action #63
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #63

CqlLexer.Token Accept_64()
    { // begin accept action #64
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #64

CqlLexer.Token Accept_65()
    { // begin accept action #65
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #65

CqlLexer.Token Accept_66()
    { // begin accept action #66
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #66

CqlLexer.Token Accept_67()
    { // begin accept action #67
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #67

CqlLexer.Token Accept_68()
    { // begin accept action #68
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #68

CqlLexer.Token Accept_69()
    { // begin accept action #69
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #69

CqlLexer.Token Accept_70()
    { // begin accept action #70
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #70

CqlLexer.Token Accept_71()
    { // begin accept action #71
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #71

CqlLexer.Token Accept_72()
    { // begin accept action #72
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #72

CqlLexer.Token Accept_73()
    { // begin accept action #73
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #73

CqlLexer.Token Accept_74()
    { // begin accept action #74
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #74

CqlLexer.Token Accept_75()
    { // begin accept action #75
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #75

CqlLexer.Token Accept_76()
    { // begin accept action #76
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #76

CqlLexer.Token Accept_77()
    { // begin accept action #77
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #77

CqlLexer.Token Accept_78()
    { // begin accept action #78
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #78

CqlLexer.Token Accept_79()
    { // begin accept action #79
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #79

CqlLexer.Token Accept_80()
    { // begin accept action #80
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #80

CqlLexer.Token Accept_81()
    { // begin accept action #81
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #81

CqlLexer.Token Accept_82()
    { // begin accept action #82
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #82

CqlLexer.Token Accept_83()
    { // begin accept action #83
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #83

CqlLexer.Token Accept_84()
    { // begin accept action #84
{
    return MapIdentifierOrKeyword(YYText);
}
    } // end accept action #84

private const int YYINITIAL = 0;
private static int[] yy_state_dtrans = new int[] 
  {   0
  };
private void yybegin (int state)
  {
  yy_lexical_state = state;
  }

private char yy_advance ()
  {
  int next_read;
  int i;
  int j;

  if (yy_buffer_index < yy_buffer_read)
    {
    return yy_translate.translate(yy_buffer[yy_buffer_index++]);
    }

  if (0 != yy_buffer_start)
    {
    i = yy_buffer_start;
    j = 0;
    while (i < yy_buffer_read)
      {
      yy_buffer[j] = yy_buffer[i];
      i++;
      j++;
      }
    yy_buffer_end = yy_buffer_end - yy_buffer_start;
    yy_buffer_start = 0;
    yy_buffer_read = j;
    yy_buffer_index = j;
    next_read = yy_reader.Read(yy_buffer,yy_buffer_read,
                  yy_buffer.Length - yy_buffer_read);
    if (next_read <= 0)
      {
      return (char) YY_EOF;
      }
    yy_buffer_read = yy_buffer_read + next_read;
    }
  while (yy_buffer_index >= yy_buffer_read)
    {
    if (yy_buffer_index >= yy_buffer.Length)
      {
      yy_buffer = yy_double(yy_buffer);
      }
    next_read = yy_reader.Read(yy_buffer,yy_buffer_read,
                  yy_buffer.Length - yy_buffer_read);
    if (next_read <= 0)
      {
      return (char) YY_EOF;
      }
    yy_buffer_read = yy_buffer_read + next_read;
    }
    return yy_translate.translate(yy_buffer[yy_buffer_index++]);
  }
private void yy_move_end ()
  {
  if (yy_buffer_end > yy_buffer_start && 
      '\n' == yy_buffer[yy_buffer_end-1])
    yy_buffer_end--;
  if (yy_buffer_end > yy_buffer_start &&
      '\r' == yy_buffer[yy_buffer_end-1])
    yy_buffer_end--;
  }
private bool yy_last_was_cr=false;
private void yy_mark_start ()
  {
  int i;
  for (i = yy_buffer_start; i < yy_buffer_index; i++)
    {
    if (yy_buffer[i] == '\n' && !yy_last_was_cr)
      {
      yyline++;
      }
    if (yy_buffer[i] == '\r')
      {
      yyline++;
      yy_last_was_cr=true;
      }
    else
      {
      yy_last_was_cr=false;
      }
    }
  yychar = yychar + yy_buffer_index - yy_buffer_start;
  yy_buffer_start = yy_buffer_index;
  }
private void yy_mark_end ()
  {
  yy_buffer_end = yy_buffer_index;
  }
private void yy_to_mark ()
  {
  yy_buffer_index = yy_buffer_end;
  yy_at_bol = (yy_buffer_end > yy_buffer_start) &&
    (yy_buffer[yy_buffer_end-1] == '\r' ||
    yy_buffer[yy_buffer_end-1] == '\n');
  }
internal string yytext()
  {
  return (new string(yy_buffer,
                yy_buffer_start,
                yy_buffer_end - yy_buffer_start)
         );
  }
internal int yy_char()
  {
  return (yychar);
  }
private int yylength ()
  {
  return yy_buffer_end - yy_buffer_start;
  }
private char[] yy_double (char[] buf)
  {
  int i;
  char[] newbuf;
  newbuf = new char[2*buf.Length];
  for (i = 0; i < buf.Length; i++)
    {
    newbuf[i] = buf[i];
    }
  return newbuf;
  }
private const int YY_E_INTERNAL = 0;
private const int YY_E_MATCH = 1;
private static string[] yy_error_string = new string[]
  {
  "Error: Internal error.\n",
  "Error: Unmatched input.\n"
  };
private void yy_error (int code,bool fatal)
  {
  //System.Console.Write(yy_error_string[code]);
  if (fatal)
    {
          throw new System.Data.EntitySqlException(EntityRes.GetString(EntityRes.ParserFatalError)); 
    }
  }
private static int[] yy_acpt = new int[]
  {
  /* 0 */   YY_NOT_ACCEPT,
  /* 1 */   YY_NO_ANCHOR,
  /* 2 */   YY_NO_ANCHOR,
  /* 3 */   YY_NO_ANCHOR,
  /* 4 */   YY_NO_ANCHOR,
  /* 5 */   YY_NO_ANCHOR,
  /* 6 */   YY_NO_ANCHOR,
  /* 7 */   YY_NO_ANCHOR,
  /* 8 */   YY_NO_ANCHOR,
  /* 9 */   YY_NO_ANCHOR,
  /* 10 */   YY_NO_ANCHOR,
  /* 11 */   YY_NO_ANCHOR,
  /* 12 */   YY_NO_ANCHOR,
  /* 13 */   YY_END,
  /* 14 */   YY_NO_ANCHOR,
  /* 15 */   YY_NO_ANCHOR,
  /* 16 */   YY_NO_ANCHOR,
  /* 17 */   YY_NO_ANCHOR,
  /* 18 */   YY_NO_ANCHOR,
  /* 19 */   YY_NOT_ACCEPT,
  /* 20 */   YY_NO_ANCHOR,
  /* 21 */   YY_NO_ANCHOR,
  /* 22 */   YY_NO_ANCHOR,
  /* 23 */   YY_NO_ANCHOR,
  /* 24 */   YY_NOT_ACCEPT,
  /* 25 */   YY_NO_ANCHOR,
  /* 26 */   YY_NO_ANCHOR,
  /* 27 */   YY_NO_ANCHOR,
  /* 28 */   YY_NO_ANCHOR,
  /* 29 */   YY_NOT_ACCEPT,
  /* 30 */   YY_NO_ANCHOR,
  /* 31 */   YY_NO_ANCHOR,
  /* 32 */   YY_NO_ANCHOR,
  /* 33 */   YY_NOT_ACCEPT,
  /* 34 */   YY_NO_ANCHOR,
  /* 35 */   YY_NO_ANCHOR,
  /* 36 */   YY_NOT_ACCEPT,
  /* 37 */   YY_NO_ANCHOR,
  /* 38 */   YY_NOT_ACCEPT,
  /* 39 */   YY_NOT_ACCEPT,
  /* 40 */   YY_NOT_ACCEPT,
  /* 41 */   YY_NOT_ACCEPT,
  /* 42 */   YY_NOT_ACCEPT,
  /* 43 */   YY_NOT_ACCEPT,
  /* 44 */   YY_NOT_ACCEPT,
  /* 45 */   YY_NOT_ACCEPT,
  /* 46 */   YY_NOT_ACCEPT,
  /* 47 */   YY_NOT_ACCEPT,
  /* 48 */   YY_NOT_ACCEPT,
  /* 49 */   YY_NOT_ACCEPT,
  /* 50 */   YY_NOT_ACCEPT,
  /* 51 */   YY_NOT_ACCEPT,
  /* 52 */   YY_NOT_ACCEPT,
  /* 53 */   YY_NO_ANCHOR,
  /* 54 */   YY_NO_ANCHOR,
  /* 55 */   YY_NO_ANCHOR,
  /* 56 */   YY_NO_ANCHOR,
  /* 57 */   YY_NO_ANCHOR,
  /* 58 */   YY_NO_ANCHOR,
  /* 59 */   YY_NO_ANCHOR,
  /* 60 */   YY_NO_ANCHOR,
  /* 61 */   YY_NO_ANCHOR,
  /* 62 */   YY_NO_ANCHOR,
  /* 63 */   YY_NO_ANCHOR,
  /* 64 */   YY_NO_ANCHOR,
  /* 65 */   YY_NO_ANCHOR,
  /* 66 */   YY_NO_ANCHOR,
  /* 67 */   YY_NO_ANCHOR,
  /* 68 */   YY_NO_ANCHOR,
  /* 69 */   YY_NO_ANCHOR,
  /* 70 */   YY_NO_ANCHOR,
  /* 71 */   YY_NO_ANCHOR,
  /* 72 */   YY_NO_ANCHOR,
  /* 73 */   YY_NO_ANCHOR,
  /* 74 */   YY_NO_ANCHOR,
  /* 75 */   YY_NO_ANCHOR,
  /* 76 */   YY_NO_ANCHOR,
  /* 77 */   YY_NO_ANCHOR,
  /* 78 */   YY_NO_ANCHOR,
  /* 79 */   YY_NO_ANCHOR,
  /* 80 */   YY_NO_ANCHOR,
  /* 81 */   YY_NO_ANCHOR,
  /* 82 */   YY_NO_ANCHOR,
  /* 83 */   YY_NO_ANCHOR,
  /* 84 */   YY_NO_ANCHOR
  };
private static int[] yy_cmap = new int[]
  {
  11, 11, 11, 11, 11, 11, 11, 11,
  11, 11, 27, 11, 11, 8, 11, 11,
  11, 11, 11, 11, 11, 11, 11, 11,
  11, 11, 11, 11, 11, 11, 11, 11,
  12, 33, 28, 11, 11, 39, 36, 10,
  40, 40, 39, 38, 40, 25, 24, 39,
  22, 22, 22, 22, 22, 22, 22, 22,
  22, 22, 40, 40, 34, 32, 35, 40,
  29, 5, 2, 30, 13, 15, 18, 20,
  30, 3, 30, 30, 23, 16, 26, 17,
  30, 30, 6, 19, 14, 21, 30, 30,
  9, 7, 30, 1, 11, 40, 11, 31,
  11, 5, 2, 30, 13, 15, 18, 20,
  30, 3, 30, 30, 23, 16, 4, 17,
  30, 30, 6, 19, 14, 21, 30, 30,
  9, 7, 30, 40, 37, 40, 11, 11,
  0, 41 
  };
private static int[] yy_rmap = new int[]
  {
  0, 1, 1, 2, 3, 4, 5, 6,
  7, 8, 9, 10, 1, 1, 11, 1,
  1, 1, 1, 12, 13, 1, 14, 14,
  15, 16, 17, 1, 18, 10, 19, 20,
  1, 21, 22, 23, 24, 25, 26, 27,
  5, 28, 29, 30, 31, 32, 33, 34,
  35, 36, 37, 38, 39, 40, 41, 42,
  43, 44, 45, 46, 47, 48, 49, 50,
  51, 52, 53, 54, 55, 56, 57, 58,
  59, 60, 61, 62, 63, 11, 64, 65,
  66, 67, 68, 11, 69 
  };
private static int[,] yy_nxt = new int[,]
  {
  { 1, 2, 3, 83, 83, 83, 83, 83,
   4, 20, 19, -1, 4, 84, 64, 83,
   83, 83, 71, 83, 72, 83, 5, 83,
   6, 7, 25, 8, 24, 29, 83, 83,
   22, 23, 28, 23, 33, 36, 32, 32,
   27, 1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 76, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   4, -1, -1, -1, 4, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, 21, -1, 39,
   21, -1, 21, -1, -1, 26, 5, 31,
   40, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 35, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, 41, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, 8, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, 19, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, 24, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 11, 11, 11, 11, 11, 11,
   -1, 11, -1, -1, -1, 11, 11, 11,
   11, 11, 11, 11, 11, 11, 11, 11,
   -1, -1, 11, -1, -1, -1, 11, 11,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 19, 19, 19, 19, 19, 19, 19,
   19, 19, 9, 19, 19, 19, 19, 19,
   19, 19, 19, 19, 19, 19, 19, 19,
   19, 19, 19, 19, 19, 19, 19, 19,
   19, 19, 19, 19, 19, 19, 19, 19,
   19, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, 38, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   32, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 24, 24, 24, 24, 24, 24, 24,
   24, 24, 24, 24, 24, 24, 24, 24,
   24, 24, 24, 24, 24, 24, 24, 24,
   24, 24, 24, 24, 10, 24, 24, 24,
   24, 24, 24, 24, 24, 24, 24, 24,
   24, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, 19, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, 24, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, 21,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   32, -1, -1, 32, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 14,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, 21, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, 32, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   44, 83, 45, -1, 44, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, 21, -1, 39,
   21, -1, 21, -1, -1, -1, 35, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, 32, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, 21, -1, -1,
   -1, -1, 21, -1, -1, -1, 37, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 38, 38, 38, 38, 38, 38, 38,
   -1, 38, 12, 38, 38, 38, 38, 38,
   38, 38, 38, 38, 38, 38, 38, 38,
   38, 38, 38, -1, -1, 38, 38, 38,
   38, 38, 38, 38, 38, 38, 38, 38,
   38, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 37, -1,
   -1, 42, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 42, -1,
   -1, -1 },
  { -1, 41, 41, 41, 41, 41, 41, 41,
   43, 41, 41, 41, 41, 41, 41, 41,
   41, 41, 41, 41, 41, 41, 41, 41,
   41, 41, 41, 13, 41, 41, 41, 41,
   41, 41, 41, 41, 41, 41, 41, 41,
   41, 13 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, 37, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, 13, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   44, -1, 45, -1, 44, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 45, 45, 45, 45, 45, 45, 45,
   -1, 45, 15, 45, 45, 45, 45, 45,
   45, 45, 45, 45, 45, 45, 45, 45,
   45, 45, 45, -1, -1, 45, 45, 45,
   45, 45, 45, 45, 45, 45, 45, 45,
   45, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   46, -1, 47, -1, 46, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 47, 47, 47, 47, 47, 47, 47,
   -1, 47, 16, 47, 47, 47, 47, 47,
   47, 47, 47, 47, 47, 47, 47, 47,
   47, 47, 47, -1, -1, 47, 47, 47,
   47, 47, 47, 47, 47, 47, 47, 47,
   47, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   48, -1, 38, -1, 48, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   49, -1, 50, -1, 49, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 50, 50, 50, 50, 50, 50, 50,
   -1, 50, 17, 50, 50, 50, 50, 50,
   50, 50, 50, 50, 50, 50, 50, 50,
   50, 50, 50, -1, -1, 50, 50, 50,
   50, 50, 50, 50, 50, 50, 50, 50,
   50, -1 },
  { -1, -1, -1, -1, -1, -1, -1, -1,
   51, -1, 52, -1, 51, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, 52, 52, 52, 52, 52, 52, 52,
   -1, 52, 18, 52, 52, 52, 52, 52,
   52, 52, 52, 52, 52, 52, 52, 52,
   52, 52, 52, -1, -1, 52, 52, 52,
   52, 52, 52, 52, 52, 52, 52, 52,
   52, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 30, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   46, 83, 47, -1, 46, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 34,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   48, 83, 38, -1, 48, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 30, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   49, 83, 50, -1, 49, 83, 83, 83,
   83, 81, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 54, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   51, 83, 52, -1, 51, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 56,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 58,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 60, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 65, 83, 83, 53, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   55, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 57,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 59, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 61, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   62, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 63,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 66, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 67, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 68, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 69, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 70, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 73, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 73, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 79, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 80,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 74, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 82, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 83, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 75, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 },
  { -1, -1, 83, 83, 83, 78, 83, 83,
   -1, 83, -1, -1, -1, 83, 83, 83,
   83, 83, 83, 83, 83, 83, 77, 83,
   -1, -1, 83, -1, -1, -1, 83, 77,
   -1, -1, -1, -1, -1, -1, -1, -1,
   -1, -1 }
  };
internal CqlLexer.Token yylex()
  {
  char yy_lookahead;
  int yy_anchor = YY_NO_ANCHOR;
  int yy_state = yy_state_dtrans[yy_lexical_state];
  int yy_next_state = YY_NO_STATE;
  int yy_last_accept_state = YY_NO_STATE;
  bool yy_initial = true;
  int yy_this_accept;

  yy_mark_start();
  yy_this_accept = yy_acpt[yy_state];
  if (YY_NOT_ACCEPT != yy_this_accept)
    {
    yy_last_accept_state = yy_state;
    yy_mark_end();
    }
  while (true)
    {
    if (yy_initial && yy_at_bol)
      yy_lookahead = (char) YY_BOL;
    else
      {
      yy_lookahead = yy_advance();
      }
    yy_next_state = yy_nxt[yy_rmap[yy_state],yy_cmap[yy_lookahead]];
    if (YY_EOF == yy_lookahead && yy_initial)
      {

    return null;
      }
    if (YY_F != yy_next_state)
      {
      yy_state = yy_next_state;
      yy_initial = false;
      yy_this_accept = yy_acpt[yy_state];
      if (YY_NOT_ACCEPT != yy_this_accept)
        {
        yy_last_accept_state = yy_state;
        yy_mark_end();
        }
      }
    else
      {
      if (YY_NO_STATE == yy_last_accept_state)
        {
             throw new System.Data.EntitySqlException(EntitySqlException.GetGenericErrorMessage (_query, yychar)); 
        }
      else
        {
        yy_anchor = yy_acpt[yy_last_accept_state];
        if (0 != (YY_END & yy_anchor))
          {
          yy_move_end();
          }
        yy_to_mark();
        if (yy_last_accept_state < 0)
          {
          if (yy_last_accept_state < 85)
            yy_error(YY_E_INTERNAL, false);
          }
        else
          {
          AcceptMethod m = accept_dispatch[yy_last_accept_state];
          if (m != null)
            {
            CqlLexer.Token tmp = m();
            if (tmp != null)
              return tmp;
            }
          }
        yy_initial = true;
        yy_state = yy_state_dtrans[yy_lexical_state];
        yy_next_state = YY_NO_STATE;
        yy_last_accept_state = YY_NO_STATE;
        yy_mark_start();
        yy_this_accept = yy_acpt[yy_state];
        if (YY_NOT_ACCEPT != yy_this_accept)
          {
          yy_last_accept_state = yy_state;
          yy_mark_end();
          }
        }
      }
    }
  }
}

}
