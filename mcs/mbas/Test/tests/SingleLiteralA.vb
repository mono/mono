Imports System
Module SingleLiteral
	Sub Main()
		Try
			Dim a As Single=1.23F
			Dim b As Single=1.23E+10F
       		        Dim c As Single=9223372036854775808F
			Dim d As Single=.23F
			Dim f As Single
                        If a<>1.23F Then
                                Console.WriteLine("#A1-SingleLiteralA:Failed")
                        End If
                         If b<>1.23E+10F Then
                                Console.WriteLine("#A2-SingleLiteralA:Failed")
                        End If
                         If c<>9.223372E+18F Then
                                Console.WriteLine("#A3-SingleLiteralA:Failed")
                        End If
			If d<>0.23 Then
				Console.WriteLine("#A4-SingleLiteralA:Failed")
			End If
			If f<>0 Then
                                Console.WriteLine("#A5-SingleLiteralA:Failed")
                        End If
                                                                                  
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
