Imports System
Module DecimalLiteral
	Sub Main()
		Dim a As Decimal=1.23D
		Dim b As Decimal=1.23E+10D
		Dim c As Decimal=9.2233720368547808E+18D
		Dim d As Decimal=.23D
		Dim f As Decimal
		If a<>1.23D Then
			Throw New Exception ("#A1-DecimalLiteralA:Failed") 
		End If
		If b<>1.23E+10D Then
			Throw New Exception ("#A1-DecimalLiteralA:Failed")
		End If
		If c<>9.2233720368547808E+18D Then
			Throw New Exception ("#A1-DecimalLiteralA:Failed") 
		End If
		If d<>0.23D Then
			Throw New Exception ("#A1-DecimalLiteralA:Failed") 
		End If
		If f<>0 Then
			Throw New Exception ("#A1-DecimalLiteralA:Failed") 
		End If             
	End Sub
End Module
