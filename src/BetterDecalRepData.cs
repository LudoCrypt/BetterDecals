using UnityEngine;
using System.Globalization;
using System.Text.RegularExpressions;
using System;

namespace BetterDecals2 {

    public class BetterDecalRepData : QuadQuadObjectData {
        public Vector2 panelPos;

        public bool useOverride = false;

        public BlendMode blendMode = BlendMode.MUL;
        public BlendMode blendModeLight = BlendMode.SCREEN;
        public BlendMode blendModeOverride = BlendMode.NORMAL;

        public bool affectTerrain = true;
        public bool affectColor1 = true;
        public bool affectColor2 = true;
        public bool affectDecals = true;
        public bool affectGrime = true;
        public bool affectHives = true;

        public float fromDepth = 0;
        public float toDepth = 1;

        public float noise = 0;

        public float[,] vertices;

        public string imageName = "PH";

        public BetterDecalRepData(PlacedObject owner) : base(owner) {
            this.vertices = new float[4, 6];

            for (int i = 0; i < 4; i++) {
                this.vertices[i, 0] = 0.50f;
                this.vertices[i, 1] = 0.25f;
                this.vertices[i, 2] = 0.00f;
                this.vertices[i, 3] = 1.00f;
                this.vertices[i, 4] = 1.00f;
                this.vertices[i, 5] = 1.00f;
            }
        }

        public int PackAffects() {
            int packedValue = 0;
            packedValue |= (affectTerrain ? 1 : 0) << 0;
            packedValue |= (affectColor1 ? 1 : 0) << 1;
            packedValue |= (affectColor2 ? 1 : 0) << 2;
            packedValue |= (affectDecals ? 1 : 0) << 3;
            packedValue |= (affectGrime ? 1 : 0) << 4;
            packedValue |= (affectHives ? 1 : 0) << 5;
            return packedValue;
        }

        public override void FromString(string s) {

            base.FromString(s);
            string[] array = Regex.Split(s, "~");

            if (array.Length < 48) {
#pragma warning disable CS0612
                LegacyFromString(s);
#pragma warning restore CS0612
                return;
            }

            int i = 8;

            this.panelPos.x = float.Parse(array[i++], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.panelPos.y = float.Parse(array[i++], NumberStyles.Any, CultureInfo.InvariantCulture);

            this.useOverride = bool.Parse(array[i++]);

            this.blendMode = BlendMode.ParseBlendMode(array[i++]);
            this.blendModeLight = BlendMode.ParseBlendMode(array[i++]);
            this.blendModeOverride = BlendMode.ParseBlendMode(array[i++]);

            this.fromDepth = float.Parse(array[i++], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.toDepth = float.Parse(array[i++], NumberStyles.Any, CultureInfo.InvariantCulture);

            this.noise = float.Parse(array[i++], NumberStyles.Any, CultureInfo.InvariantCulture);

            this.affectTerrain = bool.Parse(array[i++]);
            this.affectColor1 = bool.Parse(array[i++]);
            this.affectColor2 = bool.Parse(array[i++]);
            this.affectDecals = bool.Parse(array[i++]);
            this.affectGrime = bool.Parse(array[i++]);
            this.affectHives = bool.Parse(array[i++]);

            this.imageName = array[i++];

            if (array.Length == 48) {
                for (int j = 0; j < 24; j++) {
                    this.vertices[j / 6, j % 6] = float.Parse(array[j + i], NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                this.unrecognizedAttributes = null;
                return;
            }

            this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, i);
        }

        public override string ToString() {
            string text = base.BaseSaveString() + string.Format(CultureInfo.InvariantCulture, "~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}~{9}~{10}~{11}~{12}~{13}~{14}~{15}", new object[]
            {
                this.panelPos.x,
                this.panelPos.y,
                this.useOverride,
                this.blendMode,
                this.blendModeLight,
                this.blendModeOverride,
                this.fromDepth,
                this.toDepth,
                this.noise,
                this.affectTerrain,
                this.affectColor1,
                this.affectColor2,
                this.affectDecals,
                this.affectGrime,
                this.affectHives,
                this.imageName
            });

            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 6; j++) {
                    text += string.Format(CultureInfo.InvariantCulture, "~{0}", this.vertices[i, j]);
                }
            }

            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
        }

        [Obsolete]
        private void LegacyFromString(string s) {
            string[] array = Regex.Split(s, "~");
            this.panelPos.x = float.Parse(array[8], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.panelPos.y = float.Parse(array[9], NumberStyles.Any, CultureInfo.InvariantCulture);

            this.toDepth = float.Parse(array[11], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.noise = float.Parse(array[12], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.imageName = array[13];

            this.blendModeLight = BlendMode.ParseBlendMode(array[15]);
            this.blendModeOverride = BlendMode.ParseBlendMode(array[16]);
            this.useOverride = bool.Parse(array[17]);

            int before = 18;

            if (array.Length >= 40) {
                before = 20;
                string mode = array[18];

                switch (mode.ToUpper()) {
                    case "NONE":
                        this.affectTerrain = true;
                        this.affectColor1 = false;
                        this.affectColor2 = false;
                        break;
                    case "A":
                        this.affectTerrain = false;
                        this.affectColor1 = true;
                        this.affectColor2 = false;
                        break;
                    case "B":
                        this.affectTerrain = false;
                        this.affectColor1 = false;
                        this.affectColor2 = true;
                        break;
                    case "A AND B":
                        this.affectTerrain = false;
                        this.affectColor1 = true;
                        this.affectColor2 = true;
                        break;
                    case "NONE AND A":
                        this.affectTerrain = true;
                        this.affectColor1 = true;
                        this.affectColor2 = false;
                        break;
                    case "NONE AND B":
                        this.affectTerrain = true;
                        this.affectColor1 = false;
                        this.affectColor2 = true;
                        break;
                    case "ALL":
                        this.affectTerrain = true;
                        this.affectColor1 = true;
                        this.affectColor2 = true;
                        break;
                    default:
                        throw new ArgumentException("Invalid mode: " + mode);
                }

                for (int i = 0; i < 4; i++) {
                    this.vertices[i, 1] *= float.Parse(array[19]);
                }
            }

            bool noextraatt = false;
            if (array.Length >= 38 + (before - 18)) {
                for (int i = before; i < array.Length; i++) {
                    int v = (i - before) % 5;
                    this.vertices[(i - before) / 5, v > 0 ? v + 1 : v] = float.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                this.unrecognizedAttributes = null;
                noextraatt = true;
            }

            if (float.TryParse(array[10], out _) && !float.TryParse(array[14], out _)) {
                // in old update
                this.fromDepth = float.Parse(array[10], NumberStyles.Any, CultureInfo.InvariantCulture);
                this.blendMode = BlendMode.ParseBlendMode(array[14]);

                for (int i = 0; i < 4; i++) {
                    this.vertices[i, 1] *= this.vertices[i, 0] * this.vertices[i, 0];
                }


            }
            else if (!float.TryParse(array[10], out _) && float.TryParse(array[14], out _)) {
                // in new update
                this.fromDepth = float.Parse(array[14], NumberStyles.Any, CultureInfo.InvariantCulture);
                this.blendMode = BlendMode.ParseBlendMode(array[10]);
            }
            else {
                // Idk WHATS going on LOL
            }

            if (!noextraatt) {
                this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, before);
            }
        }
    }
}
