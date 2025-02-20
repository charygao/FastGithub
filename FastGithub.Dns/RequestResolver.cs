﻿using DNS.Client.RequestResolver;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using FastGithub.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastGithub.Dns
{
    /// <summary>
    /// dns解析者
    /// </summary> 
    sealed class RequestResolver : IRequestResolver
    {
        private readonly TimeSpan ttl = TimeSpan.FromMinutes(1d);
        private readonly FastGithubConfig fastGithubConfig;

        /// <summary>
        /// dns解析者
        /// </summary>
        /// <param name="fastGithubConfig"></param>
        public RequestResolver(FastGithubConfig fastGithubConfig)
        {
            this.fastGithubConfig = fastGithubConfig;
        }

        /// <summary>
        /// 解析域名
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default)
        {
            var response = Response.FromRequest(request);
            if (request is not RemoteEndPointRequest remoteEndPointRequest)
            {
                return response;
            }

            var question = request.Questions.FirstOrDefault();
            if (question == null || question.Type != RecordType.A)
            {
                return response;
            }

            // 解析匹配的域名指向本机ip
            var domain = question.Name;
            if (this.fastGithubConfig.IsMatch(domain.ToString()) == true)
            {
                var localAddress = remoteEndPointRequest.GetLocalIPAddress() ?? IPAddress.Loopback;
                var record = new IPAddressResourceRecord(domain, localAddress, this.ttl);
                response.AnswerRecords.Add(record);
                return response;
            }

            // 使用回退dns解析域名
            foreach (var dns in this.fastGithubConfig.FallbackDns)
            {
                try
                {
                    var fallbackResolver = new UdpRequestResolver(dns);
                    var fallbackResponse = await fallbackResolver.Resolve(request, cancellationToken);
                    if (fallbackResponse != null && fallbackResponse.AnswerRecords.Count > 0)
                    {
                        return fallbackResponse;
                    }
                }
                catch (Exception)
                {
                }
            }

            return response;
        }
    }
}
