'Checks if Default property is working or not after Inheriting, overloading....It works but gives a warning...

'Error Line:19
'Warning BC40007
'Warning: Default property 'Item1' conflicts with default property 'Item' in the base class 'base' and so should be declared 'Shadows'.

Imports System

Class base
	Public Default ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return i
		End Get
	End Property
End Class

Class derive
	Inherits base
	Public Overloads Default ReadOnly Property Item1(ByVal i as Integer)As Integer
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