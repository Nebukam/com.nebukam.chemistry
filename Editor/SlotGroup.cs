#if UNITY_EDITOR
using Nebukam.Cluster;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Unity.Mathematics.math;
using Nebukam.Utils;

namespace Nebukam.Chemistry.Ed
{

    /// <summary>
    /// A module template store core configuration used to solve & collapse a WFC grid.
    /// It is also used to generate and extract module grid models.
    /// </summary>
    [AddComponentMenu("Nebukam/WFC/Slot Group")]
    [DisallowMultipleComponent]
    public class SlotGroup : SlotGrid
    {

        [Header("Data Settings")]
        public string manifestID = "unamed slot cluster";

    }

}
#endif
