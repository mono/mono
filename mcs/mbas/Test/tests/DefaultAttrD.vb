'Checks if Default property is working or not after Inheriting, overloading....It works

Imports System

Class base
	Public Default ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return i
		End Get
	End Property
	Public Default ReadOnly Property Item(ByVal i as Integer,ByVal j as Integer)As Integer
		Get			
			Return i+j
		End Get
	End Property
End Class

Class derive
	Inherits base
	Public Overloads Default ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return 2*i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
		Dim a as derive=new derive()
		Dim i,j as Integer	
		i=a(10)
		j=a(10,20)
		if i<>20 Then
			Throw New Exception("Default Not Working")
		End If
		if j<>30 Then
			Throw New Exception("Default Not Working")
		End If
	End Sub
End Module