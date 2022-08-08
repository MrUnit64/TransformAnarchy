using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RotationAnarchy.Internal.Utils.Meshes
{
    public class TorusMesh : ProceduralMesh
    {
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
            }
        }

        public float TubeRadius
        {
            get => _tubeRadius;
            set
            {
                _tubeRadius = value;
            }
        }

        public int RadialSegments
        {
            get => _radialSegments;
            set
            {
                _radialSegments = value;
            }
        }

        public int TubeSegments
        {
            get => _tubeSegments;
            set
            {
                _tubeSegments = value;
            }
        }

        private float _radius = 10f;
        private float _tubeRadius = 1f;
        private int _radialSegments = 32;
        private int _tubeSegments = 8;

        protected override void OnUpdateMesh()
        {
            int vertCount = _tubeSegments * _radialSegments * 4;
            for (int i = 0; i < vertCount; i++)
            {
                verts.Add(new Vector3());
            }

            float uStep = (2f * Mathf.PI) / _radialSegments;
            CreateFirstQuadRing(uStep);
            int iDelta = _tubeSegments * 4;
            for (int u = 2, i = iDelta; u <= _radialSegments; u++, i += iDelta)
            {
                CreateQuadRing(u * uStep, i);
            }

            int trisCount = _tubeSegments * _radialSegments * 6;
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

            mesh.SetVertices(verts);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

        }

        private Vector3 GetPoint(float u, float v)
        {
            Vector3 point;
            float r = (_radius + _tubeRadius * Mathf.Cos(v));
            point.x = r * Mathf.Sin(u);
            point.y = r * Mathf.Cos(u);
            point.z = _tubeRadius * Mathf.Sin(v);
            return point;
        }

        private void CreateFirstQuadRing(float u)
        {
            float step = (2f * Mathf.PI) / _tubeSegments;

            Vector3 vertexA = GetPoint(0f, 0f);
            Vector3 vertexB = GetPoint(u, 0f);
            for (int v = 1, i = 0; v <= _tubeSegments; v++, i += 4)
            {
                verts[i] = vertexA;
                verts[i + 1] = vertexA = GetPoint(0f, v * step);
                verts[i + 2] = vertexB;
                verts[i + 3] = vertexB = GetPoint(u, v * step);
            }
        }

        private void CreateQuadRing(float u, int i)
        {
            float step = (2f * Mathf.PI) / _tubeSegments;
            int ringOffset = _tubeSegments * 4;

            Vector3 vertex = GetPoint(u, 0f);
            for (int v = 1; v <= _tubeSegments; v++, i += 4)
            {
                verts[i] = verts[i - ringOffset + 2];
                verts[i + 1] = verts[i - ringOffset + 3];
                verts[i + 2] = vertex;
                verts[i + 3] = vertex = GetPoint(u, v * step);
            }
        }
    }
}
