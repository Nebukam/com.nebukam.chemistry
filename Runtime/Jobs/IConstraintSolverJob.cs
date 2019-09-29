using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Nebukam.Cluster;

namespace Nebukam.Chemistry
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="S">Slot Type</typeparam>
    /// <typeparam name="T">Slot Infos Type (paired with provided Slot type)</typeparam>
    public interface IConstraintSolverJob<S, T> : IJob
        where S : ConstrainedSlot, ISlot
        where T : struct, ISlotInfos<S>
    {

        // Cluster infos
        NativeArray<T> inputSlotInfos { set;}
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
