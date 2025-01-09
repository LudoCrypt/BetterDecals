using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterDecals2 {
    public class BlendMode : ExtEnum<BlendMode> {
        public readonly int blendMode;

        public BlendMode(string value, int id, bool register = false) : base(value, register) {
            this.blendMode = id;
        }

        public BlendMode(string value, bool register = false) : base(value, register) {
            this.blendMode = ParseBlendMode(value).blendMode;
        }

        public static readonly BlendMode ADD = new("ADD", 0, true);
        public static readonly BlendMode BURN = new("BURN", 1, true);
        public static readonly BlendMode DARK = new("DARK", 2, true);
        public static readonly BlendMode DIFF = new("DIFF", 3, true);
        public static readonly BlendMode DODGE = new("DODGE", 4, true);
        public static readonly BlendMode GLOW = new("GLOW", 5, true);
        public static readonly BlendMode LIGHT = new("LIGHT", 6, true);
        public static readonly BlendMode MUL = new("MUL", 7, true);
        public static readonly BlendMode NEG = new("NEG", 8, true);
        public static readonly BlendMode OVERLAY = new("OVERLAY", 9, true);
        public static readonly BlendMode REFLECT = new("REFLECT", 10, true);
        public static readonly BlendMode SCREEN = new("SCREEN", 11, true);
        public static readonly BlendMode NORMAL = new("NORMAL", 12, true);

        public static BlendMode ParseBlendMode(string mode) {
            return mode.ToUpper() switch {
                "MUL" => BlendMode.MUL,
                "ADD" => BlendMode.ADD,
                "BURN" => BlendMode.BURN,
                "DODGE" => BlendMode.DODGE,
                "REFLECT" => BlendMode.REFLECT,
                "GLOW" => BlendMode.GLOW,
                "OVERLAY" => BlendMode.OVERLAY,
                "DIFF" => BlendMode.DIFF,
                "NEG" => BlendMode.NEG,
                "LIGHT" => BlendMode.LIGHT,
                "DARK" => BlendMode.DARK,
                "SCREEN" => BlendMode.SCREEN,
                "NORMAL" => BlendMode.NORMAL,
                _ => throw new ArgumentException("Invalid blend mode: " + mode),
            };
        }
    }

}
