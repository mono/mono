'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 19
REM ExpectedError: BC30148
REM ErrorMessage: First statement of this 'Sub New' must be a call to 'MyBase.New' or 'MyClass.New' because base class 'A' of 'AB' does not have an accessible 'Sub New' that can be called with no arguments.

Imports System

Class A
	Sub New(I as Integer)		
	End Sub
End Class

Class AB
	Inherits A
	Sub New(I as Integer)		
	End Sub
End Class

Module Test
    Public Sub Main()
      Dim a as A = New A(10)
    End Sub
End Module

