Imports System
Module teststack
	Sub Main()
		'Dim a as Long = 1
		Dim c As Decimal=92233720368.54775808D
		'Dim b as Short = CShort(c)
		Dim b as Short =c 
		Console.WriteLine("Value of b is {0}", b)
		'if b<>123 then 
		'	Throw New System.Exception("Implicit Conversion of Long to Int has Failed")
		'End if		
	End Sub
End Module
