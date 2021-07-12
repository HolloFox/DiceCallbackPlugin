using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceCallbackPlugin
{
    internal static class ResultSetter
    {
        /// <summary>
        /// This should be called from the Dice Roller callback to replace the displayed Values.
        /// This updates both the Prompt and chat results
        /// </summary>
        /// <param name="values">Values to replace</param>
        public static void SetResults(string key, int[] values)
        {

            /* This needs to be able to set the results for both the 
                - Dice Roll Results (Prompt)
                - Chat Results (Chat Right side)
             */
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void CheckResults()
        {
            
        }

        internal static void ExitBoard()
        {
            
        }

        /// <summary>
        /// Removes the = operator and DiceTotalText
        /// </summary>
        public static void RemoveResults(string key)
        {

            /* This needs to be able to remove All results for both the 
                - Dice Roll Results (Prompt)
                - Chat Results (Chat Right side)
             */
        }

        /// <summary>
        /// Add new Line Operator and DiceTotalText
        /// </summary>
        /// <param name="value">Value displayed on new Result Line</param>
        public static void NewResultLine(string key, string value)
        {

            /* This needs to be able to add last line for both the 
                - Dice Roll Results (Prompt)
                - Chat Results (Chat Right side)
             */
        }
    }
}
