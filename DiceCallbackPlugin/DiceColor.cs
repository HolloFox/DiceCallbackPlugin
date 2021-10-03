using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace DiceCallbackPlugin
{
    public class DiceColor
    {
        public static Dictionary<Guid, Texture> Materials;

        public float Red = -1f;
        public float Green = -1f;
        public float Blue = -1f;
        public Guid TexId = Guid.Empty;
        public bool isGm = false;

        [JsonConstructor]
        public DiceColor() { }

        public DiceColor(UnityEngine.Color color)
        {
            Red = color.r;
            Green = color.g;
            Blue = color.b;
        }

        public DiceColor(Guid Texture)
        {
            TexId = Texture;
        }

        public string ToTag()
        {
            if (!(Red > -1) || !(Green > -1) || !(Blue > -1))
                return TexId != Guid.Empty ? $"<tex=\"{TexId}\">" : string.Empty;
            var r = ((int) (255 * Red)).ToString("X");
            if (r.Length == 1) r = "0" + r;
            var g = ((int) (255 * Green)).ToString("X");
            if (g.Length == 1) g = "0" + g;
            var b = ((int) (255 * Blue)).ToString("X");
            if (b.Length == 1) b = "0" + b;
            return $"<color=\"{r}{g}{b}\">";
        }

        public void SetRenderer(Renderer ___dieRenderer)
        {
            if (Red > -1 && Green > -1 && Blue > -1)
            {
                ___dieRenderer.material.SetColor("_Color", new Color(Red, Green, Blue));
            }
            if (TexId != Guid.Empty )
            {
                Debug.Log("Loading Texture");
                if (Materials.ContainsKey(TexId)) ___dieRenderer.material.mainTexture = Materials[TexId];
            }
        }
    }
}
