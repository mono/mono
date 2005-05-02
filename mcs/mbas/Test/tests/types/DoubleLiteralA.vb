Imports System
Module DoubleLiteral
	Sub Main()
			Dim a As Double=1.23R
			Dim b As Double=1.23E+10R
       		Dim c As Double=92.2337203685478E+17
			Dim d As Double=.23R
			Dim f As Double
                  If a<>1.23R Then
                        Throw new System.Exception("#A1-DoubleLiteralA:Failed")
                  End If
                  If b<>1.23E+10R Then
                        Throw new System.Exception("#A2-DoubleLiteralA:Failed")
                  End If
                  If c<>9.22337203685478E+18
                        Throw new System.Exception("#A3-DoubleLiteralA:Failed"&c)
                  End If
			If d<>0.23R Then
				Throw new System.Exception("#A4-DoubleLiteralA:Failed")
			End If
			If f<>0 Then
                         Throw new System.Exception("#A5-DoubleLiteralA:Failed")
                  End If
	End Sub
End Module
