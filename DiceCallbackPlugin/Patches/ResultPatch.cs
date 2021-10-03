using System;
using System.Collections.Generic;
using System.Linq;
using DiceCallbackPlugin.Extensions;
using GameChat.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace DiceCallbackPlugin.Patches
{
    public enum CallbackType
    {
        DiceRoll,
        UIChat
    }

    public static class RegisteredCallbacks
    {
        /// <summary>
        /// Callbacks for specific dice rolls with callback name recorded.
        /// </summary>
        public static Dictionary<string, Func<DiceManager.DiceRollResultData, CallbackType, string>> Callbacks = new Dictionary<string, Func<DiceManager.DiceRollResultData, CallbackType, string>>();
        
        /// <summary>
        /// For any roll, albeit framework or not.
        /// </summary>
        public static Dictionary<string,Func<DiceManager.DiceRollResultData, CallbackType, string>> GlobalCallbacks = new Dictionary<string, Func<DiceManager.DiceRollResultData, CallbackType, string>>();

        private static List<string> GetCallBackIds(List<string> tags)
        {
            return (from tag in tags where tag.Contains("<callback=") select tag.Replace("<callback=", "").Replace("\"", "")).ToList();
        }

        public static List<Func<DiceManager.DiceRollResultData, CallbackType, string>> GetCallBacks(List<string> tags)
        {
            var ids = GetCallBackIds(tags);
            return Callbacks
                .Where(c => ids.Contains(c.Key))
                .Select(c => c.Value).ToList();
        }
    }

    [HarmonyPatch(typeof(UIDiceRollResult), "DisplayResult", typeof(DiceManager.DiceRollResultData),
        typeof(ClientGuid))]
    public class UIResultPatch
    {
        public static void Prefix(DiceManager.DiceRollResultData rollResultData, ref TextMeshProUGUI ____titleText)
        {

        }

        public static void Postfix(DiceManager.DiceRollResultData rollResultData, ref TextMeshProUGUI ____titleText)
        {
            Debug.Log("Prompt Roll Result Patch");
            var responses = new List<string>();
            if (rollResultData.RollId != -1)
            {
                Debug.Log($"Result:{rollResultData.GroupResults[0].Name}");
                var split = rollResultData.GroupResults[0].Name.SplitTags();
                var callBacks = RegisteredCallbacks.GetCallBacks(split);
                responses.AddRange(callBacks.Select(callback => callback(rollResultData, CallbackType.DiceRoll)));
            }

            responses.AddRange(RegisteredCallbacks.GlobalCallbacks
                .Select(callback => callback.Value(rollResultData, CallbackType.DiceRoll)));
            
            if (responses.Count > 0) ____titleText.text = "";
            foreach (var res in responses)
            {
                if (____titleText.text.Length > 0) ____titleText.text += "\n";
                ____titleText.text += res;
            }
        }
    }

    [HarmonyPatch(typeof(UIChatMessageRollResult), "DisplayResult", typeof(DiceManager.DiceRollResultData))]
    public class ChatResultPatch
    {
        public static void Postfix(DiceManager.DiceRollResultData rollResultData,ref TextMeshProUGUI ___rolledText)
        {
            Debug.Log("Chat Roll Result Patch");
            var responses = new List<string>();
            if (rollResultData.RollId != -1)
            {
                Debug.Log($"Result:{rollResultData.GroupResults[0].Name}");

                var split = rollResultData.GroupResults[0].Name.SplitTags();
                var callBacks = RegisteredCallbacks.GetCallBacks(split);
                responses.AddRange(callBacks.Select(callback => callback(rollResultData, CallbackType.UIChat)));
            }
            responses.AddRange(RegisteredCallbacks.GlobalCallbacks
                .Select(callback => callback.Value(rollResultData, CallbackType.UIChat)));
            if (responses.Count > 0) ___rolledText.text = "";
            foreach (var res in responses)
            {
                if (___rolledText.text.Length > 0) ___rolledText.text += "\n";
                ___rolledText.text += res;
            }
        }
    }
}
