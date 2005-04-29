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
	Public Shadows ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return 2*i
		End Get
	End Property
End Class

Class derive1
	Inherits derive
	Public Shadows Default ReadOnly Property Item1(ByVal i as Integer)As Integer
		Get			
			Return 3*i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
		Dim a as Object=new derive1()
		Dim b as derive = a
		Dim i, j, k as Integer	
		i=a(10)
		j=a.Item(10)
		k=b(10)
		if i<>30 Then
			Throw New Exception("#A1 Default Not Working properly in Derive1")
		End If
		if j<>20 Then
			Throw New Exception("#A2 Default Not Working properly in Derive")
		End If
		if k<>10 Then
			Throw New Exception("#A3 Default Not Working properly in Base ")
		End If
	End Sub
End Module
