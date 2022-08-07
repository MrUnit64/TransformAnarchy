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

        // Mesh data lists
        internal List<Vector3> verts = new List<Vector3>();
        internal List<Vector3> normals = new List<Vector3>();
        internal List<int> triangles = new List<int>();
        internal List<Vector2> uvs = new List<Vector2>();

        // Mesh
        public Mesh mesh;

        public bool recalculateNormals = true;
        public bool recalculateBounds = true;

        // Constructor
        public ProceduralMesh()
        {

            // Performs better on runtime
            mesh.MarkDynamic();

            // Update the mesh
            UpdateMesh();

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
