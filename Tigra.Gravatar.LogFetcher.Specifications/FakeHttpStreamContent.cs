using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TiGra;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /// <summary>
    /// Class FakeHttpStreamContent.
    /// Simulates an HTTP content body that can be read as a stream.
    /// In the case of a real HTTP response, the stream would typically be a MemoryStream,
    /// but in this simulated class, the caller is able to inject any appropriate stream
    /// object in the constructor.
    /// </summary>
    internal class FakeHttpStreamContent : HttpContent
        {
        const int defaultBufferSize = 4096;
        int bufferSize;
        Stream content;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeGravatarHttpContent"/> class.
        /// </summary>
        /// <param name="stream">The name of the image on disk to be used as the source of the stream stream.</param>
        /// <param name="bufferSize">Size of the internal buffer. Typically this parameter can be defaulted.</param>
        public FakeHttpStreamContent(Stream stream, int bufferSize = defaultBufferSize)
            {
            this.bufferSize = bufferSize;
            this.content = stream;
            }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeHttpStreamContent"/> class
        /// with a <see cref="FileStream"/> around the specified file on disk.
        /// </summary>
        /// <param name="file">The file containing the simulated content. The file will be opened readonly, shareable and optimized for sequential scan.</param>
        /// <param name="bufferSize">Size of the internal buffer. Typically this parameter can be defaulted.</param>
        public FakeHttpStreamContent(string file, int bufferSize = defaultBufferSize)
            : this(new FileStream(file, FileMode.Open), bufferSize) {}

        /*new FileStream(file/*,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize,
            FileOptions.SequentialScan),
            bufferSize) {}*/

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
            return this.content.CopyToAsync(stream, this.bufferSize);
            }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HHTP content.</param>
        /// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.</returns>
        protected override bool TryComputeLength(out long length)
            {
            try
                {
                length = this.content.Length;
                return true;
                }
            catch (NotSupportedException ex)
                {
                Diagnostics.TraceError(ex);
                length = -1;
                return false;
                }
            }
        }
    }