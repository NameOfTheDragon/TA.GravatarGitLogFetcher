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
using System.Net.Http;
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

    [Ignore("Uses async/await which upsets MSpec runner - needs a test sync context.")]
    [Subject(typeof(GravatarFetcher), "Web service")]
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
        It should_request_the_gravatar_hash_for_tim_long = () => UriPath.ShouldEqual("avatar/df0478426c0e47cc5e557d5391e5255d");

        static string UriPath;
        }

    public class with_fake_gravatar_web_service : with_fake_committer_list
        {
        Establish context = () =>
            {
            MessageHandler = new FakeHttpMessageHandler();
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

    [Subject(typeof(GravatarFetcher), "Integration test")]
    public class when_fetching : with_fake_gravatar_web_service
        {
            Establish context = () =>
            {
                Fetcher = new GravatarFetcher(Committers, filesystem: Filesystem);  // Use real gravatar web server
            };
            Because of = () => Fetcher.FetchGravatars(@"c:\");
            It should_be_true = () => true.ShouldBeTrue();
        }
    }
