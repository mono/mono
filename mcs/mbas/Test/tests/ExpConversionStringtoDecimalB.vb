'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ExpConversionStringtoDecimalA
	Sub Main()
			Dim b1 as Boolean=False
			try
				Dim a as Decimal
				Dim b as String= "Program"
				a = CDec(b)
				Catch e as System.Exception
					b1 = True					
			End Try
			if b1 = False then
				Throw new System.Exception("Conversion of String to Decimal not working. Expected Error: System.FormatException: Input string was not in a correct format... but didnt get it") 
			End if
	End Sub
End Module
