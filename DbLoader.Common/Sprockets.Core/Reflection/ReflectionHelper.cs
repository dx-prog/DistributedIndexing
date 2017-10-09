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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sprockets.Core.Reflection {
    /// <summary>
    ///     This class is used to help with reflection operations needed for
    ///     custom lambda or serialization
    /// </summary>
    public static class ReflectionHelper {
        /// <summary>
        ///     Returns true if the type provided is primative, or if it is an array,
        ///     the lowest ranked element is primative
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsFundementallyPrimative(this Type t) {
            if (t == null)
                return false;

            do {
                if (t == typeof(object))
                    return false;
                if (t == typeof(string))
                    return true;

                var eType = t.GetElementType();
                t = eType;
                if (t.IsPrimitive)
                    return true;

                if (!t.IsArray)
                    return false;
            } while (true);
        }

        /// <summary>
        ///     Resolves all types related to <see cref="input" /> that can be instantiated
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetRelatedConcreteTypes(this Type input) {
            return new[] {input}.GetRelatedConcreteTypes();
        }

        /// <summary>
        ///     Resolves all types related to <see cref="input" /> that can be instantiated
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetRelatedConcreteTypes(this IEnumerable<Type> input) {
            return GetRelatedTypes(input)
                .Where(knownType => !(knownType.IsGenericTypeDefinition ||
                                      knownType.IsInterface ||
                                      knownType == typeof(ValueType) ||
                                      knownType.IsAbstract));
        }

        /// <summary>
        ///     Resolves all types related to those provided in <see cref="typesToExplore" />
        /// </summary>
        /// <param name="typesToExplore"></param>
        /// <returns></returns>
        public static HashSet<Type> GetRelatedTypes(IEnumerable<Type> typesToExplore) {
            var found = new HashSet<Type>();
            foreach (var type in typesToExplore)
                ResolveRelatedTypes(type, found);

            return found;
        }

        /// <summary>
        ///     Get the class hierarchy
        /// </summary>
        /// <param name="type">type to explore</param>
        /// <param name="toFill">to prevent allocations, always fill</param>
        public static void PopulateHierarchy(this Type type, HashSet<Type> toFill) {
            if (type == null)
                return;

            var baseType = type.BaseType;
            while (baseType != null) {
                toFill.Add(baseType);
                baseType = baseType.BaseType;
            }
        }

        /// <summary>
        ///     Get the class interfaces
        /// </summary>
        /// <param name="type">type to explore</param>
        /// <param name="toFill">to prevent allocations, always fill</param>
        public static void PopulateInterfaces(this Type type, HashSet<Type> toFill) {
            if (type == null)
                return;

            foreach (var vtInterface in type.GetInterfaces())
                toFill.Add(vtInterface);
        }

        /// <summary>
        ///     Find the common hierarchy between the two provided Types
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="toFill"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static void FindCommonHierarchy(Type left,
            Type right,
            HashSet<Type> toFill,
            Predicate<Type> filter = null) {
            var leftSet = new HashSet<Type>(left.GetInterfaces());
            var rightSet = new HashSet<Type>(right.GetInterfaces());
            left.PopulateHierarchy(leftSet);
            right.PopulateHierarchy(rightSet);
            foreach (var common in leftSet.Intersect(rightSet))
                if (null == filter || filter(common))
                    toFill.Add(common);
        }

        /// <summary>
        ///     Tests if the type is IEnumerable
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type) {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }


        /// <summary>
        ///     Returns true if the type and the provided type share a generic type definition
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericDefinition"></param>
        /// <param name="matches"></param>
        /// <returns></returns>
        /// <remarks>Add chaching</remarks>
        public static bool IsOfGeneric(this Type type, Type genericDefinition, out HashSet<Type> matches) {
            matches = null;
            if (false == type.IsConstructedGenericType)
                return false;

            matches = new HashSet<Type>();
            FindCommonHierarchy(type.GetGenericTypeDefinition(), genericDefinition, matches);
            return matches.Count > 0;
        }

        /// <summary>
        ///     Get all interfaces that are constructs of constructFilter
        /// </summary>
        /// <param name="type"></param>
        /// <param name="constructFilter"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetGenericInterfacesOf(this Type type, Type constructFilter) {
            return type.GetInterfaces()
                .Where(t => t.IsGenericType
                            && t.GetGenericTypeDefinition() == constructFilter);
        }

        /// <summary>
        ///     Returns the type of the element expected to be returned during enumeration
        ///     of <see cref="type" />
        /// </summary>
        /// <param name="type">Type to explore</param>
        /// <returns>null if not IEnumerable</returns>
        public static Type GetElementTypeOfEnumerable(this Type type) {
            if (!type.IsEnumerable())
                return null;

            if (type.IsArray)
                return type.GetElementType();

            if (type.IsConstructedGenericType)
                return GetGenericInterfacesOf(type, typeof(IEnumerable<>))
                    .Select(t => t.GetGenericArguments()[0])
                    .First();

            return typeof(object);
        }

        public static ConstructorInfo GetBestConstructor(this Type type, BindingFlags flags, params Type[] args) {
            foreach (var ctor in type.GetConstructors(flags)) {
                var ctorParams = ctor.GetParameters();
                if (ctorParams.Length != args.Length)
                    continue;

                var allAreBestMatch = args.Select((t, argIdx) => ctorParams[argIdx].ParameterType.IsAssignableFrom(t))
                    .All(b => b);
                if (allAreBestMatch)
                    return ctor;
            }

            return null;
        }

        public static ConstructorInfo GetBestConstructor(this Type type, params Type[] args) {
            return GetBestConstructor(type, BindingFlags.Public | BindingFlags.Instance, args);
        }

        /// <summary>
        ///     Resolves all types associated directly or indirectly with member fields
        ///     and class hierarchy
        /// </summary>
        /// <param name="type"></param>
        /// <param name="known"></param>
        public static void ResolveRelatedTypes(this Type type, HashSet<Type> known) {
            if (!known.Add(type))
                return;

            if (type.IsArray)
                ResolveRelatedTypes(type.GetElementType(), known);
            if (type.IsConstructedGenericType) {
                ResolveRelatedTypes(type.GetGenericTypeDefinition(), known);
                foreach (var genArgType in type.GetGenericArguments())
                    ResolveRelatedTypes(genArgType, known);
            }

            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object)) {
                ResolveRelatedTypes(baseType, known);
                baseType = baseType.BaseType;
            }
            foreach (var vtInterface in type.GetInterfaces())
                ResolveRelatedTypes(vtInterface, known);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                ResolveRelatedTypes(field.FieldType, known);
        }
    }
}