'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Module ExpConversionStringtoByteA
	Sub Main()
			Dim b1 as Boolean=False
			try
				Dim a as Byte
				Dim b as String= "Program"
				a = CByte(b)
				Catch e as System.Exception
					b1 = True					
			End Try
			if b1 = False then
				Throw new System.Exception("Conversion of String to Byte not working. Expected Error: System.FormatException: Input string was not in a correct format... but didnt get it") 
			End if
	End Sub
End Module
