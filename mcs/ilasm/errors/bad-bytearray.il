//
// Mono.ILASM.Tests.bad_bytearray
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright 2004 Novell, Inc (http://www.novell.com)
//

//
// This test should not compile as it has a bytearray that is evil
//

.assembly extern mscorlib { }
.assembly bad_bytearray { }


.data some_data = bytearray (00 0G)



