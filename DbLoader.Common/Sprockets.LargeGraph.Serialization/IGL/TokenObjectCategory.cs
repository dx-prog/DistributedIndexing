/***********************************************************************************
 * Copyright 2017  David Garcia
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * *********************************************************************************/

using System;

namespace Sprockets.LargeGraph.Serialization.IGL {
    [Flags]
    [Serializable]
    public enum TokenObjectCategory {
        Unknown,

        /// <summary>
        ///     The Token relates to value type
        /// </summary>
        Value = 1,

        /// <summary>
        ///     The Token relates to a serializable type
        /// </summary>
        Serializable = 2,

        /// <summary>
        ///     The Token relates to an Enumerable
        /// </summary>
        Enumerable = 4,

        /// <summary>
        ///     The Token relates to an Type that is initialized through Set Fields
        /// </summary>
        FieldSettable = 8,

        /// <summary>
        ///     The Token relates to a type that has a special construction contract
        /// </summary>
        Contract = 16,

        /// <summary>
        ///     Internal, used to mark a Type that is likely to need iterations at being degraphed
        /// </summary>
        NeedsUngraphing = 1 << 32
    }
}