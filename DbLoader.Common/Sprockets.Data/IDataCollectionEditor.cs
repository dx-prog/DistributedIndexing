﻿/***********************************************************************************
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
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sprockets.Data {
    /// <summary>
    ///     Simple contract for accessing data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataCollectionEditor<T> {
        IDisposable UseIsolationLevel(IsolationLevel il);
        IQueryable<T> AsQueryable();
        bool Delete(Expression<Func<T, bool>> selector);
        bool Insert(T instance);
        bool UpdateOne(Expression<Func<T, bool>> selector, T instance);

        bool UpdateField<TValue>(
            Expression<Func<T, bool>> instanceSelector,
            Expression<Func<T, TValue>> fieldSelector,
            TValue instance);

        int Count();
        int Count(Expression<Func<T, bool>> selector);
    }
}