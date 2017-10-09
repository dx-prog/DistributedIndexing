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
using System.Collections.Generic;
using Sprockets.LargeGraph.Serialization.IGL;

namespace Sprockets.LargeGraph.Serialization {
    [Serializable]
    public class IglScriptBody {
        public List<long> RootObjects { get; protected set; } = new List<long>();
        public LinkedList<IglRegisterType> TypeDef { get; protected set; } = new LinkedList<IglRegisterType>();

        /// <summary>
        ///     Contains information about all the constants
        /// </summary>
        public LinkedList<IglDeclareValue> Constants { get; protected set; } = new LinkedList<IglDeclareValue>();

        /// <summary>
        ///     Contains information about what types of objects need to be declared
        /// </summary>
        public LinkedList<IglDeclareObject> Declaration { get; protected set; } = new LinkedList<IglDeclareObject>();

        /// <summary>
        ///     Contains information about what fields need to be set after all object relationships
        ///     are initialized and configured
        /// </summary>
        public LinkedList<IglSetField> FieldSets { get; protected set; } = new LinkedList<IglSetField>();

        /// <summary>
        ///     Contains information about hjow to serialize arrays of primatives
        /// </summary>
        public LinkedList<IglFastCopyTo> FastInitializations { get; protected set; } = new LinkedList<IglFastCopyTo>();

        /// <summary>
        ///     Contains informatio about how to serialize non-array enumerables, or arrays of non-primate types
        /// </summary>
        public LinkedList<IglHeavyCopyTo> HeavyInitializations { get; protected set; } =
            new LinkedList<IglHeavyCopyTo>();

        /// <summary>
        ///     contains informationa bout how to handle ISerializable
        /// </summary>
        public LinkedList<IglSpecialSerialization> SpecialInitializations { get; protected set; } =
            new LinkedList<IglSpecialSerialization>();


        public IglScriptBody GetTokens() {
            var ret = new IglScriptBody {
                RootObjects = new List<long>(RootObjects),
                TypeDef = new LinkedList<IglRegisterType>(TypeDef),
                Constants = new LinkedList<IglDeclareValue>(Constants),
                Declaration = new LinkedList<IglDeclareObject>(Declaration),
                FieldSets = new LinkedList<IglSetField>(FieldSets),
                FastInitializations = new LinkedList<IglFastCopyTo>(FastInitializations),
                HeavyInitializations = new LinkedList<IglHeavyCopyTo>(HeavyInitializations),
                SpecialInitializations = new LinkedList<IglSpecialSerialization>(SpecialInitializations)
            };
            return ret;
        }

        public void Execute(out Dictionary<long, object> graph) {
            var context = new IglTokenBase.IglTokenExecutionContext(this);

            foreach (var token in TypeDef)
                token.Execute(context);
            foreach (var token in Constants)
                token.Execute(context);
            foreach (var largeObject in Declaration)
                largeObject.Execute(context);

            foreach (var fastInitObject in FastInitializations)
                fastInitObject.Execute(context);

            foreach (var closures in FieldSets)
                closures.Execute(context);

            foreach (var heavyInitObject in HeavyInitializations)
                heavyInitObject.Execute(context);

            foreach (var special in SpecialInitializations)
                special.Execute(context);

            graph = context.Storage;
        }
    }
}