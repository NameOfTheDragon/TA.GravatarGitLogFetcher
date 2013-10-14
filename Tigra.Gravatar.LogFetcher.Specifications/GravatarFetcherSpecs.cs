// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GravatarFetcherSpecs.cs  Created: 2013-06-26@17:28
// Last modified: 2013-06-26@18:02 by Tim

/*
 * GravatarFetcher should behave as follows:
 * 
 * When created with a valid Git log file,
 *  It should produce the set of unique committers in the log
 *  It should return the count of unique committers.
 *  
 * Given a correctly constructed GravatarFetcher,
 * When passed a directory path for saving images,
 * Then (for each committer, in parallel)
 *  It should connect to the Gravatar server and retrieve that user's image in PNG format
 *  It should save the image under the user's full name, in the format 'user name.png'.
 *  It should attempt to fetch the same number of images as there are unique committers.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
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

    [Ignore("Uses async/await which upsets MSpec runner - needs a test sync context."),
     Subject(typeof(GravatarFetcher), "Web service")]
    public class when_fetching_imagaes_from_gravatar_web_service : with_fake_gravatar_web_service
        {
        Because of = () =>
            {
            Fetcher.FetchGravatars(@"c:\");
            //"http://www.gravatar.com/avatar/".md5_hex(lc $email)."?d=404&size=".$size; 
            UriPath = MessageHandler.RequestMessage.RequestUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            };

        It should_make_request_from_gravatar_dot_com =
            () => MessageHandler.RequestMessage.RequestUri.Host.ShouldEqual("www.gravatar.com");

        It should_make_a_get_request = () => MessageHandler.RequestMessage.Method.ShouldEqual(HttpMethod.Get);
        // see https://en.gravatar.com/site/check/tim@tigranetworks.co.uk
        It should_request_the_gravatar_hash_for_tim_long =
            () => UriPath.ShouldEqual("avatar/df0478426c0e47cc5e557d5391e5255d");

        static string UriPath;
        }

    public class with_fake_gravatar_web_service : with_fake_committer_list
        {
        Establish context = () =>
            {
            MessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            GravatarClient = new HttpClient(MessageHandler);
            Filesystem = A.Fake<FileSystemHelper>();
            Fetcher = new GravatarFetcher(Committers, GravatarClient, Filesystem);
            };

        protected static GravatarFetcher Fetcher;
        protected static FakeHttpMessageHandler MessageHandler;
        protected static HttpClient GravatarClient;
        protected static FileSystemHelper Filesystem;
        }

    [Subject(typeof(GravatarFetcher), "Saving to disk")]
    public class when_fetching_gravatars_for_tim_and_darth : with_fake_gravatar_web_service
        {
        Establish context = () =>
            {
            Fetcher = new GravatarFetcher(Committers);
            //Filesystem = A.Fake<FileSystemHelper>();
            };

        Because of = () => Fetcher.FetchGravatars(@"c:\");

        It should_create_gravatar_image_files_correctly;
        //=() => A.CallTo(() => Filesystem.SavePngImage(null, null)).MustHaveHappened(Repeated.Exactly.Once);
        //static GravatarFetcher Fetcher;
        //static FileSystemHelper Filesystem;
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

    // FetchSingleGravatar

    [Subject(typeof(GravatarFetcher), "Web request")]
    public class when_fetching_a_single_gravatar_and_the_gravatar_exists : with_mock_filesystem
        {
        Establish context = () =>
            {
            Tardis = new TimeMachine();
            var gravatarSuccess = Tardis.ScheduleSuccess<HttpResponseMessage>(1,FakeHttpResponse.GravatarForTimLong200());
            ReplayWebClient = new HttpClient(new TimeMachineMessageHandler(gravatarSuccess));
            GitCommitter = new Committer("Tim Long", "Tim@tigranetworks.co.uk");
            //ToDo: prime the FakeFileSystem?
            var committers = new List<Committer> {GitCommitter};
            GravatarClient = new GravatarFetcher(committers, ReplayWebClient, FakeFileSystem);
            };

        Because of = () => GravatarClient.FetchSingleGravatar(GitCommitter,
            Path.Combine(RuntimeEnvironment.OutputDirectory, RuntimeEnvironment.fileTimLong));

        It should_send_web_request_to_gravatar;
        It should_request_the_hash_code_for_tim_long;
        It should_write_the_image_to_the_correct_file;
         
        static TimeMachine Tardis;
        static HttpClient ReplayWebClient;
        static GravatarFetcher GravatarClient;
        static Committer GitCommitter;
        }

    internal static class FakeHttpResponse
        {
        internal static HttpResponseMessage GravatarForTimLong200()
            {
            var gravatarStream = new FileStream(RuntimeEnvironment.GravatarFileTimLong, FileMode.Open);
            // Response headers pasted from Fiddler2
            var response = new HttpResponseMessage(HttpStatusCode.OK) {Content = new StreamContent(gravatarStream)};
            response.Headers.Add("Accept-Ranges", "bytes");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Cache-Control", "max-age=300");
            response.Headers.Add("Content-Disposition", "inline; filename=\"df0478426c0e47cc5e557d5391e5255d.jpeg\"");
            response.Headers.Add("Content-Type", "image/jpeg");
            response.Headers.Add("Date", "Mon, 14 Oct 2013 03:46:44 GMT");
            response.Headers.Add("Expires", "Mon, 14 Oct 2013 03:51:44 GMT");
            response.Headers.Add("Last-Modified", "Tue, 16 Apr 2013 22:31:07 GMT");
            response.Headers.Add("Server", "ECS (lhr/4BFE)");
            response.Headers.Add("Source-Age", "0");
            response.Headers.Add("Via", "1.1 varnish");
            response.Headers.Add("X-Cache", "HIT");
            response.Headers.Add("X-Varnish", "3901214514 3901214096");
            //response.Headers.Add("Content-Length", "2990");
            
            return response;
            }
        }

    /// <summary>
    /// Class RuntimeEnvironment. Provides information about the execution environment.
    /// </summary>
    internal static class RuntimeEnvironment
        {
        internal const string fileTimLong = "df0478426c0e47cc5e557d5391e5255d.jpeg";

        internal static Assembly Assembly { get; private set; }
        internal static string FullPathToAssembly { get; private set; }
        internal static string AssemblyDirectory { get; private set; }
        internal static string GravatarDirectory { get; private set; }
        internal static string OutputDirectory { get; private set; }

        internal static string GravatarFileTimLong
            {
            get { return Path.Combine(GravatarDirectory, fileTimLong); }
            }

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