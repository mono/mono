'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoLongA
	Sub Main()
			Dim b1 as Boolean=False
			try
				Dim a as Long
				Dim b as String= "Program"
				a = b
				Catch e as System.Exception
					b1 = True					
			End Try
			if b1 = False then
				Throw new System.Exception("Conversion of String to Long not working. Expected Error: System.FormatException: Input string was not in a correct format... but didnt get it") 
			End if
	End Sub
End Module
