using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Tigra.Gravatar.LogFetcher;

namespace Namespace
    {
    [Subject("Get list of unique committers from Git repository")]
    public class when_retrieving_list_of_unique_committers_from_a_git_repository : with_integration_test_context
        {
        Establish context = () =>
            {
            Log = new GitLog(RepositoryLocation);
            };

        Because of = () => Committers = Log.GetListOfUniqueCommitters();

        It should_discover_two_unique_committers = () => Committers.Count().ShouldEqual(2);

        static IEnumerable<Committer> Committers { get; set; }
        static GitLog Log;
        }

    public class with_integration_test_context
        {
        const string GitRepositoryDirectory = "GitIntegrationTestRepository";

        Establish context = () =>
            {
            var myAssembly = Assembly.GetExecutingAssembly();
            var myFullPath = myAssembly.Location;
            var myDirectory = Path.GetDirectoryName(myFullPath);
            AssemblyLocation = myDirectory;
            RepositoryLocation = Path.Combine(AssemblyLocation, GitRepositoryDirectory);
            };

        protected static string AssemblyLocation { get; set; }
        protected static string RepositoryLocation { get; set; }
        }
    }