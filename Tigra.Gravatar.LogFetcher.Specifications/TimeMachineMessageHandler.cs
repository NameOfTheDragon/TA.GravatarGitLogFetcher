// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: TimeMachineMessageHandler.cs  Created: 2013-10-14@07:47
// Last modified: 2013-10-23@02:37 by Tim

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /// <summary>
    ///   Class TimeMachineMessageHandler.
    ///   The caller must supply a populated <see cref="Task{TResult}" /> in the constructor.
    ///   Typically, this task will have been returned from <see cref="TimeMachine.ScheduleSuccess{T}" />.
    ///   Upon calling <see cref="SendAsync" />, the request is recorded for later inspection, but not sent.
    ///   Instead, the pre-set response task is immediately returned.
    ///   This provides a way to replay known responses on demand, without actually sending any web traffic.
    ///   When used in conjunction with the <see cref="TimeMachine" /> class, the exact timing and sequencing
    ///   can also be controlled, which is ideal for unit testing.
    /// </summary>
    public class TimeMachineMessageHandler : DelegatingHandler
        {
        internal Task<HttpResponseMessage> ResponseTask { get; private set; }

        /// <summary>
        ///   Gets a copy of the actual request message that would have been sent.
        /// </summary>
        /// <value>The request message that was created by the <see cref="HttpClient" />.</value>
        public HttpRequestMessage RequestMessage { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="TimeMachineMessageHandler" /> class.
        /// </summary>
        /// <param name="responseTask">
        ///   The response task to be returned. When using the <see cref="TimeMachine" />,
        ///   this is normally obtained by calling <see cref="TimeMachine.ScheduleSuccess{T}" />
        ///   or <see cref="TimeMachine.ScheduleFault{T}(int,System.Exception)" />. Otherwise
        ///   <see cref="Task.FromResult{TResult}" /> can be used to turn a result into a task.
        /// </param>
        public TimeMachineMessageHandler(Task<HttpResponseMessage> responseTask)
            {
            ResponseTask = responseTask;
            }

        /// <summary>
        ///   Records the request message for later inspection, but doesn't actually send it.
        ///   Instead, the ready-made completed task is immediately returned.
        /// </summary>
        /// <param name="request">The web API request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task{System.Net.Http.HttpResponseMessage}.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
            {
            RequestMessage = request;
            return ResponseTask;
            }
        }
    }
