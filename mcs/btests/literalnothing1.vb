Imports System
Module LiteralNothing
	Sub Main()
		Try
			Dim a As String="Hello"
			a=Nothing
			Dim b As String=a.Substring(2)
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
