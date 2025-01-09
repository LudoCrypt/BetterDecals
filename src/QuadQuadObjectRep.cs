using DevInterface;
using RWCustom;
using UnityEngine;

namespace BetterDecals2 {
    public class QuadQuadObjectRep : PlacedObjectRepresentation {

        public QuadQuadObjectRep(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name) {
            this.subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(-50, -50)));
            (this.subNodes[this.subNodes.Count - 1] as Handle).pos = (pObj.data as QuadQuadObjectData).handles[0];

            this.subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(-50, 50)));
            (this.subNodes[this.subNodes.Count - 1] as Handle).pos = (pObj.data as QuadQuadObjectData).handles[1];

            this.subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(50, 50)));
            (this.subNodes[this.subNodes.Count - 1] as Handle).pos = (pObj.data as QuadQuadObjectData).handles[2];

            this.subNodes.Add(new Handle(owner, "Rect_Handle", this, new Vector2(50, -50)));
            (this.subNodes[this.subNodes.Count - 1] as Handle).pos = (pObj.data as QuadQuadObjectData).handles[3];

            for (int i = 0; i < 6; i++) {
                this.fSprites.Add(new FSprite("pixel", true));
                owner.placedObjectsContainer.AddChild(this.fSprites[1 + i]);
                this.fSprites[1 + i].anchorY = 0f;
            }
        }

        public override void Refresh() {
            base.Refresh();
            base.MoveSprite(1, this.absPos);
            for (int i = 0; i < 4; i++) {
                (this.pObj.data as QuadQuadObjectData).handles[i] = (this.subNodes[i] as Handle).pos;
            }
            base.MoveSprite(1, this.absPos);
            this.fSprites[1].scaleY = (this.subNodes[0] as Handle).pos.magnitude;
            this.fSprites[1].rotation = Custom.VecToDeg((this.subNodes[0] as Handle).pos);

            base.MoveSprite(2, this.absPos + (this.subNodes[0] as Handle).pos);
            this.fSprites[2].scaleY = Vector2.Distance((this.subNodes[0] as Handle).pos, (this.subNodes[1] as Handle).pos);
            this.fSprites[2].rotation = Custom.AimFromOneVectorToAnother((this.subNodes[0] as Handle).pos, (this.subNodes[1] as Handle).pos);

            base.MoveSprite(3, this.absPos + (this.subNodes[1] as Handle).pos);
            this.fSprites[3].scaleY = Vector2.Distance((this.subNodes[1] as Handle).pos, (this.subNodes[2] as Handle).pos);
            this.fSprites[3].rotation = Custom.AimFromOneVectorToAnother((this.subNodes[1] as Handle).pos, (this.subNodes[2] as Handle).pos);

            base.MoveSprite(4, this.absPos + (this.subNodes[2] as Handle).pos);
            this.fSprites[4].scaleY = Vector2.Distance((this.subNodes[2] as Handle).pos, (this.subNodes[3] as Handle).pos);
            this.fSprites[4].rotation = Custom.AimFromOneVectorToAnother((this.subNodes[2] as Handle).pos, (this.subNodes[3] as Handle).pos);

            base.MoveSprite(5, this.absPos + (this.subNodes[3] as Handle).pos);
            this.fSprites[5].scaleY = Vector2.Distance((this.subNodes[3] as Handle).pos, (this.subNodes[0] as Handle).pos);
            this.fSprites[5].rotation = Custom.AimFromOneVectorToAnother((this.subNodes[3] as Handle).pos, (this.subNodes[0] as Handle).pos);
        }
    }
}
