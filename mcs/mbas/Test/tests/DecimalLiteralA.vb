Imports System
Module DecimalLiteral
	Sub Main()
		Try
			Dim a As Decimal=1.23D
			Dim b As Decimal=1.23E+10D
       		        Dim c As Decimal=9223372036854775808D
			Dim d As Decimal=.23D
			Dim f As Decimal
                        If a<>1.23D Then
                                Console.WriteLine("#A1-DecimalLiteralA:Failed")
                        End If
                         If b<>1.23E+10D Then
                                Console.WriteLine("#A2-DecimalLiteralA:Failed")
                        End If
                         If c<>9.22337203685478E+18D Then
                                Console.WriteLine("#A3-DecimalLiteralA:Failed")
                        End If
			If d<>0.23D Then
				Console.WriteLine("#A4-DecimalLiteralA:Failed")
			End If
			If f<>0 Then
                                Console.WriteLine("#A5-DecimalLiteralA:Failed")
                        End If
                                                                                  
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
