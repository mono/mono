//
// Mono.Net.Dns.DnsResponse
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright 2011 Gonzalo Paniagua Javier
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Mono.Net.Dns {
	class DnsResponse : DnsPacket {
		static readonly ReadOnlyCollection<DnsResourceRecord> EmptyRR = new ReadOnlyCollection<DnsResourceRecord> (new DnsResourceRecord [0]);
		static readonly ReadOnlyCollection<DnsQuestion> EmptyQS = new ReadOnlyCollection<DnsQuestion> (new DnsQuestion [0]);

		ReadOnlyCollection<DnsQuestion> question;
		ReadOnlyCollection<DnsResourceRecord> answer;
		ReadOnlyCollection<DnsResourceRecord> authority;
		ReadOnlyCollection<DnsResourceRecord> additional;
		int offset = DnsHeader.DnsHeaderLength;

		public DnsResponse (byte [] buffer, int length)
			: base (buffer, length)
		{
		}

		public void Reset ()
		{
			question = null;
			answer = null;
			authority = null;
			additional = null;
			for (int i = 0; i < packet.Length; i++)
				packet [i] = 0;
		}

		ReadOnlyCollection<DnsResourceRecord> GetRRs (int count)
		{
			if (count <= 0)
				return EmptyRR;

			List<DnsResourceRecord> records = new List<DnsResourceRecord>(count);
			for (int i = 0; i < count; i++)
				records.Add (DnsResourceRecord.CreateFromBuffer (this, position, ref offset));
			return records.AsReadOnly ();
		}

		ReadOnlyCollection<DnsQuestion> GetQuestions (int count)
		{
			if (count <= 0)
				return EmptyQS;

			List<DnsQuestion> records = new List<DnsQuestion> (count);
			for(int i = 0; i < count; i++) {
				DnsQuestion record = new DnsQuestion ();
				offset = record.Init (this, offset);
				records.Add (record);
			}
			return records.AsReadOnly ();
		}

		public ReadOnlyCollection<DnsQuestion> GetQuestions ()
		{
			if (question == null)
				question = GetQuestions (Header.QuestionCount);
			return question;
		}

		public ReadOnlyCollection<DnsResourceRecord> GetAnswers ()
		{
			if (answer == null) {
				GetQuestions ();
				answer = GetRRs (Header.AnswerCount);
			}
			return answer;
		}

		public ReadOnlyCollection<DnsResourceRecord> GetAuthority ()
		{
			if (authority == null) {
				GetQuestions ();
				GetAnswers ();
				authority = GetRRs (Header.AuthorityCount);
			}
			return authority;
		}

		public ReadOnlyCollection<DnsResourceRecord> GetAdditional ()
		{
			if (additional == null) {
				GetQuestions ();
				GetAnswers ();
				GetAuthority ();
				additional = GetRRs (Header.AdditionalCount);
			}
			return additional;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Header);
			sb.Append("Question:\r\n");
			foreach(DnsQuestion q in GetQuestions()) {
				sb.AppendFormat("\t{0}\r\n", q);
			}
			sb.Append("Answer(s):\r\n");
			foreach(DnsResourceRecord q in GetAnswers()) {
				sb.AppendFormat("\t{0}\r\n", q);
			}
			sb.Append("Authority:\r\n");
			foreach(DnsResourceRecord q in GetAuthority()) {
				sb.AppendFormat("\t{0}\r\n", q);
			}
			sb.Append("Additional:\r\n");
			foreach(DnsResourceRecord q in GetAdditional()) {
				sb.AppendFormat("\t{0}\r\n", q);
			}
			return sb.ToString();
		}
	}
}

