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
namespace sdk.manager
{
    public class ResiliencyConfiguration : IResiliencyConfiguration
    {
        int _retryCount;
        int _backoffInterval;
        int _ciruitBreakerInterval;

        public int RetryCount { get => _retryCount; set => _retryCount = value; }
        public int BackoffInterval { get => _backoffInterval; set => _backoffInterval = value; }
        public int CiruitBreakerInterval { get => _ciruitBreakerInterval; set => _ciruitBreakerInterval = value; }

        public static ResiliencyConfiguration CreateDefault()
        {
            return new ResiliencyConfiguration()
            {
                RetryCount = 3,
                BackoffInterval = 10,
                CiruitBreakerInterval = 5
            };
        }
        public override string ToString()
        {
            return $"RetryCount:{RetryCount}, BackoffInterval: {BackoffInterval}, CiruitBreakerInterval:{CiruitBreakerInterval}";
        }
    }
}