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
using System.Threading.Tasks;
using Sprockets.Core.DocumentIndexing.Types;
using Sprockets.Core.OperationalPatterns;
using Sprockets.Scripting.Types;

namespace Sprockets.Core.DocumentIndexing.Host {
    public class ServiceAgentBaseFactory {
        private readonly IQueryCompilerProvider _queryCompilerProvider;

        public ServiceAgentBaseFactory(IQueryCompilerProvider queryCompilerProvider) {
            _queryCompilerProvider = queryCompilerProvider;
        }

        /// <summary>
        ///     Called to simply get a computation. Can be used for simple data mining operations, or
        ///     for intercommunication between various service agents.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<TryOperationResult<ComputationResults>> ExecuteComputation(TextSearch search) {
            var ret = new TryOperationResult<ComputationResults>();

            try {
                var callback = await _queryCompilerProvider.Compile(search);
                ret.SetSuccess(callback());
            }
            catch (Exception ex) {
                ret.SetFailure(ex);
            }
            return ret;
        }


        /// <summary>
        ///     Called to simply get a computation. Can be used for simple data mining operations, or
        ///     for intercommunication between various service agents.
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<TryOperationResult<ComputationResults>> PostSearchToService(TextSearch search) {
            var ret = new TryOperationResult<ComputationResults>();

            try {
                var callback = await _queryCompilerProvider.Compile(search);
                ret.SetSuccess(callback());
            }
            catch (Exception ex) {
                ret.SetFailure(ex);
            }
            return ret;
        }

        ///// <summary>
        ///// Called to simply get a computation. Can be used for simple data mining operations, or
        ///// for intercommunication between various service agents. 
        ///// </summary>
        ///// <param name="search"></param>
        ///// <returns></returns>
        //public async Task<TryOperationResult<Guid>> InstantMicroService(BuildProject search)
        //{
        //    var ret = new TryOperationResult<ComputationResults>();

        //    try
        //    {
        //        var runType = await _queryCompilerProvider.Compile(search);
        //        if(runType==null)
        //            throw new InvalidOperationException("no boot agent");

        //        //Task.Run(() => {

        //        //});

        //        //ret.SetSuccess(callback(), out _);
        //    }
        //    catch (Exception ex)
        //    {
        //        ret.SetFailure(ex, out _);
        //    }
        //    return ret;
        //}


        //AppDomain CreateWorkDomain() {

        //var current = AppDomain.CurrentDomain;
        //var domain = AppDomain.CreateDomain("SPROCKET"+DateTime.Now.Ticks);
        //    domain.;
        //return domain;

        //}
    }

    public interface IQueryCompilerProvider {
        Task<Func<ComputationResults>> Compile(TextSearch search);
        Task<Type> Compile(BuildProject search);
    }
}