using System;
using Codecov.Utilities;

namespace Codecov.Services.ContinuousIntegrationServers
{
    internal class GitHubAction : ContinuousIntegrationServer
    {
        private readonly Lazy<string> _branch = new Lazy<string>(LoadBranch);
        private readonly Lazy<string> _build = new Lazy<string>(() => EnviornmentVariable.GetEnviornmentVariable("GITHUB_RUN_ID"));
        private readonly Lazy<string> _commit = new Lazy<string>(() => EnviornmentVariable.GetEnviornmentVariable("GITHUB_SHA"));
        private readonly Lazy<bool> _detecter = new Lazy<bool>(() => CheckEnvironmentVariables("GITHUB_ACTIONS") || !string.IsNullOrWhiteSpace(EnviornmentVariable.GetEnviornmentVariable("GITHUB_ACTION")));
        private readonly Lazy<string> _pr = new Lazy<string>(LoadPullRequest);
        private readonly Lazy<string> _slug = new Lazy<string>(() => EnviornmentVariable.GetEnviornmentVariable("GITHUB_REPOSITORY"));

        public override string Branch => _branch.Value;

        public override string Build => _build.Value;

        public override string BuildUrl => LoadBuildUrl();

        public override string Commit => _commit.Value;

        public override bool Detecter => _detecter.Value;

        public override string Pr => _pr.Value;

        public override string Service => "github-actions";

        public override string Slug => _slug.Value;

        private static string ExtractSubstring(string text, string prefix, string postfix)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var textLength = text.Length;

            var startIndex = string.IsNullOrEmpty(prefix) ? 0 : text.IndexOf(prefix);
            var endIndex = string.IsNullOrEmpty(postfix) ? textLength : text.IndexOf(postfix);
            if (startIndex == -1)
            {
                startIndex = 0;
            }
            else if (!string.IsNullOrEmpty(prefix))
            {
                startIndex += prefix.Length;
            }

            if (endIndex == -1)
            {
                endIndex = textLength;
            }

            return text.Substring(startIndex, endIndex - startIndex);
        }

        private static string LoadBranch()
        {
            var headRef = EnviornmentVariable.GetEnviornmentVariable("GITHUB_HEAD_REF");
            if (!string.IsNullOrWhiteSpace(headRef))
            {
                return headRef;
            }

            var branch = EnviornmentVariable.GetEnviornmentVariable("GITHUB_REF");

            return ExtractSubstring(branch, "refs/heads/", null);
        }

        private static string LoadPullRequest()
        {
            var headRef = EnviornmentVariable.GetEnviornmentVariable("GITHUB_HEAD_REF");
            if (string.IsNullOrEmpty(headRef))
            {
                return string.Empty;
            }

            var branchRef = EnviornmentVariable.GetEnviornmentVariable("GITHUB_REF");

            if (string.IsNullOrEmpty(branchRef))
            {
                return string.Empty;
            }

            return ExtractSubstring(branchRef, "refs/pull/", "/merge");
        }

        private string LoadBuildUrl()
        {
            if (string.IsNullOrWhiteSpace(Slug) || string.IsNullOrWhiteSpace(Build))
            {
                return string.Empty;
            }

            return $"https://github.com/{Slug}/actions/runs/{Build}";
        }
    }
}
