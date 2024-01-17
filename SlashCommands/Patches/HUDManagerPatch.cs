using GameNetcodeStuff;
using HarmonyLib;
using SlashCommands.MonoBehaviours;
using UnityEngine;
using static SlashCommands.MonoBehaviours.ScNetworkManager;

namespace SlashCommands.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("AddChatMessage")]
        [HarmonyPrefix]
        static void AddChatMessagePrefix(HUDManager __instance, ref string chatMessage, ref string nameOfUserWhoTyped)
        {
            if (chatMessage[0] != '/')
            {
                return;
            }
            Plugin.mls.LogInfo(nameOfUserWhoTyped + " executed " + chatMessage);
            string[] args = chatMessage.Split(' ');
            string command = args[0].TrimStart('/');
            switch (command)
            {
                case "tp":
                    TeleportPlayer(args);
                    return;
                case "warp":
                    WarpPlayer(args);
                    break;
                case "spawn":
                    SpawnEntity(args);
                    break;
                case "time":
                    SetTime(args);
                    break;
                case "explode":
                    ExplodePlayer(args);
                    break;
                default:
                    break;
            }
        }
        
        private static void TeleportPlayer(string[] args)
        {
            if (args.Length < 3)
            {
                return;
            }
            // TODO: use player indices instead for consistency
            string targetName = args[1];
            string destinationName = args[2];
            PlayerControllerB targetController = null;
            PlayerControllerB destinationController = null;
            foreach (PlayerControllerB controller in StartOfRound.Instance.allPlayerScripts)
            {
                if (controller.playerUsername == targetName)
                {
                    targetController = controller;
                }
                if (controller.playerUsername == destinationName)
                {
                    destinationController = controller;
                }
            }
            if (targetController == null || destinationController == null)
            {
                return;
            }
            SerializableVector3 position = new SerializableVector3(destinationController.transform.position);
            if (destinationController.isInHangarShipRoom)
            {
                ScNetworkManager.instance.TeleportPlayerServerRpc(targetName, Location.Ship, position);
            }
            else
            {
                ScNetworkManager.instance.TeleportPlayerServerRpc(targetName, Location.Factory, position);
            }
        }

        private static void WarpPlayer(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }
            string playerUsername = GameNetworkManager.Instance.localPlayerController.playerUsername;
            Vector3 position;
            switch (args[1]) {
                case "ship":
                    position = GameObject.FindObjectOfType<Terminal>().transform.position;
                    ScNetworkManager.instance.TeleportPlayerServerRpc(playerUsername, Location.Ship, new SerializableVector3(position));
                    break;
                case "factory":
                    position = RoundManager.Instance.insideAINodes[0].transform.position;
                    ScNetworkManager.instance.TeleportPlayerServerRpc(playerUsername, Location.Factory, new SerializableVector3(position));
                    break;
                default:
                    break;
            }
        }

        private static void SpawnEntity(string[] args)
        {

        }

        private static void SetTime(string[] args)
        {

        }
           
        private static void ExplodePlayer(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }
            string targetName = args[1];
            foreach (PlayerControllerB controller in StartOfRound.Instance.allPlayerScripts)
            {
                if (!controller.isPlayerDead && targetName == controller.playerUsername)
                {
                    SerializableVector3 explosionPosition = new SerializableVector3(controller.transform.position);
                    ScNetworkManager.instance.SpawnExplosionServerRpc(explosionPosition);
                }
            }
        }
    }

    public enum Location
    {
        Ship, Factory
    }
}
