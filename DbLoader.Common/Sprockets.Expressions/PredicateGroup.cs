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
using System.Linq.Expressions;

namespace Sprockets.Expressions {
    /// <summary>
    ///     This for some reason does not play well with all Linq-to-* providers. This class however
    ///     might be usable in some circumstances when trying to create large or custom search queries.
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public class PredicateGroup<TElement> {
        private readonly List<Expression> _storage = new List<Expression>();

        public PredicateGroup(ParameterExpression input = null) {
            Input = input ?? Expression.Parameter(typeof(TElement), "Input");
        }

        public PredicateGroup(ParameterExpression input, params Expression<Func<TElement, bool>>[] predicates) {
            Input = input;
            foreach (var predicate in predicates)
                Add(predicate);
        }

        public PredicateGroup(ParameterExpression input, IEnumerable<Expression<Func<TElement, bool>>> predicates) {
            Input = input;
            foreach (var predicate in predicates)
                Add(predicate);
        }

        public ParameterExpression Input { get; }

        public PredicateGroup<TElement> Add(Expression<Func<TElement, bool>> predicate) {
            _storage.Add(Expression.Invoke(predicate, Input));
            return this;
        }

        public Expression<Func<TElement, bool>> Combine(
            Func<Expression, Expression, BinaryExpression> joiner) {
            List<Expression> inputs = null;
            var pending = new List<Expression>();
            var tmp = new Stack<Expression>();
            do {
                if (null == inputs) {
                    inputs = new List<Expression>(_storage);
                }
                else if (pending.Count > 0) {
                    inputs = pending;
                    pending = new List<Expression>();
                }
                if (inputs.Count == 1)
                    break;

                CondenseWorkStack(joiner, inputs, tmp, pending);
            } while (pending.Count > 0);

            switch (inputs.Count) {
                case 1:
                    return Expression.Lambda<Func<TElement, bool>>(inputs[0], Input);
                case 0:
                    return Expression.Lambda<Func<TElement, bool>>(Expression.Constant(false), Input);
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        ///     WARNING: Deply nested expressions can cause stack overflow.
        /// </summary>
        /// <param name="joiner"></param>
        /// <param name="inputs"></param>
        /// <param name="tmp"></param>
        /// <param name="pending"></param>
        private void CondenseWorkStack(Func<Expression, Expression, BinaryExpression> joiner,
            List<Expression> inputs,
            Stack<Expression> tmp,
            List<Expression> pending) {
            foreach (var entry in inputs) {
                TryCompact(joiner, tmp, pending);
                tmp.Push(entry);
            }
            while (tmp.Count > 1)
                TryCompact(joiner, tmp, pending);
            while (tmp.Count > 0)
                pending.Add(tmp.Pop());
        }

        private void TryCompact(Func<Expression, Expression, BinaryExpression> joiner,
            Stack<Expression> tmp,
            List<Expression> pending) {
            if (tmp.Count == 2)
                pending.Add(joiner(
                    Encapulate(tmp.Pop()),
                    Encapulate(tmp.Pop())));
        }

        private Expression Encapulate(Expression tmp) {
            switch (tmp) {
                case LambdaExpression lambda:
                    return Expression.Invoke(lambda, Input);
                case InvocationExpression _:
                case BinaryExpression _:
                case ConstantExpression _:
                    return tmp;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}