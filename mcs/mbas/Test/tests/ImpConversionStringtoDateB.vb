'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.
Imports System
Module ImpConversionStringtoDateB
	Sub Main()
			Dim b1 as Boolean=False
			try
				Dim a as Date
				Dim b as String= "Program"
				a = b
				'Console.WriteLine("Value of a is {0}",a)
				'Console.WriteLine("Value of b is {0}",b)

				Catch e as System.Exception
					b1 = False					
			End Try
			if b1 = True then
				Throw new System.Exception("Conversion of String to Date not working. Expected Error: System.FormatException: Input string was not in a correct format... but didnt get it") 
			End if
	End Sub
End Module
