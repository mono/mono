'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoByteA
	Sub Main()
		Dim i as Boolean = False
		try
			Dim a as Byte
			Dim b as Double = 3000000000
			a = CDbl(b)
		Catch e as System.Exception
				System.Console.WriteLine(" Arithmetic operation resulted in an overflow.")			
				i = True				
		End Try		
		if i = False Then
				System.Console.WriteLine("Double to Byte Conversion is not working properly.")		
		End if		
	End Sub
End Module
