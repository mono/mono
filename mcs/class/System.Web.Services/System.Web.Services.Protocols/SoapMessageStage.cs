// 
// System.Web.Services.Protocols.SoapMessageStage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {

	[Serializable]
	public enum SoapMessageStage {
		AfterDeserialize = 0x8,
		AfterSerialize = 0x2,
		BeforeDeserialize = 0x4,
		BeforeSerialize = 0x1
	}
}
