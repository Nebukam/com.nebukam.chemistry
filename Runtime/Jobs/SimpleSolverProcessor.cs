// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using Nebukam.JobAssist;
using Unity.Collections;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{

    public class SimpleSolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN> : AbstractSolverProcessor<T_SLOT, T_SLOT_INFOS, SimpleSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN>, T_BRAIN>, ISolverProcessor<T_SLOT, T_SLOT_INFOS, T_BRAIN>
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : unmanaged, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        protected override void InternalLock()
        {

        }

        protected override void Prepare(ref SimpleSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN> job, float delta)
        {
            base.Prepare(ref job, delta);
        }

        protected override void Apply(ref SimpleSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN> job)
        {

        }

        protected override void InternalUnlock() { }

    }
    
}
