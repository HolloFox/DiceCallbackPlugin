using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DiceCallbackPlugin
{
    public static class DiceRoller
    {
        private static Dictionary<string, (Action<Dictionary<DiceType, List<int>>[],string, string,object>, object,string)> DiceCallbacks = new Dictionary<string, (Action<Dictionary<DiceType, List<int>>[],string, string, object>, object,string)>();

        internal static void ExitBoard()
        {
            DiceCallbacks.Clear();
            last = "";
            last_key = "";
        }

        /// <summary>
        /// Rolls dice
        /// </summary>
        /// <param name="flavor">Name of what you're rolling</param>
        /// <param name="formula">text formula such as "1d6+5"</param>
        /// <param name="callback">Callback with results</param>
        /// <param name="passThrough">Object to return via callback (part of pass through)</param>
        public static void RollDice(string flavor, string formula, Action<Dictionary<DiceType, List<int>>[],string, string, object> callback = null, object passThrough = null)
        {
            flavor = flavor.Replace(" ", "_");
            formula = formula.Replace(" ", "");
            var id = Guid.NewGuid();

            var title = $"{flavor}<size=0>{id}";

            var command = $"talespire://dice/{title}:{formula}";
            DiceCallbacks.Add($"{title}", (callback,passThrough,formula));

            Debug.Log(command);
            System.Diagnostics.Process.Start(command).WaitForExit();
        }

        /// <summary>
        /// Wrapper to convert single formula into array and still call
        /// </summary>
        /// <param name="flavor"></param>
        /// <param name="formula"></param>
        /// <param name="callback"></param>
        /// <param name="passThrough"></param>
        public static void RollDice(string flavor, Dice formula,
            Action<Dictionary<DiceType, List<int>>[], string, string, object> callback = null, object passThrough = null)
        {
            RollDice(flavor, new Dice[]{formula}, callback, passThrough);
        }

        /// <summary>
        /// Rolls dice
        /// </summary>
        /// <param name="flavor">Name of what you're rolling</param>
        /// <param name="formulas">Collection of dice to roll</param>
        /// <param name="callback">Callback with results</param>
        /// <param name="passThrough">Object to return via callback (part of pass through)</param>
        public static void RollDice(string flavor, Dice[] formulas, Action<Dictionary<DiceType, List<int>>[],string, string, object> callback = null, object passThrough = null)
        {
            var textFormula = "";
            for(var count = 0; count < formulas.Length; count++)
            {
                var formula = formulas[count];
                var keys = formula.GetKeys();
                var first = true;

                if (count > 0 && keys.Length > 0)
                {
                    textFormula += "/";
                }
                for (var i = 0; i < keys.Length; i++)
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

                count++;
            }

            textFormula = textFormula.Replace("+-","-");
            RollDice(flavor,textFormula, callback, passThrough);
        }

        private static string last = "";
        private static string last_key = "";

        internal static void CheckDice()
        {
            if (DiceManager.HasInstance)
            {
                var obj = GameObject.Find("GUIManager/PANEL_STRUCTURE/MiddlePanel/MiddlePanel/MiddlePanel/MidTopPanel/DiceRollResults");
                if (obj != null)
                {
                    var comp = obj.GetComponent<UIDiceRollResult>();
                    if (comp.isActiveAndEnabled)
                    {
                        var TXT = comp.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                        var key = TXT.text;
                        if (key.Contains("Rolled ") )
                        {
                            key = key.Replace("Rolled ", "");

                            // We only process if the roll is registered by the Key
                            if (DiceCallbacks.ContainsKey(key))
                            {
                                var single = comp.transform.GetChild(1).GetChild(1).GetChild(0)
                                    .GetComponent<UIDiceScoreItem>();
                                var multi = comp.transform.GetChild(1).GetChild(2);

                                var diceCollections = new List<Dictionary<DiceType, List<int>>>();

                                if (single.isActiveAndEnabled)
                                {
                                    var diceCollection = new Dictionary<DiceType, List<int>>();
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

                                    if (!diceCollection.ContainsKey(d))
                                    {
                                        diceCollection.Add(d, new List<int>());
                                    }

                                    diceCollection[d].Add(int.Parse(r.text));
                                    diceCollections.Add(new Dictionary<DiceType, List<int>>(diceCollection));
                                }
                                else
                                {
                                    var add = true;
                                    var total = false;
                                    var diceCollection = new Dictionary<DiceType, List<int>>();

                                    var children = multi.Children().ToArray();
                                    for (var i = 0; i < children.Count(); i++)
                                    {
                                        var child = children[i];
                                        var ScoreItem = child.GetComponent<UIDiceScoreItem>();
                                        var TextItem = child.GetComponent<TextMeshProUGUI>();
                                        var TotalItem = child.GetComponent<UIDiceTotalItem>();

                                        if (ScoreItem != null)
                                        {
                                            var r = ScoreItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                                            var t = ScoreItem.transform.GetComponent<RawImage>();
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

                                            if (!diceCollection.ContainsKey(d))
                                            {
                                                diceCollection.Add(d, new List<int>());
                                            }

                                            var value = int.Parse(r.text);
                                            if (!add) value = -value;
                                            diceCollection[d].Add(value);
                                        }
                                        else if (TotalItem != null)
                                        {
                                            if (total)
                                            {
                                                diceCollections.Add(
                                                    new Dictionary<DiceType, List<int>>(diceCollection));
                                                diceCollection.Clear();
                                                total = false;
                                                add = true;
                                            }
                                            else
                                            {
                                                var r = TotalItem.transform.GetComponent<TextMeshProUGUI>();
                                                if (!diceCollection.ContainsKey(DiceType.modifier))
                                                {
                                                    diceCollection.Add(DiceType.modifier, new List<int>());
                                                }

                                                var value = int.Parse(r.text);
                                                if (!add) value = -value;
                                                diceCollection[DiceType.modifier].Add(value);
                                            }
                                        }
                                        else if (TextItem != null)
                                        {
                                            switch (TextItem.text)
                                            {
                                                case "-":
                                                    add = false;
                                                    break;
                                                case "+":
                                                    add = true;
                                                    break;
                                                case "=":
                                                    total = true;
                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (!key.Equals(last_key) || !last.Equals(JsonConvert.SerializeObject(diceCollections)))
                                {
                                    last_key = key;
                                    last = JsonConvert.SerializeObject(diceCollections);
                                    var title = last_key.Substring(0, last_key.IndexOf("<size=0>"));
                                    var formula = "";
                                    if (DiceCallbacks.ContainsKey(key)) formula = DiceCallbacks[key].Item3;
                                    Debug.Log($"Formula:{formula}");
                                    Debug.Log($"Dice Data:{JsonConvert.SerializeObject(diceCollections)}");
                                    if (DiceCallbacks.ContainsKey(key) && DiceCallbacks[key].Item1 != null)
                                        DiceCallbacks[key].Item1(diceCollections.ToArray(), title, formula,
                                            DiceCallbacks[key].Item2);
                                }
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
