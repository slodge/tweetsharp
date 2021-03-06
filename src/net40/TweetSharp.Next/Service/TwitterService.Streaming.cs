﻿using System;
using Hammock;
using Hammock.Streaming;
using Hammock.Web;

namespace TweetSharp
{
    partial class TwitterService
    {
        private readonly RestClient _userStreamingClient;
        private readonly RestClient _searchStreamingClient;

        /// <summary>
        /// Cancels pending streaming actions from this service.
        /// </summary>
        public virtual void CancelStreaming()
        {
            if(_userStreamingClient != null)
            {
                _userStreamingClient.CancelStreaming();
            }
            if (_searchStreamingClient != null)
            {
                _searchStreamingClient.CancelStreaming();
            }
        }

        /// <summary>
        /// Accesses an asynchronous Twitter filter stream indefinitely, until terminated.
        /// </summary>
        /// <seealso href="http://dev.twitter.com/pages/streaming_api_methods#statuses-filter" />
        /// <param name="action"></param>
        /// <returns></returns>
#if !WINDOWS_PHONE
        public virtual IAsyncResult StreamFilter(Action<TwitterStreamArtifact, TwitterResponse> action)
#else
        public virtual void StreamFilter(Action<TwitterStreamArtifact, TwitterResponse> action)
#endif
        {
            var options = new StreamOptions { ResultsPerCallback = 1 };
#if !WINDOWS_PHONE
            return 
#endif
            WithHammockSearchStreaming(options, action, "statuses/filter.json");
        }

        /// <summary>
        /// Accesses an asynchronous Twitter user stream indefinitely, until terminated.
        /// </summary>
        /// <seealso href="http://dev.twitter.com/pages/user_streams" />
        /// <param name="action"></param>
        /// <returns></returns>
#if !WINDOWS_PHONE
        public virtual IAsyncResult StreamUser(Action<TwitterStreamArtifact, TwitterResponse> action)
#else
        public virtual void StreamUser(Action<TwitterStreamArtifact, TwitterResponse> action)
#endif
        {
            var options = new StreamOptions { ResultsPerCallback = 1 };

#if !WINDOWS_PHONE
            return 
#endif
            WithHammockUserStreaming(options, action, "user.json");
        }

#if !WINDOWS_PHONE
        private IAsyncResult WithHammockUserStreaming<T>(StreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#else
        private void WithHammockUserStreaming<T>(StreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#endif
        {
            var request = PrepareHammockQuery(path);
#if !WINDOWS_PHONE
            return 
#endif 
            WithHammockStreamingImpl(_userStreamingClient, request, options, action);
        }

        #if !WINDOWS_PHONE
        private IAsyncResult WithHammockSearchStreaming<T>(StreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#else
        private void WithHammockSearchStreaming<T>(StreamOptions options, Action<T, TwitterResponse> action, string path) where T : class
#endif
        {
            var request = PrepareHammockQuery(path);
#if !WINDOWS_PHONE
            return 
#endif 
            WithHammockStreamingImpl(_searchStreamingClient, request, options, action);
        }

#if !WINDOWS_PHONE
        private IAsyncResult WithHammockStreamingImpl<T>(RestClient client, RestRequest request, StreamOptions options, Action<T, TwitterResponse> action)
#else
        private static void WithHammockStreamingImpl<T>(RestClient client, RestRequest request, StreamOptions options, Action<T, TwitterResponse> action)
#endif
        {
            request.StreamOptions = options;
            request.Method = WebMethod.Get;
#if SILVERLIGHT
            request.AddHeader("X-User-Agent", client.UserAgent); 
#endif
            
#if !WINDOWS_PHONE
            return
#endif
            client.BeginRequest(request, new RestCallback<T>((req, resp, state) =>
            {
                Exception exception;
                var entity = TryAsyncResponse(() => 
                        {
#if !SILVERLIGHT
                            SetResponse(resp);
#endif
                            return resp.ContentEntity;
                        },
                        out exception);
                action(entity, new TwitterResponse(resp, exception));
            }));
        }
    }
}
