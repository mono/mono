'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionStringtoShortA
	Sub Main()
			Dim b1 as Boolean=False
			try
				Dim a as Short
				Dim b as String= "Program"
				a = CShort(b)
				Catch e as System.Exception
					b1 = True					
			End Try
			if b1 = False then
				Throw new System.Exception("Conversion of String to Short not working. Expected Error: System.FormatException: Input string was not in a correct format... but didnt get it") 
			End if
	End Sub
End Module
