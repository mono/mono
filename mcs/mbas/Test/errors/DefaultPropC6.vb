'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 19
REM ExpectedError: BC30154
REM ErrorMessage: 'derive' must implement 'ReadOnly Default Property Item(i As Integer, j As Integer) As Integer' for interface 'base'. Implementing property must have matching 'ReadOnly'/'WriteOnly' specifiers.

Imports System

Interface base
	Default ReadOnly Property Item(ByVal i as Integer)As Integer				
	Default ReadOnly Property Item(ByVal i as Integer, ByVal j as Integer)As Integer	
End Interface

Class derive
	Implements base
	Public Shadows ReadOnly Default Property Item(ByVal i as Integer)As Integer Implements base.Item
		Get			
			Return 2*i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
		Dim a as derive=new derive()
		Dim i as Integer	
		i=a(10)		
		if i<>20 Then
			Throw New Exception("Default Not Working")
		End If		
	End Sub
End Module
