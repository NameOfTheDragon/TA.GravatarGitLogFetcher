// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: FakeHttpMessageHandler.cs  Created: 2013-07-07@04:19
// Last modified: 2013-10-14@05:14 by Tim

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /// <summary>
    ///   Class FakeHttpMessageHandler.
    ///   Provides a fake HttpMessageHandler that can be injected into HttpClient.
    ///   The class requires a ready-made response message to be passed in the constructor,
    ///   which is simply returned when requested. Additionally, the web request is captured in the
    ///   RequestMessage property for later examination.
    /// </summary>
        public class FakeHttpMessageHandler : DelegatingHandler
        {
            internal HttpResponseMessage ResponseMessage { get; private set; }
            internal HttpRequestMessage RequestMessage { get; private set; }

            public FakeHttpMessageHandler(HttpResponseMessage responseMessage)
            {
                ResponseMessage = responseMessage;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                RequestMessage = request;
                return Task.FromResult(ResponseMessage);
            }
        }
    }
