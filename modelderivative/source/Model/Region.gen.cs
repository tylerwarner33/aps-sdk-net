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
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Autodesk.ModelDerivative.Model
{
    /// <summary>
    /// Specifies where the referenced files are stored. Possible values are: 
    ///    
    ///    - `US` - Data center for the US region.
    ///    - `EMEA` - Data center for the European Union, Middle East, and Africa. 
    ///    - `AUS` - (Beta) Data centre for the Australia region.
    ///
    ///**Note**: Beta features are subject to change. Please avoid using them in production environments.
    /// </summary>
    ///<value>Specifies where the referenced files are stored. Possible values are: 
    ///    
    ///    - `US` - Data center for the US region.
    ///    - `EMEA` - Data center for the European Union, Middle East, and Africa. 
    ///    - `AUS` - (Beta) Data centre for the Australia region.
    ///
    ///**Note**: Beta features are subject to change. Please avoid using them in production environments.</value>

    [JsonConverter(typeof(StringEnumConverter))]

    public enum Region
    {

        /// <summary>
        /// Enum US for value: US
        /// </summary>
        [EnumMember(Value = "US")]
        US,

        /// <summary>
        /// Enum EMEA for value: EMEA
        /// </summary>
        [EnumMember(Value = "EMEA")]
        EMEA,

        /// <summary>
        /// Enum APAC for value: APAC
        /// </summary>
        [EnumMember(Value = "APAC")]
        [Obsolete("Please use AUS instead.")]
        APAC,

        /// <summary>
        /// Enum AUS for value: AUS
        /// </summary>
        [EnumMember(Value = "AUS")]
        AUS
    }
}


