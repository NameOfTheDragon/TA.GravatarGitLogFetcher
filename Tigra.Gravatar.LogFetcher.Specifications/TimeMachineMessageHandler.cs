using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /// <summary>
    /// Class TimeMachineMessageHandler.
    /// The caller must supply a populated <see cref="Task{HttpResponseMessage}"/> in the constructor.
    /// Typically, this task will have been returned from <see cref="TimeMachine.ScheduleSuccess{T}"/>.
    /// Upon calling <see cref="SendAsync"/>, the request is recorded for later inspection, but not sent.
    /// Instead, the preset response task is immediately returned.
    /// This provides a way to replay known responses on demand, without actually sending any web traffic.
    /// When used in conjunction with the <see cref="TimeMachine"/> class, the exact timing and sequencing
    /// can also be controlled, which is ideal for unit testing.
    /// </summary>
    public class TimeMachineMessageHandler : DelegatingHandler
        {
        internal Task<HttpResponseMessage> ResponseTask { get; private set; }
        internal HttpRequestMessage RequestMessage { get; private set; }

        public TimeMachineMessageHandler(Task<HttpResponseMessage> responseTask)
            {
            ResponseTask = responseTask;
            }

        /// <summary>
        /// Records the request message for later inspection, but doesn't actually send it.
        /// Instead, the preset task is immediately returned.
        /// </summary>
        /// <param name="request">The web API request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>System.Threading.Tasks.Task{System.Net.Http.HttpResponseMessage}.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
            RequestMessage = request;
            return ResponseTask;
            }
        }
    }