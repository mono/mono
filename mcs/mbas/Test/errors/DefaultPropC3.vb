'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 13 
REM ExpectedError: BC30490
REM ErrorMessage: 'Default' cannot be combined with 'Private'.

Imports System

Class base
	Private Default ReadOnly Property Item(i as Integer)As Integer
		Get			
			Return 10
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()		
	End Sub
End Module
