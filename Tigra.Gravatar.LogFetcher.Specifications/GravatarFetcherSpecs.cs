// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GravatarFetcherSpecs.cs  Created: 2013-06-26@17:28
// Last modified: 2013-06-26@18:02 by Tim

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Machine.Specifications;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    [Subject(typeof(GravatarFetcher), "Unique Committers")]
    public class when_creating_a_new_gravatar_fetcher : with_fake_log_streamreader
        {
        Because of = () => { Fetcher = new GravatarFetcher(Reader); };
        It should_have_2_unique_committers = () => Fetcher.UniqueCommitterCount.ShouldEqual(2);
        It should_have_first_committer_tim_long = () => Fetcher.UniqueCommitters[0].Name.ShouldEqual("Tim Long");
        It should_have_second_committer_darth_vader = () => Fetcher.UniqueCommitters[1].Name.ShouldEqual("Darth Vader");
        static GravatarFetcher Fetcher;
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

    public class GravatarFetcher
        {
        StreamReader reader;

        public GravatarFetcher(StreamReader reader)
            {
            this.reader = reader;
            ParseCommitLog();
            }

        void ParseCommitLog()
            {
            UniqueCommitters = new List<Committer>();

            while (!reader.EndOfStream)
                {
                var logLine = reader.ReadLine();
                if (string.IsNullOrEmpty(logLine))
                    {
                    continue;
                    }
                var splitLine = logLine.Split('|');
                if (splitLine.Length != 2)
                    {
                    Debug.WriteLine("Skipping invalid line: " + logLine);
                    }
                var committer = new Committer(name: splitLine[0], emailAddress: splitLine[1]);
                if (UniqueCommitters.Contains(committer))
                    continue;

                UniqueCommitters.Add(committer);
                }
            }

        /// <summary>
        ///   Gets the unique committer count.
        /// </summary>
        /// <value>The unique committer count.</value>
        public int UniqueCommitterCount { get { return UniqueCommitters.Count; } }

        /// <summary>
        ///   Gets the collection of unique committers.
        /// </summary>
        /// <value>The unique committers.</value>
        public IList<Committer> UniqueCommitters { get; private set; }
        }

    /// <summary>
    ///   Class Committer - represents a person who appears in a Git repository commit log.
    /// </summary>
    public class Committer
        {
        public string Name { get; private set; }
        public string EmailAddress { get; private set; }

        /// <summary>
        ///   Initializes a new instance of the <see cref="Committer" /> class.
        /// </summary>
        /// <param name="name">The committer's display name.</param>
        /// <param name="emailAddress">The committer's email address.</param>
        public Committer(string name, string emailAddress)
            {
            Name = name;
            EmailAddress = emailAddress;
            }

        public override string ToString()
            {
            return string.Format("{0} <{1}>", Name, EmailAddress);
            }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance,
        /// using some fairly liberal comparison rules. Items are assumed to be equal if any of
        /// the following are true:
        /// <list type="number">
        /// <item>The other object is a reference to the same object in memory (ReferenceEquals)</item>
        /// <item>The other object is a Committer and the <see cref="Name"/> properties are equal</item>
        /// <item>The other object is a committer and the <see cref="EmailAddress"/> properties are equal</item>
        /// <item>The other object is a string and it matches the <see cref="Name"/> property</item>
        /// <item>The other object is a string and it matches the <see cref="EmailAddress"/> property</item>
        /// <item>The other object is a string and it matches the <see cref="ToString"/> representation of this committer</item>
        /// </list>
        /// If none of the above are true, then the objects are considered unequal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
            {
            if (ReferenceEquals(obj, this))
                return true;    // The same object therefore always equal.
            
            if (obj is Committer)
                {
                var other = obj as Committer;
                if (Name == other.Name)
                    return true;
                if (EmailAddress == other.EmailAddress)
                    return true;
                }

            if (obj is string)
                {
                var other = obj as string;
                if (this.Name == other)
                    return true;
                if (this.EmailAddress == other)
                    return true;
                if (this.ToString() == other)
                    return true;
                }
            return false;   // Default to not equal
            }

        public override int GetHashCode()
            {
            return this.ToString().GetHashCode();
            }
        }
    }
