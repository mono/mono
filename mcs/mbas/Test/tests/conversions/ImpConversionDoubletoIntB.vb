'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDoubletoIntegerC
	Sub Main()
			Dim a as Double = 111.9
			Dim b as Integer = 111 + a
			if b <> 223
				Throw new System.Exception("Addition of Double & Integer not working. Expected 223 but got " &b) 
			End if		
	End Sub
End Module
