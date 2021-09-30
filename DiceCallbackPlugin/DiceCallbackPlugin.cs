using UnityEngine;
using BepInEx;
using ExtraAssetsLibrary;
using HarmonyLib;

namespace DiceCallbackPlugin
{

    [BepInPlugin(Guid, "HolloFoxes' Dice Callback Plug-In", Version)]
    public class DiceCallbackPlugin : BaseUnityPlugin
    {
        // constants
        public const string Guid = "org.hollofox.plugins.DiceCallbackPlugin";
        private const string Version = "3.0.0.0";

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Debug.Log("DiceCallback Plug-in loaded");
            ModdingUtils.Initialize(this, Logger);
            var harmony = new Harmony(Guid);
            harmony.PatchAll();
        }

        /// <summary>
        /// Looping method run by plugin
        /// </summary>
        void Update()
        {

        }
    }
}
