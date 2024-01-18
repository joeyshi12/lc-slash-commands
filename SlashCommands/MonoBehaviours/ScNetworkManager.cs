using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace SlashCommands.MonoBehaviours
{
    internal class ScNetworkManager : NetworkBehaviour
    {
        public static ScNetworkManager Instance;

        void Awake()
        {
            Instance = this;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerServerRpc(int playerId, bool isInFactory, SerializableVector3 position)
        {
            TeleportPlayerClientRpc(playerId, isInFactory, position);
        }

        [ClientRpc]
        public void TeleportPlayerClientRpc(int playerId, bool isInFactory, SerializableVector3 position)
        {
            PlayerControllerB playerControllerB = StartOfRound.Instance.allPlayerScripts[playerId];
            if (playerControllerB == null)
            {
                return;
            }
            if (isInFactory)
            {
                playerControllerB.isInElevator = false;
                playerControllerB.isInHangarShipRoom = false;
                playerControllerB.isInsideFactory = true;
            }
            else
            {
                playerControllerB.isInElevator = true;
                playerControllerB.isInHangarShipRoom = true;
                playerControllerB.isInsideFactory = false;
            }
            playerControllerB.averageVelocity = 0f;
            playerControllerB.velocityLastFrame = Vector3.zero;
            playerControllerB.transform.position = position.ToVector3();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnExplosionServerRpc(SerializableVector3 position)
        {
            SpawnExplosionClientRpc(position);
        }

        [ClientRpc]
        private void SpawnExplosionClientRpc(SerializableVector3 position)
        {
            Landmine.SpawnExplosion(position.ToVector3(), spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6.4f);
        }

        [Serializable]
        public class SerializableVector3
        {
            public float x, y, z;

            public SerializableVector3(Vector3 vec3)
            {
                x = vec3.x;
                y = vec3.y;
                z = vec3.z;
            }

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
