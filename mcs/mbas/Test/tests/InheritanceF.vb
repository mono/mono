'Testing access to protected members of a class from it's derived class
Class C1
	Protected a As Integer
	Protected Sub S()
		a=47
	End Sub
End Class
Class C2
    Inherits C1
	Public Function F() As Integer
		S()
		Return a
	End Function
End Class
Module Inheritence
	Sub Main()
		Dim myC As New C2()
		Dim b As Integer=myC.F()
		If b<>47 Then
			Throw New System.Exception("InheritenceF:Failed-Error in accessing protected member from a derived class")
		End If
	End Sub
End Module
