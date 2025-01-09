using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BetterDecals2 {
    public class QuadQuadObjectData : PlacedObject.Data {

        public Vector2[] handles;

        public QuadQuadObjectData(PlacedObject owner) : base(owner) {
            this.handles = new Vector2[4];
            this.handles[0] = new Vector2(-50, -50);
            this.handles[1] = new Vector2(-50, 50);
            this.handles[2] = new Vector2(50, 50);
            this.handles[3] = new Vector2(50, -50);
        }

        public override void FromString(string s) {
            string[] array = Regex.Split(s, "~");
            this.handles[0].x = float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[0].y = float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[1].x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[1].y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[2].x = float.Parse(array[4], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[2].y = float.Parse(array[5], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[3].x = float.Parse(array[6], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.handles[3].y = float.Parse(array[7], NumberStyles.Any, CultureInfo.InvariantCulture);
            this.unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8);
        }

        protected string BaseSaveString() {
            return string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}", new object[]
            {
                this.handles[0].x,
                this.handles[0].y,
                this.handles[1].x,
                this.handles[1].y,
                this.handles[2].x,
                this.handles[2].y,
                this.handles[3].x,
                this.handles[3].y
            });
        }

        public override string ToString() {
            string text = this.BaseSaveString();
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "~", this.unrecognizedAttributes);
        }
    }
}
