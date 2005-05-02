'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30282
REM ErrorMessage: Constructor call is valid only as the first statement in an instance constructor.

Imports System

Class A
	Sub New()		
		Dim i as integer 
		Me.New(1)
	End Sub
	Sub New(I as Integer)		
	End Sub
End Class

Module Test
    Public Sub Main()
      Dim a as A = New A(10)
    End Sub
End Module

