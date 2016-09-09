/*
The MIT License (MIT)
Copyright (c) 2015 Sebastian Schöner
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

The original can be found at https://github.com/sschoener/cities-skylines-detour

This version has been converted to work with Unity 32-bit, with the help of Sebastian Schöner.
*/

using System;
using System.Reflection;

namespace Redirection
{

    public struct RedirectCallsState
    {
        public byte a, b, c, d;
        public uint e;
    }

    /// <summary>
    /// Helper class to deal with detours. This version is for Unity 5 x32 on Windows.
    /// We provide three different methods of detouring.
    /// </summary>
    public static class RedirectionHelper
    {
        /// <summary>
        /// Redirects all calls from method 'from' to method 'to'.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static RedirectCallsState RedirectCalls(MethodInfo from, MethodInfo to)
        {
            // GetFunctionPointer enforces compilation of the method.
            var fptr1 = from.MethodHandle.GetFunctionPointer();
            var fptr2 = to.MethodHandle.GetFunctionPointer();
            return PatchJumpTo(fptr1, fptr2);
        }

        public static RedirectCallsState RedirectCalls(RuntimeMethodHandle from, RuntimeMethodHandle to)
        {
            // GetFunctionPointer enforces compilation of the method.
            var fptr1 = from.GetFunctionPointer();
            var fptr2 = to.GetFunctionPointer();
            return PatchJumpTo(fptr1, fptr2);
        }

        public static void RevertRedirect(MethodInfo from, RedirectCallsState state)
        {
            var fptr1 = from.MethodHandle.GetFunctionPointer();
            RevertJumpTo(fptr1, state);
        }

        /// <summary>
        /// Primitive patching. Inserts a jump to 'target' at 'site'. Works even if both methods'
        /// callers have already been compiled.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="target"></param>
        public static RedirectCallsState PatchJumpTo(IntPtr site, IntPtr target)
        {
            RedirectCallsState state = new RedirectCallsState();

            // R11 is volatile.
            unsafe
            {
                byte* sitePtr = (byte*)site.ToPointer();
                state.a = *sitePtr;
                state.b = *(sitePtr + 5);
                state.c = *(sitePtr + 6);
                state.d = *(sitePtr + 7);
                state.e = *((uint*)(sitePtr + 1));

                *sitePtr = 0xBB; // mov EBX, target
                *((uint*)(sitePtr + 1)) = (uint)target.ToInt32();
                *(sitePtr + 5) = 0x41; // jmp EBX
                *(sitePtr + 6) = 0xFF;
                *(sitePtr + 7) = 0xE3;
            }

            return state;
        }

        public static void RevertJumpTo(IntPtr site, RedirectCallsState state)
        {
            unsafe
            {
                byte* sitePtr = (byte*)site.ToPointer();
                *sitePtr = state.a; // mov EBX, target
                *((uint*)(sitePtr + 1)) = state.e;
                *(sitePtr + 5) = state.b; // jmp EBX
                *(sitePtr + 6) = state.c;
                *(sitePtr + 7) = state.d;
            }
        }

    }
}