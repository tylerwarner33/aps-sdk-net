/* 
 * Forge SDK
 *
 * The Forge Platform contains an expanding collection of web service components that can be used with Autodesk cloud-based products or your own technologies. Take advantage of Autodesk’s expertise in design and engineering.
 *
 * oss
 *
 * The Object Storage Service (OSS) allows your application to download and upload raw files (such as PDF, XLS, DWG, or RVT) that are managed by the Data service.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Autodesk.SDKManager
{
    public class SDKManager : ISDKManager
    {
        public SDKManager(IApsConfiguration apsConfiguration, IResiliencyConfiguration resiliencyConfiguration, IAuthClient authClient, ILogger logger )
            : this(apsConfiguration, resiliencyConfiguration, authClient, logger, null)
        {
        }

        /// <summary>
        /// 	Builds an SDK Manager over a caller-supplied <see cref="IApsClient"/>.
        /// </summary>
        /// <remarks>
        /// 	Pass <c>null</c> for <paramref name="apsClient"/> to get the default client.
        /// 	Supplying one is the seam a test uses to intercept HTTP traffic.
        /// 	The constructor sets <c>BaseAddress</c> on the supplied client's <c>HttpClient</c>, so supply a
        /// 	client that has not sent a request yet, and do not share one client between two managers.
        /// 	A supplied client keeps its own resiliency behavior, so
        /// 	<paramref name="resiliencyConfiguration"/> does not apply to it.
        /// </remarks>
        /// <param name="apsConfiguration">
        /// 	Region and base address configuration.
        /// </param>
        /// <param name="resiliencyConfiguration">
        /// 	Retry and circuit breaker configuration for the default client.
        /// 	Ignored when <paramref name="apsClient"/> is supplied.
        /// </param>
        /// <param name="authClient">
        /// 	The authentication client. There is no default.
        /// </param>
        /// <param name="logger">
        /// 	The logger. Defaults to <see cref="NullLogger"/>.
        /// </param>
        /// <param name="apsClient">
        /// 	The transport client, or <c>null</c> to build the default one.
        /// </param>
        public SDKManager(IApsConfiguration apsConfiguration, IResiliencyConfiguration resiliencyConfiguration, IAuthClient authClient, ILogger logger, IApsClient apsClient)
        {
           // Default option is important for simplest case.
           _apsConfiguration =  apsConfiguration ?? new ApsConfiguration();
           IResiliencyConfiguration currentResiliencyConfiguration = resiliencyConfiguration ?? ResiliencyConfiguration.CreateDefault();

           _aPSClient = apsClient ?? new ApsClient(currentResiliencyConfiguration);
           _aPSClient.Service.Client.BaseAddress = _apsConfiguration.BaseAddress;

           _authClient = authClient; // There is no default auth client so far.
           _logger = logger ?? NullLogger.Instance;
        }

        public IApsClient ApsClient { get => _aPSClient; }
        public IAuthClient AuthClient { get => _authClient;}
        public ILogger Logger { get => _logger; }
        public IApsConfiguration ApsConfiguration { get => _apsConfiguration; set => _apsConfiguration = value; }

        ILogger _logger = NullLogger.Instance;
        IAuthClient _authClient;
        IApsConfiguration _apsConfiguration;
        IApsClient _aPSClient;
    }
}