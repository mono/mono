/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Messages
{
	
	/// <summary> Default implementation of Message interface.
	/// For Native Language Support (NLS), system of software internationalization.
	/// </summary>
	[Serializable]
	public class MessageImpl : Message
	{
		
		private const long serialVersionUID = - 3077643314630884523L;
		
		private System.String key;
		
		private System.Object[] arguments = new System.Object[0];
		
		public MessageImpl(System.String key)
		{
			this.key = key;
		}
		
		public MessageImpl(System.String key, System.Object[] args):this(key)
		{
			this.arguments = args;
		}
		
		public virtual System.Object[] GetArguments()
		{
			return this.arguments;
		}
		
		public virtual System.String GetKey()
		{
			return this.key;
		}
		
		public virtual System.String GetLocalizedMessage()
		{
			return GetLocalizedMessage(System.Threading.Thread.CurrentThread.CurrentCulture);
		}
		
		public virtual System.String GetLocalizedMessage(System.Globalization.CultureInfo locale)
		{
			return NLS.GetLocalizedMessage(GetKey(), locale, GetArguments());
		}
		
		public override System.String ToString()
		{
			System.Object[] args = GetArguments();
			System.String argsString = "";
			if (args != null)
			{
				for (int i = 0; i < args.Length; i++)
				{
					argsString += (args[i] + (i < args.Length?"":", "));
				}
			}
			return GetKey() + " " + argsString;
		}
	}
}
