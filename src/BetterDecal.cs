using System.Linq;
using System.IO;
using RWCustom;
using UnityEngine;
using static BetterDecals2.ExtraVertexInfo;
using System.Windows.Forms;
using BDLogs;

namespace BetterDecals2 {

    public class BetterDecal : UpdatableAndDeletable, IDrawable {

        public PlacedObject placedObject;

        public Vector2[] quad;
        public Vector2[] verts;

        public bool meshDirty;
        public bool elementDirty;

        private int gridDiv = 1;

        public BetterDecalRepData Data {
            get {
                return (placedObject.data as BetterDecalRepData);
            }
        }

        public BetterDecal(PlacedObject placedObject) {
            this.placedObject = placedObject;
            this.quad = new Vector2[4];

            for (int i = 0; i < 4; i++) {
                this.quad[i] = placedObject.pos + Data.handles[i];
            }

            this.gridDiv = this.GetIdealGridDiv();
            this.meshDirty = true;

            this.LoadFile(Data.imageName);
        }

        public void LoadFile(string fileName) {
            if (Futile.atlasManager.GetAtlasWithName(fileName) != null) {
                return;
            }

            string str = AssetManager.ResolveFilePath("Decals" + Path.DirectorySeparatorChar.ToString() + fileName + ".png");

            Texture2D texture = new(1, 1, TextureFormat.ARGB32, false);

            AssetManager.SafeWWWLoadTexture(ref texture, "file:///" + str, true, true);

            HeavyTexturesCache.LoadAndCacheAtlasFromTexture(fileName, texture, false);
        }

        public void UpdateAsset() {
            this.LoadFile(Data.imageName);
            this.elementDirty = true;
        }

        public void UpdateMesh() {
            this.meshDirty = true;
        }

        public override void Update(bool eu) {
            base.Update(eu);
            if (this.quad[0] != this.placedObject.pos + Data.handles[0] || this.quad[1] != this.placedObject.pos + Data.handles[1] || this.quad[2] != this.placedObject.pos + Data.handles[2] || this.quad[3] != this.placedObject.pos + Data.handles[3]) {
                this.meshDirty = true;
            }
        }

        public int GetIdealGridDiv() {
            float num = 0f;

            for (int i = 0; i < 3; i++) {
                if (Vector2.Distance(this.quad[i], this.quad[i + 1]) > num) {
                    num = Vector2.Distance(this.quad[i], this.quad[i + 1]);
                }
            }

            if (Vector2.Distance(this.quad[0], this.quad[3]) > num) {
                num = Vector2.Distance(this.quad[0], this.quad[3]);
            }

            return Mathf.Clamp(Mathf.RoundToInt(num / 150f), 1, 20);
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            sLeaser.sprites = new FSprite[1];
            TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh(Data.imageName, this.gridDiv);

            TangentTriangleMeshData data = TangentTriangleMeshData.GetData(triangleMesh);

            data.useTangents = true;

            data.tangents = new Vector4[triangleMesh.verticeColors.Length];
            data.texCoord2 = new Vector2[triangleMesh.verticeColors.Length];
            data.texCoord3 = new Vector2[triangleMesh.verticeColors.Length];

            for (int k = 0; k < data.tangents.Length; k++) {
                data.tangents[k] = new Vector4(0f, 0f);
                data.texCoord2[k] = new Vector2(0f, 0f);
                data.texCoord3[k] = new Vector2(0f, 0f);
            }

            sLeaser.sprites[0] = triangleMesh;
            sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["BDCustomDecal"];

            this.verts = new Vector2[triangleMesh.vertices.Length];

            this.AddToContainer(sLeaser, rCam, null);

            this.meshDirty = true;
        }

        private void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) {
            for (int i = 0; i < 4; i++) {
                this.quad[i] = placedObject.pos + Data.handles[i];
            }

            int idealGridDiv = this.GetIdealGridDiv();

            if (idealGridDiv != this.gridDiv) {
                this.gridDiv = idealGridDiv;
                sLeaser.sprites[0].RemoveFromContainer();
                this.InitiateSprites(sLeaser, rCam);
            }

            Random.State state = Random.state;
            Random.InitState((int)(this.quad[0].x + this.quad[0].y + this.quad[1].x + this.quad[1].y + this.quad[2].x + this.quad[2].y + this.quad[3].x + this.quad[3].y));

            TriangleMesh mesh = (sLeaser.sprites[0] as TriangleMesh);
            TangentTriangleMeshData meshData = TangentTriangleMeshData.GetData(mesh);

            float gridSegments = (float)this.gridDiv;

            float overrideBlend = Data.useOverride ? ((float)(Data.blendModeOverride.blendMode + 1) / ((float)BlendMode.values.Count + 1)) : 0f;
            float shadeBlend = (float)Data.blendMode.blendMode / (float)BlendMode.values.Count;
            float lightBlend = (float)Data.blendModeLight.blendMode / (float)BlendMode.values.Count;

            float affects = (float)Data.PackAffects() / 64.0f;

            for (int i = 0; i <= this.gridDiv; i++) {
                for (int j = 0; j <= this.gridDiv; j++) {
                    int vertex = j * (this.gridDiv + 1) + i;

                    float x = i / gridSegments;
                    float y = j / gridSegments;

                    Vector2 a1 = Vector2.Lerp(this.quad[0], this.quad[1], y);
                    Vector2 b1 = Vector2.Lerp(this.quad[3], this.quad[2], y);
                    Vector2 a2 = Vector2.Lerp(this.quad[0], this.quad[3], x);
                    Vector2 b2 = Vector2.Lerp(this.quad[1], this.quad[2], x);

                    this.verts[vertex] = Custom.LineIntersection(a1, b1, a2, b2);

                    float r = Quadify(3, x, y);
                    float g = Quadify(4, x, y);
                    float b = Quadify(5, x, y);

                    float a = Quadify(0, x, y);
                    float s = Quadify(1, x, y);

                    float e = Quadify(2, x, y);

                    mesh.verticeColors[vertex] = new Color(r, g, b, overrideBlend);

                    meshData.tangents[vertex] = new Vector4(Data.fromDepth, Data.toDepth, e, a);
                    meshData.texCoord2[vertex] = new Vector2(shadeBlend, lightBlend);
                    meshData.texCoord3[vertex] = new Vector2(affects, s);
                }
            }

            Random.state = state;
        }

        private float Quadify(int v, float x, float y) {
            return DoRandom(Mathf.Lerp(Mathf.Lerp(Data.vertices[3, v], Data.vertices[2, v], x), Mathf.Lerp(Data.vertices[0, v], Data.vertices[1, v], x), y));
        }

        private float DoRandom(float v) {
            float v2 = Mathf.Pow(v, 1f + Mathf.Lerp(-0.5f, 0.5f, Random.value) * Data.noise);
            return Mathf.Lerp(v2, Random.value, Data.noise * Mathf.Pow(1f - 2f * Mathf.Abs(v2 - 0.5f), 2.5f));
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos) {
            if (this.meshDirty) {
                this.UpdateVerts(sLeaser, rCam);
                this.meshDirty = false;
            }

            if (this.elementDirty) {
                sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(Data.imageName);
                this.elementDirty = false;
            }

            for (int i = 0; i < this.verts.Length; i++) {
                (sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, this.verts[i] - camPos);
            }

            if (base.slatedForDeletetion || this.room != rCam.room) {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) {
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner) {
            sLeaser.sprites[0].RemoveFromContainer();
            rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
        }
    }
}
