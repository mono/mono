'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoByteC
	Sub Main()
			Dim a as Decimal = 111.9
			Dim b as Byte = 111 + a
			if b <> 223
				Throw new System.Exception("Addition of Decimal & Byte not working. Expected 223 but got " &b) 
			End if		
	End Sub
End Module
