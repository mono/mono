'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionBytetoDecimalC
	Sub Main()
			Dim a as Byte = 111
			Dim b as Decimal = 111.9 + a
			if b <> 222.9
				Throw new System.Exception("Addition of Byte & Decimal not working. Expected 222.9 but got " &b) 
			End if		
	End Sub
End Module


