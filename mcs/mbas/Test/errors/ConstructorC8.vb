'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 29
REM ExpectedError: BC30057
REM ErrorMessage: Too many arguments to 'Public Sub New()'.

Imports System

Class A
	Sub New()		
	End Sub
	Sub New(I as Integer)		
	End Sub
End Class

Class AB
	Inherits A
	Public i as Integer
	Sub New()			
	End Sub
End Class

Module Test
    Public Sub Main()
      Dim a as A = New A(10)
      Dim a1 as AB= New AB(10)
    End Sub
End Module

