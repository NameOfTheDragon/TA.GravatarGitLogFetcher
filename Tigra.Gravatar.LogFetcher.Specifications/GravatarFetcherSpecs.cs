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
using System.IO;
using System.Net.Http;
using FakeItEasy;
using Machine.Specifications;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    [Ignore("Uses async/await which upsets MSpec runner - needs a test sync context.")]
    [Subject(typeof(GravatarFetcher), "Unique Committers")]
    public class when_creating_a_new_gravatar_fetcher : with_fake_log_streamreader
        {
        Because of = () => { Fetcher = new GravatarFetcher(Reader); };
        It should_have_2_unique_committers = () => Fetcher.UniqueCommitterCount.ShouldEqual(2);
        It should_have_first_committer_tim_long = () => Fetcher.UniqueCommitters[0].Name.ShouldEqual("Tim Long");
        It should_have_second_committer_darth_vader = () => Fetcher.UniqueCommitters[1].Name.ShouldEqual("Darth Vader");
        static GravatarFetcher Fetcher;
        }

    [Ignore("Uses async/await which upsets MSpec runner - needs a test sync context.")]
    [Subject(typeof(GravatarFetcher), "Web service")]
    public class when_fetching_imagaes_from_gravatar_web_service : with_fake_gravatar_web_service
        {
        Establish context = () =>
            {
            Fetcher.UniqueCommitters.Clear();
            Fetcher.UniqueCommitters.Add(new Committer("Tim Long", "Tim@tigranetworks.co.uk"));
            };

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
        It should_request_the_gravatar_hash_for_tim_long = () => UriPath.ShouldEqual("avatar/df0478426c0e47cc5e557d5391e5255d");

        static string UriPath;
        }

    public class with_fake_gravatar_web_service : with_fake_log_streamreader
        {
        Establish context = () =>
            {
            MessageHandler = new FakeHttpMessageHandler();
            GravatarClient = new HttpClient(MessageHandler);
            Filesystem = A.Fake<FileSystemHelper>();
            Fetcher = new GravatarFetcher(Reader, GravatarClient, Filesystem);
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
            Fetcher = new GravatarFetcher(Reader);
            //Filesystem = A.Fake<FileSystemHelper>();
            };
        Because of = () => Fetcher.FetchGravatars(@"c:\");

        It should_create_gravatar_image_files_correctly;
        //=() => A.CallTo(() => Filesystem.SavePngImage(null, null)).MustHaveHappened(Repeated.Exactly.Once);
        //static GravatarFetcher Fetcher;
        //static FileSystemHelper Filesystem;
        }

    public class with_fake_log_streamreader
        {
        protected static StreamReader Reader;

        Establish context = () =>
            {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.WriteLine("Tim Long|Tim@tigranetworks.co.uk");
            writer.WriteLine("Tim Long|Tim@tigranetworks.co.uk");
            writer.WriteLine("Tim Long|Tim@tigranetworks.co.uk");
            writer.WriteLine("Darth Vader|Darth@deathstar.com"); // Darth has no gravatar
            writer.WriteLine("Darth Vader|Darth@deathstar.com");
            writer.WriteLine("Tim Long|Tim@tigranetworks.co.uk");
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            Reader = new StreamReader(stream);
            };
        }

    [Subject(typeof(GravatarFetcher), "Integration test")]
    public class when_fetching : with_fake_gravatar_web_service
        {
            Establish context = () =>
            {
                Fetcher = new GravatarFetcher(Reader, filesystem: Filesystem);  // Use real gravatar web server
                Fetcher.UniqueCommitters.Clear();
                Fetcher.UniqueCommitters.Add(new Committer("Tim Long", "Tim@tigranetworks.co.uk"));
            };
            Because of = () => Fetcher.FetchGravatars(@"c:\");
            It should_be_true = () => true.ShouldBeTrue();
        }
    }
