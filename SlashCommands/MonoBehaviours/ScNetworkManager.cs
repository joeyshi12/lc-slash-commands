using GameNetcodeStuff;
using SlashCommands.Patches;
using Unity.Netcode;
using UnityEngine;

namespace SlashCommands.MonoBehaviours
{
    internal class ScNetworkManager : NetworkBehaviour
    {
        public static ScNetworkManager instance;

        void Awake()
        {
            instance = this;
        }

        [ServerRpc(RequireOwnership = false)]
        public void TeleportPlayerServerRpc(string playerUsername, Location location, SerializableVector3 position)
        {
            TeleportPlayerClientRpc(playerUsername, location, position);
        }

        [ClientRpc]
        public void TeleportPlayerClientRpc(string playerUsername, Location location, SerializableVector3 position)
        {
            PlayerControllerB playerControllerB = Array.Find(StartOfRound.Instance.allPlayerScripts, (PlayerControllerB script) => script.playerUsername == playerUsername);
            if (playerControllerB == null)
            {
                return;
            }
            switch (location) {
                case Location.Ship:
                    playerControllerB.isInElevator = true;
                    playerControllerB.isInHangarShipRoom = true;
                    playerControllerB.isInsideFactory = false;
                    break;
                case Location.Factory:
                    playerControllerB.isInElevator = false;
                    playerControllerB.isInHangarShipRoom = false;
                    playerControllerB.isInsideFactory = true;
                    break;
                default:
                    break;
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
            Landmine.SpawnExplosion(position.ToVector3() + Vector3.up, spawnExplosionEffect: true, killRange: 5.7f, damageRange: 6.4f);
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
