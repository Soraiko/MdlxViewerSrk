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

namespace MdlxViewer
{
    class OBJ : Object3D
    {
        string[] document;

        public OBJ(string filename)
        {


            this.Skeleton = new Skeleton(this);
            
            this.Vertices = new List<Vector3>(0);
            this.Normal = new List<Vector3>(0);
            this.TextureCoordinates = new List<Vector2>(0);
            

            this.Triangle = new List<List<int[]>>(0);
            
            this.materialNames = new List<string>(0);
            this.meshNames = new List<string>(0);
            
            this.materialFileNames = new List<string>(0);
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.DirectoryName = Path.GetDirectoryName(filename);
            this.FileName = filename;
            this.document = File.ReadAllLines(filename);
        }

        Vector3 GetTriangleVertex(int meshIndex, int vertIndex)
        {
            Vector3 output = Vector3.Zero;
            if (meshIndex < this.Triangle.Count)
            {
                if (vertIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][vertIndex][0];
                    if (index < this.Vertices.Count)
                    {
                        output = this.Vertices[index];
                    }
                }
            }
            return output;
        }
        
        Vector2 GetTriangleUv(int meshIndex, int uvIndex)
        {
            Vector2 output = Vector2.Zero;
            if (meshIndex < this.Triangle.Count)
            {
                if (uvIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][uvIndex][1];
                    if (index < this.TextureCoordinates.Count)
                    {
                        output = this.TextureCoordinates[index];
                    }
                }
            }
            return output;
        }

        Vector3 GetTriangleNormal(int meshIndex, int normalIndex)
        {
            Vector3 output = Vector3.Zero;
            if (meshIndex < this.Triangle.Count)
            {
                if (normalIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][normalIndex][2];
                    if (index < this.Normal.Count)
                    {
                        output = this.Normal[index];
                    }
                }
            }
            return output;
        }

        public bool ApplyTransformations { get; set; }
        public VertexPositionColorTexture[][] RenderBuffer;

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            for (int mesh = 0; mesh < this.Triangle.Count; mesh++)
            {
                int ind = this.Triangle[mesh].Count-1;
                //Object3D.orderZindex
                for (int triInd = 0; triInd < this.Triangle[mesh].Count; triInd += 3)
                {
                    for (int k=0;k<3;k++)
                    {
                        VertexPositionColorTexture vpt = new VertexPositionColorTexture();
                        vpt.Position = GetTriangleVertex(mesh, triInd + k);
                        vpt.TextureCoordinate = GetTriangleUv(mesh, triInd + k);
                        vpt.Color = Microsoft.Xna.Framework.Color.White;

                        vpt.Position += this.Position;
                        RenderBuffer[mesh][ind] = vpt;
                        ind--;
                    }
                }
                if (be!=null)
                {
                    be.TextureEnabled = true;
                    be.Texture = SrkBinary.GetT2D(this.materialFileNames[this.MaterialIndices[mesh]]);
                    be.CurrentTechnique.Passes[0].Apply();
                }
                if (rs!=null)
                {
                    gcm.GraphicsDevice.RasterizerState = rs;
                }
                if (RenderBuffer.Length>0)
                {
                    gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, RenderBuffer[mesh], 0, RenderBuffer[mesh].Length / 3);
                }
            }
            //this.Skeleton.GetGraphic();
            //this.Skeleton.Draw(gcm, be, rs);
        }

        public string FileName = "";
        public string Name = "";
        public string DirectoryName = "";
        
        public List<Vector3> Vertices;
        public List<Vector3> Normal;
        public List<Vector2> TextureCoordinates;
        public List<List<int[]>> Triangle;
        

        public static string Format(string toFormat)
        {
            toFormat = toFormat.Replace("\r\n", "\n");
            while (toFormat.Contains("\n\n"))
                toFormat = toFormat.Replace("\n\n", "\n");

            toFormat = toFormat.Replace("\n", " ");

            while (toFormat.Contains("  "))
                toFormat = toFormat.Replace("  ", " ");
            toFormat = toFormat.Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
            if (toFormat[0] == ' ')
                toFormat = toFormat.Remove(0, 1);
            if (toFormat[toFormat.Length - 1] == ' ')
                toFormat = toFormat.Remove(toFormat.Length - 1, 1);
            return toFormat;
        }


        List<string> materialNames;
        List<string> meshNames;
        public List<string> materialFileNames;

        public void Parse()
        {
            this.Triangle.Clear();
            this.TextureCoordinates.Clear();
            this.Vertices.Clear();
            bool newMesh = false;

            bool defaultMTL = false;
            int mtlIndex = 0;
            int meshIndex = 0;

            for (int i=0;i<this.document.Length;i++)
            {
                string currLine = this.document[i].ToLower();
                int valsInd = 0;
                string[] spli = currLine.Split(' ');
                bool waitForMtlLib = false;
                bool waitForMtl = false;
                bool vert = false;
                bool uv = false;
                bool triangle = false;
                float[] vals = new float[3];
                string[] vals_S = new string[3];

                for (int j=0; j < spli.Length;j++)
                {
                    if (spli[j] == "v")
                    {
                        vert = true;
                        continue;
                    }
                    if (spli[j] == "vt")
                    {
                        uv = true;
                        continue;
                    }
                    if (newMesh)
                    {
                        if (spli[j].Length>0)
                        {
                            int indexOf = meshNames.IndexOf(spli[j]);
                            if (spli[j].Length > 0 && indexOf < 0)
                            {
                                meshIndex = meshNames.Count;
                                this.Triangle.Add(new List<int[]>(0));
                                meshNames.Add(spli[j]);
                                MaterialIndices.Add(mtlIndex);
                            }
                            else
                            {
                                meshIndex = indexOf;
                                //MaterialIndices.Add(mtlIndex);
                            }
                        }
                        
                        newMesh = false;
                        continue;
                    }
                    if (spli[j] == "f")
                    {
                        triangle = true;
                        continue;
                    }
                    if (spli[j] == "g")
                    {
                        newMesh = true;
                        continue;
                    }
                    if (spli[j] == "mtllib")
                    {
                        waitForMtlLib = true;
                        continue;
                    }
                    if (spli[j] == "usemtl")
                    {
                        waitForMtl = true;
                        continue;
                    }
                    if (waitForMtl)
                    {
                        if (defaultMTL)
                        {
                            int indexOf = materialNames.IndexOf(spli[j]);
                            if (indexOf<0)
                            {
                                mtlIndex = materialNames.Count;

                                if (File.Exists(this.DirectoryName + @"\" + spli[j].Replace("/","\\")   ))
                                    materialFileNames.Add(this.DirectoryName + @"\" + spli[j].Replace("/", "\\"));
                                else
                                    materialFileNames.Add(spli[j]);

                                materialNames.Add(spli[j]);
                            }
                            else
                            {
                                mtlIndex = indexOf;
                            }
                        }
                        else
                        {
                            mtlIndex = materialNames.IndexOf(spli[j]);
                        }
                        MaterialIndices[meshIndex] = mtlIndex;
                        waitForMtl = false;
                        continue;
                    }
                    if (waitForMtlLib)
                    {
                        string[] mtl = new string[0];
                        if (File.Exists(this.DirectoryName + @"\" + spli[j].Replace("/", "\\")  ))
                            mtl = File.ReadAllLines(this.DirectoryName + @"\" + spli[j].Replace("/", "\\")  );
                        else if (File.Exists(spli[j]))
                            mtl = File.ReadAllLines(spli[j]);
                        if (mtl.Length>0)
                        {
                            bool newMTL = false;
                            bool waitForImage = false;
                            for (int k=0;k<mtl.Length;k++)
                            {
                                string currLine_ = mtl[k].ToLower();
                                string[] spli_ = currLine_.Split(' ');
                                for (int l = 0; l < spli_.Length; l++)
                                {
                                    if (spli_[l] == "newmtl")
                                    {
                                        newMTL = true;
                                        continue;
                                    }
                                    if (spli_[l] == "map_kd")
                                    {
                                        waitForImage = true;
                                        continue;
                                    }
                                    if (newMTL)
                                    {
                                        if (materialFileNames.Count < materialNames.Count)
                                            materialFileNames.Add("noImage.png");
                                        materialNames.Add(spli_[l]);
                                        newMTL = false;
                                    }
                                    if (waitForImage)
                                    {
                                        if (File.Exists(this.DirectoryName + @"\" + spli_[l].Replace("/", "\\")    ))
                                            materialFileNames.Add(this.DirectoryName + @"\" + spli_[l].Replace("/", "\\")    );
                                        else
                                            materialFileNames.Add(spli_[l]);
                                        waitForImage = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            defaultMTL = true;
                        }
                        waitForMtlLib = false;
                        continue;
                    }
                    float currVal = 0;
                    if (Single.TryParse(spli[j].Replace(".", System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator),out currVal))
                    {
                        vals[valsInd] = currVal;
                        valsInd++;
                    }
                    if (spli[j].Contains("/"))
                    {
                        vals_S[valsInd] = spli[j];
                        valsInd++;
                    }
                }
                if (vert)
                {
                    float x = vals[0];
                    float y = vals[1];
                    float z = vals[2];

                    if (x > this.MaxCoords.X) this.MaxCoords.X = x;
                    if (x < this.MinCoords.X) this.MinCoords.X = x;

                    if (y > this.MaxCoords.Y) this.MaxCoords.Y = y;
                    if (y < this.MinCoords.Y) this.MinCoords.Y = y;

                    if (z > this.MaxCoords.Z) this.MaxCoords.Z = z;
                    if (z < this.MinCoords.Z) this.MinCoords.Z = z;

                    this.Vertices.Add(new Vector3(x,y,z));
                    vert = false;
                }
                if (uv)
                {
                    this.TextureCoordinates.Add(new Vector2(vals[0], 1-vals[1]));
                    uv = false;
                }
                if (triangle)
                {
                    for (int m=0;m< vals_S.Length;m++)
                    {
                        string[] spli__ = vals_S[m].Split('/');
                        this.Triangle[meshIndex].Add(new int[3]);
                        for (int n=0;n< spli__.Length; n++)
                        {
                            this.Triangle[meshIndex].Last()[n] = int.Parse(spli__[n])-1;
                        }
                    }
                    triangle = false;
                }
            }
            this.RenderBuffer = new VertexPositionColorTexture[this.Triangle.Count][];
            for (int i=0;i< this.RenderBuffer.Length;i++)
            {
                this.RenderBuffer[i] = new VertexPositionColorTexture[this.Triangle[i].Count];
                for (int j = 0; j < this.RenderBuffer[i].Length; j++)
                {
                    this.RenderBuffer[i][j] = new VertexPositionColorTexture();
                }
            }
        }

        public List<int> MaterialIndices = new List<int>(0);
        //public float scaleVert = 1;
        
    }
}
