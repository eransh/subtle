﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CookComputing.XmlRpc;
using Subtle.Model.Requests;
using Subtle.Model.Responses;

namespace Subtle.Model
{
    public class OSDbClient
    {
        public const string DefaultUserAgent = "OSTestUserAgent"; // Subtle/0.1
        public const string ApiUrl = "http://api.opensubtitles.org:80/xml-rpc";

        /// <summary>
        /// Default limit for SearchSubtitles. Maximum is 500.
        /// </summary>
        public const int SearchLimit = 100;

        private readonly IOSDbProxy proxy;
        private Session session;

        public OSDbClient() : this(DefaultUserAgent)
        {
        }

        public OSDbClient(string userAgent)
        {
            proxy = XmlRpcProxyGen.Create<IOSDbProxy>();
            proxy.Url = ApiUrl;
            proxy.UserAgent = userAgent;
        }

        public async Task InitSessionAsync()
        {
            session = await Task.Run(() => proxy.LogIn(
                string.Empty, string.Empty, string.Empty, DefaultUserAgent));

            if (!session.IsSuccess)
            {
                throw new OSDbException(
                    $"Failed to initialize session: {session.Status}");
            }
        }

        public Task<SubtitleFile> DownloadSubtitleAsync(string fileId)
        {
            CheckSession();
            return Task.Run(() => proxy.DownloadSubtitles(
                session.Token,
                new[] { fileId }).First());
        }

        public Task<ImdbSearchResultCollection> SearchVideos(string query)
        {
            CheckSession();
            return Task.Run(() => proxy.SearchVideos(
                session.Token, query));
        }

        public LanguageCollection GetLanguages()
        {
            return proxy.GetLanguages();
        }

        public async Task<SubtitleSearchResultCollection> SearchSubtitlesAsync(params SearchQuery[] query)
        {
            CheckSession();

            var options = new SearchOptions { Limit = SearchLimit };
            var result = await Task.Run(() => proxy.SearchSubtitles(
                session.Token, query, options));

            if (!result.IsSuccess)
            {
                throw new OSDbException(
                    $"Failed to search for subtitles: {result.Status}");
            }

            return result;
        }

        private void CheckSession()
        {
            if (string.IsNullOrEmpty(session?.Token))
            {
                throw new InvalidOperationException("Session is not initialized");
            }
        }
    }
}
