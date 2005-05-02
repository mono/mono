'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 19
REM ExpectedError: BC30569
REM ErrorMessage: 'New' cannot be used on a class that is declared 'MustInherit'.

Imports System

Class AA
	Public shared Function fun(ByVal a as System.MarshalByRefObject)
	End Function
End Class

Module Test
    Public Sub Main()
		dim a as System.MarshalByRefObject = new System.MarshalByRefObject()
		AA.fun(a)
    End Sub
End Module

