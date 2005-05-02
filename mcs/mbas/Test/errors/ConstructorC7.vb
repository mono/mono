'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError: BC30282
REM ErrorMessage:  Constructor call is valid only as the first statement in an instance constructor.

Imports System

Class A
	Sub New()		
	End Sub
End Class

Module Test
    Public Sub Main()
      Dim a as A = New A()
	a.New()
    End Sub
End Module

