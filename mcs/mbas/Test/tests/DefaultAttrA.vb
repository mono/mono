'Checks if Default is working or not...It works

Imports System

Class base
	Public Default ReadOnly Property Item(ByVal i as Integer)As Integer
		Get			
			Return i
		End Get
	End Property
End Class

Module DefaultA
	Sub Main()
		Dim a as base=new base()
		Dim i as Integer	
		i=a(10)
		if i<>10 Then
			Throw New Exception("Default Not Working")
		End If
	End Sub
End Module
