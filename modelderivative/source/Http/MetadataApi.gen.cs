/* 
 * APS SDK
 *
 * The APS Platform contains an expanding collection of web service components that can be used with Autodesk cloud-based products or your own technologies. Take advantage of Autodesk’s expertise in design and engineering.
 *
 * Model Derivative
 *
 * Use the Model Derivative API to translate designs from one CAD format to another. You can also use this API to extract metadata from a model.
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
using Autodesk.Forge.Core;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using Autodesk.ModelDerivative.Model;
using Autodesk.ModelDerivative.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Autodesk.SDKManager;

namespace Autodesk.ModelDerivative.Http
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IMetadataApi
    {
        /// <summary>
        /// Fetch Specific Properties
        /// </summary>
        /// <remarks>
        ///Queries the objects in the Model View (Viewable) specified by the `modelGuid` parameter and returns the specified properties in a paginated list. You can limit the number of objects to be queried by specifying a filter using the `query` attribute in the request body.
        ///
        ///**Note:** A design file must be translated to SVF or SVF2 before you can query object properties.  
        ///
        ///Before you call this operation:
        ///
        ///- Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET/) operation to obtain the list of Model Views in the source design.
        ///- Pick the ID of the Model View you want to query and specify that ID as the value for the `modelGuid`  parameter.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="modelGuid">
        ///The ID of the Model View you are querying. Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET) operation to get the IDs of the Model Views in the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <param name="specificPropertiesPayload">
        /// (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;SpecificProperties&gt;</returns>

        System.Threading.Tasks.Task<ApiResponse<SpecificProperties>> FetchSpecificPropertiesAsync(string urn, string modelGuid, string acceptEncoding = default(string), Region? region = null, SpecificPropertiesPayload specificPropertiesPayload = default(SpecificPropertiesPayload), string accessToken = null, bool throwOnError = true);
        /// <summary>
        /// Fetch All Properties
        /// </summary>
        /// <remarks>
        ///Returns a list of properties of all objects in the  Model View specified by the `modelGuid` parameter. 
        ///
        ///This operation returns properties of all objects by default. However, you can restrict the results to a specific object by specifying its ID as the `objectid` parameter.
        ///
        ///Properties are returned as a flat list, ordered, by their `objectid`. The `objectid` is a non-persistent ID assigned to an object when the source design is translated to the SVF or SVF2 format. This means that:
        ///
        ///- A design file must be translated to SVF or SVF2 before you can retrieve properties.
        ///- The `objectid` of an object can change if the design is translated to SVF or SVF2 again. If you require a persistent ID across translations, use `externalId` to reference objects, instead of `objectid`.
        ///
        ///Before you call this operation:
        ///
        ///- Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET/) operation to obtain the list of Model Views (Viewables) in the source design. 
        ///- Pick the ID of the Model View you want to query and specify that ID as the value for the `modelGuid` parameter.
        ///
        ///**Tip**: Use [Fetch Specific Properties](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-guid-properties-query-POST/) to retrieve only the objects and properties of interest. What’s more, the response is paginated. So, when the number of properties returned is large, responses start arriving more promptly.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="modelGuid">
        ///The ID of the Model View you are querying. Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET) operation to get the IDs of the Model Views in the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="xAdsForce">
        ///`true`: Forces the system to parse property data all over again. Use this option to retrieve an object tree when previous attempts have failed.
        ///
        ///`false`: (Default) Use previously parsed property data to extract the object tree. (optional)
        /// </param>
        /// <param name="xAdsDerivativeFormat">
        ///Specifies what Object IDs to return, if the design has legacy SVF derivatives generated by the BIM Docs service. Possible values are:  
        ///
        ///- `latest` - (Default) Return SVF2 Object IDs. 
        ///- `fallback` - Return SVF Object IDs.  
        ///
        ///**Note:**  
        ///
        ///1. This parameter applies only to designs with legacy SVF derivatives generated by the BIM 360 Docs service. 
        ///2. The BIM 360 Docs service now generates SVF2 derivatives. SVF2 Object IDs may not be compatible with the SVF Object IDs previously generated by the BIM 360 Docs service. Setting this header to fallback may resolve backward compatibility issues resulting from Object IDs of legacy SVF derivatives being retained on the client side. 
        ///3. If you use this parameter with one derivative (URN), you must use it consistently across the following: 
        ///
        ///   - [Create Translation Job](en/docs/model-derivative/v2/reference/http/job-POST) (for OBJ output) 
        ///   - [Fetch Object Tree](en/docs/model-derivative/v2/reference/http/urn-metadata-modelguid-GET)
        ///   - [Fetch All Properties](en/docs/model-derivative/v2/reference/http/urn-metadata-guid-properties-GET) (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <param name="objectid">
        ///The Object ID of the object you want to restrict the response to. If you do not specify this parameter, all properties of all objects within the Model View are returned.   (optional)
        /// </param>
        /// <param name="forceget">
        ///`true`: Retrieves large resources, even beyond the 20 MB limit. If exceptionally large (over 800 MB), the system acts as if `forceget` is `false`. 
        ///
        ///`false`: (Default) Does not retrieve resources if they are larger than 20 MB. (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;AllProperties&gt;</returns>

        System.Threading.Tasks.Task<ApiResponse<AllProperties>> GetAllPropertiesAsync(string urn, string modelGuid, string acceptEncoding = default(string), bool? xAdsForce = default(bool?), XAdsDerivativeFormat? xAdsDerivativeFormat = null, Region? region = null, int? objectid = default(int?), string forceget = default(string), string accessToken = null, bool throwOnError = true);
        /// <summary>
        /// List Model Views
        /// </summary>
        /// <remarks>
        ///Returns a list of Model Views (Viewables) in the source design specified by the `urn` parameter. It also returns an ID that uniquely identifies the Model View. You can use these IDs with other metadata operations to obtain information about the objects within those Model Views.
        ///
        ///Designs created with applications like Fusion 360 and Inventor contain only one Model View per design. Applications like Revit allow multiple Model Views per design.
        ///
        ///**Note:** You can retrieve metadata only from a design that has already been translated to SVF or SVF2.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;ModelViews&gt;</returns>

        System.Threading.Tasks.Task<ApiResponse<ModelViews>> GetModelViewsAsync(string urn, string acceptEncoding = default(string), Region? region = null, string accessToken = null, bool throwOnError = true);
        /// <summary>
        /// Fetch Object tree
        /// </summary>
        /// <remarks>
        ///Retrieves the structured hierarchy of objects, known as an object tree, from the specified Model View (Viewable) within the specified source design. The object tree represents the arrangement and relationships of various objects present in that Model View.
        ///
        ///**Note:** A design file must be translated to SVF or SVF2 before you can retrieve its object tree.  
        ///
        ///Before you call this operation:
        ///
        ///- Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET/) operation to obtain the list of Model Views in the source design.
        ///- Pick the ID of the Model View you want to query and specify that ID as the value for the `modelGuid`  parameter.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="modelGuid">
        ///The ID of the Model View you are extracting the object tree from. Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET) operation to get the IDs of the Model Views in the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <param name="xAdsForce">
        ///`true`: Forces the system to parse property data all over again. Use this option to retrieve an object tree when previous attempts have failed.
        ///
        ///`false`: (Default) Use previously parsed property data to extract the object tree. (optional)
        /// </param>
        /// <param name="xAdsDerivativeFormat">
        ///Specifies what Object IDs to return, if the design has legacy SVF derivatives generated by the BIM Docs service. Possible values are:  
        ///
        ///- `latest` - (Default) Return SVF2 Object IDs. 
        ///- `fallback` - Return SVF Object IDs.  
        ///
        ///**Note:**  
        ///
        ///1. This parameter applies only to designs with legacy SVF derivatives generated by the BIM 360 Docs service. 
        ///2. The BIM 360 Docs service now generates SVF2 derivatives. SVF2 Object IDs may not be compatible with the SVF Object IDs previously generated by the BIM 360 Docs service. Setting this header to fallback may resolve backward compatibility issues resulting from Object IDs of legacy SVF derivatives being retained on the client side. 
        ///3. If you use this parameter with one derivative (URN), you must use it consistently across the following: 
        ///
        ///   - [Create Translation Job](en/docs/model-derivative/v2/reference/http/job-POST) (for OBJ output) 
        ///   - [Fetch Object Tree](en/docs/model-derivative/v2/reference/http/urn-metadata-modelguid-GET)
        ///   - [Fetch All Properties](en/docs/model-derivative/v2/reference/http/urn-metadata-guid-properties-GET) (optional)
        /// </param>
        /// <param name="forceget">
        ///`true`: Retrieves large resources, even beyond the 20 MB limit. If exceptionally large (over 800 MB), the system acts as if `forceget` is `false`. 
        ///
        ///`false`: (Default) Does not retrieve resources if they are larger than 20 MB. (optional)
        /// </param>
        /// <param name="objectid">
        ///If specified, retrieves the sub-tree that has the specified Object ID as its parent node. If this parameter is not specified, retrieves the entire object tree. (optional)
        /// </param>
        /// <param name="level">
        ///Specifies how many child levels of the hierarchy to return, when the `objectid`  parameter is specified. Currently supports only `level` = `1`. (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;ObjectTree&gt;</returns>

        System.Threading.Tasks.Task<ApiResponse<ObjectTree>> GetObjectTreeAsync(string urn, string modelGuid, string acceptEncoding = default(string), Region? region = null, bool? xAdsForce = default(bool?), XAdsDerivativeFormat? xAdsDerivativeFormat = null, string forceget = default(string), int? objectid = default(int?), string level = default(string), string accessToken = null, bool throwOnError = true);
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class MetadataApi : IMetadataApi
    {
        ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataApi"/> class
        /// using SDKManager object
        /// </summary>
        /// <param name="sdkManager">An instance of SDKManager</param>
        /// <returns></returns>
        public MetadataApi(SDKManager.SDKManager sdkManager)
        {
            this.Service = sdkManager.ApsClient.Service;
            this.logger = sdkManager.Logger;
        }
        private void SetQueryParameter(string name, object value, Dictionary<string, object> dictionary)
        {
            if (value is Enum)
            {
                var type = value.GetType();
                var memberInfos = type.GetMember(value.ToString());
                var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == type);
                var valueAttributes = enumValueMemberInfo.GetCustomAttributes(typeof(EnumMemberAttribute), false);
                if (valueAttributes.Length > 0)
                {
                    dictionary.Add(name, ((EnumMemberAttribute)valueAttributes[0]).Value);
                }
            }
            else if (value is int)
            {
                if ((int)value > 0)
                {
                    dictionary.Add(name, value);
                }
            }
            else
            {
                if (value != null)
                {
                    dictionary.Add(name, value);
                }
            }
        }
        private void SetHeader(string baseName, object value, HttpRequestMessage req)
        {
            if (value is DateTime)
            {
                if ((DateTime)value != DateTime.MinValue)
                {
                    req.Headers.TryAddWithoutValidation(baseName, LocalMarshalling.ParameterToString(value)); // header parameter
                }
            }
            else
            {
                if (value != null)
                {
                    if (!string.Equals(baseName, "Content-Range"))
                    {
                        req.Headers.TryAddWithoutValidation(baseName, LocalMarshalling.ParameterToString(value)); // header parameter
                    }
                    else
                    {
                        req.Content.Headers.Add(baseName, LocalMarshalling.ParameterToString(value));
                    }
                }
            }

        }

        /// <summary>
        /// Gets or sets the ApsConfiguration object
        /// </summary>
        /// <value>An instance of the ForgeService</value>
        public ForgeService Service { get; set; }

        /// <summary>
        /// Fetch Specific Properties
        /// </summary>
        /// <remarks>
        ///Queries the objects in the Model View (Viewable) specified by the `modelGuid` parameter and returns the specified properties in a paginated list. You can limit the number of objects to be queried by specifying a filter using the `query` attribute in the request body.
        ///
        ///**Note:** A design file must be translated to SVF or SVF2 before you can query object properties.  
        ///
        ///Before you call this operation:
        ///
        ///- Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET/) operation to obtain the list of Model Views in the source design.
        ///- Pick the ID of the Model View you want to query and specify that ID as the value for the `modelGuid`  parameter.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="modelGuid">
        ///The ID of the Model View you are querying. Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET) operation to get the IDs of the Model Views in the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <param name="specificPropertiesPayload">
        /// (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;SpecificProperties&gt;></returns>

        public async System.Threading.Tasks.Task<ApiResponse<SpecificProperties>> FetchSpecificPropertiesAsync(string urn, string modelGuid, string acceptEncoding = default(string), Region? region = null, SpecificPropertiesPayload specificPropertiesPayload = default(SpecificPropertiesPayload), string accessToken = null, bool throwOnError = true)
        {
            logger.LogInformation("Entered into FetchSpecificPropertiesAsync ");
            using (var request = new HttpRequestMessage())
            {
                var queryParam = new Dictionary<string, object>();
                request.RequestUri =
                    Marshalling.BuildRequestUri("/modelderivative/v2/designdata/{urn}/metadata/{modelGuid}/properties:query",
                        routeParameters: new Dictionary<string, object> {
                            { "urn", urn},
                            { "modelGuid", modelGuid},
                        },
                        queryParameters: queryParam
                    );

                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("User-Agent", "APS SDK/MODEL DERIVATIVE/C#/1.0.0");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                }

                request.Content = Marshalling.Serialize(specificPropertiesPayload); // http body (model) parameter


                SetHeader("Accept-Encoding", acceptEncoding, request);
                SetHeader("region", region, request);

                // tell the underlying pipeline what scope we'd like to use
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }

                request.Method = new HttpMethod("POST");

                // make the HTTP request
                var response = await this.Service.Client.SendAsync(request);

                if (throwOnError)
                {
                    try
                    {
                        await response.EnsureSuccessStatusCodeAsync();
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new ModelDerivativeApiException(ex.Message, response, ex);
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"response unsuccess with status code: {response.StatusCode}");
                    return new ApiResponse<SpecificProperties>(response, default(SpecificProperties));
                }
                logger.LogInformation($"Exited from FetchSpecificPropertiesAsync with response statusCode: {response.StatusCode}");
                return new ApiResponse<SpecificProperties>(response, await LocalMarshalling.DeserializeAsync<SpecificProperties>(response.Content));

            } // using
        }
        /// <summary>
        /// Fetch All Properties
        /// </summary>
        /// <remarks>
        ///Returns a list of properties of all objects in the  Model View specified by the `modelGuid` parameter. 
        ///
        ///This operation returns properties of all objects by default. However, you can restrict the results to a specific object by specifying its ID as the `objectid` parameter.
        ///
        ///Properties are returned as a flat list, ordered, by their `objectid`. The `objectid` is a non-persistent ID assigned to an object when the source design is translated to the SVF or SVF2 format. This means that:
        ///
        ///- A design file must be translated to SVF or SVF2 before you can retrieve properties.
        ///- The `objectid` of an object can change if the design is translated to SVF or SVF2 again. If you require a persistent ID across translations, use `externalId` to reference objects, instead of `objectid`.
        ///
        ///Before you call this operation:
        ///
        ///- Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET/) operation to obtain the list of Model Views (Viewables) in the source design. 
        ///- Pick the ID of the Model View you want to query and specify that ID as the value for the `modelGuid` parameter.
        ///
        ///**Tip**: Use [Fetch Specific Properties](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-guid-properties-query-POST/) to retrieve only the objects and properties of interest. What’s more, the response is paginated. So, when the number of properties returned is large, responses start arriving more promptly.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="modelGuid">
        ///The ID of the Model View you are querying. Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET) operation to get the IDs of the Model Views in the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="xAdsForce">
        ///`true`: Forces the system to parse property data all over again. Use this option to retrieve an object tree when previous attempts have failed.
        ///
        ///`false`: (Default) Use previously parsed property data to extract the object tree. (optional)
        /// </param>
        /// <param name="xAdsDerivativeFormat">
        ///Specifies what Object IDs to return, if the design has legacy SVF derivatives generated by the BIM Docs service. Possible values are:  
        ///
        ///- `latest` - (Default) Return SVF2 Object IDs. 
        ///- `fallback` - Return SVF Object IDs.  
        ///
        ///**Note:**  
        ///
        ///1. This parameter applies only to designs with legacy SVF derivatives generated by the BIM 360 Docs service. 
        ///2. The BIM 360 Docs service now generates SVF2 derivatives. SVF2 Object IDs may not be compatible with the SVF Object IDs previously generated by the BIM 360 Docs service. Setting this header to fallback may resolve backward compatibility issues resulting from Object IDs of legacy SVF derivatives being retained on the client side. 
        ///3. If you use this parameter with one derivative (URN), you must use it consistently across the following: 
        ///
        ///   - [Create Translation Job](en/docs/model-derivative/v2/reference/http/job-POST) (for OBJ output) 
        ///   - [Fetch Object Tree](en/docs/model-derivative/v2/reference/http/urn-metadata-modelguid-GET)
        ///   - [Fetch All Properties](en/docs/model-derivative/v2/reference/http/urn-metadata-guid-properties-GET) (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <param name="objectid">
        ///The Object ID of the object you want to restrict the response to. If you do not specify this parameter, all properties of all objects within the Model View are returned.   (optional)
        /// </param>
        /// <param name="forceget">
        ///`true`: Retrieves large resources, even beyond the 20 MB limit. If exceptionally large (over 800 MB), the system acts as if `forceget` is `false`. 
        ///
        ///`false`: (Default) Does not retrieve resources if they are larger than 20 MB. (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;AllProperties&gt;></returns>

        public async System.Threading.Tasks.Task<ApiResponse<AllProperties>> GetAllPropertiesAsync(string urn, string modelGuid, string acceptEncoding = default(string), bool? xAdsForce = default(bool?), XAdsDerivativeFormat? xAdsDerivativeFormat = null, Region? region = null, int? objectid = default(int?), string forceget = default(string), string accessToken = null, bool throwOnError = true)
        {
            logger.LogInformation("Entered into GetAllPropertiesAsync ");
            using (var request = new HttpRequestMessage())
            {
                var queryParam = new Dictionary<string, object>();
                SetQueryParameter("objectid", objectid, queryParam);
                SetQueryParameter("forceget", forceget, queryParam);
                request.RequestUri =
                    Marshalling.BuildRequestUri("/modelderivative/v2/designdata/{urn}/metadata/{modelGuid}/properties",
                        routeParameters: new Dictionary<string, object> {
                            { "urn", urn},
                            { "modelGuid", modelGuid},
                        },
                        queryParameters: queryParam
                    );

                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("User-Agent", "APS SDK/MODEL DERIVATIVE/C#/1.0.0");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                }



                SetHeader("Accept-Encoding", acceptEncoding, request);
                SetHeader("x-ads-force", xAdsForce, request);
                SetHeader("x-ads-derivative-format", xAdsDerivativeFormat, request);
                SetHeader("region", region, request);

                // tell the underlying pipeline what scope we'd like to use
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }

                request.Method = new HttpMethod("GET");

                // make the HTTP request
                var response = await this.Service.Client.SendAsync(request);

                if (throwOnError)
                {
                    try
                    {
                        await response.EnsureSuccessStatusCodeAsync();
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new ModelDerivativeApiException(ex.Message, response, ex);
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"response unsuccess with status code: {response.StatusCode}");
                    return new ApiResponse<AllProperties>(response, default(AllProperties));
                }
                logger.LogInformation($"Exited from GetAllPropertiesAsync with response statusCode: {response.StatusCode}");
                return new ApiResponse<AllProperties>(response, await LocalMarshalling.DeserializeAsync<AllProperties>(response.Content));

            } // using
        }
        /// <summary>
        /// List Model Views
        /// </summary>
        /// <remarks>
        ///Returns a list of Model Views (Viewables) in the source design specified by the `urn` parameter. It also returns an ID that uniquely identifies the Model View. You can use these IDs with other metadata operations to obtain information about the objects within those Model Views.
        ///
        ///Designs created with applications like Fusion 360 and Inventor contain only one Model View per design. Applications like Revit allow multiple Model Views per design.
        ///
        ///**Note:** You can retrieve metadata only from a design that has already been translated to SVF or SVF2.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;ModelViews&gt;></returns>

        public async System.Threading.Tasks.Task<ApiResponse<ModelViews>> GetModelViewsAsync(string urn, string acceptEncoding = default(string), Region? region = null, string accessToken = null, bool throwOnError = true)
        {
            logger.LogInformation("Entered into GetModelViewsAsync ");
            using (var request = new HttpRequestMessage())
            {
                var queryParam = new Dictionary<string, object>();
                request.RequestUri =
                    Marshalling.BuildRequestUri("/modelderivative/v2/designdata/{urn}/metadata",
                        routeParameters: new Dictionary<string, object> {
                            { "urn", urn},
                        },
                        queryParameters: queryParam
                    );

                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("User-Agent", "APS SDK/MODEL DERIVATIVE/C#/1.0.0");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                }



                SetHeader("Accept-Encoding", acceptEncoding, request);
                SetHeader("region", region, request);

                // tell the underlying pipeline what scope we'd like to use
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }

                request.Method = new HttpMethod("GET");

                // make the HTTP request
                var response = await this.Service.Client.SendAsync(request);

                if (throwOnError)
                {
                    try
                    {
                        await response.EnsureSuccessStatusCodeAsync();
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new ModelDerivativeApiException(ex.Message, response, ex);
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"response unsuccess with status code: {response.StatusCode}");
                    return new ApiResponse<ModelViews>(response, default(ModelViews));
                }
                logger.LogInformation($"Exited from GetModelViewsAsync with response statusCode: {response.StatusCode}");
                return new ApiResponse<ModelViews>(response, await LocalMarshalling.DeserializeAsync<ModelViews>(response.Content));

            } // using
        }
        /// <summary>
        /// Fetch Object tree
        /// </summary>
        /// <remarks>
        ///Retrieves the structured hierarchy of objects, known as an object tree, from the specified Model View (Viewable) within the specified source design. The object tree represents the arrangement and relationships of various objects present in that Model View.
        ///
        ///**Note:** A design file must be translated to SVF or SVF2 before you can retrieve its object tree.  
        ///
        ///Before you call this operation:
        ///
        ///- Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET/) operation to obtain the list of Model Views in the source design.
        ///- Pick the ID of the Model View you want to query and specify that ID as the value for the `modelGuid`  parameter.
        /// </remarks>
        /// <exception cref="HttpRequestException">Thrown when fails to make API call</exception>
        /// <param name="urn">
        ///The URL-safe Base64 encoded URN of the source design.
        /// </param>
        /// <param name="modelGuid">
        ///The ID of the Model View you are extracting the object tree from. Use the [List Model Views](/en/docs/model-derivative/v2/reference/http/metadata/urn-metadata-GET) operation to get the IDs of the Model Views in the source design.
        /// </param>
        /// <param name="acceptEncoding">
        ///A comma separated list of the algorithms you want the response to be encoded in, specified in the order of preference.  
        ///
        ///If you specify `gzip` or `*`, content is compressed and returned in gzip format. (optional)
        /// </param>
        /// <param name="region">
        ///Specifies the data center where the manifest and derivatives of the specified source design are stored. Possible values are:
        ///
        ///- `US` - (Default) Data center for the US region.
        ///- `EMEA` - Data center for the European Union, Middle East, and Africa. 
        ///- `APAC` - (Beta) Data center for the Australia region.
        ///
        ///**Note**: Beta features are subject to change. Please avoid using them in production environments. (optional)
        /// </param>
        /// <param name="xAdsForce">
        ///`true`: Forces the system to parse property data all over again. Use this option to retrieve an object tree when previous attempts have failed.
        ///
        ///`false`: (Default) Use previously parsed property data to extract the object tree. (optional)
        /// </param>
        /// <param name="xAdsDerivativeFormat">
        ///Specifies what Object IDs to return, if the design has legacy SVF derivatives generated by the BIM Docs service. Possible values are:  
        ///
        ///- `latest` - (Default) Return SVF2 Object IDs. 
        ///- `fallback` - Return SVF Object IDs.  
        ///
        ///**Note:**  
        ///
        ///1. This parameter applies only to designs with legacy SVF derivatives generated by the BIM 360 Docs service. 
        ///2. The BIM 360 Docs service now generates SVF2 derivatives. SVF2 Object IDs may not be compatible with the SVF Object IDs previously generated by the BIM 360 Docs service. Setting this header to fallback may resolve backward compatibility issues resulting from Object IDs of legacy SVF derivatives being retained on the client side. 
        ///3. If you use this parameter with one derivative (URN), you must use it consistently across the following: 
        ///
        ///   - [Create Translation Job](en/docs/model-derivative/v2/reference/http/job-POST) (for OBJ output) 
        ///   - [Fetch Object Tree](en/docs/model-derivative/v2/reference/http/urn-metadata-modelguid-GET)
        ///   - [Fetch All Properties](en/docs/model-derivative/v2/reference/http/urn-metadata-guid-properties-GET) (optional)
        /// </param>
        /// <param name="forceget">
        ///`true`: Retrieves large resources, even beyond the 20 MB limit. If exceptionally large (over 800 MB), the system acts as if `forceget` is `false`. 
        ///
        ///`false`: (Default) Does not retrieve resources if they are larger than 20 MB. (optional)
        /// </param>
        /// <param name="objectid">
        ///If specified, retrieves the sub-tree that has the specified Object ID as its parent node. If this parameter is not specified, retrieves the entire object tree. (optional)
        /// </param>
        /// <param name="level">
        ///Specifies how many child levels of the hierarchy to return, when the `objectid`  parameter is specified. Currently supports only `level` = `1`. (optional)
        /// </param>
        /// <returns>Task of ApiResponse&lt;ObjectTree&gt;></returns>

        public async System.Threading.Tasks.Task<ApiResponse<ObjectTree>> GetObjectTreeAsync(string urn, string modelGuid, string acceptEncoding = default(string), Region? region = null, bool? xAdsForce = default(bool?), XAdsDerivativeFormat? xAdsDerivativeFormat = null, string forceget = default(string), int? objectid = default(int?), string level = default(string), string accessToken = null, bool throwOnError = true)
        {
            logger.LogInformation("Entered into GetObjectTreeAsync ");
            using (var request = new HttpRequestMessage())
            {
                var queryParam = new Dictionary<string, object>();
                SetQueryParameter("forceget", forceget, queryParam);
                SetQueryParameter("objectid", objectid, queryParam);
                SetQueryParameter("level", level, queryParam);
                request.RequestUri =
                    Marshalling.BuildRequestUri("/modelderivative/v2/designdata/{urn}/metadata/{modelGuid}",
                        routeParameters: new Dictionary<string, object> {
                            { "urn", urn},
                            { "modelGuid", modelGuid},
                        },
                        queryParameters: queryParam
                    );

                request.Headers.TryAddWithoutValidation("Accept", "application/json");
                request.Headers.TryAddWithoutValidation("User-Agent", "APS SDK/MODEL DERIVATIVE/C#/1.0.0");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
                }



                SetHeader("Accept-Encoding", acceptEncoding, request);
                SetHeader("region", region, request);
                SetHeader("x-ads-force", xAdsForce, request);
                SetHeader("x-ads-derivative-format", xAdsDerivativeFormat, request);

                // tell the underlying pipeline what scope we'd like to use
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }
                // if (scopes == null)
                // {
                // TBD:Naren FORCE-4027 - If accessToken is null, acquire auth token using auth SDK, with defined scope.
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), "data:read ");
                // }
                // else
                // {
                // request.Properties.Add(ForgeApsConfiguration.ScopeKey.ToString(), scopes);
                // }

                request.Method = new HttpMethod("GET");

                // make the HTTP request
                var response = await this.Service.Client.SendAsync(request);

                if (throwOnError)
                {
                    try
                    {
                        await response.EnsureSuccessStatusCodeAsync();
                    }
                    catch (HttpRequestException ex)
                    {
                        throw new ModelDerivativeApiException(ex.Message, response, ex);
                    }
                }
                else if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"response unsuccess with status code: {response.StatusCode}");
                    return new ApiResponse<ObjectTree>(response, default(ObjectTree));
                }
                logger.LogInformation($"Exited from GetObjectTreeAsync with response statusCode: {response.StatusCode}");
                return new ApiResponse<ObjectTree>(response, await LocalMarshalling.DeserializeAsync<ObjectTree>(response.Content));

            } // using
        }
    }
}
