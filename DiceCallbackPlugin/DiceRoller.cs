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
        /// <summary>
        /// Rolls dice
        /// </summary>
        /// <param name="flavor">Name of what you're rolling</param>
        /// <param name="formula">text formula such as "1d6+5"</param>
        /// <param name="callbackId">Identifier of Callback from result</param>
        /// <param name="diceColors">Assign dice colors and textures to pools</param>
        public static void RollDice(string flavor, string formula, string callbackId, DiceColor[] diceColors = null)
        {
            // Manual Dice Roll
            flavor = flavor.Replace(" ", "_");
            formula = formula.Replace(" ", "");
            if (diceColors == null) diceColors = new DiceColor[0];

            var colors = "";
            foreach (var color in diceColors)
            { 
                if (color != null) colors += color.ToTag();
            }
            var title = $"{flavor}<size=0>{colors}<callback=\"{callbackId}\">";
            var command = $"talespire://dice/{title}:{formula}";

            Debug.Log(command);
            System.Diagnostics.Process.Start(command).WaitForExit();
        }

        /// <summary>
        /// Wrapper to convert single formula into array and still call
        /// </summary>
        /// <param name="flavor"></param>
        /// <param name="formula"></param>
        /// <param name="callbackId">Identifier of Callback from result</param>
        /// <param name="diceColors">Assign dice colors and textures to pools</param>
        public static void RollDice(string flavor, Dice formula, string callbackId, DiceColor diceColors = null)
        {
            RollDice(flavor, new Dice[]{formula}, callbackId, new DiceColor[] {diceColors});
        }

        /// <summary>
        /// Rolls dice
        /// </summary>
        /// <param name="flavor">Name of what you're rolling</param>
        /// <param name="formulas">Collection of dice to roll</param>
        /// <param name="callbackId">Identifier of Callback from result</param>
        /// <param name="diceColors">Assign dice colors and textures to pools</param>
        public static void RollDice(string flavor, Dice[] formulas, string callbackId, DiceColor[] diceColors = null)
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
            RollDice(flavor,textFormula, callbackId, diceColors);
        }

        private static string last = "";
        private static string last_key = "";

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
