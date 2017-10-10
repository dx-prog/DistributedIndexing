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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Sprockets.Core.Collection;
using Sprockets.Core.Reflection;
using Sprockets.LargeGraph.Serialization.IGL;

namespace Sprockets.LargeGraph.Serialization {
    public class IglScriptBuilder {
        private readonly TwoWayMap<Type, FieldInfo[]> _blitMap = new TwoWayMap<Type, FieldInfo[]>();
        private readonly WorkBacklog<IglScriptBody, Tuple<object, FieldInfo[]>> _blitwork;
        private readonly TwoWayLongMap<object> _constants = new TwoWayLongMap<object>();
        private readonly WorkBacklog<IglScriptBody, IEnumerable> _enumerationWork;
        private readonly TwoWayLongMap<object> _instances = new TwoWayLongMap<object>();
        private readonly HashSet<Type> _knownTypes = new HashSet<Type>();
        private readonly IglScriptBody _script;

        private readonly WorkBacklog<IglScriptBody, Tuple<object, IglRegisterType, SerializationInfo>>
            _serializationWork;

        private readonly Dictionary<Type, TokenObjectCategory> _tokenType = new Dictionary<Type, TokenObjectCategory>();
        private readonly TwoWayMap<Type, IglRegisterType> _typeMap = new TwoWayMap<Type, IglRegisterType>();
        private long _idCounter;

        public IglScriptBuilder(IglScriptBody script) {
            _blitwork = new WorkBacklog<IglScriptBody, Tuple<object, FieldInfo[]>>(script);
            _serializationWork =
                new WorkBacklog<IglScriptBody, Tuple<object, IglRegisterType, SerializationInfo>>(script);
            _enumerationWork = new WorkBacklog<IglScriptBody, IEnumerable>(script);
            _script = script;
        }


        public long TokenCount => _idCounter;


        public bool HasPressure => _blitwork.HasWork || _enumerationWork.HasWork || _serializationWork.HasWork;

        /// <summary>
        ///     Register the data, and related types associated with the
        ///     objType passed into this function
        /// </summary>
        /// <param name="objType"></param>
        public void RegisterType(Type objType) {
            if (_typeMap.TryGetId(objType, out var igl))
                return;

            var name = objType.AssemblyQualifiedName;
            if (string.IsNullOrEmpty(name))
                return;

            igl = new IglRegisterType(_idCounter++, objType.AssemblyQualifiedName);
            _typeMap.Map(igl, objType);

            _script.TypeDef.AddLast(igl);
            objType.ResolveRelatedTypes(_knownTypes);
            foreach (var type in _knownTypes)
                RegisterType(type);
        }


        public virtual void Report(TextWriter sw) {
            sw.WriteLine("#HEADER");
            foreach (var type in _script.TypeDef)
                sw.WriteLine("{0}:IMPORT {1}", type.Index, type.TypeName);

            sw.WriteLine("#CONST_TABLE");
            foreach (var type in _script.Constants)
                sw.WriteLine("{0}:CONST {1}", type.Index, type.Value);

            sw.WriteLine("#DECLARE");
            foreach (var type in _script.Declaration)
                sw.WriteLine("{0}:DECALARE AS TYPE {1}", type.Index, type.TypeId);

            sw.WriteLine("#BLOBS");
            foreach (var type in _script.FastInitializations)
                sw.WriteLine("{0}:FAST_COPY @{1} WITH {2}", type.Index, type.ObjectId, type.BlobToString());

            sw.WriteLine("#GRAPHS LINES");
            foreach (var type in _script.HeavyInitializations)
                sw.WriteLine("{0}:INIT {1} WITH @{2}", type.Index, type.ObjectId, string.Join(",", type.Blob));

            sw.WriteLine("#SPECIAL");
            foreach (var type in _script.SpecialInitializations)
                sw.WriteLine("{0}:INIT {1} WITH @{2}", type.Index, type.ObjectId, string.Join(",", type.Blob));

            sw.WriteLine("#CLOSURE");
            foreach (var type in _script.FieldSets)
                sw.WriteLine("{0}:SET {1}.{2} WITH @{3}", type.Index, type.ObjectId, type.FieldName, type.ValueId);
        }

        public long RegisterRoot(object obj, out TokenObjectCategory type) {
            return RegisterObject(obj, out type, true);
        }

        public virtual long RegisterObject(object obj, out TokenObjectCategory type, bool root = false) {
            type = TokenObjectCategory.Unknown;
            var datum = Tuple.Create(obj);
            if (TryRegisterNullConstantForNullObject(obj, root, datum, out var registerObject))
                return registerObject;

            var objectType = obj.GetType();
            RegisterType(objectType);
            // primatives are not subject to classification
            if (objectType.IsPrimitive || objectType == typeof(string)) {
                _tokenType[objectType] = type = TokenObjectCategory.Value;
                // will get the current id, or all add a new entry and update the existing
                // ID counter
                if (!_constants.TryGetOrAdd(ref _idCounter, datum, out var cosntId))
                    return cosntId;

                type |= TokenObjectCategory.NeedsUngraphing;
                _script.Constants.AddLast(
                    new IglDeclareValue(
                        cosntId,
                        _typeMap.GetId(objectType),
                        obj
                    ));
                if (root)
                    _script.RootObjects.Add(cosntId);
                return cosntId;
            }

            // true if the object is added the first time
            if (_instances.TryGetOrAdd(ref _idCounter, datum, out var objectId)) {
                type = BackLogSerialization(obj);
                type |= TokenObjectCategory.NeedsUngraphing;
                _tokenType[objectType] = type;

                if (!HandleCustomPlacement(type, objectType, obj, objectId))
                    _script.Declaration.AddLast(
                        new IglDeclareObject(
                            objectId,
                            _typeMap.GetId(objectType),
                            GetArrayInitDetails(obj)
                        ));
                if (root)
                    _script.RootObjects.Add(objectId);
                return objectId;
            }

            _tokenType.TryGetValue(objectType, out type);

            return objectId;
        }


        public virtual void RegisterFieldSet(object instance, string fieldName, object valueInstance) {
            var instanceId = RegisterObject(instance, out _);
            var valueId = RegisterObject(valueInstance, out _);

            _script.FieldSets.AddLast(new IglSetField(_idCounter++, instanceId, fieldName, valueId));
        }

        /// <summary>
        ///     Dump copy for arrays of primative types
        /// </summary>
        /// <param name="eid"></param>
        /// <param name="enumerable"></param>
        public virtual void RegisterDumbCopy(long eid, Array enumerable) {
            _script.FastInitializations.AddLast(
                new IglFastCopyTo(_idCounter++, eid, enumerable));
        }

        /// <summary>
        ///     A heavy copy involves pulling in referenced objects
        /// </summary>
        /// <param name="eid"></param>
        /// <param name="copyObjects"></param>
        public virtual void RegisterHeavyCopy(long eid, List<long> copyObjects) {
            _script.HeavyInitializations.AddLast(
                new IglHeavyCopyTo(_idCounter++, eid, copyObjects.ToArray()));
        }

        public virtual void RegisterSerialization(long root,
            IglRegisterType registryToken,
            Dictionary<string, long> specials) {
            _script.SpecialInitializations.AddLast(
                new IglSpecialSerialization(_idCounter++, root, registryToken, specials));
        }

        public void Freeze(Type t, long eid) {
        }


        public void RegisterSerialization(object o, IglRegisterType regToken, SerializationInfo info) {
            _serializationWork.AddWorkFor(Tuple.Create(o, regToken, info));
        }

        public void RegisterEnumeratableInitialization(IEnumerable enumerable) {
            _enumerationWork.AddWorkFor(enumerable);
        }

        public void RegisterBlit(object o, FieldInfo[] fieldMap) {
            _blitwork.AddWorkFor(Tuple.Create(o, fieldMap));
        }

        public void Pump(int maxCycles = 1024 * 8) {
            while (HasPressure) {
                OnBeforeStagePump(1, 0, ref maxCycles);
                {
                    while (_blitwork.Execute(ProcessBlitsAndSets) > 0)
                        if (maxCycles-- < 0)
                            throw new InvalidOperationException();
                }

                OnAfterStagePump(1, 0, ref maxCycles);
                OnBeforeStagePump(2, 0, ref maxCycles);
                {
                    while (_enumerationWork.Execute(ProcessEnumerables) > 0)
                        if (maxCycles-- < 0)
                            throw new InvalidOperationException();
                }

                OnAfterStagePump(2, 0, ref maxCycles);
                OnBeforeStagePump(3, 0, ref maxCycles);
                {
                    while (_serializationWork.Execute(ProcessSerializables) > 0)
                        if (maxCycles-- < 0)
                            throw new InvalidOperationException();
                }

                OnAfterStagePump(3, 0, ref maxCycles);
            }
        }

        protected virtual bool
            HandleCustomPlacement(TokenObjectCategory type, Type objectType, object o, long objectId) {
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="stageId"></param>
        /// <param name="stageType">Any number other then zero to indicate and track custom logic</param>
        /// <param name="cyleCounter"></param>
        protected virtual void OnBeforeStagePump(int stageId, int stageType, ref int cyleCounter) {
        }

        protected virtual void OnAfterStagePump(int stageId, int stageType, ref int cyleCounter) {
        }

        protected virtual TokenObjectCategory BackLogSerialization(object obj) {
            var objType = obj.GetType();
            if (TryContractSaving(objType, obj))
                return TokenObjectCategory.Contract;

            switch (obj) {
                case ISerializable serializable:
                    var context = new StreamingContext(StreamingContextStates.Other);
                    var info = new SerializationInfo(objType, new FormatterConverter());
                    serializable.GetObjectData(info, context);
                    RegisterSerialization(obj, _typeMap.GetId(objType), info);

                    return TokenObjectCategory.Serializable;

                // cannot use blitting on enumerables as there may be clobbering
                // developer will just need to implement iserializable for any special cases
                case IEnumerable enumerable:
                    RegisterEnumeratableInitialization(enumerable);
                    return TokenObjectCategory.Enumerable;

                default:
                    if (objType.GetCustomAttributes<SerializableAttribute>() == null)
                        throw new InvalidOperationException();

                    if (!_blitMap.TryGetId(objType, out var fieldMap)) {
                        var fieldData = new List<FieldInfo>();
                        foreach (var field in objType.GetFields(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                            if (field.GetCustomAttribute<NonSerializedAttribute>() != null)
                                continue;

                            fieldData.Add(field);
                        }

                        _blitMap.Map(fieldMap = fieldData.ToArray(), objType);
                    }

                    RegisterBlit(obj, fieldMap);

                    return TokenObjectCategory.FieldSettable;
            }
        }

        /// <summary>
        ///     Extend to handle data types in third-party libs that cannot
        ///     be modified to be serializable.
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        protected virtual bool TryContractSaving(Type objType, object o) {
            return false;
        }

        private bool TryRegisterNullConstantForNullObject(object obj, bool root, Tuple<object> datum, out long registerObject) {
            registerObject = 0;
            if (obj != null)
                return false;

            _constants.TryGetOrAdd(ref _idCounter, datum, out var cosntId);
            if (root)
                _script.RootObjects.Add(cosntId);

            _script.Constants.AddLast(
                new IglDeclareValue(
                    cosntId,
                    null,
                    null
                ));

            registerObject = cosntId;
            return true;
        }

        private Tuple<int, int> GetArrayInitDetails(object o) {
            if (!(o is Array array))
                return null;

            if (array.Rank > 1)
                throw new NotSupportedException();


            return Tuple.Create(array.Length, array.Rank);
        }


        private void ProcessSerializables(IglScriptBody script,
            Tuple<object, IglRegisterType, SerializationInfo> arg2) {
            var root = RegisterObject(arg2.Item1, out var objType);
            var numerator = arg2.Item3.GetEnumerator();
            while (numerator.MoveNext())
                RegisterObject(numerator.Value, out _);

            numerator = arg2.Item3.GetEnumerator();
            var specials = new Dictionary<string, long>();
            while (numerator.MoveNext())
                specials[numerator.Name] = RegisterObject(numerator.Value, out _);

            RegisterSerialization(root, arg2.Item2, specials);
        }

        private void ProcessEnumerables(IglScriptBody script, IEnumerable enumerable) {
            var eid = RegisterObject(enumerable, out var objType);
            var type = enumerable.GetType();
            if ((objType & TokenObjectCategory.NeedsUngraphing) == 0)
                return;

            Freeze(type, eid);
            // arrays of primatives can be small
            // but the ID's used are big
            // to prevent bloat we just gong to define a dump copy
            if (type.IsArray)
                if (type.IsFundementallyPrimative()) {
                    RegisterDumbCopy(eid, (Array) enumerable);
                    return;
                }

            var copyObjects = (from object obj in enumerable select RegisterObject(obj, out _)).ToList();
            RegisterHeavyCopy(eid, copyObjects);
        }

        private void ProcessBlitsAndSets(IglScriptBody script, Tuple<object, FieldInfo[]> kv) {
            foreach (var field in kv.Item2) {
                var fieldValue = field.GetValue(kv.Item1);
                RegisterObject(fieldValue, out var objType);
            }

            foreach (var field in kv.Item2)
                RegisterFieldSet(kv.Item1, field.Name, field.GetValue(kv.Item1));
        }
    }
}