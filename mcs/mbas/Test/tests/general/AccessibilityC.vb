Imports System
Class C1
	Protected Friend a As Integer=20
End Class
Class C2
  Inherits C1
	Public Sub S()
		Try
			If a<>20 Then
				Throw New Exception("#A1-AccessibilityA:Failed-error accessing value of protected friend data member from derived class")
			
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Class

Module Accessibility
	Sub Main()
		Dim myC1 As New C1()
		 Try
                        If myC1.a<>20 Then
                               Throw New Exception("#A2-AccessibilityA:Failed-error accessing value of protected friend data member from another module") 
                       End If
                Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try

		Dim myC2 As New C2()
		myC2.S()

	End Sub
End Module
