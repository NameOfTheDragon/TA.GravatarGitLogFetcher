// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GitLogSpecs.cs  Created: 2013-06-22@16:41
// Last modified: 2013-06-23@22:27 by Tim

using System;
using System.Diagnostics;
using System.IO;
using FakeItEasy;
using Machine.Specifications;

namespace Tigra.Gravatar.LogFetcher.Specifications
    {
    /*
     * When creating a GitLog object, it should accept a path as a parameter in the constructor.
     * If the parameter is null, then the current working directory should be assumed.
     * The target directory must contain a Git repository (i.e. there must be a .git directory).
     * If there is no .git directory, an exception should be thrown. The valid path shoule be available
     * as a readonly property.
     * 
     * Having established a path to a valid Git repository, another method should return
     * a stream containing the text of the log. The Git log
     * should be opened and piped to a stream and the stream returned.
     */

    [Subject(typeof(GitLog), "Path to repository")]
    public class when_creating_a_gitlog_with_valid_path_and_valid_git_repo : with_mock_filesystem
        {
        Establish context = () =>
            {
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.IsEqualTo(FakeDirectory))).Returns(true);
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.EndsWith(".git"))).Returns(true);
            };

        Because of = () => Log = new GitLog(FakeDirectory, FakeFileSystem);
        It should_create_the_instance = () => Log.ShouldNotBeNull();
        It should_return_the_full_path = () => Log.GitWorkingCopyPath.ShouldEqual(FakeDirectory);
        }

    public class with_mock_filesystem
        {
        Establish context = () =>
            {
            Log = null;
            Thrown = null;
            var fileSystem = new FileSystemHelper();
            FakeFileSystem = A.Fake<FileSystemHelper>(x => x.Wrapping(fileSystem));
            //A.CallTo(() => FakeFileSystem.GetFullPath(A<string>.Ignored)).Returns(FakeDirectory);
            };

        protected const string FakeDirectory = @"C:\Fakedirectory";
        protected static GitLog Log;
        protected static Exception Thrown;
        protected static FileSystemHelper FakeFileSystem;
        }

    [Subject(typeof(GitLog), "Path to repository")]
    public class when_creating_a_gitlog_with_valid_path_but_no_git_repo : with_mock_filesystem
        {
        Establish context = () =>
            {
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.IsEqualTo(FakeDirectory))).Returns(false);
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.EndsWith(".git"))).Returns(false);
            };

        Because of = () => Thrown = Catch.Exception(() => Log = new GitLog(FakeDirectory, FakeFileSystem));
        It should_not_create_the_instance = () => Log.ShouldBeNull();
        It should_throw = () => Thrown.ShouldBeOfType<ArgumentException>();
        }

    [Subject(typeof(GitLog), "Path to repository")]
    public class when_creating_a_gitlog_with_valid_path_but_directory_does_not_exist : with_mock_filesystem
        {
        Establish context = () =>
            {
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.IsEqualTo(FakeDirectory))).Returns(true);
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.EndsWith(".git"))).Returns(false);
            };

        Because of = () => Thrown = Catch.Exception(() => Log = new GitLog(FakeDirectory, FakeFileSystem));
        It should_not_create_the_instance = () => Log.ShouldBeNull();
        It should_throw = () => Thrown.ShouldBeOfType<InvalidOperationException>();
        }

    [Subject(typeof(GitLog), "Path to repository")]
    public class when_creating_a_gitlog_with_invalid_path : with_mock_filesystem
        {
        Because of = () =>
                     Thrown =
                     Catch.Exception(
                         () => Log = new GitLog(new string(Path.GetInvalidPathChars()), new FileSystemHelper()));

        It should_throw = () => Thrown.ShouldBeOfType<ArgumentException>();
        It should_have_expected_error_message = () => Thrown.Message.ShouldStartWith("Illegal characters");
        It should_not_create_the_instance = () => Log.ShouldBeNull();
        }


    /// <summary>
    /// Class when_creating_the_git_log_stream
    /// This isn't really a proper test, just some code I used to quickly exercise something.
    /// </summary>
    [Subject(typeof(GitLog), "git log process")]
    public class when_creating_the_git_log_stream : with_mock_filesystem
        {
    //    const string workingCopy = @"C:\Users\Tim\Documents\Visual Studio 2012\Projects\orchard-cms";

    //    Because of = () =>
    //        {
    //        Log = new GitLog(workingCopy, new FileSystemHelper());
    //        Debug.WriteLine("Opening git process");
    //        var stream = Log.GetLogStream();
    //        Debug.WriteLine("Reading output");
    //        var output = stream.ReadToEnd();
    //        Debug.WriteLine("Writing output");
    //        Debug.WriteLine(output);
    //        };

    //    It should_create_the_instance = () => Log.ShouldNotBeNull();
    //    It should_return_the_full_path = () => Log.GitWorkingCopyPath.ShouldEqual(FakeDirectory);
        }

    }
