'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Delegate Function DoubleFunc(x As Double)as Double
Delegate Function DoubleFunc1(x As Double)as Double
Class A
	Private f1 As New DoubleFunc1(AddressOf Square)
	Private f As New DoubleFunc(AddressOf Square)
 	  Overloads Shared Function Square(x As Single) As Single
		Return x * x
	End Function 
	Overloads Shared Function Square(x As Double) as Double
		Return x * x
	End Function 
End Class
Module M
        Sub Main()
        End Sub
End Module
