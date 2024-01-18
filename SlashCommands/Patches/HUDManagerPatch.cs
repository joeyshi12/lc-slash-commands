using DunGen;
using GameNetcodeStuff;
using HarmonyLib;
using SlashCommands.MonoBehaviours;
using System.Text;
using System;
using TMPro;
using UnityEngine;
using static SlashCommands.MonoBehaviours.ScNetworkManager;

namespace SlashCommands.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("AddChatMessage")]
        [HarmonyPrefix]
        static bool AddChatMessagePrefix(HUDManager __instance, ref string chatMessage, ref string nameOfUserWhoTyped)
        {
            if (chatMessage[0] != '/')
            {
                return true;
            }
            Plugin.mls.LogInfo(nameOfUserWhoTyped + " executed " + chatMessage);
            string[] args = chatMessage.Split(' ');
            string command = args[0].TrimStart('/');
            switch (command)
            {
                case "players":
                    ListPlayers();
                    break;
                case "tp":
                    TeleportPlayer(args);
                    break;
                case "warp":
                    WarpPlayer(args);
                    break;
                case "enemies":
                    ListEnemies();
                    break;
                case "spawn":
                    SpawnEntity(args);
                    break;
                case "explode":
                    ExplodePlayer(args);
                    break;
                default:
                    break;
            }
            return false;
        }

        private static void ListPlayers()
        {
            foreach (PlayerControllerB controller in StartOfRound.Instance.allPlayerScripts)
            {
                if (controller.isPlayerControlled)
                {
                    string message = string.Format("{0} [clientId={1}]", controller.playerUsername, controller.playerClientId);
                    HUDManager.Instance.AddTextToChatOnServer(message);
                }
            }
        }
        
        private static void TeleportPlayer(string[] args)
        {
            if (args.Length < 3)
            {
                return;
            }
            if (!int.TryParse(args[1], out int targetId) || !int.TryParse(args[2], out int destinationId))
            {
                return;
            }
            if (StartOfRound.Instance.allPlayerScripts.GetValue(targetId) is PlayerControllerB target && target.isActiveAndEnabled &&
                StartOfRound.Instance.allPlayerScripts.GetValue(destinationId) is PlayerControllerB destination && destination.isActiveAndEnabled) {
                ScNetworkManager.Instance.TeleportPlayerServerRpc(targetId, destination.isInsideFactory, new SerializableVector3(destination.transform.position));
            }
        }

        private static void WarpPlayer(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }
            PlayerControllerB controller = GameNetworkManager.Instance.localPlayerController;
            if (!controller.isPlayerControlled)
            {
                return;
            }
            Vector3 position;
            switch (args[1]) {
                case "ship":
                    position = GameObject.FindObjectOfType<Terminal>().transform.position;
                    ScNetworkManager.Instance.TeleportPlayerServerRpc((int)controller.playerClientId, false, new SerializableVector3(position));
                    break;
                case "factory":
                    position = RoundManager.Instance.insideAINodes[0].transform.position;
                    ScNetworkManager.Instance.TeleportPlayerServerRpc((int)controller.playerClientId, true, new SerializableVector3(position));
                    break;
                default:
                    break;
            }
        }

        private static void ListEnemies()
        {
            Plugin.mls.LogInfo(StartOfRound.Instance.currentLevel.name);
            foreach (SpawnableEnemyWithRarity enemy in StartOfRound.Instance.currentLevel.OutsideEnemies)
            {
                Plugin.mls.LogInfo(enemy.enemyType.name);
            }
            
        }

        private static void SpawnEntity(string[] args)
        {
            PlayerControllerB controller = StartOfRound.Instance.localPlayerController;
            if (Physics.Raycast(controller.transform.position, controller.transform.forward, out RaycastHit hit, 10f, 605030721))
            {
                ScNetworkManager.Instance.SpawnExplosionServerRpc(new SerializableVector3(hit.point + Vector3.up));
                //Start
                //currentRound.SpawnEnemyOnServer(currentRound.allEnemyVents[Random.Range(0, currentRound.allEnemyVents.Length)].floorNode.position, currentRound.allEnemyVents[i].floorNode.eulerAngles.y, currentLevel.Enemies.IndexOf(enemy));
                //hit.point
            }
        }
           
        private static void ExplodePlayer(string[] args)
        {
            if (args.Length < 2)
            {
                SerializableVector3 explosionPosition = new SerializableVector3(StartOfRound.Instance.localPlayerController.transform.position + Vector3.up);
                ScNetworkManager.Instance.SpawnExplosionServerRpc(explosionPosition);
                return;
            }
            if (!int.TryParse(args[1], out int playerId))
            {
                return;
            }
            if (StartOfRound.Instance.allPlayerScripts.GetValue(playerId) is PlayerControllerB controller && controller.isPlayerControlled)
            {
                SerializableVector3 explosionPosition = new SerializableVector3(controller.transform.position + Vector3.up);
                ScNetworkManager.Instance.SpawnExplosionServerRpc(explosionPosition);
            }
        }
    }
}
