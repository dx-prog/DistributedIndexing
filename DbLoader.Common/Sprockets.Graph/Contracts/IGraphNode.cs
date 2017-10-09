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

namespace Sprockets.Graph.Contracts {
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///     DO NOT EXPOSE AS IENUMERABLE OTHERWISE PEOPLE WILL USE RECURSION
    /// </remarks>
    public interface IGraphNode<T> : IComparable<IGraphNode<T>> {
        T Value { get; set; }
        IComparable Id { get; }
        ICollection<IGraphNode<T>> Peers { get; }
        IEnumerable<IGraphNode<T>> Search(GraphNodeVisitationControl<T> history, NodeSearch<T> predicate);
        void UnjoinFrom(IGraphNode<T> other);
        void JoinTo(IGraphNode<T> other);
    }
}