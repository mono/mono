'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionSingletoIntA
	Sub Main()
		Dim i as Boolean = False
		try
			Dim a as Integer 
			Dim b as Single = 3000000000
			a = CInt(b)
		Catch e as System.Exception
				System.Console.WriteLine(" Arithmetic operation resulted in an overflow.")			
				i = True				
		End Try		
		if i = False Then
				System.Console.WriteLine("Single to Integer Conversion is not working properly.")		
		End if		
	End Sub
End Module
