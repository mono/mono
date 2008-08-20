//
// IssuedSecurityTokenProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Security.Tokens
{
	public class IssuedSecurityTokenProvider : SecurityTokenProvider, ICommunicationObject
	{
		public IssuedSecurityTokenProvider ()
		{
		}

		IssuedTokenCommunicationObject comm =
			new IssuedTokenCommunicationObject ();

		SecurityKeyEntropyMode entropy_mode =
			SecurityKeyEntropyMode.CombinedEntropy;
		TimeSpan max_cache_time = TimeSpan.MaxValue;
		MessageSecurityVersion version = MessageSecurityVersion.Default;
		int threshold = 60;
		IdentityVerifier verifier = IdentityVerifier.CreateDefault ();
		bool cache_issued_tokens = true;
		Collection<XmlElement> request_params =
			new Collection<XmlElement> ();

		CommunicationState state = CommunicationState.Created;

		internal IssuedTokenCommunicationObject Communication {
			get { return comm; }
		}

		public bool CacheIssuedTokens {
			get { return cache_issued_tokens; }
			set { cache_issued_tokens = value; }
		}

		public virtual TimeSpan DefaultCloseTimeout {
			get { return comm.DefaultCloseTimeout; }
		}

		public virtual TimeSpan DefaultOpenTimeout {
			get { return comm.DefaultOpenTimeout; }
		}

		public IdentityVerifier IdentityVerifier {
			get { return verifier; }
			set { verifier = value; }
		}

		public int IssuedTokenRenewalThresholdPercentage {
			get { return threshold; }
			set { threshold = value; }
		}

		public EndpointAddress IssuerAddress {
			get { return comm.IssuerAddress; }
			set { comm.IssuerAddress = value; }
		}

		public Binding IssuerBinding {
			get { return comm.IssuerBinding; }
			set { comm.IssuerBinding = value; }
		}

		public KeyedByTypeCollection<IEndpointBehavior> IssuerChannelBehaviors {
			get { return comm.IssuerChannelBehaviors; }
		}

		public SecurityKeyEntropyMode KeyEntropyMode {
			get { return entropy_mode; }
			set { entropy_mode = value; }
		}

		public TimeSpan MaxIssuedTokenCachingTime {
			get { return max_cache_time; }
			set { max_cache_time = value; }
		}

		public MessageSecurityVersion MessageSecurityVersion {
			get { return version; }
			set { version = value; }
		}

		public SecurityAlgorithmSuite SecurityAlgorithmSuite {
			get { return comm.SecurityAlgorithmSuite; }
			set { comm.SecurityAlgorithmSuite = value; }
		}

		public SecurityTokenSerializer SecurityTokenSerializer {
			get { return comm.SecurityTokenSerializer; }
			set { comm.SecurityTokenSerializer = value; }
		}

		public EndpointAddress TargetAddress {
			get { return comm.TargetAddress; }
			set { comm.TargetAddress = value; }
		}

		public Collection<XmlElement> TokenRequestParameters {
			get { return request_params; }
		}

		// SecurityTokenProvider

		[MonoTODO ("support it then")]
		public override bool SupportsTokenCancellation {
			get { return true; }
		}

		[MonoTODO]
		protected override SecurityToken GetTokenCore (TimeSpan timeout)
		{
			if (State != CommunicationState.Opened)
				throw new InvalidOperationException ("Open the provider before issuing actual request to get token.");
			return comm.GetToken (timeout);
		}

		[MonoTODO]
		protected override IAsyncResult BeginGetTokenCore (
			TimeSpan timeout,
			AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override SecurityToken EndGetTokenCore (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		// ICommunicationObject

		public CommunicationState State {
			get { return comm.State; }
		}

		[MonoTODO]
		public void Abort ()
		{
			comm.Abort ();
		}

		public void Open ()
		{
			comm.Open ();
		}

		[MonoTODO]
		public void Open (TimeSpan timeout)
		{
			comm.Open (timeout);
		}

		public IAsyncResult BeginOpen (AsyncCallback callback, object state)
		{
			return comm.BeginOpen (callback, state);
		}

		[MonoTODO]
		public IAsyncResult BeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return comm.BeginOpen (timeout, callback, state);
		}

		[MonoTODO]
		public void EndOpen (IAsyncResult result)
		{
			comm.EndOpen (result);
		}

		public void Close ()
		{
			comm.Close ();
		}

		[MonoTODO]
		public void Close (TimeSpan timeout)
		{
			comm.Close (timeout);
		}

		public IAsyncResult BeginClose (AsyncCallback callback, object state)
		{
			return comm.BeginClose (callback, state);
		}

		[MonoTODO]
		public IAsyncResult BeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return comm.BeginClose (timeout, callback, state);
		}

		[MonoTODO]
		public void EndClose (IAsyncResult result)
		{
			comm.EndClose (result);
		}

		public void Dispose ()
		{
			Close ();
		}

		public event EventHandler Opened {
			add { comm.Opened += value; }
			remove { comm.Opened -= value; }
		}
		public event EventHandler Opening {
			add { comm.Opening += value; }
			remove { comm.Opening -= value; }
		}
		public event EventHandler Closed {
			add { comm.Closed += value; }
			remove { comm.Closed -= value; }
		}
		public event EventHandler Closing {
			add { comm.Closing += value; }
			remove { comm.Closing -= value; }
		}
		public event EventHandler Faulted {
			add { comm.Faulted += value; }
			remove { comm.Faulted -= value; }
		}
	}
}
