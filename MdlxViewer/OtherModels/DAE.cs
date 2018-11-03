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
    class DAE : Object3D
    {
        XmlDocument Document;

        public DAE(string filename)
        {
            this.Skeleton = new Skeleton(this);
            this.Document = new XmlDocument();
            this.vertexTriInd = new List<int>(0);
            this.normalTriInd = new List<int>(0);
            this.uvTriInd = new List<int>(0);
            this.colorTriInd = new List<int>(0);

            this.GeometryIDs = new List<string>(0);
            this.Vertices = new List<List<Vector3>>(0);
            this.Normal = new List<List<Vector3>>(0);
            this.TextureCoordinates = new List<List<Vector2>>(0);

            this.VertexColor = new List<List<byte[]>>(0);

            this.Triangle = new List<List<int[]>>(0);
            this.Influences = new List<List<List<float>>>(0);
            this.InfluencesIndices = new List<List<List<int>>>(0);
            
            this.materialNames = new List<string>(0);
            
            this.materialFileNames = new List<string>(0);
            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.DirectoryName = Path.GetDirectoryName(filename);
            this.FileName = filename;
        }

        Vector3 GetTriangleVertex(int meshIndex, int vertIndex)
        {
            Vector3 output = Vector3.Zero;
            if (meshIndex < this.Triangle.Count && meshIndex < this.vertexTriInd.Count && this.vertexTriInd[meshIndex] > -1)
            {
                if (vertIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][vertIndex][this.vertexTriInd[meshIndex]];
                    if (index < this.Vertices[meshIndex].Count)
                    {
                        output = this.Vertices[meshIndex][index];
                    }
                }
            }
            return output;
        }

        List<float> GetInfluences(int meshIndex, int vertIndex)
        {
            List<float> output = new List<float>(0);
            if (meshIndex < this.Triangle.Count && meshIndex < this.vertexTriInd.Count && this.vertexTriInd[meshIndex] > -1)
            {
                if (vertIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][vertIndex][this.vertexTriInd[meshIndex]];
                    if (index < this.Influences[meshIndex].Count)
                    {
                        output = this.Influences[meshIndex][index];
                    }
                }
            }
            return output;
        }

        List<int> GetMatrices(int meshIndex, int vertIndex)
        {
            List<int> output = new List<int>(0);
            if (meshIndex < this.Triangle.Count && meshIndex < this.vertexTriInd.Count && this.vertexTriInd[meshIndex] > -1)
            {
                if (vertIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][vertIndex][this.vertexTriInd[meshIndex]];
                    if (index < this.InfluencesIndices[meshIndex].Count)
                    {
                        output = this.InfluencesIndices[meshIndex][index];
                    }
                }
            }
            return output;
        }

        Vector2 GetTriangleUv(int meshIndex, int uvIndex)
        {
            Vector2 output = Vector2.Zero;
            if (meshIndex < this.Triangle.Count && meshIndex < this.uvTriInd.Count && this.uvTriInd[meshIndex] > -1)
            {
                if (uvIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][uvIndex][this.uvTriInd[meshIndex]];
                    if (index < this.TextureCoordinates[meshIndex].Count)
                    {
                        output = this.TextureCoordinates[meshIndex][index];
                    }
                }
            }
            return output;
        }

        Vector3 GetTriangleNormal(int meshIndex, int normalIndex)
        {
            Vector3 output = Vector3.Zero;
            if (meshIndex < this.Triangle.Count && meshIndex < this.normalTriInd.Count && this.normalTriInd[meshIndex] > -1)
            {
                if (normalIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][normalIndex][this.normalTriInd[meshIndex]];
                    if (index < this.Normal[meshIndex].Count)
                    {
                        output = this.Normal[meshIndex][index];
                    }
                }
            }
            return output;
        }

        Microsoft.Xna.Framework.Color GetTriangleColor(int meshIndex, int vertIndex)
        {
            Microsoft.Xna.Framework.Color output = new Microsoft.Xna.Framework.Color(255,255,255,255);
            if (meshIndex < this.Triangle.Count && meshIndex < this.colorTriInd.Count && this.colorTriInd[meshIndex] > -1)
            {
                if (vertIndex < this.Triangle[meshIndex].Count)
                {
                    int index = this.Triangle[meshIndex][vertIndex][this.colorTriInd[meshIndex]];
                    if (index < this.VertexColor[meshIndex].Count)
                    {
                        output.R = this.VertexColor[meshIndex][index][0];
                        output.G = this.VertexColor[meshIndex][index][1];
                        output.B = this.VertexColor[meshIndex][index][2];
                        output.R = this.VertexColor[meshIndex][index][3];
                    }
                }
            }
            return output;
        }

        public bool ApplyTransformations { get; set; }

        public void ComputeMatricesWithChanges()
        {
            for (int i = 0; i < this.Skeleton.Bones.Length; i++)
            {
                float rX = 0;
                float rY = 0;
                float rZ = 0;
                if (i == this.Skeleton.selectedBone)
                {
                    Vector3 pos = Vector3.Transform(Vector3.Zero, this.Skeleton.BonesReMatrices[this.Skeleton.selectedBone]);
                    View.transformationBox.Position = pos;
                    rX = View.transformationBox.X;
                    rY = View.transformationBox.Y;
                    rZ = View.transformationBox.Z;
                }

                this.Skeleton.BonesReMatrices[i] = Matrix.CreateFromYawPitchRoll(
                    this.Skeleton.Bones[i].RotateX_Addition+ rX, 
                    this.Skeleton.Bones[i].RotateY_Addition+ rY,
                    this.Skeleton.Bones[i].RotateZ_Addition+ rZ) * this.Skeleton.Bones[i].Matrice;
            }
            for (int i = 0; i < this.Skeleton.BonesReMatrices.Length; i++)
            {
                Matrix mat = this.Skeleton.BonesReMatrices[i];
                for (int j = 0; j < this.Skeleton.BonesReMatrices.Length; j++)
                {
                    if (j == i) continue;
                    if (this.Skeleton.Bones[j].ParentID == this.Skeleton.Bones[i].ID)
                    {
                        this.Skeleton.BonesReMatrices[j] *= mat;
                    }
                }
            }
        }

        public void UpdateSkinCondition()
        {
            for (int i=0;i<this.Skeleton.Bones.Length;i++)
            {
                this.Skeleton.Bones[i].IsSkinned = false;
            }
            List<int> alreadyDone = new List<int>(0);
            for (int i = 0; i < this.InfluencesIndices.Count; i++)
            {
                for (int j = 0; j < this.InfluencesIndices[i].Count; j++)
                {
                    for (int k = 0; k < this.InfluencesIndices[i][j].Count; k++)
                    {
                        int index = alreadyDone.IndexOf(this.InfluencesIndices[i][j][k]);
                        if (index<0)
                        {
                            alreadyDone.Add(this.InfluencesIndices[i][j][k]);
                            this.Skeleton.Bones[this.InfluencesIndices[i][j][k]].IsSkinned = true;
                        }
                    }
                }
            }
        }

        public VertexPositionColorTexture[][] RenderBuffer;

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            this.UpdateSkinCondition();
            this.ComputeMatricesWithChanges();


            for (int mesh = 0; mesh < this.Triangle.Count; mesh++)
            {
                int ind = this.Triangle[mesh].Count-1;
                for (int triInd = 0; triInd < this.Triangle[mesh].Count; triInd += 3)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        VertexPositionColorTexture vpt = new VertexPositionColorTexture();
                        vpt.Position = GetTriangleVertex(mesh, triInd + k);
                        var infsV = GetInfluences(mesh, triInd + k);
                        var infs = GetMatrices(mesh, triInd + k);

                        Vector3 final = Vector3.Zero;

                        for (int l = 0; l < infs.Count; l++)
                        {
                            Matrix mat = this.Skeleton.BonesMatrices[infs[l]];
                            float inf = infsV[l];
                            Vector3 v3 = Vector3.Transform(vpt.Position, Matrix.Invert(mat));

                            v3 = Vector3.Transform(v3, this.Skeleton.BonesReMatrices[infs[l]]);
                            final.X += v3.X * inf;
                            final.Y += v3.Y * inf;
                            final.Z += v3.Z * inf;
                        }
                        vpt.Position = final;
                        vpt.TextureCoordinate = GetTriangleUv(mesh, triInd + k);
                        vpt.Color = GetTriangleColor(mesh, triInd + k);

                        vpt.Position += this.Position;
                        this.RenderBuffer[mesh][ind] = vpt;
                        ind--;
                    }
                    //ApplyTransformations
                }

                
                gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

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
                gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.RenderBuffer[mesh], 0, this.Triangle[mesh].Count / 3);
                
            }
            this.Skeleton.GetGraphic();
            this.Skeleton.Draw(gcm, be, rs);
        }

        public string FileName = "";
        public string Name = "";
        public string DirectoryName = "";

        List<string> GeometryIDs;

        public List<List<Vector3>> Vertices;
        public List<List<Vector3>> Normal;
        public List<List<Vector2>> TextureCoordinates;

        public List<List<byte[]>> VertexColor;

        public List<List<int[]>> Triangle;

        public List<List<List<int>>> InfluencesIndices;
        public List<List<List<float>>> Influences;
        

        public static string ToString(Matrix m)
        {
            string s = "";
            s += m.M11.ToString("0.000000") + " " + m.M21.ToString("0.000000") + " " + m.M31.ToString("0.000000") + " " + m.M41.ToString("0.000000") + "\r\n";
            s += m.M12.ToString("0.000000") + " " + m.M22.ToString("0.000000") + " " + m.M32.ToString("0.000000") + " " + m.M42.ToString("0.000000") + "\r\n";
            s += m.M13.ToString("0.000000") + " " + m.M23.ToString("0.000000") + " " + m.M33.ToString("0.000000") + " " + m.M43.ToString("0.000000") + "\r\n";
            s += m.M14.ToString("0.000000") + " " + m.M24.ToString("0.000000") + " " + m.M34.ToString("0.000000") + " " + m.M44.ToString("0.000000") + "\r\n";
            return s;
        }
        public static string ToStringAccurate(Matrix m)
        {
            string s = "";
            s += ((Decimal)m.M11) + " " + ((Decimal)m.M21) + " " + ((Decimal)m.M31) + " " + ((Decimal)m.M41) + "\r\n";
            s += ((Decimal)m.M12) + " " + ((Decimal)m.M22) + " " + ((Decimal)m.M32) + " " + ((Decimal)m.M42) + "\r\n";
            s += ((Decimal)m.M13) + " " + ((Decimal)m.M23) + " " + ((Decimal)m.M33) + " " + ((Decimal)m.M43) + "\r\n";
            s += ((Decimal)m.M14) + " " + ((Decimal)m.M24) + " " + ((Decimal)m.M34) + " " + ((Decimal)m.M44) + "\r\n";
            return s;
        }

        public static string Format(string toFormat)
        {
            while (toFormat.Contains("\r"))
                toFormat = toFormat.Replace("\r", "");

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
        public List<int> vertexTriInd;
        public List<int> normalTriInd;
        public List<int> uvTriInd;
        public List<int> colorTriInd;

        List<string> materialNames;
        
        public List<string> materialFileNames;

        public void Parse()
        {
            byte[] data = SrkBinary.GetBytesArray(this.FileName);

            string fileData = Encoding.ASCII.GetString(data);

            fileData = fileData.Replace("xmlns", "whocares");
            this.Document.LoadXml(fileData);
            

            XmlNodeList materials = this.Document.SelectNodes("//library_materials/material");

            if (materials != null)
                for (int i = 0; i < materials.Count; i++)
                {
                    string materialID = materials[i].SelectNodes("@id")[0].InnerText;
                    string effectID = materials[i].SelectNodes("instance_effect/@url")[0].InnerText.Remove(0, 1);

                    XmlNode effectNode = this.Document.SelectNodes("//library_effects/effect[@id='" + effectID + "']")[0];
                    string imageID = "";
                    XmlNodeList surfInitNode = effectNode.SelectNodes("profile_COMMON//surface/init_from");
                    if (surfInitNode != null && surfInitNode.Count > 0)
                    {
                        imageID = surfInitNode[0].InnerText;
                    }

                    XmlNodeList textureNode = effectNode.SelectNodes("profile_COMMON//texture/@texture");
                    if (imageID.Length == 0 && textureNode != null && textureNode.Count > 0)
                    {
                        imageID = textureNode[0].InnerText;
                    }
                    string fileName = this.Document.SelectNodes("//library_images/image[@id='" + imageID + "']/init_from")[0].InnerText;
                    fileName = fileName.Replace("file://", "");
                    fileName = fileName.Replace("../", "");
                    fileName = fileName.Replace("./", "");
                    fileName = fileName.Replace("/", "\\");


                    if (Path.GetExtension(fileName).Length == 0)
                    {
                        string dir = Path.GetDirectoryName(fileName);
                        bool notFoundExt = true;
                        if (Directory.Exists(dir))
                        {
                            string[] dirFiles = Directory.GetFiles(dir);
                            for (int d=0;d<dirFiles.Length;d++)
                            {
                                if (Path.GetFileNameWithoutExtension(dirFiles[d])== Path.GetFileNameWithoutExtension(fileName))
                                {
                                    fileName += Path.GetExtension(dirFiles[d]);
                                    notFoundExt = false;
                                    break;
                                }
                            }
                        }
                        if (notFoundExt)
                        fileName += ".png";
                    }

                    string[] spli = fileName.Split('\\');
                    bool exists = false;

                    string fname = "";
                    if (spli.Length > 0)
                    {
                        fileName = spli[spli.Length - 1];
                        if (File.Exists(this.DirectoryName + @"\" + fileName))
                        {
                            fname = this.DirectoryName + @"\" + fileName;
                            exists = true;
                        }
                    }

                    if (!exists)
                    {
                        fname = fileName;
                        exists = true;
                    }

                    this.materialNames.Add(materialID);
                    this.materialFileNames.Add(fname);
                }

            var wrapnode = this.Document.SelectNodes("//technique[@profile='MAYA']");
            bool wrapUV = wrapnode != null && wrapnode.Count > 0;
            XmlNodeList geometries = this.Document.SelectNodes("//library_geometries/geometry");
            for (int i = 0; i < geometries.Count; i++)
            {
                XmlNode mesh = geometries[i].SelectNodes("mesh")[0];
                this.GeometryIDs.Add(geometries[i].SelectNodes("@id")[0].InnerText);

                XmlNodeList inputs = mesh.SelectNodes("*/input[@semantic]");


                string vertexID = "";
                string normalID = "";
                string texcoordID = "";
                string colorID = "";

                vertexTriInd.Add(-1);
                normalTriInd.Add(-1);
                uvTriInd.Add(-1);
                colorTriInd.Add(-1);
                int stride = 0;
                for (int j = 0; j < inputs.Count; j++)
                {
                    string semanticAtt = inputs[j].SelectNodes("@semantic")[0].InnerText.ToUpper();
                    XmlNodeList offsetAtt = inputs[j].SelectNodes("@offset");

                    switch (semanticAtt)
                    {
                        case "VERTEX":
                            vertexTriInd[vertexTriInd.Count - 1] = 0;
                            if (offsetAtt != null && offsetAtt.Count > 0)
                            {
                                vertexTriInd[vertexTriInd.Count - 1] = int.Parse(offsetAtt[0].InnerText);
                                stride++;
                            }
                            break;
                        case "POSITION":
                            vertexID = inputs[j].SelectNodes("@source")[0].InnerText.Remove(0, 1);
                            break;
                        case "NORMAL":
                            normalTriInd[normalTriInd.Count - 1] = 0;
                            if (offsetAtt != null && offsetAtt.Count > 0)
                            {
                                normalTriInd[normalTriInd.Count - 1] = int.Parse(offsetAtt[0].InnerText);
                                stride++;
                            }
                            normalID = inputs[j].SelectNodes("@source")[0].InnerText.Remove(0, 1);
                            break;
                        case "TEXCOORD":
                            uvTriInd[uvTriInd.Count - 1] = 0;
                            if (offsetAtt != null && offsetAtt.Count > 0)
                            {
                                uvTriInd[uvTriInd.Count - 1] = int.Parse(offsetAtt[0].InnerText);
                                stride++;
                            }
                            texcoordID = inputs[j].SelectNodes("@source")[0].InnerText.Remove(0, 1);
                            break;
                        case "COLOR":
                            colorTriInd[colorTriInd.Count - 1] = 0;
                            if (offsetAtt != null && offsetAtt.Count > 0)
                            {
                                colorTriInd[colorTriInd.Count - 1] = int.Parse(offsetAtt[0].InnerText);
                                stride++;
                            }
                            colorID = inputs[j].SelectNodes("@source")[0].InnerText.Remove(0, 1);
                            break;
                    }
                }

                if (vertexID.Length == 0)
                    throw new Exception("Error: Mesh " + i + " does not have vertices. Remove this mesh before to use this tool.");

                string[] vertexArray = new string[0];
                string[] normalArray = new string[0];
                string[] texcoordArray = new string[0];
                string[] colorArray = new string[0];

                if (vertexID.Length > 0)
                    vertexArray = Format(mesh.SelectNodes(@"source[@id='" + vertexID + "']/float_array")[0].InnerText).Split(' ');
                if (normalID.Length > 0)
                    normalArray = Format(mesh.SelectNodes(@"source[@id='" + normalID + "']/float_array")[0].InnerText).Split(' ');
                if (texcoordID.Length > 0)
                    texcoordArray = Format(mesh.SelectNodes(@"source[@id='" + texcoordID + "']/float_array")[0].InnerText).Split(' ');
                if (colorID.Length > 0)
                    colorArray = Format(mesh.SelectNodes(@"source[@id='" + colorID + "']/float_array")[0].InnerText).Split(' ');

                this.Vertices.Add(new List<Vector3>(0));
                this.Normal.Add(new List<Vector3>(0));
                this.TextureCoordinates.Add(new List<Vector2>(0));
                this.VertexColor.Add(new List<byte[]>(0));

                this.Triangle.Add(new List<int[]>(0));

                this.Influences.Add(new List<List<float>>(0));
                this.InfluencesIndices.Add(new List<List<int>>(0));

                for (int j = 0; j < vertexArray.Length; j += 3)
                {
                    float x = Single.Parse(vertexArray[j ]);
                    float y = Single.Parse(vertexArray[j + 1]);
                    float z = Single.Parse(vertexArray[j + 2]);

                    if (x > this.MaxCoords.X) this.MaxCoords.X = x;
                    if (x < this.MinCoords.X) this.MinCoords.X = x;

                    if (y > this.MaxCoords.Y) this.MaxCoords.Y = y;
                    if (y < this.MinCoords.Y) this.MinCoords.Y = y;

                    if (z > this.MaxCoords.Z) this.MaxCoords.Z = z;
                    if (z < this.MinCoords.Z) this.MinCoords.Z = z;

                    this.Vertices.Last().Add(new Vector3(x,y,z));
                }

                for (int j = 0; j < normalArray.Length; j += 3)
                    this.Normal.Last().Add(new Vector3(Single.Parse(normalArray[j]), Single.Parse(normalArray[j + 1]), Single.Parse(normalArray[j + 2])));

                for (int j = 0; j < texcoordArray.Length; j += 2)
                {
                    Vector2 uv = new Vector2(Single.Parse(texcoordArray[j]), 1-Single.Parse(texcoordArray[j + 1]));
                    this.TextureCoordinates.Last().Add(uv);
                }

                for (int j = 0; j < colorArray.Length; j += 4)
                    this.VertexColor.Last().Add(new byte[] {
                        (byte)(Single.Parse(colorArray[j]) * 128) ,
                        (byte)(Single.Parse(colorArray[j + 1]) * 128),
                        (byte)(Single.Parse(colorArray[j + 2]) * 128),
                        (byte)(Single.Parse(colorArray[j + 3]) * 128)});

                string triangleString = mesh.SelectNodes(@"triangles/p")[0].InnerText;

                    string[] triangleArray = Format(triangleString).Split(' ');
                    for (int j = 0; j < triangleArray.Length; j += stride)
                    {
                        int[] indices = new int[stride];
                        for (int k = 0; k < stride; k++)
                            indices[k] = int.Parse(triangleArray[j + k]);

                        this.Triangle.Last().Add(indices);
                    }

            }

            var bonesNode = this.Document.SelectNodes(@"//library_visual_scenes/visual_scene//node[@type='JOINT']"); // and not(contains(@name,'mesh'))

            if (bonesNode != null && bonesNode.Count > 0)
            {
                this.Skeleton.Bones = new Bone[bonesNode.Count];
                this.Skeleton.BonesMatrices = new Matrix[bonesNode.Count];
                this.Skeleton.BonesReMatrices = new Matrix[bonesNode.Count];

                bonesNode = this.Document.SelectNodes(@"//library_visual_scenes/visual_scene/node[@type='JOINT']");
                for (int i=0;i< bonesNode.Count;i++)
                {
                    XmlNode bone000 = bonesNode[i];
                    GetBoneNames(bone000);
                    GetBones(bone000);
                    UnwrapSkeleton(bone000, -1);
                }
                ComputeMatrices();
            }

            XmlNodeList skinnings = this.Document.SelectNodes(@"//library_controllers/controller/skin");
            for (int i = 0; i < skinnings.Count; i++)
            {
                var sourceNodes = skinnings[i].SelectNodes("@source");
                if (sourceNodes == null || sourceNodes.Count == 0) continue;
                int geometryIndex = this.GeometryIDs.IndexOf(sourceNodes[0].InnerText.Remove(0, 1));
                if (geometryIndex < 0)
                    continue;

                string jointID = skinnings[i].SelectNodes(@"*/input[@semantic='JOINT']/@source")[0].InnerText.Remove(0, 1);
                string[] jointsArray = Format(skinnings[i].SelectNodes(@"source[@id='" + jointID + "']/Name_array")[0].InnerText).Split(' ');
                string[] skinCounts = Format(skinnings[i].SelectNodes(@"vertex_weights/vcount")[0].InnerText).Split(' ');
                string[] skins = Format(skinnings[i].SelectNodes(@"vertex_weights/v")[0].InnerText).Split(' ');
                string[] skinsWeights = Format(skinnings[i].SelectNodes(@"source[contains(@id, 'eights')]/float_array")[0].InnerText).Split(' ');

                int tabIndex = 0;

                for (int j = 0; j < skinCounts.Length; j++)
                {
                    int influenceCount = int.Parse(skinCounts[j]);

                    this.Influences[geometryIndex].Add(new List<float>(0));
                    this.InfluencesIndices[geometryIndex].Add(new List<int>(0));

                    for (int k = 0; k < influenceCount; k++)
                    {
                        int jointIndex = int.Parse(skins[tabIndex]);
                        int weightIndex = int.Parse(skins[tabIndex + 1]);
                        float weight = Single.Parse(skinsWeights[weightIndex]);

                        string jointName = jointsArray[jointIndex];

                        this.Influences[geometryIndex].Last().Add(weight);
                        this.InfluencesIndices[geometryIndex].Last().Add(BoneNames.IndexOf(jointName));
                        
                        tabIndex += 2;
                    }

                }
            }
            
            List<string> materialsFnames = new List<string>(0);

                for (int i = 0; i < this.Triangle.Count; i++)
                {
                    string currController = "";
                    for (int l = 0; l < skinnings.Count; l++)
                    {
                        var node = skinnings[l].SelectNodes("@source");
                        if (node != null && node.Count > 0 && node[0].InnerText == "#" + this.GeometryIDs[i])
                        {
                            node = skinnings[l].ParentNode.SelectNodes("@id");
                            if (node != null && node.Count > 0)
                            {
                                currController = node[0].InnerText;
                                break;
                            }
                        }
                    }
                    string matID = "";
                    if (matID.Length == 0)
                    {
                        XmlNodeList instanceGeometry = this.Document.SelectNodes("//library_visual_scenes/visual_scene/node/instance_geometry[@url='#" + GeometryIDs[i] + "']//instance_material/@target");
                        if (instanceGeometry != null && instanceGeometry.Count > 0)
                        {
                            matID = instanceGeometry[0].InnerText.Remove(0, 1);
                            if (!materialNames.Contains(matID))
                                matID = "";
                        }
                    }
                    if (matID.Length == 0)
                    {
                        XmlNodeList instanceController = this.Document.SelectNodes("//library_visual_scenes/visual_scene/node/instance_controller[@url='#" + currController + "']//instance_material/@target");
                        if (instanceController != null && instanceController.Count > 0)
                        {
                            matID = instanceController[0].InnerText.Remove(0, 1);
                            if (!materialNames.Contains(matID))
                                matID = "";
                        }
                    }

                    if (matID.Length > 0)
                    {
                        string fname = materialFileNames[materialNames.IndexOf(matID)];
                        int mmaterialIndex = materialsFnames.Count;

                        if (!materialsFnames.Contains(fname))
                            materialsFnames.Add(fname);
                        else
                            mmaterialIndex = materialsFnames.IndexOf(fname);
                        MaterialIndices.Add(mmaterialIndex);
                    }
            }
            this.RenderBuffer = new VertexPositionColorTexture[this.Triangle.Count][];
            for (int i = 0; i < this.RenderBuffer.Length; i++)
            {
                this.RenderBuffer[i] = new VertexPositionColorTexture[this.Triangle[i].Count];
                for (int j = 0; j < this.RenderBuffer[i].Length; j++)
                {
                    this.RenderBuffer[i][j] = new VertexPositionColorTexture();
                }
            }
        }
        public int[] Cloner(int[] input)
        {
            int[] output = new int[input.Length];

            Array.Copy(input, output, input.Length);
            return output;
        }
        public List<int> MaterialIndices = new List<int>(0);
        //public float scaleVert = 1;
        

        public void ComputeMatrices()
        {
            for (int i = 0; i < this.Skeleton.BonesMatrices.Length; i++)
            {
                Matrix mat = this.Skeleton.BonesMatrices[i];
                for (int j = 0; j < this.Skeleton.BonesMatrices.Length; j++)
                {
                    if (j == i) continue;
                    if (this.Skeleton.Bones[j] != null && this.Skeleton.Bones[j].ParentID == this.Skeleton.Bones[i].ID)
                    {
                        this.Skeleton.BonesMatrices[j] *= mat;
                    }
                }
            }
        }
        
        List<string> BoneNames = new List<string>(0);

        public void GetBoneNames(XmlNode node)
        {
            string name = GetBoneName(node);
            if (!BoneNames.Contains(name))
                BoneNames.Add(name);
            XmlNodeList bones = node.SelectNodes("node");
            for (int i = 0; i < bones.Count; i++)
            {
                GetBoneNames(bones[i]);
            }
        }

        public void GetBones(XmlNode node)
        {
            short id = GetBoneID(node);
            this.Skeleton.Bones[id] = new Bone(id);
            this.Skeleton.Bones[id].Matrice = GetBoneMatrix(node);
            this.Skeleton.BonesMatrices[id] = GetBoneMatrix(node);

            XmlNodeList bones = node.SelectNodes("node");
            for (int i = 0; i < bones.Count; i++)
            {
                GetBones(bones[i]);
            }
        }

        public void UnwrapSkeleton(XmlNode node, short parentID)
        {
            short id = GetBoneID(node);
            if (parentID > -1)
                this.Skeleton.Bones[id].ParentID = parentID;

            XmlNodeList bones = node.SelectNodes("node");
            for (int i = 0; i < bones.Count; i++)
            {
                UnwrapSkeleton(bones[i], id);
            }
        }

        public short GetBoneID(XmlNode node)
        {
            short ind = (short)this.BoneNames.IndexOf(GetBoneName(node));
            return ind;
        }

        public string GetBoneName(XmlNode node)
        {
            for (int i = 0; i < node.Attributes.Count; i++)
                if (node.Attributes[i].Name.ToLower() == "name")
                {
                    return node.Attributes[i].InnerText;
                }
            return "";
        }


        public Matrix GetBoneMatrix(XmlNode node)
        {
            Matrix mat = Matrix.Identity;
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                if (node.ChildNodes[i].Name.ToLower() == "matrix")
                {
                    string[] inner = Format(node.ChildNodes[i].InnerText).Split(' ');
                    mat.M11 = Single.Parse(inner[0]);
                    mat.M21 = Single.Parse(inner[1]);
                    mat.M31 = Single.Parse(inner[2]);
                    mat.M41 = Single.Parse(inner[3]);
                    mat.M12 = Single.Parse(inner[4]);
                    mat.M22 = Single.Parse(inner[5]);
                    mat.M32 = Single.Parse(inner[6]);
                    mat.M42 = Single.Parse(inner[7]);
                    mat.M13 = Single.Parse(inner[8]);
                    mat.M23 = Single.Parse(inner[9]);
                    mat.M33 = Single.Parse(inner[10]);
                    mat.M43 = Single.Parse(inner[11]);
                    mat.M14 = Single.Parse(inner[12]);
                    mat.M24 = Single.Parse(inner[13]);
                    mat.M34 = Single.Parse(inner[14]);
                    mat.M44 = Single.Parse(inner[15]);
                    break;
                }
            }
            return mat;
        }
    }
}
