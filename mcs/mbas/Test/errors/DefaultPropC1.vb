'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 14
REM ExpectedError: BC31048
REM ErrorMessage: Properties with no required parameters cannot be declared 'Default'.

Imports System

Class base
	Public Default ReadOnly Property Item()As Integer
		Get			
			Return 10
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()		
	End Sub
End Module
