Imports System
Module SingleLiteral
	Sub Main()
		Try
			Dim a As Single=True
			If a<>-1 Then
                                Console.WriteLine("SingleLiteralB:Failed")
                        End If
                                
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
