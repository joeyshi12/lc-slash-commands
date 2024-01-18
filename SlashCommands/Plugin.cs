using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SlashCommands.MonoBehaviours;
using SlashCommands.Patches;
using RuntimeNetcodeRPCValidator;

namespace SlashCommands
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string modGUID = "coolcat0.SlashCommands";
        const string modName = "SlashCommands";
        const string modVersion = "0.1.0";
        private readonly Harmony harmony = new Harmony(modGUID);

        public static Plugin Instance;
        public static ManualLogSource mls;
        private NetcodeValidator netcodeValidator;

        void Awake()
        {
            Instance = this;

            netcodeValidator = new NetcodeValidator(modGUID);
            netcodeValidator.PatchAll();
            netcodeValidator.BindToPreExistingObjectByBehaviour<ScNetworkManager, Terminal>();

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo($"{modName} version {modVersion} has been loaded");

            harmony.PatchAll(typeof(HUDManagerPatch));
        }
    }
}
