using BepInEx;
using UnityEngine;
using DevInterface;
using BDLogs;

namespace BetterDecals2 {

    [BepInPlugin(MOD_ID, "betterdecals", "1.0.0")]
    class Plugin : BaseUnityPlugin {
        private const string MOD_ID = "ludocrypt.betterdecals";

        static bool loaded = false;

        public static AssetBundle bdShadersBundle;
        private static Shader bdShader;

        public static readonly PlacedObject.Type BetterDecal = new("BetterDecal", true);

        public void OnEnable() {
            Logfix.__SwitchToBepinexLogger(Logger);
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;
            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;
            On.Room.Loaded += Room_Loaded;
            new ExtraVertexInfo().OnEnable();
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
            orig(self);
            if (!loaded) {
                loaded = true;
                bdShadersBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/betterdecals/bdshaders"));
                bdShader = bdShadersBundle.LoadAsset<Shader>("Assets/BDCustomDecal.shader");
                self.Shaders["BDCustomDecal"] = FShader.CreateShader("BDCustomDecal", bdShader);
            }
        }

        private void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self) {
            orig(self);
            if (self.type == BetterDecal) {
                self.data = new BetterDecalRepData(self);
            }
        }

        private void Room_Loaded(On.Room.orig_Loaded orig, Room self) {
            var firstTimeRealized = self.abstractRoom.firstTimeRealized;
            orig(self);
            if (self.game is null) return;
            try {
                self.abstractRoom.firstTimeRealized = firstTimeRealized;
                for (int i = 0; i < self.roomSettings.placedObjects.Count; i++) {
                    if (self.roomSettings.placedObjects[i].active) {
                        if (self.roomSettings.placedObjects[i].type == BetterDecal) {
                            self.AddObject(new BetterDecal(self.roomSettings.placedObjects[i]));
                        }
                    }
                }
            }
            finally {
                self.abstractRoom.firstTimeRealized = false;
            }
        }

        private void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj) {
            orig(self, tp, pObj);
            if (tp != BetterDecal) {
                return;
            }

            var isNew = pObj == null;
            if (isNew) pObj = self.RoomSettings.placedObjects[self.RoomSettings.placedObjects.Count - 1];
            PlacedObjectRepresentation placedObjectRepresentation = new BetterDecalRep(self.owner, tp.ToString() + "_Rep", self, pObj);
            PlacedObjectRepresentation old = (PlacedObjectRepresentation)self.tempNodes.Pop();
            self.subNodes.Pop();
            old.ClearSprites();
            self.tempNodes.Add(placedObjectRepresentation);
            self.subNodes.Add(placedObjectRepresentation);
        }
    }
}