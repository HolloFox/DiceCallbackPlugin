using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bounce.Singletons;
using HarmonyLib;
using UnityEngine;

namespace DiceCallbackPlugin.Patches
{
    [HarmonyPatch(typeof(Die), "SetMaterial")]
    public class DicePatch
    { 
        static bool Prefix(ref Renderer ___dieRenderer, ref bool gmDie, Material ___normalMaterial, Material ___gmMaterial)
        {
            if (gmDie)
            {
                if (!((UnityEngine.Object) ___dieRenderer.sharedMaterial != (UnityEngine.Object) ___gmMaterial))
                    return false;

                var color = ___dieRenderer.material.GetColor("_Color");
                ___dieRenderer.sharedMaterial = ___gmMaterial;
                ___dieRenderer.material.SetColor("_Color", color);
            }
            else
            {
                if (!(___dieRenderer.sharedMaterial != ___normalMaterial))
                    return false;

                var color = ___dieRenderer.material.GetColor("_Color");
                ___dieRenderer.sharedMaterial = ___normalMaterial;
                ___dieRenderer.material.SetColor("_Color", color);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Die), "Init")]
    public class DicePatch2
    {
        public static List<Color> color = new List<Color>();
        static void Postfix(ref Renderer ___dieRenderer, int rollId, byte groupId, bool gmOnly, Material ___normalMaterial, Material ___gmMaterial)
        {
            if (!gmOnly && color.Count > 0)
            {
                ___dieRenderer.material.SetColor("_Color", color[0]);
                color.RemoveAt(0);
            }
        }
    }

    static class StringExtensions
    {

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", nameof(partLength));

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

    }

    [HarmonyPatch(typeof(UIDiceTray), "Spawn")]
    public class UIDiceTrayPatch
    {
        static List<Color> StripColorTags(string flavor)
        {
            var o = new List<Color>();
            flavor = flavor.Replace(" ",""); // strip white space
            var i = flavor.IndexOf("<color=");
            if (i > 0)
            {
                var xml = flavor.Substring(i);
                var split = xml.Split('>').ToList();
                split.Remove("");
                foreach (var tag in split)
                {
                    var hex = tag.Replace("<color=","").Replace("\"","");
                    Debug.Log($"Hex:{hex}");
                    var hexParts = hex.SplitInParts(2).ToList();
                    
                    if (hexParts.Count == 3) hexParts.Add("FF");
                    
                    float red = int.Parse(hexParts[0], System.Globalization.NumberStyles.HexNumber)/255f;
                    float green = int.Parse(hexParts[1], System.Globalization.NumberStyles.HexNumber)/255f;
                    float blue = int.Parse(hexParts[2], System.Globalization.NumberStyles.HexNumber)/255f;
                    float alpha = int.Parse(hexParts[3], System.Globalization.NumberStyles.HexNumber)/255f;
                    var color = new Color(red, green, blue, alpha);
                    o.Add(color);
                }
            }
            return o;
        }

        private static int last = -1;
        private static int count = 0;
        static bool Prefix(bool isGmRoll, ref DiceRollDescriptor ____lastRollDescriptor, ref int ____diceRollId)
        {
            Debug.Log($"Created:{____lastRollDescriptor.IsCreated}");
            DicePatch2.color.Clear();
            if (____lastRollDescriptor.Groups == null) return true;
            if (____lastRollDescriptor.Groups.Length > 0 )
            {
                if (____diceRollId == -1)
                {
                    if (count > 0)
                    {
                        return true;
                    }
                }

                count++;    

                Debug.Log($"{last}=>{____diceRollId}");
                last = ____diceRollId;
                var name = ____lastRollDescriptor.Groups[0].Name;
                var colors = StripColorTags(name);
                // Debug.Log($"Groups: {____lastRollDescriptor.Groups.Length}");
                foreach (var group in ____lastRollDescriptor.Groups)
                {
                    var count = 0;
                    if (colors.Count > 0)
                    {
                        var color = colors[0];
                        foreach (var dice in group.Dice)
                        {
                            count += dice.Count;
                            for (var i = 0; i < dice.Count; i++)
                            {
                                DicePatch2.color.Add(color);
                                DicePatch2.color.Add(color);
                            }
                        }
                        colors.RemoveAt(0);
                        // Debug.Log($"Dice Count: {count}");
                    }
                }
                
            }
            return true;
        }
    }
}