using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace LethalThings.MonoBehaviours
{
    public abstract class SaveableNetworkBehaviour : NetworkBehaviour
    {
        public int uniqueId = 0;

        public abstract void SaveObjectData();

        public abstract void LoadObjectData();
    }
}
