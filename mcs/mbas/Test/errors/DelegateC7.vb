'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError: BC30035
REM ErrorMessage:  Syntax Error - Not caught by VB .net also

REM : Delegate cannot be delcared within a Procedure.... 

Imports System

Module M
        Function f1(i as integer)as Integer
		return 10
        End Function 

	  Sub Main()
	        Delegate Function SD(i as Integer)as Integer
	  End sub

End Module
