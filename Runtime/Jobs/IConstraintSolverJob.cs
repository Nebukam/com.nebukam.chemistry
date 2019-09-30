using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T_SLOT">Slot Type</typeparam>
    /// <typeparam name="T_SLOT_INFOS">Slot Infos Type (paired with provided Slot type)</typeparam>
    public interface IConstraintSolverJob<T_SLOT, T_SLOT_INFOS, T_BRAIN> : IJob
        where T_SLOT : ConstrainedSlot, ISlot
        where T_SLOT_INFOS : struct, ISlotInfos<T_SLOT>
        where T_BRAIN : struct, IClusterBrain
    {

        // Cluster infos
        T_BRAIN brain { set; }
        NativeArray<T_SLOT_INFOS> inputSlotInfos { set;}
        NativeHashMap<ByteTrio, int> inputSlotCoordinateMap { set; }

        // Model infos
        int socketCount { set; }
        NativeArray<int3> offsets { set; }
        NativeArray<int3> mirrors { set; }
        NativeArray<int> mirrorsIndices { set; }

        // Manifest infos
        int headerCount { set; }
        NativeArray<int3> headerIndices { set; }
        NativeArray<int> neighbors { set; }
        NativeArray<int> results { set; }

    }
}
