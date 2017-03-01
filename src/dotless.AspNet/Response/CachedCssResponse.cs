﻿namespace dotless.Core.Response
{
    using System;
    using System.Web;
    using Abstractions;

    public class CachedCssResponse : CssResponse
    {
        private readonly int _httpExpiryInMinutes;
        private readonly IClock _clock;

        public CachedCssResponse(IHttp http, bool isCompressionHandledByResponse, int httpExpiryInMinutes) :
            this(http, isCompressionHandledByResponse, httpExpiryInMinutes, new Clock())
        {
        }

        public CachedCssResponse(IHttp http, bool isCompressionHandledByResponse, int httpExpiryInMinutes, IClock clock) 
            : base(http, isCompressionHandledByResponse)
        {
            _httpExpiryInMinutes = httpExpiryInMinutes;
            _clock = clock;
        }

        public override void WriteHeaders()
        {
            var response = Http.Context.Response;
			response.Headers.Add("lessCache", "true");

			if (_httpExpiryInMinutes < 1)
			{
				response.Cache.SetCacheability(HttpCacheability.NoCache);
				response.Cache.SetMaxAge(new TimeSpan(0));
				return;
			}

            response.Cache.SetCacheability(HttpCacheability.Public);
			var expire = DateTime.UtcNow.AddMinutes(_httpExpiryInMinutes);
			response.Cache.SetExpires(expire);
			response.Cache.SetMaxAge((expire - DateTime.Now));

            response.Cache.SetETagFromFileDependencies();
            response.Cache.SetLastModifiedFromFileDependencies();

            // only modify the vary header if we are modifying the encoding
            if (IsCompressionHandledByResponse)
            {
                // response.Cache.SetOmitVaryStar(true);
                response.Cache.SetVaryByCustom("Accept-Encoding");
            }

            base.WriteHeaders();
        }
    }
}
