'Checking for interfaces

Imports System

Interface base
	Default ReadOnly Property Item(ByVal i as Integer)As Integer				
End Interface

Class derive
	Implements base
	Public Overloads ReadOnly Default Property Item(ByVal i as Integer)As Integer Implements base.Item
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