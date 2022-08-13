using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RotationAnarchy.Internal.Utils.Meshes
{

    [System.Serializable]
    public class TorusMesh : ProceduralMesh
    {
        [SerializeField]
        public bool Taper;
        [SerializeField]
        public float TaperEndRadius;
        [SerializeField]
        public float TaperStartRadius;
        [SerializeField]
        public float Arc = 360;
        [SerializeField]
        public bool ForwardZ = true;
        [SerializeField]
        public float Radius = 10f;
        [SerializeField]
        public float TubeRadius = 1f;
        [SerializeField]
        public int RadialSegments = 32;
        [SerializeField]
        public int TubeSegments = 8;

        protected override void OnUpdateMesh()
        {
            if (Radius == 0 || TubeRadius == 0 || Arc == 0)
                return;

            Arc = Mathf.Clamp(Arc, 0, 360f);
            RadialSegments = Mathf.Clamp(RadialSegments, 3, 64);
            TubeSegments = Mathf.Clamp(TubeSegments, 3, 64);
            TubeRadius = Mathf.Clamp(TubeRadius, 0, float.MaxValue);
            Radius = Mathf.Clamp(Radius, 0, float.MaxValue);
            TaperEndRadius = Mathf.Clamp(TaperEndRadius, 0, TubeRadius);
            TaperStartRadius = Mathf.Clamp(TaperStartRadius, 0, TubeRadius);

            bool capsNeeded = (Arc <= 359.999f && !Taper) || (Taper);

            int vertCount = TubeSegments * RadialSegments * 4;
            for (int i = 0; i < vertCount; i++)
            {
                verts.Add(new Vector3());
            }

            float uStep = (Mathf.Deg2Rad * Arc) / RadialSegments;
            CreateFirstQuadRing(uStep, ComputeTaper(1));
            int iDelta = TubeSegments * 4;

            for (int u = 2, i = iDelta; u <= RadialSegments; u++, i += iDelta)
            {
                CreateQuadRing(u * uStep, i, ComputeTaper(u));
            }

            int trisCount = TubeSegments * RadialSegments * 6;
            for (int i = 0; i < trisCount; i++)
            {
                triangles.Add(0);
            }

            for (int t = 0, i = 0; t < trisCount; t += 6, i += 4)
            {
                triangles[t] = i;
                triangles[t + 1] = triangles[t + 4] = i + 1;
                triangles[t + 2] = triangles[t + 3] = i + 2;
                triangles[t + 5] = i + 3;
            }

            if (capsNeeded)
            {
                int capsVertCount = TubeSegments + 1;
                int capTrisCount = TubeSegments * 3 + 3;

                for (int i = 0; i < capsVertCount; i++)
                    verts.Add(new Vector3());

                for (int i = 0; i < capTrisCount; i++)
                    triangles.Add(0);

                CreateCap(0, vertCount - 1, trisCount, true, ComputeTaper(0));

                if (!Taper || (Taper && TaperEndRadius > 0.001f))
                {
                    for (int i = 0; i < capsVertCount; i++)
                        verts.Add(new Vector3());

                    for (int i = 0; i < capTrisCount; i++)
                        triangles.Add(0);

                    CreateCap(RadialSegments * uStep, vertCount + capsVertCount - 1, trisCount + capTrisCount, false, ComputeTaper(RadialSegments));
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(triangles, 0);
        }

        private float ComputeTaper(int u)
        {
            float taper = 1f;
            if (Taper)
            {
                float taperFractionStart = TaperStartRadius / TubeRadius;
                float taperFractionEnd = TaperEndRadius / TubeRadius;
                float taperDistance = Mathf.InverseLerp(0, RadialSegments, u);
                taper = Mathf.Lerp(taperFractionStart, taperFractionEnd, taperDistance);
            }
            return taper;
        }

        private Vector3 GetPoint(float u, float v, float taper = 1f)
        {
            Vector3 point;
            float r = (Radius + TubeRadius * taper * Mathf.Cos(v));

            if (ForwardZ)
            {
                point.x = r * Mathf.Sin(u);
                point.y = r * Mathf.Cos(u);
                point.z = TubeRadius * Mathf.Sin(v) * taper;
            }
            else
            {
                point.x = r * Mathf.Cos(u);
                point.y = TubeRadius * Mathf.Sin(v) * taper;
                point.z = r * Mathf.Sin(u);
            }

            return point;
        }

        private void CreateFirstQuadRing(float u, float taper = 1f)
        {
            float step = (2f * Mathf.PI) / TubeSegments;

            Vector3 vertexA = GetPoint(0f, 0f, taper);
            Vector3 vertexB = GetPoint(u, 0f, taper);
            for (int v = 1, i = 0; v <= TubeSegments; v++, i += 4)
            {
                verts[i] = vertexA;
                verts[i + 1] = vertexA = GetPoint(0f, v * step, taper);
                verts[i + 2] = vertexB;
                verts[i + 3] = vertexB = GetPoint(u, v * step, taper);
            }
        }

        private void CreateCap(float u, int i, int t, bool reverse, float taper = 1f)
        {
            float step = (2f * Mathf.PI) / TubeSegments;

            for (int v = 1; v <= TubeSegments; v++)
            {
                verts[i + v] = GetPoint(u, v * step, taper);
            }

            int center = i + TubeSegments + 1;
            verts[center] = GetCenterPoint(u);

            int capTrisCount = t + TubeSegments * 3;
            for (int v = t, d = 1; v < capTrisCount; v += 3, d += 1)
            {
                triangles[v] = center;
                if (reverse)
                {
                    triangles[v + 1] = i + d + 1;
                    triangles[v + 2] = i + d;
                }
                else
                {
                    triangles[v + 1] = i + d;
                    triangles[v + 2] = i + d + 1;
                }
            }

            triangles[capTrisCount] = center;
            if (reverse)
            {
                triangles[capTrisCount + 1] = i + 1;
                triangles[capTrisCount + 2] = i + TubeSegments;
            }
            else
            {
                triangles[capTrisCount + 1] = i + TubeSegments;
                triangles[capTrisCount + 2] = i + 1;
            }
        }

        private Vector3 GetCenterPoint(float u)
        {
            Vector3 point;
            float r = Radius;

            if (ForwardZ)
            {
                point.x = r * Mathf.Sin(u);
                point.y = r * Mathf.Cos(u);
                point.z = 0;
            }
            else
            {
                point.x = r * Mathf.Cos(u);
                point.y = 0;
                point.z = r * Mathf.Sin(u);
            }

            return point;
        }

        private void CreateQuadRing(float u, int i, float taper)
        {
            float step = (2f * Mathf.PI) / TubeSegments;
            int ringOffset = TubeSegments * 4;

            Vector3 vertex = GetPoint(u, 0f, taper);
            for (int v = 1; v <= TubeSegments; v++, i += 4)
            {
                verts[i] = verts[i - ringOffset + 2];
                verts[i + 1] = verts[i - ringOffset + 3];
                verts[i + 2] = vertex;
                verts[i + 3] = vertex = GetPoint(u, v * step, taper);
            }
        }


    }
}
