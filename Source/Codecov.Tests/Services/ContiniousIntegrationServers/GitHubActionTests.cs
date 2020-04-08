using System;
using Codecov.Services.ContinuousIntegrationServers;
using FluentAssertions;
using Xunit;

namespace Codecov.Tests.Services.ContiniousIntegrationServers
{
    public class GitHubActionTests : IDisposable
    {
        [Fact]
        public void Branch_Should_Be_Empty_When_Environment_Variable_Does_Not_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REF", null);
            Environment.SetEnvironmentVariable("GITHUB_HEAD_REF", null);
            var githubAction = new GitHubAction();

            // When
            var branch = githubAction.Branch;

            // Then
            branch.Should().BeEmpty();
        }

        [Fact]
        public void Branch_Should_Be_Set_From_Head_Ref_When_Environment_Variable_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REF", "refs/pull/234/merge");
            Environment.SetEnvironmentVariable("GITHUB_HEAD_REF", "develop");
            var githubAction = new GitHubAction();

            // When
            var branch = githubAction.Branch;

            // Then
            branch.Should().Be("develop");
        }

        [Fact]
        public void Branch_Should_Be_Set_When_Enviornment_Variable_Exits()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REF", "refs/heads/develop");
            Environment.SetEnvironmentVariable("GITHUB_HEAD_REF", null);
            var githubAction = new GitHubAction();

            // When
            var branch = githubAction.Branch;

            // Then
            branch.Should().Be("develop");
        }

        [Fact]
        public void Build_Should_Be_Empty_When_Environment_Variable_Does_Not_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_RUN_ID", null);
            var githubAction = new GitHubAction();

            // When
            var build = githubAction.Build;

            // Then
            build.Should().BeEmpty();
        }

        [Fact]
        public void Build_Should_Be_Set_When_Enviornment_Variable_Exits()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_RUN_ID", "32402849");
            var githubAction = new GitHubAction();

            // When
            var build = githubAction.Build;

            // Then
            build.Should().Be("32402849");
        }

        [Fact]
        public void BuildUrl_Should_Be_Empty_When_Build_Is_Empty()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", "codecov/codecov-exe");
            Environment.SetEnvironmentVariable("GITHUB_RUN_ID", null);
            var githubAction = new GitHubAction();

            // When
            var buildUrl = githubAction.BuildUrl;

            // Then
            buildUrl.Should().BeEmpty();
        }

        [Fact]
        public void BuildUrl_Should_Be_Empty_When_Slug_Is_Empty()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", null);
            Environment.SetEnvironmentVariable("GITHUB_RUN_ID", "some-id");
            var githubAction = new GitHubAction();

            // When
            var buildUrl = githubAction.BuildUrl;

            // Then
            buildUrl.Should().BeEmpty();
        }

        [Fact]
        public void BuildUrl_Should_Not_Be_Empty_When_Environment_Variables_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", "codecov/codecov-exe");
            Environment.SetEnvironmentVariable("GITHUB_RUN_ID", "23432");
            var githubAction = new GitHubAction();

            // When
            var buildUrl = githubAction.BuildUrl;

            // Then
            buildUrl.Should().Be("https://github.com/codecov/codecov-exe/actions/runs/23432");
        }

        [Fact]
        public void Commit_Should_Be_Empty_String_When_Enviornment_Variable_Does_Not_Exits()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_SHA", null);
            var githubAction = new GitHubAction();

            // When
            var commit = githubAction.Commit;

            // Then
            commit.Should().BeEmpty();
        }

        [Fact]
        public void Commit_Should_Be_Set_When_Enviornment_Variable_Exits()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_SHA", "123");
            var githubAction = new GitHubAction();

            // When
            var commit = githubAction.Commit;

            // Then
            commit.Should().Be("123");
        }

        [Theory, InlineData(null), InlineData(""), InlineData("    ")]
        public void Detecter_Should_Be_False_When_Action_Environment_Variable_Is_Null_Or_Empty(string environmentData)
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_ACTION", environmentData);
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", null);
            var githubAction = new GitHubAction();

            // When
            var detecter = githubAction.Detecter;

            // Then
            detecter.Should().BeFalse();
        }

        [Theory, InlineData(null), InlineData(""), InlineData("False"), InlineData("false"), InlineData("foo")]
        public void Detecter_Should_Be_False_When_Actions_And_Action_Environment_Variable_Does_Not_Exist_Or_Is_Not_True(string environmentData)
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", environmentData);
            Environment.SetEnvironmentVariable("GITHUB_ACTION", null);
            var githubActions = new GitHubAction();

            // When
            var detecter = githubActions.Detecter;

            // Then
            detecter.Should().BeFalse();
        }

        [Fact]
        public void Detecter_Should_Be_True_When_Action_Environment_Variable_Exist_And_Is_Not_Empty()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_ACTION", "my-awesome-github-action");
            var githubActions = new GitHubAction();

            // When
            var detecter = githubActions.Detecter;

            // Then
            detecter.Should().BeTrue();
        }

        [Theory, InlineData("True"), InlineData("true")]
        public void Detecter_Should_Be_True_When_Actions_Environment_Variable_Exist_And_Is_True(string environmentData)
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_ACTIONS", environmentData);
            var githubActions = new GitHubAction();

            // When
            var detecter = githubActions.Detecter;

            // Then
            detecter.Should().BeTrue();
        }

        public void Dispose()
        {
            // We will remove all environment variables that could have been set during unit test
            var envVariable = new[]
            {
                "GITHUB_REF",
                "GITHUB_SHA",
                "GITHUB_ACTIONS",
                "GITHUB_ACTION",
                "GITHUB_HEAD_REF",
                "GITHUB_REPOSITORY"
            };

            foreach (var variable in envVariable)
            {
                Environment.SetEnvironmentVariable(variable, null);
            }
        }

        [Fact]
        public void PR_Should_Not_Be_Empty_When_Environment_Variables_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_HEAD_REF", "patch-2");
            Environment.SetEnvironmentVariable("GITHUB_REF", "refs/pull/7/merge");
            var githubAction = new GitHubAction();

            // When
            var pr = githubAction.Pr;
            var branch = githubAction.Branch;

            // Then
            pr.Should().Be("7");
            branch.Should().Be("patch-2");
        }

        [Theory, InlineData(null), InlineData(""), InlineData("   ")]
        public void PR_Should_Not_be_Set_If_Head_Ref_Is_Empyt(string environmentData)
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_HEAD_REF", environmentData);
            var githubAction = new GitHubAction();

            // When
            var pr = githubAction.Pr;

            // THen
            pr.Should().BeEmpty();
        }

        [Fact]
        public void Service_Should_Be_Set_To_GitHubActions()
        {
            // Given
            var githubAction = new GitHubAction();

            // When
            var service = githubAction.Service;

            // Then
            service.Should().Be("github-actions");
        }

        [Fact]
        public void Slug_Should_Be_Empty_String_When_Environment_Variable_Does_Not_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", null);
            var githubAction = new GitHubAction();

            // When
            var slug = githubAction.Slug;

            // Then
            slug.Should().BeEmpty();
        }

        [Fact]
        public void Slug_Should_Be_Set_When_Environment_Variable_Exist()
        {
            // Given
            Environment.SetEnvironmentVariable("GITHUB_REPOSITORY", "foo/bar");
            var githubAction = new GitHubAction();

            // When
            var slug = githubAction.Slug;

            // Then
            slug.Should().Be("foo/bar");
        }
    }
}
