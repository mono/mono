'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionIntegertoLongC
	Sub Main()
			Dim a as Integer= 111
			Dim b as Long = 111 + a
			if b <> 222
				Throw new System.Exception("Addition of Integer& Long not working. Expected 222 but got " &b) 
			End if		
	End Sub
End Module
