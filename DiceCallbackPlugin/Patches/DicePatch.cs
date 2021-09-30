using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiceCallbackPlugin.Extensions;
using HarmonyLib;
using Newtonsoft.Json;
using Unity.Mathematics;
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

    [HarmonyPatch(typeof(Die), "Spawn")]
    public class DicePatchSpawn
    {
        static bool Prefix(string resource,
            float3 pos,
            quaternion rot,
            int rollId,
            byte groupId,
            bool gmOnlyDie, ref Die __result)
        {
            if (DicePatch2.color.Count == 0) return true;
            var color = DicePatch2.color[0];

            object[] data = new object[4]
            {
                (object) rollId,
                (object) groupId,
                (object) gmOnlyDie,
                (object) JsonConvert.SerializeObject(color),
            };
            Die component = PhotonNetwork.Instantiate(resource, (Vector3)pos, (Quaternion)rot, (byte)0, data).GetComponent<Die>();
            Type classType = component.GetType();
            MethodInfo mi = classType.GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(component, new object[] { rollId, groupId, gmOnlyDie });
            __result = component;
            return false;
        }
    }

    [HarmonyPatch(typeof(Die), "OnPhotonInstantiate")]
    public class DicePatchOnPhotonInstantiate
    {

        static void Postfix(PhotonMessageInfo info, ref Renderer ___dieRenderer, ref Die __instance)
        {
            object[] instantiationData = __instance.photonView?.instantiationData;
            if (instantiationData != null && instantiationData.Length > 3)
            {
                var color = JsonConvert.DeserializeObject<DiceColor>((string) instantiationData[3]);
                color.SetRenderer(___dieRenderer);
            }
        }
    }

    [HarmonyPatch(typeof(Die), "Init")]
    public class DicePatch2
    {
        public static List<DiceColor> color = new List<DiceColor>();
        static void Postfix(ref Renderer ___dieRenderer, int rollId, byte groupId, bool gmOnly, Material ___normalMaterial, Material ___gmMaterial)
        {
            if (!gmOnly && color.Count > 0)
            {
                color[0].SetRenderer(___dieRenderer);
                color.RemoveAt(0);
            }
        }
    }

    [HarmonyPatch(typeof(UIDiceTray), "Spawn")]
    public class UIDiceTrayPatch
    {
        static List<DiceColor> StripTags(string flavor)
        {
            var o = new List<DiceColor>();
            flavor = flavor.Replace(" ",""); // strip white space
            var i = flavor.IndexOf("<");
            if (i > 0)
            {
                var split = flavor.SplitTags();
                foreach (var tag in split)
                {
                    if (tag.Contains("<color="))
                    {
                        var hex = tag.Replace("<color=", "").Replace("\"", "");
                        Debug.Log($"Hex:{hex}");
                        var hexParts = hex.SplitInParts(2).ToList();

                        if (hexParts.Count == 3) hexParts.Add("FF");

                        float red = int.Parse(hexParts[0], System.Globalization.NumberStyles.HexNumber) / 255f;
                        float green = int.Parse(hexParts[1], System.Globalization.NumberStyles.HexNumber) / 255f;
                        float blue = int.Parse(hexParts[2], System.Globalization.NumberStyles.HexNumber) / 255f;
                        float alpha = int.Parse(hexParts[3], System.Globalization.NumberStyles.HexNumber) / 255f;
                        var color = new DiceColor(new Color(red, green, blue, alpha));
                        o.Add(color);
                    }else if (tag.Contains("<tex="))
                    {
                        var tex = tag.Replace("<tex=", "").Replace("\"", "");
                        Debug.Log($"Texture:{tex}");
                        var guid = new Guid(tex);
                        Debug.Log($"Guid:{guid}");
                        var color = new DiceColor(guid);
                        o.Add(color);
                    }
                }
            }
            return o;
        }

        private static int last = -1;
        private static int count;
        static bool Prefix(bool isGmRoll, ref DiceRollDescriptor ____lastRollDescriptor, ref int ____diceRollId)
        {
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
                last = ____diceRollId;
                var name = ____lastRollDescriptor.Groups[0].Name;
                var colors = StripTags(name);
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
                    }
                }
            }
            return true;
        }
    }
}