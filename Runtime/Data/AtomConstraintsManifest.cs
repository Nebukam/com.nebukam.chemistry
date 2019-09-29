using System.Collections.Generic;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Nebukam.Chemistry
{

    [CreateAssetMenu(fileName = "AtomManifest", menuName = "Nebukam/WFC/Atom Constraints Manifest", order = 1)]
    public class AtomConstraintsManifest : ScriptableObject
    {

        public AtomConstraintsModel model;
        public AtomConstraints[] infos;
        public AtomConstraintsManifestInlined inlinedManifest;

        // Number of neighbor slots
        public int socketCount;
        // Header count
        public int headerCount;
        // Header count
        public int u;

    }

}
