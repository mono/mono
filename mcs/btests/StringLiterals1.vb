Imports System
Module StringLiteral
	Sub Main()
		Try
			Dim a As String="Hello"
			Dim b As String="World"
			Dim c As String=a*b
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	
	End Sub
End Module
