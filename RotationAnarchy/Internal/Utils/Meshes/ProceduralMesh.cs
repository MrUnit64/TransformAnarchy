using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RotationAnarchy.Internal.Utils.Meshes
{
    public abstract class ProceduralMesh
    {
        public Mesh mesh { get; private set; }

        // Mesh data lists
        protected List<Vector3> verts = new List<Vector3>();
        protected List<Vector3> normals = new List<Vector3>();
        protected List<Vector2> uvs = new List<Vector2>();
        protected List<int> triangles = new List<int>();

        // Mesh

        public bool recalculateNormals = true;
        public bool recalculateBounds = true;

        // Constructor
        public ProceduralMesh()
        {
            mesh = new Mesh();
            // Performs better on runtime
            mesh.MarkDynamic();
        }

        // Call to update mesh
        public void UpdateMesh()
        {
            // Clear lists
            verts.Clear();
            triangles.Clear();
            uvs.Clear();
            normals.Clear();

            // Call create mesh
            OnUpdateMesh();

            // Recalc norms
            if (recalculateNormals)
            {
                mesh.RecalculateNormals();
            }

            // Recalculate bounds
            if (recalculateBounds)
            {
                mesh.RecalculateBounds();
            }
        }

        // Override this with the mesh generation code
        protected abstract void OnUpdateMesh();

    }
}
