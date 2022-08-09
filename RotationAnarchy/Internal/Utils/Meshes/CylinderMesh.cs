using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RotationAnarchy.Internal.Utils.Meshes
{
    public class CylinderMesh : ProceduralMesh
    {
        public float BottomRadius;
        public float TopRadius;
        public float Length;
        public int Slices;
        public int Stacks;

        /// <summary>
        /// Creates a <see cref="CylinderMesh"/>
        /// </summary>
        /// <remarks>
        /// The values are as follows:
        /// Vertex Count    = slices * (stacks + 1) + 2
        /// Primitive Count = slices * (stacks + 1) * 2
        /// </remarks>
        /// <param name="bottomRadius">Radius at the negative Y end. Value should be greater than or equal to 0.0f.</param>
        /// <param name="topRadius">Radius at the positive Y end. Value should be greater than or equal to 0.0f.</param>
        /// <param name="length">Length of the cylinder along the Y-axis.</param>
        /// <param name="slices">Number of slices about the Y axis.</param>
        /// <param name="stacks">Number of stacks along the Y axis.</param>
        public CylinderMesh(float bottomRadius = 1, float topRadius = 1, float length = 1, int slices = 12, int stacks = 1)
        {
            BottomRadius = bottomRadius;
            TopRadius = topRadius;
            Length = length;
            Slices = slices;
            Stacks = stacks;
        }

        protected override void OnUpdateMesh()
        {
            // if both the top and bottom have a radius of zero, just return null, because invalid
            if (BottomRadius <= 0 && TopRadius <= 0)
            {
                return;
            }

            mesh.name = "CylinderMesh";
            float sliceStep = (float)Math.PI * 2.0f / Slices;
            float heightStep = Length / Stacks;
            float radiusStep = (TopRadius - BottomRadius) / Stacks;
            float currentHeight = -Length / 2;
            int vertexCount = (Stacks + 1) * Slices + 2; //cone = stacks * slices + 1
            int triangleCount = (Stacks + 1) * Slices * 2; //cone = stacks * slices * 2 + slices
            int indexCount = triangleCount * 3;
            float currentRadius = BottomRadius;

            // Start at the bottom of the cylinder            
            int currentVertex = 0;
            verts.Add(new Vector3(0, currentHeight, 0));
            normals.Add(Vector3.down);
            currentVertex++;
            for (int i = 0; i <= Stacks; i++)
            {
                float sliceAngle = 0;
                for (int j = 0; j < Slices; j++)
                {
                    float x = currentRadius * (float)Math.Cos(sliceAngle);
                    float y = currentHeight;
                    float z = currentRadius * (float)Math.Sin(sliceAngle);

                    Vector3 position = new Vector3(x, y, z);
                    verts.Add(position);
                    normals.Add(Vector3.Normalize(position));
                    //uvs.Add(new Vector2((float)(Math.Sin(normals[currentVertex].x) / Math.PI + 0.5f),
                    //        (float)(Math.Sin(normals[currentVertex].y) / Math.PI + 0.5f)));

                    currentVertex++;

                    sliceAngle += sliceStep;
                }
                currentHeight += heightStep;
                currentRadius += radiusStep;
            }
            verts.Add(new Vector3(0, Length / 2, 0));
            normals.Add(Vector3.up);
            currentVertex++;

            UpdateIndexBuffer(vertexCount, indexCount, Slices);

            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            //mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);

        }

        // Index buffer for triangles
        internal void UpdateIndexBuffer(int vertexCount, int indexCount, int slices)
        {
            for (int i = 0; i < indexCount; i++)
            {
                triangles.Add(0);
            }
            
            int currentIndex = 0;

            // Bottom circle/cone of shape
            for (int i = 1; i <= slices; i++)
            {
                triangles[currentIndex++] = i;
                triangles[currentIndex++] = 0;
                if (i - 1 == 0)
                    triangles[currentIndex++] = i + slices - 1;
                else
                    triangles[currentIndex++] = i - 1;
            }

            // Middle sides of shape
            for (int i = 1; i < vertexCount - slices - 1; i++)
            {
                triangles[currentIndex++] = i + slices;
                triangles[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    triangles[currentIndex++] = i + slices + slices - 1;
                else
                    triangles[currentIndex++] = i + slices - 1;

                triangles[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    triangles[currentIndex++] = i + slices - 1;
                else
                    triangles[currentIndex++] = i - 1;
                if ((i - 1) % slices == 0)
                    triangles[currentIndex++] = i + slices + slices - 1;
                else
                    triangles[currentIndex++] = i + slices - 1;
            }

            // Top circle/cone of shape
            for (int i = vertexCount - slices - 1; i < vertexCount - 1; i++)
            {
                triangles[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    triangles[currentIndex++] = i + slices - 1;
                else
                    triangles[currentIndex++] = i - 1;
                triangles[currentIndex++] = vertexCount - 1;
            }
        }
    }
}
