'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Delegate Function DoubleFunc(x As Double)as Double

Class A
	Public f As New DoubleFunc(AddressOf Square)
	Function Square(x As Double) as Double
		Return x * x
	End Function 
End Class

Class AA
	Inherits A
	Function Square(x As Double) as Double
		Return x * x
	End Function 
End Class

Module M	  
        Sub Main()
		
        End Sub
End Module
