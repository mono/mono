'Author:
'   K. Satya Sudha (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
                                                                                                               
Imports System
Module Test
        Sub Main()
                Dim a As Boolean = "7" Like "[][!!][]"
                if a <> true then
                        throw new Exception ("#A1 - Like operator failed")
                end if
		
		a = "7" Like "[][!-][]"
                if a <> true then
                        throw new Exception ("#A2 - Like operator failed")
                end if

		a = "d" Like "[abc!def]"
                if a <> false then
                        throw new Exception ("#A3 - Like operator failed")
                end if
	
		a = "r" Like "[!-*]"
                if a <> true then
                        throw new Exception ("#A4 - Like operator failed")
                end if

		a = "-" Like "[!-*]"
                if a <> false then
                        throw new Exception ("#A5 - Like operator failed")
                end if

		a = "*" Like "[!-*]"
                if a <> false then
                        throw new Exception ("#A6 - Like operator failed")
                end if

		a = "%" Like "[!#--D]"
                if a <> false then
                        throw new Exception ("#A7 - Like operator failed")
                end if

		a = "A" Like "[?-D]"
                if a <> true then
                        throw new Exception ("#A8 - Like operator failed")
                end if

		a = "-" Like "[?-D]"
                if a <> false then
                        throw new Exception ("#A9 - Like operator failed")
                end if

		a = "-" Like "[--D]"
                if a <> true then
                        throw new Exception ("#A10 - Like operator failed")
                end if

		a = "0" Like "[--D]"
                if a <> true then
                        throw new Exception ("#A11 - Like operator failed")
                end if

		a = "+" Like "[*--D]"
                if a <> true then
                        throw new Exception ("#A12 - Like operator failed")
                end if

		a = "[" Like "[[-a]"
                if a <> true then
                        throw new Exception ("#A13 - Like operator failed")
                end if

		a = "-" Like "[a-c-e]"
                if a <> true then
                        throw new Exception ("#A14 - Like operator failed")
                end if

		a = "d" Like "[a-c-e]"
                if a <> false then
                        throw new Exception ("#A15 - Like operator failed")
                end if

		a = "b" Like "[a-c-e]"
                if a <> true then
                        throw new Exception ("#A16 - Like operator failed")
                end if

        End Sub
End Module

