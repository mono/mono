'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 13
REM ExpectedError: BC30502
REM ErrorMessage: 'Shared' cannot be combined with 'Default' on a property declaration.

Imports System

Class base
	Shared Default ReadOnly Property Item(i as Integer)As Integer
		Get			
			Return 10
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()		
	End Sub
End Module
