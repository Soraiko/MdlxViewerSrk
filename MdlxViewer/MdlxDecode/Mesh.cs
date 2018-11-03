using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BAR_Editor;

namespace MdlxViewer
{
    public class Mesh
    {
        public Object3D Objet;

        public List<Vector3> Vertices;
        public List<Vector2> TextureCoordinates;
        public List<byte[]> VertexColor;
        public List<int[]> Triangle;
        public List<List<int>> InfluencesIndices;
        public List<List<float>> Influences;
        
        public List<int> DMA_Pointers;

        public SrkBinary Binary { get; set; }
        
        public Mesh(byte[] bytes)
        {
            this.Vertices = new List<Vector3>(0);
            this.TextureCoordinates = new List<Vector2>(0);
            this.VertexColor = new List<byte[]>(0);
            this.Triangle = new List<int[]>(0);
            this.InfluencesIndices = new List<List<int>>(0);
            this.Influences = new List<List<float>>(0);
            this.DMA_Pointers = new List<int>(0);
        }

        Vector3 GetTriangleVertex(int vertIndex)
        {
            if (vertIndex < this.Triangle.Count)
            {
                int index = this.Triangle[vertIndex][0];
                if (index < this.Vertices.Count)
                {
                    return this.Vertices[index];
                }
            }

            return this.Vertices[0];
        }

        List<float> GetInfluences(int vertIndex)
        {
            if (vertIndex < this.Triangle.Count)
            {
                int index = this.Triangle[vertIndex][0];
                if (index < this.Influences.Count)
                {
                    return this.Influences[index];
                }
            }
            
            return this.Influences[0];
        }

        List<int> GetMatrices(int vertIndex)
        {
            if (vertIndex < this.Triangle.Count)
            {
                int index = this.Triangle[vertIndex][0];
                if (index < this.InfluencesIndices.Count)
                {
                    return this.InfluencesIndices[index];
                }
            }
            
            return this.InfluencesIndices[0];
        }

        Vector2 GetTriangleUv(int uvIndex)
        {
            if (uvIndex < this.Triangle.Count)
            {
                int index = this.Triangle[uvIndex][1];
                if (index < this.TextureCoordinates.Count)
                {
                    return this.TextureCoordinates[index];
                }
            }

            return this.TextureCoordinates[0];
        }
        

        Microsoft.Xna.Framework.Color GetTriangleColor(int vertIndex)
        {
            if (vertIndex < this.Triangle.Count && this.Triangle[vertIndex].Length == 3)
            {
                int index = this.Triangle[vertIndex][2];
                if (index < this.VertexColor.Count)
                {
                    return new Microsoft.Xna.Framework.Color(this.VertexColor[index][0], this.VertexColor[index][1], this.VertexColor[index][2], this.VertexColor[index][3]);
                }
            }
            
            return Microsoft.Xna.Framework.Color.White;
        }

        public void Draw(GraphicsDeviceManager gcm)
        {
            if (this.RenderBuffer.Length==0)
                return;
            int ind = this.Triangle.Count - 1;

            for (int triInd = 0; triInd < this.Triangle.Count; triInd += 3)
            {
                for (int k = 0; k < 3; k++)
                {
                    VertexPositionColorTexture vpt = new VertexPositionColorTexture();
                    vpt.Position = GetTriangleVertex(triInd + k);
                    var infsV = GetInfluences(triInd + k);
                    var infs = GetMatrices(triInd + k);

                    Vector3 final = Vector3.Zero;

                    for (int l = 0; l < infs.Count; l++)
                    {
                        Matrix mat = this.Objet.Skeleton.BonesMatrices[infs[l]];
                        float inf = infsV[l];
                        Vector3 v3 = Vector3.Transform(vpt.Position, Matrix.Invert(mat));

                        v3 = Vector3.Transform(v3, this.Objet.Skeleton.BonesReMatrices[infs[l]]);
                        final.X += v3.X * inf;
                        final.Y += v3.Y * inf;
                        final.Z += v3.Z * inf;
                    }
                    vpt.Position = final;

                    vpt.TextureCoordinate = GetTriangleUv(triInd + k);
                    vpt.Color = GetTriangleColor(triInd + k);

                    //vpt.Position += this.Position;
                    this.RenderBuffer[ind] = vpt;
                    ind--;
                }
            }
            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.RenderBuffer, 0, this.Triangle.Count / 3);
        }

        public VertexPositionColorTexture[] RenderBuffer;

        public void GetData()
        {
            this.Vertices.Clear();
            this.TextureCoordinates.Clear();
            this.VertexColor.Clear();
            this.Triangle.Clear();
            this.InfluencesIndices.Clear();
            this.Influences.Clear();
            List<int[]> IndexBuffer = new List<int[]>(0);

            for (int i=0;i< this.DMA_Pointers.Count;i++)
            {
                int currVifOffset = this.Binary.ReadInt(this.DMA_Pointers[i] + 4, true);

                currVifOffset += 8;      /* skip 01 01 00 01     00 80 04 6C */

                int type = this.Binary.ReadInt(currVifOffset, true);

                int uvIndFlag_count = this.Binary.ReadInt(currVifOffset + 0x10, true);
                int uvIndFlag_offset = this.Binary.ReadInt(currVifOffset + 0x14, true);
                int matCountPerVert_offset = this.Binary.ReadInt(currVifOffset + 0x18, true);
                int dmaMatrices_offset = this.Binary.ReadInt(currVifOffset + 0x1C, true);

                int vertColor_count = this.Binary.ReadInt(currVifOffset + 0x20, true);
                int vertColor_offset = this.Binary.ReadInt(currVifOffset + 0x24, true);
                int spec_count = this.Binary.ReadInt(currVifOffset + 0x28, true);
                int spec_offset = this.Binary.ReadInt(currVifOffset + 0x2C, true);

                int verts_count = this.Binary.ReadInt(currVifOffset + 0x30, true);
                int verts_offset = this.Binary.ReadInt(currVifOffset + 0x34, true);
                int matCountPerVert_count = this.Binary.ReadInt(currVifOffset + 0x3C, true);
                
                int nextVifOffset = 0;

                if (i == this.DMA_Pointers.Count-1)
                {
                    nextVifOffset = this.DMA_Pointers[0];
                }
                else
                {
                    nextVifOffset = this.Binary.ReadInt(this.DMA_Pointers[i+1] + 4, true);
                }
                
                int uv_ind_flag = 0;

                if (type == 2)
                {
                    verts_count = vertColor_count;
                    vertColor_count = 0;
                    verts_offset = vertColor_offset;
                    vertColor_offset = 0;
                    matCountPerVert_count = spec_offset;
                    spec_offset = 0;
                    uv_ind_flag = 1;
                }

                byte[][] colors = new byte[vertColor_count][];
                Vector4[] vertices = new Vector4[verts_count];
                Vector2[] uvs = new Vector2[type == 1 ? uvIndFlag_count : 0];
                byte[] indices = new byte[uvIndFlag_count];
                byte[] flags = new byte[uvIndFlag_count];
                int[] matrices = new int[matCountPerVert_count];

                for (int it=0;it< matrices.Length; it++)
                {
                    matrices[it] = this.Binary.ReadInt(this.DMA_Pointers[i] + 0x14+ it * 0x10, true);
                }

                int[] matricesCountPerVertex = new int[verts_count];

                while (currVifOffset < nextVifOffset)
                {
                    currVifOffset += 4;
                    if (this.Binary.Buffer[this.Binary.Pointer + currVifOffset + 2] > 0) continue;
                    if (this.Binary.Buffer[this.Binary.Pointer + currVifOffset + 3] != 1) continue;
                    if (this.Binary.Buffer[this.Binary.Pointer + currVifOffset + 0] != 1) continue;
                    if (this.Binary.Buffer[this.Binary.Pointer + currVifOffset + 1] != 1) continue;
                    byte offset = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + 4];
                    currVifOffset += 8;

                    if (offset == uvIndFlag_offset)
                    {
                        switch (uv_ind_flag)
                        {
                            case 0: // UV 
                                for (int it=0;it< uvs.Length; it++)
                                {
                                    short u = this.Binary.ReadShort(currVifOffset + it * 4, true);
                                    short v = this.Binary.ReadShort(currVifOffset + it * 4 + 2, true);
                                    uvs[it] = new Vector2((u / 4095f), (v / 4095f));
                                }
                            break;
                            case 1: // ind
                                for (int it = 0; it < uvs.Length; it++)
                                {
                                    indices[it] = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + it];
                                }
                            break;
                            case 2:  // flag 
                                for (int it = 0; it < uvs.Length; it++)
                                {
                                    flags[it] = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + it];
                                }
                            break;
                        }
                        uv_ind_flag++;
                    }
                    if (offset == verts_offset)
                    {
                        int stride = spec_count > 0 ? 16 : 12;
                        for (int it = 0; it < vertices.Length; it++)
                        {
                            float x = this.Binary.ReadFloat(currVifOffset + it * stride, true);
                            float y = this.Binary.ReadFloat(currVifOffset + it * stride + 4, true);
                            float z = this.Binary.ReadFloat(currVifOffset + it * stride + 8, true);
                            float w = 1;
                            if (spec_count > 0)
                                w = this.Binary.ReadFloat(currVifOffset + it * stride + 12, true);

                            vertices[it] = new Vector4(x,y,z,w);
                        }
                    }
                    if (offset == vertColor_offset)
                    {
                        for (int it = 0; it < colors.Length; it++)
                        {
                            int r = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + it * 4]*2;
                            int g = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + it * 4 +1]*2;
                            int b = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + it * 4 +2]*2;
                            int a = this.Binary.Buffer[this.Binary.Pointer + currVifOffset + it * 4 +3]*2;
                            if (r > 255) r = 255;
                            if (g > 255) g = 255;
                            if (b > 255) b = 255;
                            if (a > 255) a = 255;
                            colors[it] = new byte[] {(byte)r, (byte)g, (byte)b, (byte)a };
                        }
                    }
                    if (offset == matCountPerVert_offset)
                    {
                        int totalIndex = 0; /* on décompresse les matCountPerVert */
                        for (int it = 0; it < matCountPerVert_count; it++)
                        {
                            int currCount = this.Binary.ReadInt(currVifOffset + it * 4, true);
                            for (int it2=0; it2< currCount; it2++)
                            {
                                matricesCountPerVertex[totalIndex] = matrices[it];
                                totalIndex++;
                            }
                        }
                        Vector3[] outputVertices;

                        for (int it = 0; it < vertices.Length; it++)
                        {
                            vertices[it] = Vector4.Transform(vertices[it], this.Objet.Skeleton.BonesMatrices[matricesCountPerVertex[it]]);
                        }

                        List<int>[] infIndices = new List<int>[0];
                        List<float>[] infs = new List<float>[0];

                        if (spec_count>0)
                        {
                            int alignByWhat = currVifOffset % 16;

                            currVifOffset += matCountPerVert_count * 4;

                            while (currVifOffset % 16 != alignByWhat)
                                currVifOffset++;

                            int[] specsCounts = new int[spec_count];
                            int outputVertCount = 0;
                            for (int it=0;it< specsCounts.Length; it++)
                            {
                                specsCounts[it] = this.Binary.ReadInt(currVifOffset, true);
                                outputVertCount += specsCounts[it];
                                currVifOffset += 4;
                            }
                            outputVertices = new Vector3[outputVertCount];
                            infIndices = new List<int>[outputVertCount];
                            infs = new List<float>[outputVertCount];

                            while (currVifOffset % 16 != alignByWhat)
                                currVifOffset++;
                            
                            totalIndex = 0;
                            for (int it = 0; it < specsCounts.Length; it++)
                            {
                                for (int it2 = 0; it2 < specsCounts[it]; it2++)
                                {
                                    infIndices[totalIndex] = new List<int>(0);
                                    infs[totalIndex] = new List<float>(0);
                                    outputVertices[totalIndex] = Vector3.Zero;
                                    for (int it3 = 0; it3 < (it+1); it3++)
                                    {
                                        int currInfIndex = this.Binary.ReadInt(currVifOffset, true);
                                        infIndices[totalIndex].Add(matricesCountPerVertex[currInfIndex]);
                                        infs[totalIndex].Add(vertices[currInfIndex].W);
                                        outputVertices[totalIndex] += new Vector3(vertices[currInfIndex].X, vertices[currInfIndex].Y, vertices[currInfIndex].Z);
                                        currVifOffset += 4;
                                    }
                                    totalIndex++;
                                }
                                while (currVifOffset % 16 != alignByWhat)
                                    currVifOffset++;
                            }
                        }
                        else
                        {
                            outputVertices = new Vector3[vertices.Length];
                            infIndices = new List<int>[vertices.Length];
                            infs = new List<float>[vertices.Length];

                            for (int it = 0; it < vertices.Length; it++)
                            {
                                infIndices[it] = new List<int>(0);
                                infIndices[it].Add(matricesCountPerVertex[it]);

                                infs[it] = new List<float>(0);
                                infs[it].Add(1);
                                
                                outputVertices[it] = new Vector3(vertices[it].X, vertices[it].Y, vertices[it].Z);
                            }
                        }
                        int globalOffsetVert = this.Vertices.Count;
                        int globalOffsetUV = this.TextureCoordinates.Count;
                        int globalOffsetColor = this.VertexColor.Count;

                        for (int it = 0; it < outputVertices.Length; it++)
                        {
                            if (outputVertices[it].X<3000 && outputVertices[it].X > this.Objet.MaxCoords.X)
                                this.Objet.MaxCoords.X = outputVertices[it].X;
                            if (outputVertices[it].X > -3000 && outputVertices[it].X < this.Objet.MinCoords.X)
                                this.Objet.MinCoords.X = outputVertices[it].X;

                            if (outputVertices[it].Y < 3000 && outputVertices[it].Y > this.Objet.MaxCoords.Y)
                                this.Objet.MaxCoords.Y = outputVertices[it].Y;
                            if (outputVertices[it].Y > -3000 && outputVertices[it].Y < this.Objet.MinCoords.Y)
                                this.Objet.MinCoords.Y = outputVertices[it].Y;

                            if (outputVertices[it].Z < 3000 && outputVertices[it].Z > this.Objet.MaxCoords.Z)
                                this.Objet.MaxCoords.Z = outputVertices[it].Z;
                            if (outputVertices[it].Z > -3000 && outputVertices[it].Z < this.Objet.MinCoords.Z)
                                this.Objet.MinCoords.Z = outputVertices[it].Z;

                            this.Vertices.Add(outputVertices[it]);
                            this.Influences.Add(infs[it]);
                            this.InfluencesIndices.Add(infIndices[it]);
                        }


                        for (int it = 0; it < uvs.Length; it++)
                        {
                            this.TextureCoordinates.Add(uvs[it]);
                        }
                        /*if (colors.Length==0)
                        {
                            colors = new byte[uvs.Length][];
                            for (int it = 0; it < colors.Length; it++)
                            {
                                colors[it] = new byte[] { 255, 255, 255, 255 };
                            }
                        }*/
                        for (int it = 0; it < colors.Length; it++)
                        {
                            this.VertexColor.Add(colors[it]);
                        }
                        // globalOffsetVert
                        // globalOffsetUV
                        // globalOffsetColor

                        for (int it=0;it<indices.Length;it++)
                        {
                            int index = indices[it];
                            byte flag = flags[it];

                            int[] currTriangle = new int[colors.Length > 0 ? 3 : 2];
                            currTriangle[0] = globalOffsetVert + index;
                            currTriangle[1] = globalOffsetUV + it;

                            if (colors.Length > 0)
                                currTriangle[2] = globalOffsetColor + it;
                            
                            if (IndexBuffer.Count<4)
                            {
                                IndexBuffer.Add(currTriangle);
                            }
                            else
                            {
                                IndexBuffer[0] = IndexBuffer[1];
                                IndexBuffer[1] = IndexBuffer[2];
                                IndexBuffer[2] = IndexBuffer[3];
                                IndexBuffer[3] = currTriangle;
                            }
                            if (IndexBuffer.Count>2)
                            {
                                if (flag == 0 || flag == 0x20)
                                {
                                    this.Triangle.Add(IndexBuffer[IndexBuffer.Count - 3]);
                                    this.Triangle.Add(IndexBuffer[IndexBuffer.Count - 2]);
                                    this.Triangle.Add(IndexBuffer[IndexBuffer.Count - 1]);
                                }
                                if (flag == 0 || flag == 0x30)
                                {
                                    this.Triangle.Add(IndexBuffer[IndexBuffer.Count - 1]);
                                    this.Triangle.Add(IndexBuffer[IndexBuffer.Count - 2]);
                                    this.Triangle.Add(IndexBuffer[IndexBuffer.Count - 3]);
                                }
                            }
                        }
                        infIndices = null;
                        infs = null;
                        outputVertices = null;
                    }
                }
                colors = null;
                indices = null;
                vertices = null;
                uvs = null;
                flags = null;
                matrices = null;
            }
            this.RenderBuffer = new VertexPositionColorTexture[this.Triangle.Count];
            for (int i = 0; i < this.RenderBuffer.Length; i++)
            {
                this.RenderBuffer[i] = new VertexPositionColorTexture();
            }
        }
    }
}
