using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameChat.UI;
using Mono.Cecil.Cil;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceCallbackPlugin
{
    public static class DiceRoller
    {
        private static Dictionary<string, Action<Dictionary<DiceType, List<int>>,string, string>> DiceCallbacks = new Dictionary<string, Action<Dictionary<DiceType, List<int>>,string, string>>();

        /// <summary>
        /// Rolls dice
        /// </summary>
        /// <param name="flavor">Name of what you're rolling</param>
        /// <param name="formula">text formula such as "1d6+5"</param>
        /// <param name="callback">Callback with results</param>
        public static void RollDice(string flavor, string formula, Action<Dictionary<DiceType, List<int>>,string, string> callback = null)
        {
            flavor = flavor.Replace(" ", "_");
            formula = formula.Replace(" ", "");
            var id = Guid.NewGuid();

            var title = $"{flavor}<size=0>{id}";

            var command = $"talespire://dice/{title}:{formula}";
            DiceCallbacks.Add($"{title}", callback);

            Debug.Log(command);
            System.Diagnostics.Process.Start(command).WaitForExit();
        }

        /// <summary>
        /// Rolls dice
        /// </summary>
        /// <param name="flavor">Name of what you're rolling</param>
        /// <param name="formula">Collection of dice to roll</param>
        /// <param name="callback">Callback with results</param>
        public static void RollDice(string flavor, Dice formula, Action<Dictionary<DiceType, List<int>>,string, string> callback = null)
        {
            var textFormula = "";
            var keys = formula.GetKeys();
            var first = true;
            for(var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                
                if (formula.Get(key) != 0)
                {
                    if (!first && formula.Get(key) > 0) textFormula += "+";
                    if (key == DiceType.modifier)
                    {
                        textFormula += $"{formula.Get(key)}";
                    }
                    else textFormula += $"{formula.Get(key)}{key}";
                    first = false;
                }
            }
            RollDice(flavor,textFormula, callback);
        }

        private static string last = "";
        private static string last_key = "";

        public static void CheckDice()
        {
            if (DiceManager.HasInstance)
            {
                // var obj = GameObject.Find("GUIManager/PANEL_STRUCTURE/MiddlePanel/MiddlePanel/RightPanel/ContentPanel/Content/#PANEL_Chat/UIChatManager/Scroll View/Viewport/Content/ChatSection(Clone)"); //.GetComponent<UIChatMessageSection>();
                var obj = GameObject.Find("GUIManager/PANEL_STRUCTURE/MiddlePanel/MiddlePanel/MiddlePanel/MidTopPanel/DiceRollResults"); //.GetComponent<UIChatMessageSection>();
                if (obj != null)
                {
                    var comp = obj.GetComponent<UIDiceRollResult>();
                    if (comp.isActiveAndEnabled)
                    {
                        // Need to detect change as well not just open/close
                        var TXT = comp.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                        var key = TXT.text;
                        if (key.Contains("Rolled ") )
                        {
                            key = key.Replace("Rolled ", "");

                            var single = comp.transform.GetChild(1).GetChild(1).GetChild(0)
                                .GetComponent<UIDiceScoreItem>();
                            var multi = comp.transform.GetChild(1).GetChild(2);

                            Dictionary<DiceType, List<int>> DiceCollection = new Dictionary<DiceType, List<int>>();

                            if (single.isActiveAndEnabled)
                            {
                                var r = single.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                                var t = single.transform.GetComponent<RawImage>();
                                var d = DiceType.d4;
                                switch (t.texture.name)
                                {
                                    case "1D4":
                                        d = DiceType.d4;
                                        break;
                                    case "1D6":
                                        d = DiceType.d6;
                                        break;
                                    case "1D8":
                                        d = DiceType.d8;
                                        break;
                                    case "1D10":
                                        d = DiceType.d10;
                                        break;
                                    case "1D100":
                                        d = DiceType.d100;
                                        break;
                                    case "1D12":
                                        d = DiceType.d12;
                                        break;
                                    case "1D20":
                                        d = DiceType.d20;
                                        break;
                                }
                                if (!DiceCollection.ContainsKey(d))
                                {
                                    DiceCollection.Add(d, new List<int>());
                                }
                                DiceCollection[d].Add(int.Parse(r.text));
                            }
                            else
                            {
                                var components = multi.GetComponentsInChildren<UIDiceScoreItem>();
                                foreach (var component in components)
                                {
                                    var r = component.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                                    var t = component.transform.GetComponent<RawImage>();
                                    var d = DiceType.d4;
                                    switch (t.texture.name)
                                    {
                                        case "1D4":
                                            d = DiceType.d4;
                                            break;
                                        case "1D6":
                                            d = DiceType.d6;
                                            break;
                                        case "1D8":
                                            d = DiceType.d8;
                                            break;
                                        case "1D10":
                                            d = DiceType.d10;
                                            break;
                                        case "1D100":
                                            d = DiceType.d100;
                                            break;
                                        case "1D12":
                                            d = DiceType.d12;
                                            break;
                                        case "1D20":
                                            d = DiceType.d20;
                                            break;
                                    }
                                    if (!DiceCollection.ContainsKey(d))
                                    {
                                        DiceCollection.Add(d, new List<int>());
                                    }
                                    DiceCollection[d].Add(int.Parse(r.text));
                                }

                                var totalComponents = multi.GetComponentsInChildren<UIDiceTotalItem>();
                                if (totalComponents.Length == 2)
                                {
                                    var r = totalComponents[0].gameObject.GetComponent<TextMeshProUGUI>();
                                    var d = DiceType.modifier;
                                    if (!DiceCollection.ContainsKey(d))
                                    {
                                        DiceCollection.Add(d, new List<int>());
                                    }
                                    DiceCollection[d].Add(int.Parse(r.text));
                                }
                            }
                            
                            if (!key.Equals(last_key) || !last.Equals(JsonConvert.SerializeObject(DiceCollection)))
                            {
                                last_key = key;
                                last = JsonConvert.SerializeObject(DiceCollection);
                                var title = last_key.Substring(0, last_key.IndexOf("<size=0>"));
                                var formula = "";

                                var indexes = DiceCollection.Keys.ToArray();
                                for (int i = 0; i < indexes.Length; i++)
                                {
                                    var index = indexes[i];
                                    if (i > 0) formula += "+";
                                    if (index != DiceType.modifier) formula += $"{DiceCollection[index].Count}";
                                    else formula += $"{DiceCollection[index][0]}";
                                    if (index != DiceType.modifier) formula += $"{index}";
                                }
                                if (DiceCallbacks.ContainsKey(key) && DiceCallbacks[key] != null) DiceCallbacks[key](DiceCollection,title,formula);
                            }
                        }
                    }
                }
            }
            else
            {
                last = "";
                last_key = "";
            }
        }

        public class Dice
        {
            private Dictionary<DiceType, int> DiceCollection = new Dictionary<DiceType, int>();
            public Dice()
            {
                DiceCollection.Add(DiceType.d4,0);
                DiceCollection.Add(DiceType.d6,0);
                DiceCollection.Add(DiceType.d8,0);
                DiceCollection.Add(DiceType.d10,0);
                DiceCollection.Add(DiceType.d100,0);
                DiceCollection.Add(DiceType.d12,0);
                DiceCollection.Add(DiceType.d20,0);
                DiceCollection.Add(DiceType.modifier,0);
            }
            public Dice((DiceType t,int v)[] values)
            {
                DiceCollection.Add(DiceType.d4, 0);
                DiceCollection.Add(DiceType.d6, 0);
                DiceCollection.Add(DiceType.d8, 0);
                DiceCollection.Add(DiceType.d10, 0);
                DiceCollection.Add(DiceType.d100, 0);
                DiceCollection.Add(DiceType.d12, 0);
                DiceCollection.Add(DiceType.d20, 0);
                DiceCollection.Add(DiceType.modifier, 0);

                foreach (var (t, v) in values)
                {
                    Set(t,v);
                }
            }

            public void Clear()
            {
                foreach (var key in GetKeys())
                {
                    DiceCollection[key] = 0;
                }
            }

            public void Set(DiceType key, int value)
            {
                DiceCollection[key] = value;
            }

            public void Add(DiceType key, int value)
            {
                DiceCollection[key] += value;
            }

            public void Remove(DiceType key, int value)
            {
                DiceCollection[key] -= value;
            }

            public int Get(DiceType key)
            {
                return DiceCollection[key];
            }

            public DiceType[] GetKeys()
            {
                return DiceCollection.Keys.ToArray();
            }
        }

        public enum DiceType
        {
            d4,
            d6,
            d8,
            d10,
            d100,
            d12,
            d20,
            modifier
        }
    }
}
