'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDoubletoDecimalC
	Sub Main()
			Dim a as Double = 111.9
			Dim b as Decimal = 111.9 + a
			if b <> 223.8
				Throw new System.Exception("Addition of Double & Decimal not working. Expected 223.8 but got " &b) 
			End if		
	End Sub
End Module
