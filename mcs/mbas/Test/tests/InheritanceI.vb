Imports System

'Testing a inner class that inherits from it's outer class
                                                                                                                             
Class C4
        Class C5
	   Inherits C4
		Public Function F1() As Integer
			Return F()
		End Function
        End Class
	Public Function F() As Integer
		Return 1
	End Function
End Class


Module Inheritance
	Sub Main()
		Dim myC As New C4.C5()
		If myC.F1()<>1 Then
			Throw New Exception("InheritanceI:Failed-Error inheriting an inner class from it's outer class")
		End If
	End Sub
End Module
