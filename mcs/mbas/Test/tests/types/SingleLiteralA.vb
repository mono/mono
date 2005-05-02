Imports System
Module SingleLiteral
	Sub Main()
			Dim a As Single=1.23F
			Dim b As Single=1.23E+10F
    	            Dim c As Single=92.23372E+17F 
			Dim d As Single=.23F
			Dim f As Single
                  if a<>1.23F Then
                        Throw new System.Exception("#A1-SingleLiteralA:Failed")
                  End If
                  If b<>1.23E+10F Then
                        Throw new System.Exception("#A2-SingleLiteralA:Failed")
                  End If
                  If c<>9.223372E+18F Then
                        Throw new System.Exception("#A3-SingleLiteralA:Failed")
                  End If
			If d<>0.23f Then
				Throw new System.Exception("#A4-SingleLiteralA:Failed")
			End If
			If f<>0 Then
                        Throw new System.Exception("#A5-SingleLiteralA:Failed")
                  End If
	End Sub
End Module
