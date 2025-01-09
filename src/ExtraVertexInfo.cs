using System;
using UnityEngine;

namespace BetterDecals2 {
    internal class ExtraVertexInfo {

        public void OnEnable() {
            On.TriangleMesh.PopulateRenderLayer += TriangleMesh_PopulateRenderLayer;
            On.FFacetRenderLayer.UpdateMeshProperties += FFacetRenderLayer_UpdateMeshProperties;
            On.FQuadRenderLayer.ShrinkMaxFacetLimit += FQuadRenderLayer_ShrinkMaxFacetLimit;
            On.FQuadRenderLayer.ExpandMaxFacetLimit += FQuadRenderLayer_ExpandMaxFacetLimit;
            On.FTriangleRenderLayer.ExpandMaxFacetLimit += FTriangleRenderLayer_ExpandMaxFacetLimit;
            On.FTriangleRenderLayer.ShrinkMaxFacetLimit += FTriangleRenderLayer_ShrinkMaxFacetLimit;
        }

        private void FTriangleRenderLayer_ShrinkMaxFacetLimit(On.FTriangleRenderLayer.orig_ShrinkMaxFacetLimit orig, FTriangleRenderLayer self, int deltaDecrease) {
            orig(self, deltaDecrease);
            FFacetRenderLayer_resizeLimits(self, deltaDecrease);
        }

        private void FTriangleRenderLayer_ExpandMaxFacetLimit(On.FTriangleRenderLayer.orig_ExpandMaxFacetLimit orig, FTriangleRenderLayer self, int deltaIncrease) {
            orig(self, deltaIncrease);
            FFacetRenderLayer_resizeLimits(self, deltaIncrease);
        }

        private void FQuadRenderLayer_ExpandMaxFacetLimit(On.FQuadRenderLayer.orig_ExpandMaxFacetLimit orig, FQuadRenderLayer self, int deltaIncrease) {
            orig(self, deltaIncrease);
            FFacetRenderLayer_resizeLimits(self, deltaIncrease);
        }

        private void FQuadRenderLayer_ShrinkMaxFacetLimit(On.FQuadRenderLayer.orig_ShrinkMaxFacetLimit orig, FQuadRenderLayer self, int deltaDecrease) {
            orig(self, deltaDecrease);
            FFacetRenderLayer_resizeLimits(self, deltaDecrease);
        }

        private void FFacetRenderLayer_resizeLimits<G>(G self, int delta) where G : FFacetRenderLayer {
            if (delta <= 0) {
                return;
            }

            RenderLayerTangentData data = RenderLayerTangentData.GetData(self);

            data.tangentsUpdated = true;

            int arrSize = FieldHelper.GetField<Vector2[]>(self, "_uvs").Length;

            Array.Resize<Vector4>(ref data.tangents, arrSize);
            Array.Resize<Vector2>(ref data.texCoord2, arrSize);
            Array.Resize<Vector2>(ref data.texCoord3, arrSize);
        }

        private void FFacetRenderLayer_UpdateMeshProperties(On.FFacetRenderLayer.orig_UpdateMeshProperties orig, FFacetRenderLayer self) {
            RenderLayerTangentData data = RenderLayerTangentData.GetData(self);

            if (data.tangentsUpdated == true || FieldHelper.GetField<bool>(self, "_didVertCountChange")) {
                Mesh mesh = FieldHelper.GetField<Mesh>(self, "_mesh");

                mesh.SetTangents(data.tangents);
                mesh.SetUVs(2, data.texCoord2);
                mesh.SetUVs(3, data.texCoord3);

                data.tangentsUpdated = false;
            }

            orig(self);
        }

        private void TriangleMesh_PopulateRenderLayer(On.TriangleMesh.orig_PopulateRenderLayer orig, TriangleMesh self) {
            TangentTriangleMeshData mesh = TangentTriangleMeshData.GetData(self);

            if (mesh.useTangents) {
                if (FieldHelper.GetField<bool>(self, "_isOnStage") && self.firstFacetIndex != -1) {
                    int num = self.firstFacetIndex * 3;
                    RenderLayerTangentData layer = RenderLayerTangentData.GetData(FieldHelper.GetField<FFacetRenderLayer>(self, "_renderLayer"));

                    Vector4[] tangents = layer.tangents;
                    Vector2[] tex2 = layer.texCoord2;
                    Vector2[] tex3 = layer.texCoord3;

                    if (mesh.tangents.Length > 0 && mesh.texCoord2.Length > 0 && mesh.texCoord3.Length > 0) {
                        for (int i = 0; i < self.triangles.Length; i++) {
                            for (int j = 0; j < 3; j++) {
                                int tid = self.triangles[i].GetAt(j);
                                tangents[num + i * 3 + j] = mesh.tangents[tid];
                                tex2[num + i * 3 + j] = mesh.texCoord2[tid];
                                tex3[num + i * 3 + j] = mesh.texCoord3[tid];
                            }
                        }
                    }

                    layer.tangentsUpdated = true;
                }
            }
            orig(self);
        }

        public class RenderLayerTangentData : ExtraDataClass<FFacetRenderLayer, RenderLayerTangentData> {

            public bool usetangents = false;
            public bool tangentsUpdated = false;
            public Vector4[] tangents = new Vector4[0];
            public Vector2[] texCoord2 = new Vector2[0];
            public Vector2[] texCoord3 = new Vector2[0];

        }

        public class TangentTriangleMeshData : ExtraDataClass<TriangleMesh, TangentTriangleMeshData> {

            public bool useTangents = false;
            public Vector4[] tangents = new Vector4[0];
            public Vector2[] texCoord2 = new Vector2[0];
            public Vector2[] texCoord3 = new Vector2[0];

        }

    }
}
