Imports System
Module DoubleLiteral
	Sub Main()
		Try
			Dim a As Double=1.23R
			Dim b As Double=1.23E+10R
       		        Dim c As Double=9223372036854775808R
			Dim d As Double=.23R
			Dim f As Double
                        If a<>1.23R Then
                                Console.WriteLine("#A1-DoubleLiteralA:Failed")
                        End If
                         If b<>1.23E+10R Then
                                Console.WriteLine("#A2-DoubleLiteralA:Failed")
                        End If
                         If c<>9.22337203685478E+18R
                                Console.WriteLine("#A3-DoubleLiteralA:Failed")
                        End If
			If d<>0.23R Then
				Console.WriteLine("#A4-DoubleLiteralA:Failed")
			End If
			If f<>0 Then
                                Console.WriteLine("#A5-DoubleLiteralA:Failed")
                        End If
                                                                                  
			
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
