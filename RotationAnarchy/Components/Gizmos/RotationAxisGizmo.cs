namespace RotationAnarchy
{
    using RotationAnarchy.Internal;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class RotationAxisGizmo : GizmoBase
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

        private List<Vector3> verts = new List<Vector3>();
        private List<int> tris = new List<int>();

        public RotationAxisGizmo(string cmdBufferID, GizmoAxis axis) : base(cmdBufferID, axis) { }

        public override void SnapToActiveBuilder()
        {
            base.SnapToActiveBuilder();

            // first we need total bounds of the object
            var buildable = RA.Controller.ActiveBuilder.getBuiltObject();
            if(RA.Controller.ActiveGhost)
            {
                var ghost = RA.Controller.ActiveGhost;

                // we are going to do a funny hack here
                var originalRotation = ghost.transform.rotation;
                ghost.transform.rotation = Quaternion.identity;
                var bounds = GameObjectUtil.ComputeTotalBounds(ghost);
                ghost.transform.rotation = originalRotation;

                TubeRadius = ComputeGizmoWidth(ghost.transform.position);
                // now we need to update gizmo dimensions to the size of the object
                float xWidth = bounds.max.x - bounds.min.x;
                float zWidth = bounds.max.z - bounds.min.z;
                float width = Mathf.Max(xWidth, zWidth) + (TubeRadius * 2 + 0.5f);
                Radius = width / 2f;
                UpdateMesh();
            }
        }

        public override void UpdateMesh()
        {
            base.UpdateMesh();

            verts.Clear();
            tris.Clear();

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
            mesh.SetVertices(verts);

            int trisCount = _tubeSegments * _radialSegments * 6;
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