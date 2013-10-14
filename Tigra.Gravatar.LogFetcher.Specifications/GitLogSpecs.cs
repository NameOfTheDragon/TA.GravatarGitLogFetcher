// This file is part of the Tigra.Gravatar.LogFetcher project
// 
// Copyright © 2013 TiGra Networks, all rights reserved.
// 
// File: GitLogSpecs.cs  Created: 2013-06-22@16:41
// Last modified: 2013-06-23@22:27 by Tim

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public class when_creating_the_git_log_stream : with_mock_git_process
        {
        Establish context = () =>
            {
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.IsEqualTo(FakeDirectory))).Returns(true);
            A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.EndsWith(".git"))).Returns(true);
            Log = new GitLog(FakeDirectory, FakeFileSystem);
            };

        Because of = () =>
            {
            LogStream = Log.GetLogStream(GitScriptPSI);
            LogOutput = LogStream.ReadToEnd();
            Debug.WriteLine(string.Format("Got {0} characters", LogOutput.Length));
            };

        It should_create_the_stream = () => LogStream.ShouldNotBeNull();

        It should_return_the_expected_output =
            () =>
                LogOutput.ShouldEqual(
                    "tim@tigranetworks.co.uk|Tim Long\r\nTim@tigranetworks.co.uk|Tim Long\r\nTim@tigranetworks.co.uk|Tim Long\r\nTim@tigranetworks.co.uk|Tim Long\r\nTim@tigranetworks.co.uk|Tim Long\r\nfernjampel@hotmail.co.uk|Fern Hughes\r\nTim@tigranetworks.co.uk|Tim Long\r\nTim@tigranetworks.co.uk|Tim Long\r\n");

        static StreamReader LogStream;
        static string LogOutput;
        }

    [Subject(typeof(GitLog), "get committer list")]
    public class when_getting_the_list_of_unique_committers : with_mock_git_process
        {
            Establish context = () =>
            {
                A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.IsEqualTo(FakeDirectory))).Returns(true);
                A.CallTo(() => FakeFileSystem.DirectoryExists(A<string>.That.EndsWith(".git"))).Returns(true);
                Log = new GitLog(FakeDirectory, FakeFileSystem);
            };

        Because of = () =>
            {
            Committers = Log.GetListOfUniqueCommitters(GitScriptPSI);
            Debug.WriteLine(string.Format("Got {0} unique committers", Committers.Count()));
            };

        It should_contain_two_entries = () => Committers.Count().ShouldEqual(2);
        It should_contain_tim_long = () => Committers.Count(p => p.Name == "Tim Long").ShouldEqual(1);
        It should_contain_fern_hughes = () => Committers.Count(p => p.Name == "Fern Hughes").ShouldEqual(1);

        static StreamReader LogStream;
        static string LogOutput;
        static IEnumerable<Committer> Committers;
        }

    /// <summary>
    /// Class with_mock_git_process.
    /// Provides a process that returns fake output as if from Git.
    /// Actually uses a simple VBScript that just writes static text to stdout.
    /// </summary>
    public class with_mock_git_process : with_mock_filesystem
        {
        const string MockGitScript = "MockGit.vbs";
        const string argumentsFormat = "\"{0}\" //NoLogo //T:5";
        protected static ProcessStartInfo GitScriptPSI;
        Establish context = () =>
            {
            var me = Assembly.GetExecutingAssembly();
            var myExecutable = me.Location;
            var myWorkingDirectory = Path.GetDirectoryName(myExecutable);
            var fullPathToGitScript = Path.Combine(myWorkingDirectory, MockGitScript);
            var arguments = string.Format(argumentsFormat, fullPathToGitScript);
            GitScriptPSI = new ProcessStartInfo("cscript.exe", arguments);
            GitScriptPSI.WorkingDirectory = myWorkingDirectory;
            };

        Cleanup after = () =>
            {
            GitScriptPSI = null;
            };
        }
    }
