'Checking for interfaces.. though this is not a test for "default".. included this to check for proper functioning of the "property"
'Error Line: 14
'Error BC30154: 
'Error: 'derive' must implement 'ReadOnly Default Property Item(i As Integer, j As Integer) As Integer' for interface 'base'. Implementing property must have matching 'ReadOnly'/'WriteOnly' specifiers.

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