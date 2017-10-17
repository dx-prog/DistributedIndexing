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
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Sprockets.Core.Disposables;

namespace Sprockets.Core.Security {
    public static class StringExtension {
        public static IDisposable Pinned<T>(this T obj) where T : class {
            return Pinned(obj, out _);
        }

        public static string ToCopy(this string str) {
            return string.Copy(str ?? "");
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IDisposable Pinned<T>(this T obj, out IntPtr ptr) where T : class {
            IDisposableAction ret = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
                ret = new FinalizerAction(() => handle.Free());
                ptr = handle.AddrOfPinnedObject();
            }
            return ret;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IDisposable PinnedOutWithClear<T>(this T[]obj, out T[]ptr) where T : struct {
            IDisposable ret = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                ret = obj.ToAutoClear();
                ptr = obj;
            }
            return ret;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static SecureString ToSecureString(this string str, bool erase) {
            var ret = default(SecureString);
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                if (erase)
                    using (ToAutoClear(str)) {
                        ret = ToSecureString(str);
                    }
                else
                    ret = ToSecureString(str);
            }
            return ret;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static SecureString ToSecureString(this string str) {
            var ret = new SecureString();
            foreach (var c in str)
                ret.AppendChar(c);


            return ret;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IDisposable ToPinnedArray(this string str, bool clear, out byte[] buffer) {
            buffer = new byte[str.Length * 2];
            var ret = clear ? buffer.PinnedOutWithClear(out _) : buffer.Pinned();

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                for (var i = 0; i < str.Length; i++) {
                    var c = str[i];
                    buffer[i * 2 + 0] = (byte) (c & 0xFF);
                    buffer[i * 2 + 1] = (byte) ((c >> 8) & 0xFF);
                }
            }

            return ret;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static SecureString GetSecureString(this byte[] buffer) {
            var ret = new SecureString();
            using (buffer.Pinned()) {
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    for (var i = 0; i < buffer.Length; i += 2)
                        ret.AppendChar(
                            (char) (buffer[i + 0] |
                                    (buffer[i + 1] << 8))
                        );
                }
            }

            return ret;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IDisposable ToAutoClear(this string str) {
            IDisposable ret = null;
            try {
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    var handle = str.Pinned();
                    ret = new FinalizerAction(() => {
                        try {
                            MemoryClearString(str);
                        }
                        finally {
                            handle.Dispose();
                        }
                    });
                }
                return ret;
            }
            catch (ThreadAbortException) {
                ret?.Dispose();
                throw;
            }
            catch (Exception) {
                ret?.Dispose();
                throw;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IDisposable ToAutoClear<T>(this T[] buffer) where T : struct {
            IDisposable ret = null;
            try {
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    var handle = buffer.Pinned();
                    ret = new FinalizerAction(() => {
                        try {
                            MemoryClearArray(buffer);
                        }
                        finally {
                            handle.Dispose();
                        }
                    });
                }
                return ret;
            }
            catch (ThreadAbortException) {
                ret?.Dispose();
                throw;
            }
            catch (Exception) {
                ret?.Dispose();
                throw;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void MemoryClearArray<T>(this T[] buffer) where T : struct {
            if (buffer == null)
                return;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                using (buffer.Pinned(out var ptr)) {
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static void MemoryClearString(this string str, bool allowInterned = false) {
            if (str == null)
                return;

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            }
            finally {
                using (str.Pinned(out var ptr)) {
                    if (ReferenceEquals(str, string.IsInterned(str)) && false == allowInterned)
                        throw new InvalidOperationException();

                    for (var i = 0; i < str.Length; i++)
                        Marshal.WriteInt16(ptr, i * sizeof(char), ' ');
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static IDisposable Decrypt(this SecureString value, out string willAutoErase) {
            var valuePtr = IntPtr.Zero;

            try {
                using ((willAutoErase = new string(' ', value.Length)).Pinned(out var ptrDst)) {
                    var ret = (IDisposable) null;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {
                    }
                    finally {
                        valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                        for (var i = 0; i < willAutoErase.Length; i++)
                            Marshal.WriteInt16(ptrDst, i * sizeof(char), Marshal.ReadInt16(valuePtr, i * sizeof(char)));

                        ret = willAutoErase.ToAutoClear();
                    }

                    return ret;
                }
            }
            finally {
                if (IntPtr.Zero != valuePtr)
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}