using DevInterface;
using RWCustom;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BetterDecals2 {

    public class BetterDecalRep : QuadQuadObjectRep {
        public BetterDecal decal;

        private readonly BetterDecalControlPanel controlPanel;

        private readonly int lineSprite;

        public string[] decalFiles;
        public Vector2 savLastPos;

        public BetterDecalRepData Data {
            get {
                return (pObj.data as BetterDecalRepData);
            }
        }

        public BetterDecalRep(DevUI owner, string IDstring, DevInterface.DevUINode parentNode, PlacedObject pObj) : base(owner, IDstring, parentNode, pObj, "Better Decal") {
            this.controlPanel = new(owner, "Better_Decal_Panel", this, Data.panelPos);

            this.subNodes.Add(this.controlPanel);

            this.fSprites.Add(new FSprite("pixel", true));
            this.lineSprite = this.fSprites.Count - 1;

            owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);

            this.fSprites[this.lineSprite].anchorY = 0f;

            if (this.decal == null) {
                for (int i = 0; i < owner.room.updateList.Count; i++) {
                    if (owner.room.updateList[i] is BetterDecal && (owner.room.updateList[i] as BetterDecal).placedObject == pObj) {
                        this.decal = (owner.room.updateList[i] as BetterDecal);
                        break;
                    }
                }
                if (this.decal == null) {
                    this.decal = new BetterDecal(pObj);
                    owner.room.AddObject(this.decal);
                }
            }

            string[] array = AssetManager.ListDirectory("decals", false, false);

            List<string> list = new();

            for (int j = 0; j < array.Length; j++) {
                if (array[j].ToLowerInvariant().EndsWith(".png")) {
                    list.Add(Path.GetFileNameWithoutExtension(array[j]));
                }
            }

            this.decalFiles = list.ToArray();
        }

        public override void Update() {
            base.Update();

            if (this.pObj.pos != this.savLastPos) {
                this.savLastPos = this.pObj.pos;

                if (Input.GetKey("l") && Futile.atlasManager.GetAtlasWithName(Data.imageName) != null) {
                    float x = Futile.atlasManager.GetAtlasWithName(Data.imageName).textureSize.x;
                    float y = Futile.atlasManager.GetAtlasWithName(Data.imageName).textureSize.y;
                    Data.handles[0] = new Vector2(-10f, -10f);
                    Data.handles[1] = new Vector2(-10f, y);
                    Data.handles[2] = new Vector2(x, y);
                    Data.handles[3] = new Vector2(x, -10f);
                    this.MoveAllHandles();
                    return;
                }

                if (Input.GetKey("k")) {
                    for (int i = 0; i < 4; i++) {
                        Data.handles[i] *= 1.025f;
                    }
                    this.MoveAllHandles();
                    return;
                }

                if (Input.GetKey("j")) {
                    for (int j = 0; j < 4; j++) {
                        Data.handles[j] *= 0.975f;
                    }
                    this.MoveAllHandles();
                }
            }
        }

        private void MoveAllHandles() {

            for (int i = 0; i < 4; i++) {
                (this.subNodes[i] as Handle).pos = Data.handles[i];
            }

            this.decal.UpdateMesh();
            this.Refresh();
        }

        public override void Refresh() {
            base.Refresh();

            base.MoveSprite(this.lineSprite, this.absPos);

            this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
            this.fSprites[this.lineSprite].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);

            Data.panelPos = this.controlPanel.pos;
        }

        public class BetterDecalControlPanel : Panel, IDevUISignals {

            public CustomDecalRepresentation.SelectDecalPanel decalsSelectPanel;

            public string currentOpen = "";
            public bool isSideOpen = false;
            public List<DevUINode> sideNodes = new();

            public BetterDecalControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250, 205), "Better Decal") {
                this.subNodes.Add(new BetterDecalButton(owner, "Blend_Mode", this, new Vector2(5, 185), (parentNode as BetterDecalRep).Data.blendMode.ToString()));
                this.subNodes.Add(new BetterDecalButton(owner, "Blend_Mode_Light", this, new Vector2(5, 165), (parentNode as BetterDecalRep).Data.blendModeLight.ToString()));
                this.subNodes.Add(new BetterDecalButton(owner, "Blend_Mode_Override", this, new Vector2(125, 165), (parentNode as BetterDecalRep).Data.blendModeOverride.ToString()));
                this.subNodes.Add(new BetterDecalButton(owner, "Does_Blend_Mode_Override", this, new Vector2(125, 185), (parentNode as BetterDecalRep).Data.useOverride.ToString()));

                this.subNodes.Add(new BetterDecalButton(owner, "Open_Color", this, new Vector2(5, 145), "Color"));
                this.subNodes.Add(new BetterDecalButton(owner, "Open_Masking", this, new Vector2(125, 145), "Masking"));

                this.subNodes.Add(new BetterDecalButton(owner, "Open_Shade", this, new Vector2(5, 65), ""));
                this.subNodes.Add(new BetterDecalButton(owner, "Open_Intensity", this, new Vector2(5, 45), ""));
                this.subNodes.Add(new BetterDecalButton(owner, "Open_Erosion", this, new Vector2(5, 25), ""));

                this.subNodes.Add(new BetterDecalControlSlider(owner, "From_Depth_Slider", this, new Vector2(5, 125), "From Depth: "));
                this.subNodes.Add(new BetterDecalControlSlider(owner, "To_Depth_Slider", this, new Vector2(5, 105), "To Depth: "));

                this.subNodes.Add(new BetterDecalControlSlider(owner, "Noise_Slider", this, new Vector2(5, 85), "Noise: "));

                this.subNodes.Add(new BetterDecalControlSlider(owner, "Shade_Slider", this, new Vector2(5, 65), "Shadow Alpha: "));
                this.subNodes.Add(new BetterDecalControlSlider(owner, "Intensity_Slider", this, new Vector2(5, 45), "Light Alpha: "));

                this.subNodes.Add(new BetterDecalControlSlider(owner, "Erosion_Slider", this, new Vector2(5, 25), "Erosion: "));

                this.subNodes.Add(new Button(owner, "Select_Decal_Panel_Button", this, new Vector2(5, 5), 240, "Decal : " + (parentNode as BetterDecalRep).Data.imageName));

            }

            public void Signal(DevUISignalType type, DevUINode sender, string message) {
                BetterDecal decal = (this.parentNode as BetterDecalRep)?.decal;
                BetterDecalRepData data = (this.parentNode as BetterDecalRep).pObj.data as BetterDecalRepData;

                if (sender.IDstring == "Select_Decal_Panel_Button") {
                    if (this.decalsSelectPanel != null) {
                        this.subNodes.Remove(this.decalsSelectPanel);
                        this.decalsSelectPanel.ClearSprites();
                        this.decalsSelectPanel = null;
                        return;
                    }
                    this.decalsSelectPanel = new CustomDecalRepresentation.SelectDecalPanel(this.owner, this, new Vector2(200f, 15f) - this.absPos, (this.parentNode as BetterDecalRep).decalFiles);
                    this.subNodes.Add(this.decalsSelectPanel);
                    return;
                }

                if (sender.IDstring.StartsWith("Open_")) {

                    if (isSideOpen) {
                        foreach (DevUINode node in sideNodes) {
                            node.ClearSprites();
                            this.subNodes.Remove(node);
                        }

                        this.sideNodes.Clear();

                        if (currentOpen == sender.IDstring) {
                            this.size = new Vector2(250, 205);
                            isSideOpen = false;
                            currentOpen = "";
                            return;
                        }
                    }

                    this.size = new Vector2(500, 205);
                    isSideOpen = true;

                    if (sender.IDstring == "Open_Masking") {
                        this.size = new Vector2(370, 205);
                    }

                    switch (sender.IDstring) {
                        case "Open_Red":
                        case "Open_Green":
                        case "Open_Blue":
                        case "Open_Color":
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Red_Slider", this, new Vector2(255, 185), "Red Tint: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Green_Slider", this, new Vector2(255, 165), "Green Tint: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Blue_Slider", this, new Vector2(255, 145), "Blue Tint: "));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Open_Red", this, new Vector2(255, 185), ""));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Open_Green", this, new Vector2(255, 165), ""));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Open_Blue", this, new Vector2(255, 145), ""));

                            if (sender.IDstring != "Open_Color") {
                                string col = sender.IDstring.Substring(5);
                                this.sideNodes.Add(new BetterDecalControlSlider(owner, col + "_Slider_0", this, new Vector2(255, 125), col + " Tint 0: "));
                                this.sideNodes.Add(new BetterDecalControlSlider(owner, col + "_Slider_1", this, new Vector2(255, 105), col + " Tint 1: "));
                                this.sideNodes.Add(new BetterDecalControlSlider(owner, col + "_Slider_2", this, new Vector2(255, 85), col + " Tint 2: "));
                                this.sideNodes.Add(new BetterDecalControlSlider(owner, col + "_Slider_3", this, new Vector2(255, 65), col + " Tint 3: "));
                            }

                            currentOpen = "Open_Color";
                            break;
                        case "Open_Masking":
                            this.sideNodes.Add(new BetterDecalButton(owner, "Mask_Terrain", this, new Vector2(255, 185), "Terrain: " + data.affectTerrain.ToString()));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Mask_Color1", this, new Vector2(255, 165), "Color A: " + data.affectColor1.ToString()));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Mask_Color2", this, new Vector2(255, 145), "Color B: " + data.affectColor2.ToString()));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Mask_Decals", this, new Vector2(255, 125), "Baked Decals: " + data.affectDecals.ToString()));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Mask_Grime", this, new Vector2(255, 105), "Grime: " + data.affectGrime.ToString()));
                            this.sideNodes.Add(new BetterDecalButton(owner, "Mask_Hive", this, new Vector2(255, 85), "Hive: " + data.affectHives.ToString()));

                            currentOpen = "Open_Masking";
                            break;
                        case "Open_Shade":
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Shade_Slider_0", this, new Vector2(255, 185), "Shade Alpha 0: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Shade_Slider_1", this, new Vector2(255, 165), "Shade Alpha 1: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Shade_Slider_2", this, new Vector2(255, 145), "Shade Alpha 2: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Shade_Slider_3", this, new Vector2(255, 125), "Shade Alpha 3: "));

                            currentOpen = "Open_Shade";
                            break;
                        case "Open_Intensity":
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Intensity_Slider_0", this, new Vector2(255, 185), "Light Alpha 0: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Intensity_Slider_1", this, new Vector2(255, 165), "Light Alpha 1: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Intensity_Slider_2", this, new Vector2(255, 145), "Light Alpha 2: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Intensity_Slider_3", this, new Vector2(255, 125), "Light Alpha 3: "));

                            currentOpen = "Open_Intensity";
                            break;
                        case "Open_Erosion":
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Erosion_Slider_0", this, new Vector2(255, 185), "Erosion 0: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Erosion_Slider_1", this, new Vector2(255, 165), "Erosion 1: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Erosion_Slider_2", this, new Vector2(255, 145), "Erosion 2: "));
                            this.sideNodes.Add(new BetterDecalControlSlider(owner, "Erosion_Slider_3", this, new Vector2(255, 125), "Erosion 3: "));

                            currentOpen = "Open_Erosion";
                            break;
                    }

                    foreach (DevUINode node in sideNodes) {
                        this.subNodes.Add(node);
                    }

                    return;
                }

                if (sender.IDstring == "Blend_Mode") {
                    if ((int)data.blendMode >= ExtEnum<BlendMode>.values.Count - 1) {
                        data.blendMode = new BlendMode(ExtEnum<BlendMode>.values.GetEntry(0), false);
                    }
                    else {
                        data.blendMode = new BlendMode(ExtEnum<BlendMode>.values.GetEntry(data.blendMode.Index + 1), false);
                    }
                    (sender as Button).Text = data.blendMode.ToString();
                    decal.UpdateMesh();
                    return;
                }

                if (sender.IDstring == "Blend_Mode_Light") {
                    if ((int)data.blendModeLight >= ExtEnum<BlendMode>.values.Count - 1) {
                        data.blendModeLight = new BlendMode(ExtEnum<BlendMode>.values.GetEntry(0), false);
                    }
                    else {
                        data.blendModeLight = new BlendMode(ExtEnum<BlendMode>.values.GetEntry(data.blendModeLight.Index + 1), false);
                    }
                    (sender as Button).Text = data.blendModeLight.ToString();
                    decal.UpdateMesh();
                    return;
                }

                if (sender.IDstring == "Blend_Mode_Override") {
                    if ((int)data.blendModeOverride >= ExtEnum<BlendMode>.values.Count - 1) {
                        data.blendModeOverride = new BlendMode(ExtEnum<BlendMode>.values.GetEntry(0), false);
                    }
                    else {
                        data.blendModeOverride = new BlendMode(ExtEnum<BlendMode>.values.GetEntry(data.blendModeOverride.Index + 1), false);
                    }
                    (sender as Button).Text = data.blendModeOverride.ToString();
                    decal.UpdateMesh();
                    return;
                }

                if (sender.IDstring == "Does_Blend_Mode_Override") {
                    data.useOverride = !data.useOverride;
                    (sender as Button).Text = data.useOverride.ToString();
                    decal.UpdateMesh();
                    return;
                }

                if (sender.IDstring.StartsWith("Mask_")) {

                    switch (sender.IDstring) {
                        case "Mask_Terrain":
                            data.affectTerrain = !data.affectTerrain;
                            (sender as Button).Text = "Terrain: " + data.affectTerrain.ToString();
                            break;
                        case "Mask_Color1":
                            data.affectColor1 = !data.affectColor1;
                            (sender as Button).Text = "Color A: " + data.affectColor1.ToString();
                            break;
                        case "Mask_Color2":
                            data.affectColor2 = !data.affectColor2;
                            (sender as Button).Text = "Color B: " + data.affectColor2.ToString();
                            break;
                        case "Mask_Decals":
                            data.affectDecals = !data.affectDecals;
                            (sender as Button).Text = "Baked Decals: " + data.affectDecals.ToString();
                            break;
                        case "Mask_Grime":
                            data.affectGrime = !data.affectGrime;
                            (sender as Button).Text = "Grime: " + data.affectGrime.ToString();
                            break;
                        case "Mask_Hive":
                            data.affectHives = !data.affectHives;
                            (sender as Button).Text = "Hive: " + data.affectHives.ToString();
                            break;
                    }

                    decal.UpdateMesh();
                    return;
                }

                if (sender.IDstring == "BackPage99289..?/~") {
                    this.decalsSelectPanel.PrevPage();
                    return;
                }

                if (sender.IDstring == "NextPage99289..?/~") {
                    this.decalsSelectPanel.NextPage();
                    return;
                }

                data.imageName = sender.IDstring;
                decal.UpdateAsset();

                for (int i = 0; i < this.subNodes.Count; i++) {
                    if (this.subNodes[i].IDstring == "Select_Decal_Panel_Button") {
                        (this.subNodes[i] as Button).Text = "Decal : " + data.imageName;
                    }
                }

                if (this.decalsSelectPanel != null) {
                    this.subNodes.Remove(this.decalsSelectPanel);
                    this.decalsSelectPanel.ClearSprites();
                    this.decalsSelectPanel = null;
                }
            }

            public class BetterDecalButton : Button {
                public BetterDecalButton(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string text) : base(owner, IDstring, parentNode, pos, 110, text) {
                }

                public override void Clicked() {
                    base.Clicked();
                    (this.parentNode.parentNode as BetterDecalRep)?.decal.UpdateMesh();
                }

            }

            public class BetterDecalControlSlider : Slider {

                public BetterDecalControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f) {
                }

                public override void Refresh() {
                    base.Refresh();

                    float num = 0f;

                    string idstring = this.IDstring;
                    if (idstring != null) {
                        var decalData = ((this.parentNode.parentNode as BetterDecalRep)?.Data);

                        if (decalData != null) {
                            bool isShadeAlpha = idstring.Contains("Shade");
                            bool isErosion = idstring.Contains("Erosion");
                            bool isRed = idstring.Contains("Red");
                            bool isGreen = idstring.Contains("Green");
                            bool isBlue = idstring.Contains("Blue");

                            int element = isErosion ? 2 : isRed ? 3 : isGreen ? 4 : isBlue ? 5 : isShadeAlpha ? 1 : 0;

                            switch (idstring) {
                                case "From_Depth_Slider":
                                    num = decalData.fromDepth;
                                    base.NumberText = ((int)(num * 30f)).ToString();
                                    break;
                                case "To_Depth_Slider":
                                    num = decalData.toDepth;
                                    base.NumberText = ((int)(num * 30f)).ToString();
                                    break;
                                case "Noise_Slider":
                                    num = decalData.noise;
                                    base.NumberText = ((int)(num * 100f)).ToString() + "%";
                                    break;
                                case "Shade_Slider_0":
                                case "Shade_Slider_1":
                                case "Shade_Slider_2":
                                case "Shade_Slider_3":
                                case "Intensity_Slider_0":
                                case "Intensity_Slider_1":
                                case "Intensity_Slider_2":
                                case "Intensity_Slider_3":
                                case "Erosion_Slider_0":
                                case "Erosion_Slider_1":
                                case "Erosion_Slider_2":
                                case "Erosion_Slider_3":
                                case "Red_Slider_0":
                                case "Red_Slider_1":
                                case "Red_Slider_2":
                                case "Red_Slider_3":
                                case "Green_Slider_0":
                                case "Green_Slider_1":
                                case "Green_Slider_2":
                                case "Green_Slider_3":
                                case "Blue_Slider_0":
                                case "Blue_Slider_1":
                                case "Blue_Slider_2":
                                case "Blue_Slider_3":
                                    int vertex = int.Parse(idstring.Substring(idstring.Length - 1));

                                    num = decalData.vertices[vertex, element];

                                    base.NumberText = ((int)(num * 100f)).ToString() + "%";
                                    break;
                                case "Erosion_Slider":
                                case "Shade_Slider":
                                case "Intensity_Slider":
                                case "Red_Slider":
                                case "Green_Slider":
                                case "Blue_Slider":
                                    num = decalData.vertices[0, element];

                                    bool isSameValue = true;

                                    for (int i = 1; i < 4; i++) {
                                        if (num != decalData.vertices[i, element]) {
                                            isSameValue = false;
                                            break;
                                        }
                                    }
                                    base.NumberText = isSameValue ? ((int)(num * 100f)).ToString() + "%" : "N/A";
                                    break;
                            }
                        }
                    }

                    base.RefreshNubPos(num);
                }

                public override void NubDragged(float nubPos) {
                    string idstring = this.IDstring;
                    var decalData = (this.parentNode.parentNode as BetterDecalRep)?.Data;

                    if (decalData != null && idstring != null) {

                        bool isShadeAlpha = idstring.Contains("Shade");
                        bool isErosion = idstring.Contains("Erosion");
                        bool isRed = idstring.Contains("Red");
                        bool isGreen = idstring.Contains("Green");
                        bool isBlue = idstring.Contains("Blue");

                        int element = isErosion ? 2 : isRed ? 3 : isGreen ? 4 : isBlue ? 5 : isShadeAlpha ? 1 : 0;

                        switch (idstring) {
                            case "From_Depth_Slider":
                                decalData.fromDepth = Mathf.Min(nubPos, decalData.toDepth);
                                break;
                            case "To_Depth_Slider":
                                decalData.toDepth = Mathf.Max(nubPos, decalData.fromDepth);
                                break;
                            case "Noise_Slider":
                                decalData.noise = nubPos;
                                break;
                            case "Shade_Slider_0":
                            case "Shade_Slider_1":
                            case "Shade_Slider_2":
                            case "Shade_Slider_3":
                            case "Intensity_Slider_0":
                            case "Intensity_Slider_1":
                            case "Intensity_Slider_2":
                            case "Intensity_Slider_3":
                            case "Erosion_Slider_0":
                            case "Erosion_Slider_1":
                            case "Erosion_Slider_2":
                            case "Erosion_Slider_3":
                            case "Red_Slider_0":
                            case "Red_Slider_1":
                            case "Red_Slider_2":
                            case "Red_Slider_3":
                            case "Green_Slider_0":
                            case "Green_Slider_1":
                            case "Green_Slider_2":
                            case "Green_Slider_3":
                            case "Blue_Slider_0":
                            case "Blue_Slider_1":
                            case "Blue_Slider_2":
                            case "Blue_Slider_3":
                                int vertex = int.Parse(idstring.Substring(idstring.Length - 1));
                                decalData.vertices[vertex, element] = nubPos;
                                break;
                            case "Erosion_Slider":
                            case "Shade_Slider":
                            case "Intensity_Slider":
                            case "Red_Slider":
                            case "Green_Slider":
                            case "Blue_Slider":
                                for (int i = 0; i < 4; i++) {
                                    decalData.vertices[i, element] = nubPos;
                                }
                                break;
                        }

                        this.parentNode.parentNode.Refresh();

                        (this.parentNode.parentNode as BetterDecalRep)?.decal.UpdateMesh();

                        this.Refresh();
                    }
                }
            }

        }
    }
}
