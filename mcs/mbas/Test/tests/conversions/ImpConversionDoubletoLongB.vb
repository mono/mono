'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDoubletoLongC
	Sub Main()
			Dim a as Double = 111.9
			Dim b as Long = 111 + a
			if b <> 223
				Throw new System.Exception("Addition of Double & Long not working. Expected 223 but got " &b) 
			End if		
	End Sub
End Module
