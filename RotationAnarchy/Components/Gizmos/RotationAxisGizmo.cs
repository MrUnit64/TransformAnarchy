namespace RotationAnarchy
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class RotationAxisGizmo : GizmoBase
    {
        public float Radius { get; set; } = 10f;
        public float TubeRadius { get; set; } = 1f;

        public int radialSegments = 32;
        public int tubeSegments = 8;

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();

        protected override void Construct()
        {
            base.Construct();
        }

        public override void UpdateMesh()
        {
            base.UpdateMesh();

            verts.Clear();
            tris.Clear();

            int vertCount = tubeSegments * radialSegments * 4;
            for (int i = 0; i < vertCount; i++)
            {
                verts.Add(new Vector3());
            }

            float uStep = (2f * Mathf.PI) / radialSegments;
            CreateFirstQuadRing(uStep);
            int iDelta = tubeSegments * 4;
            for (int u = 2, i = iDelta; u <= radialSegments; u++, i += iDelta)
            {
                CreateQuadRing(u * uStep, i);
            }
            mesh.SetVertices(verts);

            int trisCount = tubeSegments * radialSegments * 6;
            for (int i = 0; i < trisCount; i++)
            {
                tris.Add(0);
            }

            for (int t = 0, i = 0; t < trisCount; t += 6, i += 4)
            {
                tris[t] = i;
                tris[t + 1] = tris[t + 4] = i + 1;
                tris[t + 2] = tris[t + 3] = i + 2;
                tris[t + 5] = i + 3;
            }
            mesh.SetTriangles(tris, 0);
        }


        private Vector3 GetPoint(float u, float v)
        {
            Vector3 point;
            float r = (Radius + TubeRadius * Mathf.Cos(v));
            point.x = r * Mathf.Sin(u);
            point.y = r * Mathf.Cos(u);
            point.z = TubeRadius * Mathf.Sin(v);
            return point;
        }

        private void CreateFirstQuadRing(float u)
        {
            float step = (2f * Mathf.PI) / tubeSegments;

            Vector3 vertexA = GetPoint(0f, 0f);
            Vector3 vertexB = GetPoint(u, 0f);
            for (int v = 1, i = 0; v <= tubeSegments; v++, i += 4)
            {
                verts[i] = vertexA;
                verts[i + 1] = vertexA = GetPoint(0f, v * step);
                verts[i + 2] = vertexB;
                verts[i + 3] = vertexB = GetPoint(u, v * step);
            }
        }

        private void CreateQuadRing(float u, int i)
        {
            float step = (2f * Mathf.PI) / tubeSegments;
            int ringOffset = tubeSegments * 4;

            Vector3 vertex = GetPoint(u, 0f);
            for (int v = 1; v <= tubeSegments; v++, i += 4)
            {
                verts[i] = verts[i - ringOffset + 2];
                verts[i + 1] = verts[i - ringOffset + 3];
                verts[i + 2] = vertex;
                verts[i + 3] = vertex = GetPoint(u, v * step);
            }
        }
    }
}