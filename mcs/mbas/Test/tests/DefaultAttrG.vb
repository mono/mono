'Checks if Default is working or not...It works

Imports System

Class base
	Dim ia as integer
	Public Default Property Item(ByVal i as Integer)As Integer
		Get			
			Return ia
		End Get
		Set(ByVal j as Integer)			
			ia=j
		End Set
	End Property
End Class

Module DefaultA
	Sub Main()
		Dim a as base=new base()
		Dim i as Integer	
		a(1)=4
		i=a(10)
		if i<>4 Then
			Throw New Exception("Default Not Working")
		End If
	End Sub
End Module
