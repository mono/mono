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
		AfterDeserialize,
		AfterSerialize,
		BeforeDeserialize,
		BeforeSerialize
	}
}
