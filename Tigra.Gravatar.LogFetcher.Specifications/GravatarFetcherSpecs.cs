// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GravatarFetcherSpecs.cs  Created: 2013-06-26@17:28
// Last modified: 2013-10-23@02:18 by Tim

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using Machine.Specifications;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    [Subject(typeof(GravatarFetcher), "Unique Committers")]
    public class when_creating_a_new_gravatar_fetcher : with_fake_committer_list
        {
        Because of = () => { Fetcher = new GravatarFetcher(Committers); };
        It should_have_2_unique_committers = () => Fetcher.UniqueCommitterCount.ShouldEqual(2);

        It should_have_first_committer_tim_long =
            () => Fetcher.UniqueCommitters.Count(p => p.Name == "Tim Long").ShouldEqual(1);

        It should_have_second_committer_darth_vader =
            () => Fetcher.UniqueCommitters.Count(p => p.Name == "Darth Vader").ShouldEqual(1);

        static GravatarFetcher Fetcher;
        }

    [Subject(typeof(GravatarFetcher), "Web service")]
    public class when_fetching_imagaes_from_gravatar_web_service : with_fake_gravatar_web_service
        {
        Because of = () =>
            {
            Fetcher.FetchGravatars(@"c:\").Wait(5000);
            //"http://www.gravatar.com/avatar/".md5_hex(lc $email)."?d=404&size=".$size; 
            UriPath = MessageHandler.RequestMessage.RequestUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            };

        It should_make_request_from_gravatar_dot_com =
            () => MessageHandler.RequestMessage.RequestUri.Host.ShouldEqual("www.gravatar.com");

        It should_make_a_get_request = () => MessageHandler.RequestMessage.Method.ShouldEqual(HttpMethod.Get);
        // see https://en.gravatar.com/site/check/tim@tigranetworks.co.uk
        It should_request_the_gravatar_hash_for_tim_long =
            () => UriPath.ShouldStartWith("avatar/df0478426c0e47cc5e557d5391e5255d");

        static string UriPath;
        }

    public class with_fake_gravatar_web_service : with_fake_committer_list
        {
        Establish context = () =>
            {
            MessageHandler = new LoggingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            GravatarClient = new HttpClient(MessageHandler);
            Filesystem = A.Fake<FakeFileSystemWrapper>();
            Fetcher = new GravatarFetcher(Committers, GravatarClient, Filesystem);
            };

        protected static GravatarFetcher Fetcher;
        protected static LoggingHttpMessageHandler MessageHandler;
        protected static HttpClient GravatarClient;
        protected static FakeFileSystemWrapper Filesystem;
        }

    [Subject(typeof(GravatarFetcher), "Saving to disk")]
    public class when_fetching_gravatars_for_tim_and_darth : with_fake_gravatar_web_service
        {
        Establish context = () =>
            {
            Fetcher = new GravatarFetcher(Committers);
            //Filesystem = A.Fake<FakeFileSystemWrapper>();
            };

        Because of = () => Fetcher.FetchGravatars(@"c:\");

        It should_create_gravatar_image_files_correctly;
        //=() => A.CallTo(() => Filesystem.SaveImage(null, null)).MustHaveHappened(Repeated.Exactly.Once);
        //static GravatarFetcher Fetcher;
        //static FakeFileSystemWrapper Filesystem;
        }

    public class with_fake_committer_list
        {
        protected static IEnumerable<Committer> Committers;

        Establish context = () =>
            {
            Committers = new SortedSet<Committer>
                {
                new Committer("Tim Long", "Tim@tigranetworks.co.uk"),
                new Committer("Darth Vader", "Darth@deathstar.space"), // Darth has no gravatar
                };
            };
        }

    /// <summary>
    ///   SUT: GravatarFetcher.FetchSingleGravatar
    ///   This method exercises the following behaviours:
    ///   Calculating the Gravatar hash;
    ///   Constructing the URL and making a web request;
    ///   Receiving the result asynchronously.
    ///   Because of the asynchronicity, a more complicated test is required that allows us to
    ///   marshall the asynchronous code back onto the test context.
    ///   We've employed Jon Skeet's 'time machine' technique.
    /// </summary>
    [Subject(typeof(GravatarFetcher), "FetchSingleGravatar happy path")]
    public class when_fetching_a_single_gravatar_and_the_gravatar_exists : with_mock_filesystem
        {
        const string expectedHash = "df0478426c0e47cc5e557d5391e5255d";

        Establish context = () =>
            {
            // Mock the filesystem so that it reports that the Git repo and the output folder exist.
            A.CallTo(
                () => FakeFileSystem.DirectoryExists(A<string>.That.IsEqualTo(RuntimeEnvironment.OutputDirectory)))
                .Returns(true);
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.EndsWith(".git"))).Returns(true);

            Tardis = new TimeMachine();

            // Create fake HttpContent that is loaded with a Gravatar icon.
            // We set the fake content object up to return a file stream instead of the memory stream
            // that HttpContent woudl normally return.
            Content = new HttpContentFromFile(RuntimeEnvironment.GravatarFileTimLong);
            // Create HttpResponseMessage and inject fake content. Response headers can probably be left unset.
            var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = Content};
            // Sets response headers as received from Gravatar (observed in Fiddler). This is probably unnecessary.
            response.SetGravatarHeaders();  
            // Schedule success for HttpClient.SendAsync at time T=1 and save the task result.
            WebClientSuccess = Tardis.ScheduleSuccess(1, response);
            Tardis.ScheduleSuccess(2, new object());
            // Create TimeMachineMessageHandler and inject the prepared task result.
            MessageHandler = new TimeMachineMessageHandler(WebClientSuccess);
            // Create the WebClient and inject the prepared TimeMachineMessageHandler.
            WebClient = new HttpClient(MessageHandler, false);
            // Create the GravatarFetcher class (the unit under test) and inject the prepared objects.
            Committers = new List<Committer> {new Committer("Tim Long", "Tim@tigranetworks.co.uk")};
            Fetcher = new GravatarFetcher(Committers, WebClient, FakeFileSystem);
            };

        // This is a bit odd because I'm making assertions in my "Act" phase, but assertions about the task
        // states can only be done here. This is a double-check that things are going as I intended.
        Because of = () => Tardis.ExecuteInContext(advancer =>
            {
            fetcherTask = Fetcher.FetchSingleGravatar(Committers.First(), RuntimeEnvironment.OutputDirectory);
            fetcherTask.Status.ShouldEqual(TaskStatus.WaitingForActivation);
            advancer.Advance(); // Advance to T=1; Web request completes successfully.
            WebClientSuccess.Status.ShouldEqual(TaskStatus.RanToCompletion);
            MessageHandler.RequestMessage.ShouldNotBeNull();
            advancer.AdvanceTo(99); // Make sure everything has run to completion.
            });

        // Now we can make assertions on all of the tasks involved in the surety that they have all completed.
        It should_send_a_web_request_to_gravatar =
            () => MessageHandler.RequestMessage.RequestUri.Host.ShouldEqual("www.gravatar.com");

        It should_use_the_http_get_method = () => MessageHandler.RequestMessage.Method.ShouldEqual(HttpMethod.Get);

        It should_request_the_expected_url_and_gravatar_hash =
            () => MessageHandler.RequestMessage.RequestUri.PathAndQuery.ShouldStartWith("/avatar/" + expectedHash);

        It should_request_a_portable_network_graphic = () =>
            MessageHandler.RequestMessage.RequestUri.PathAndQuery.ShouldContain(".png");

        It should_request_image_size_90_pixels =
            () => MessageHandler.RequestMessage.RequestUri.Query.ShouldContain("size=90");

        It should_request_a_g_rated_image =
            () => MessageHandler.RequestMessage.RequestUri.Query.ShouldContain("rating=g");

        It should_request_a_http404_response_if_no_gravatar_exists =
            () => MessageHandler.RequestMessage.RequestUri.Query.ShouldContain("default=404");

        It should_write_the_image_to_the_correct_file = () =>
            A.CallTo(() => FakeFileSystem.SaveImage(
                A<string>.Ignored,
                A<Bitmap>.Ignored,
                A<ImageFormat>.Ignored)
                )
                .MustHaveHappened(Repeated.Exactly.Once);

        static TimeMachine Tardis;
        static Task<HttpResponseMessage> WebClientSuccess;
        static HttpClient WebClient;
        static GravatarFetcher Fetcher;
        static List<Committer> Committers;
        static TimeMachineMessageHandler MessageHandler;
        static HttpContentFromFile Content;
        static Task fetcherTask;
        }

    internal static class FakeHttpResponse
        {
        internal static void SetGravatarHeaders(this HttpResponseMessage response)
            {
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Cache-Control", "max-age=300");
            response.Headers.Add("Date", "Mon, 14 Oct 2013 03:46:44 GMT");
            response.Headers.Add("Server", "ECS (lhr/4BFE)");
            response.Headers.Add("Source-Age", "0");
            response.Headers.Add("Via", "1.1 varnish");
            response.Headers.Add("X-Cache", "HIT");
            response.Headers.Add("X-Varnish", "3901214514 3901214096");
            // Even though Fiddler shows these headers in the response packet, ASP Web API will not allow them...
            //response.Headers.Add("Content-Disposition", "inline; filename=\"df0478426c0e47cc5e557d5391e5255d.jpeg\"");
            //response.Headers.Add("Content-Type", "image/jpeg");
            //response.Headers.Add("Expires", "Mon, 14 Oct 2013 03:51:44 GMT");
            //response.Headers.Add("Last-Modified", "Tue, 16 Apr 2013 22:31:07 GMT");
            //response.Headers.Add("Content-Length", "2990");
            }
        }

    /// <summary>
    ///   Class HttpContentFromFile.
    ///   A unit test helper that enables HttpContent to be streamed from a file.
    ///   Given suitable fakes and set-up, this enables repeatable playback of
    ///   previously captured HTTP responses.
    /// </summary>
    internal class HttpContentFromFile : HttpContent
        {
        String sourceFile;
        readonly Stream sourceStream;
        internal Task CopyTask;

        /// <summary>
        ///   Initializes a new instance of the <see cref="HttpContentFromFile" /> class.
        /// </summary>
        /// <param name="sourceFile">The file containing the HTTP content body to be played back.</param>
        public HttpContentFromFile(string sourceFile)
            {
            this.sourceFile = sourceFile;
            sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
            CopyTask = sourceStream.CopyToAsync(stream);
            return CopyTask;
            }

        protected override bool TryComputeLength(out long length)
            {
            length = sourceStream.Length;
            return true;
            }
        }

    /// <summary>
    ///   Class RuntimeEnvironment. Provides information about the execution environment.
    /// </summary>
    internal static class RuntimeEnvironment
        {
        internal const string fileTimLong = "df0478426c0e47cc5e557d5391e5255d.jpeg";

        internal static Assembly Assembly { get; private set; }
        internal static string FullPathToAssembly { get; private set; }
        internal static string AssemblyDirectory { get; private set; }
        internal static string GravatarDirectory { get; private set; }
        internal static string OutputDirectory { get; private set; }

        internal static string GravatarFileTimLong { get { return Path.Combine(GravatarDirectory, fileTimLong); } }

        static RuntimeEnvironment()
            {
            Assembly = Assembly.GetExecutingAssembly();
            FullPathToAssembly = Assembly.Location;
            AssemblyDirectory = Path.GetDirectoryName(FullPathToAssembly);
            GravatarDirectory = Path.Combine(AssemblyDirectory, "Gravatars");
            OutputDirectory = Path.Combine(AssemblyDirectory, "Output");
            }
        }
    }
