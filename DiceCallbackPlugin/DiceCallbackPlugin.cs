using UnityEngine;
using BepInEx;

namespace DiceCallbackPlugin
{

    [BepInPlugin(Guid, "HolloFoxes' Dice Callback Plug-In", Version)]
    public class DiceCallbackPlugin : BaseUnityPlugin
    {
        // constants
        private const string Guid = "org.hollofox.plugins.DiceCallbackPlugin";
        private const string Version = "1.0.1.0";

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Debug.Log("DiceCallback Plug-in loaded");
        }

        private static bool throwing = true;

        /// <summary>
        /// Looping method run by plugin
        /// </summary>
        void Update()
        {
            if (OnBoard())
            {
                if (throwing)
                {
                    DiceRoller.RollDice("magic missile",
                        new DiceRoller.Dice(
                            new (DiceRoller.DiceType t, int v)[]
                            {
                                (DiceRoller.DiceType.d6,5),
                                (DiceRoller.DiceType.d10,4),
                                (DiceRoller.DiceType.modifier,4),
                            })
                    );
                    throwing = false;
                }
                DiceRoller.CheckDice();
            }
        }

        private bool OnBoard()
        {
            return (CameraController.HasInstance &&
                    BoardSessionManager.HasInstance &&
                    BoardSessionManager.HasBoardAndIsInNominalState &&
                    !BoardSessionManager.IsLoading);
        }
    }
}
