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

        }

        // Call to create mesh
        public void Generate()
        {

            // Clear lists
            verts.Clear();
            triangles.Clear();
            uvs.Clear();
            normals.Clear();

            // Call create mesh
            CreateMesh();

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

        // Override this to create your mesh
        internal abstract void CreateMesh();

        // Index buffer for triangles
        internal static int[] CreateIndexBuffer(int vertexCount, int indexCount, int slices)
        {
            int[] indices = new int[indexCount];
            int currentIndex = 0;

            // Bottom circle/cone of shape
            for (int i = 1; i <= slices; i++)
            {
                indices[currentIndex++] = i;
                indices[currentIndex++] = 0;
                if (i - 1 == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
            }

            // Middle sides of shape
            for (int i = 1; i < vertexCount - slices - 1; i++)
            {
                indices[currentIndex++] = i + slices;
                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;

                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices + slices - 1;
                else
                    indices[currentIndex++] = i + slices - 1;
            }

            // Top circle/cone of shape
            for (int i = vertexCount - slices - 1; i < vertexCount - 1; i++)
            {
                indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    indices[currentIndex++] = i + slices - 1;
                else
                    indices[currentIndex++] = i - 1;
                indices[currentIndex++] = vertexCount - 1;
            }

            return indices;
        }

    }
}
