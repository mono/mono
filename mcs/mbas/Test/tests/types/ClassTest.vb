Imports System
Class C1
	Public a As Integer=47
	Public Sub S()
		Dim myInnerC As New C2()
		Dim b As Integer=myInnerC.H()
		If b<>147 Then
			Throw New Exception("#A5:Error in accessing inner class")
		End If
	End Sub
	Public Function G() As Integer
		Return 38
	End Function
	Class C2
		Public Function H() As Integer
			Return 147
		End Function
	End Class
End Class
Module M
	Sub Main()
		Try
			Dim myCInstance As New C1()
		Catch e As Exception
			Console.WriteLine("#A1:Error in creating class instance:"+e.Message)
		End Try
		
		Dim myC As New C1()

		Try
			If myC.a<>47 Then
				Throw New Exception("#A2-ClassTest:Failed-Error in accessing class data member")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
		
		Try
			myC.S()
		Catch e As Exception
			Console.WriteLine("#A3:Error in accessing Sub from a class:"+e.Message)
		End Try
		
		Try
			Dim c As Integer=myC.G()
			If c<>38 Then
				Throw New Exception("#A4-ClassTest:Failed-Error in  accessing Function of a class")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
