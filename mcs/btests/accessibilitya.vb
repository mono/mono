Imports System
Class C1
	Protected Friend a As Integer
End Class
Class C2
  Inherits C1
	Public Sub S()
		a=100
	End Sub
End Class

Module Accessibility
	Sub Main()
		Dim myC1 As New C1()
		myC1.a=1000
		Dim myC2 As New C2()
		myC2.S()

	End Sub
End Module
