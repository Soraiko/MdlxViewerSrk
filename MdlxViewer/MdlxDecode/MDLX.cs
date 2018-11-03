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
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace MdlxViewer
{
    public class MDLX : Object3D
    {
        Mdl_t4 t4;
        Tim_t7 t7;
        BAR bar;

        public string FileName = "";
        public string Name = "";
        public string DirectoryName = "";

        
        public MDLX(string filename)
        {
            FileStream str = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read,FileShare.Read);

            this.bar = new BAR(str);
            this.t4 = new Mdl_t4(ref this.bar);
            this.t7 = new Tim_t7(ref this.bar);
            this.meshes = new List<Mesh>(0);
            this.shadowMeshes = new List<Mesh>(0);

            this.Name = Path.GetFileNameWithoutExtension(filename);
            this.DirectoryName = Path.GetDirectoryName(filename);
            this.FileName = filename;
        }

        public void Parse()
        {
            int start = 0;
            while (this.t4.binary.Buffer[start] == 0) start += 16;

            this.t4.binary.Pointer = start;

            this.meshes.Clear();
            this.shadowMeshes.Clear();

            ParseMeshes();
            int shadowPointer = this.t4.binary.ReadInt(0x0C, true);
            if (shadowPointer > 0)
            {
                this.t4.binary.Pointer += shadowPointer;
                ParseMeshes();
            }
        }


        List<Mesh> meshes;
        List<Mesh> shadowMeshes;

        SrkBinary binarySkeleton;
        SrkBinary binarySkeletonDefinitons;

        public void UpdateSkinCondition()
        {
            for (int i = 0; i < this.Skeleton.Bones.Length; i++)
            {
                this.Skeleton.Bones[i].IsSkinned = false;
            }
            List<int> alreadyDone = new List<int>(0);
            for (int h = 0; h < this.meshes.Count; h++)
            {
                for (int i = 0; i < this.meshes[h].InfluencesIndices.Count; i++)
                {
                    for (int j = 0; j < this.meshes[h].InfluencesIndices[i].Count; j++)
                    {

                            int index = alreadyDone.IndexOf(this.meshes[h].InfluencesIndices[i][j]);
                            if (index < 0)
                            {
                                alreadyDone.Add(this.meshes[h].InfluencesIndices[i][j]);
                                this.Skeleton.Bones[this.meshes[h].InfluencesIndices[i][j]].IsSkinned = true;
                            }
                    }
                }
            }
            alreadyDone = null;
        }

        public void ExportDAE(string fname)
        {
            string dir = Path.GetDirectoryName(fname);


            XmlDocument doc = new XmlDocument();
            string sampleDocText = File.ReadAllText(View.GetCurrentDirectory()+@"\Content\sample.dae");
            doc.PreserveWhitespace = true;
            sampleDocText = sampleDocText.Replace("xmlns=", "whocares=");

            doc.LoadXml(sampleDocText);

            var libraryImages = doc.SelectNodes("//library_images")[0];
            var libraryMaterials = doc.SelectNodes("//library_materials")[0];
            var libraryEffects = doc.SelectNodes("//library_effects")[0];

            var imageSample = libraryImages.SelectNodes("image")[0];
            var materialSample = libraryMaterials.SelectNodes("material")[0];
            var effectSample = libraryEffects.SelectNodes("effect")[0];

            libraryImages.RemoveChild(imageSample);
            libraryMaterials.RemoveChild(materialSample);
            libraryEffects.RemoveChild(effectSample);

            for (int i = 0; i < this.t7.TextureCount - this.t7.Patches.Count; i++)
            {
                var newImage = imageSample.CloneNode(true);
                var contenuARemplacer = newImage.SelectNodes("//init_from")[0];
                contenuARemplacer.InnerText = contenuARemplacer.InnerText.Replace("000", i.ToString("d3"));
                newImage.Attributes[0].Value = newImage.Attributes[0].Value.Replace("@", i.ToString());
                newImage.Attributes[1].Value = newImage.Attributes[1].Value.Replace("@", i.ToString());
                libraryImages.AppendChild(newImage);

                var newMaterial = materialSample.CloneNode(true);
                contenuARemplacer = newMaterial.SelectNodes("//instance_effect")[0];
                contenuARemplacer.Attributes[0].Value = contenuARemplacer.Attributes[0].Value.Replace("@", i.ToString());
                newMaterial.Attributes[0].Value = newMaterial.Attributes[0].Value.Replace("@", i.ToString());
                newMaterial.Attributes[1].Value = newMaterial.Attributes[1].Value.Replace("@", i.ToString());
                libraryMaterials.AppendChild(newMaterial);

                var newEffect = effectSample.CloneNode(true);
                newEffect.Attributes[0].Value = newEffect.Attributes[0].Value.Replace("@", i.ToString());
                newEffect.Attributes[1].Value = newEffect.Attributes[1].Value.Replace("@", i.ToString());
                contenuARemplacer = newEffect.SelectNodes("//texture")[0];
                contenuARemplacer.Attributes[0].Value = contenuARemplacer.Attributes[0].Value.Replace("@", i.ToString());

                libraryEffects.AppendChild(newEffect);

                this.t7.getBitmap(i).Save(dir + @"\texture" + i.ToString("d3") + ".png");
            }

            var libraryGeometries = doc.SelectNodes("//library_geometries")[0];
            var libraryControllers = doc.SelectNodes("//library_controllers")[0];
            var visualScene = doc.SelectNodes("//library_visual_scenes/visual_scene")[0];

            var geometrySample = libraryGeometries.SelectNodes("geometry")[0];
            var controllerSample = libraryControllers.SelectNodes("controller")[0];
            var sceneNodeSample = visualScene.SelectNodes("node[@name='polySurface☺']")[0];

            libraryGeometries.RemoveChild(geometrySample);
            libraryControllers.RemoveChild(controllerSample);
            visualScene.RemoveChild(sceneNodeSample);
            XmlNodeList recherche;

            for (int i = 0; i < this.meshes.Count; i++)
            {
                var newGeometry = geometrySample.CloneNode(true);
                recherche = newGeometry.SelectNodes("//@*[contains(., '☺')]");

                for (int mR=0;mR< recherche.Count;mR++)
                {
                    recherche[mR].Value = recherche[mR].Value.Replace("☺", i.ToString());
                }
                recherche = newGeometry.SelectNodes("//@*[contains(., '@')]");
                for (int mR = 0; mR < recherche.Count; mR++)
                {
                    recherche[mR].Value = recherche[mR].Value.Replace("@", this.meshes[i].Binary.ReadInt(4,false).ToString());
                }
                recherche = newGeometry.SelectNodes("//float_array[text() = 'listeDeVertices']");
                recherche[0].Attributes[1].Value = (this.meshes[i].Vertices.Count*3).ToString();
                recherche[0].InnerText = "";
                for (int j = 0; j < this.meshes[i].Vertices.Count; j++)
                    recherche[0].InnerText += this.meshes[i].Vertices[j].X.ToString() + " " +
                        this.meshes[i].Vertices[j].Y.ToString() + " " +
                        this.meshes[i].Vertices[j].Z.ToString() + "\r\n";

                recherche = newGeometry.SelectNodes("//accessor[contains(@source,'POSITION-array')]/@count");
                for (int p = 0; p < recherche.Count; p++)
                    recherche[p].Value = this.meshes[i].Vertices.Count.ToString();

                recherche = newGeometry.SelectNodes("//accessor[contains(@source,'Normal0-array')]/@count");
                for (int p = 0; p < recherche.Count; p++)
                    recherche[p].Value = this.meshes[i].Vertices.Count.ToString();


                recherche = newGeometry.SelectNodes("//accessor[contains(@source,'UV0-array')]/@count");
                for (int p = 0; p < recherche.Count; p++)
                    recherche[p].Value = this.meshes[i].TextureCoordinates.Count.ToString();


                recherche = newGeometry.SelectNodes("//float_array[text() = 'listeDeNormals']");
                recherche[0].Attributes[1].Value = (this.meshes[i].Vertices.Count * 3).ToString();
                recherche[0].InnerText ="";
                for (int j = 0; j < this.meshes[i].Vertices.Count; j++)
                    recherche[0].InnerText += "0.000000 0.000000 0.000000\r\n";

                recherche = newGeometry.SelectNodes("//float_array[text() = 'listeDeUvs']");
                recherche[0].Attributes[1].Value = (this.meshes[i].TextureCoordinates.Count * 2).ToString();
                recherche[0].InnerText = "";
                for (int j = 0; j < this.meshes[i].TextureCoordinates.Count; j++)
                    recherche[0].InnerText += this.meshes[i].TextureCoordinates[j].X.ToString() + " " +
                        (1-this.meshes[i].TextureCoordinates[j].Y).ToString() + "\r\n";


                recherche = newGeometry.SelectNodes("//triangles");
                recherche[0].Attributes[0].Value = (this.meshes[i].Triangle.Count / 3).ToString();
                var paragraphe = doc.CreateElement("p");

                if (this.meshes[i].VertexColor.Count==0)
                {
                    for (int j = 0; j < this.meshes[i].Triangle.Count; j++)
                        paragraphe.InnerText += this.meshes[i].Triangle[j][0] + " " +
                            this.meshes[i].Triangle[j][1] + " ";
                    recherche[0].AppendChild(paragraphe);


                    XmlNodeList colors = newGeometry.SelectNodes("//input[@semantic='COLOR']");
                    recherche[0].RemoveChild(colors[0]);

                    recherche = newGeometry.SelectNodes("//mesh");
                    colors = newGeometry.SelectNodes("//source[contains(@id,'COLOR')]");
                    recherche[0].RemoveChild(colors[0]);
                    colors = null;

                }
                else
                {
                    for (int j = 0; j < this.meshes[i].Triangle.Count; j++)
                    {
                        paragraphe.InnerText += this.meshes[i].Triangle[j][0] + " " +
                            this.meshes[i].Triangle[j][1] + " ";

                        if (this.meshes[i].Triangle[j].Length > 2)
                        {
                            paragraphe.InnerText += (this.meshes[i].Triangle[j][2] + 1) + " ";
                        }
                        else
                        {
                            paragraphe.InnerText += "0 ";
                        }
                    }
                    recherche[0].AppendChild(paragraphe);


                    recherche = newGeometry.SelectNodes("//source[contains(@id,'COLOR')]/float_array/@count");
                    recherche[0].Value = ((this.meshes[i].VertexColor.Count+1) * 4).ToString();

                    recherche = newGeometry.SelectNodes("//source[contains(@id,'COLOR')]//accessor/@count");
                    recherche[0].Value = (this.meshes[i].VertexColor.Count+1).ToString();

                    recherche = newGeometry.SelectNodes("//source[contains(@id,'COLOR')]/float_array");
                    recherche[0].InnerText = "1.000000 1.000000 1.000000 1.000000\r\n";
                    for (int j = 0; j < this.meshes[i].VertexColor.Count; j++)
                        recherche[0].InnerText += (this.meshes[i].VertexColor[j][0] / 255f) + " " + (this.meshes[i].VertexColor[j][1] / 255f) + " " +
                            (this.meshes[i].VertexColor[j][2] / 255f) + " " + (this.meshes[i].VertexColor[j][3] / 255f) +"\r\n";
                }



                libraryGeometries.AppendChild(newGeometry);

                var newController = controllerSample.CloneNode(true);
                recherche = newController.SelectNodes("//@*[contains(., '@')]");

                for (int mR = 0; mR < recherche.Count; mR++)
                {
                    recherche[mR].Value = recherche[mR].Value.Replace("@", i.ToString());
                }
                List<int> matrices = new List<int>(0);
                List<float> infs = new List<float>(0);
                infs.Add(1f);
                List<int> vcount = new List<int>(0);
                List<int> v = new List<int>(0);

                for (int j = 0; j < this.meshes[i].InfluencesIndices.Count; j++)
                {
                    vcount.Add(this.meshes[i].InfluencesIndices[j].Count);
                    for (int k = 0; k < this.meshes[i].InfluencesIndices[j].Count; k++)
                    {
                        int currMat = this.meshes[i].InfluencesIndices[j][k];
                        int index = matrices.IndexOf(currMat);
                        if (index < 0)
                        {
                            v.Add(matrices.Count);
                            matrices.Add(currMat);
                        }
                        else
                        {
                            v.Add(index);
                        }

                        int indexInf = -1;
                        float currInf = this.meshes[i].Influences[j][k];
                        for (int l=0;l< infs.Count;l++)
                        {
                            if (Math.Abs(currInf-infs[l])<0.000001)
                            {
                                indexInf = l;
                                break;
                            }
                        }
                        //Console.WriteLine(indexInf);
                        if (indexInf < 0)
                        {
                            v.Add(infs.Count);
                            infs.Add(currInf);
                        }
                        else
                        {
                            v.Add(indexInf);
                        }
                    }
                }

                recherche = newController.SelectNodes("//Name_array[text() = 'listeDeJoints']");
                recherche[0].Attributes[1].Value = matrices.Count.ToString();
                recherche[0].InnerText = "";
                for (int j = 0; j < matrices.Count; j++)
                    recherche[0].InnerText += "bone"+matrices[j].ToString("d3") + " ";
                recherche[0].InnerText = recherche[0].InnerText.Remove(recherche[0].InnerText.Length - 1);


                recherche = newController.SelectNodes("//accessor[@stride='16']/@count");
                for (int p = 0; p < recherche.Count; p++)
                    recherche[p].Value = matrices.Count.ToString();

                recherche = newController.SelectNodes("//accessor[contains(@source,'oints')]/@count");
                for (int p = 0; p < recherche.Count; p++)
                    recherche[p].Value = matrices.Count.ToString();



                recherche = newController.SelectNodes("//float_array[text() = 'listeDeMatrices']");
                recherche[0].Attributes[1].Value = (matrices.Count*16).ToString();
                recherche[0].InnerText = "";
                for (int j = 0; j < matrices.Count; j++)
                {
                    Microsoft.Xna.Framework.Matrix mat = Matrix.Invert(this.Skeleton.BonesMatrices[matrices[j]]);

                    recherche[0].InnerText += mat.M11.ToString() + " ";
                    recherche[0].InnerText += mat.M21.ToString() + " ";
                    recherche[0].InnerText += mat.M31.ToString() + " ";
                    recherche[0].InnerText += mat.M41.ToString() + " ";

                    recherche[0].InnerText += mat.M12.ToString() + " ";
                    recherche[0].InnerText += mat.M22.ToString() + " ";
                    recherche[0].InnerText += mat.M32.ToString() + " ";
                    recherche[0].InnerText += mat.M42.ToString() + " ";

                    recherche[0].InnerText += mat.M13.ToString() + " ";
                    recherche[0].InnerText += mat.M23.ToString() + " ";
                    recherche[0].InnerText += mat.M33.ToString() + " ";
                    recherche[0].InnerText += mat.M43.ToString() + " ";

                    recherche[0].InnerText += mat.M14.ToString() + " ";
                    recherche[0].InnerText += mat.M24.ToString() + " ";
                    recherche[0].InnerText += mat.M34.ToString() + " ";
                    recherche[0].InnerText += mat.M44.ToString() + " ";
                }
                recherche[0].InnerText = recherche[0].InnerText.Remove(recherche[0].InnerText.Length - 1);

                recherche = newController.SelectNodes("//float_array[text() = 'listeDeWeigths']");
                recherche[0].Attributes[1].Value = infs.Count.ToString();
                recherche[0].InnerText = "";

                for (int j = 0; j < infs.Count; j++)
                {
                    recherche[0].InnerText += infs[j].ToString() + " ";
                }
                recherche[0].InnerText = recherche[0].InnerText.Remove(recherche[0].InnerText.Length - 1);


                recherche = newController.SelectNodes("//accessor[contains(@source,'eights')]/@count");
                for (int p = 0; p < recherche.Count; p++)
                    recherche[p].Value = infs.Count.ToString();


                recherche = newController.SelectNodes("//vertex_weights");
                recherche[0].Attributes[0].Value = vcount.Count.ToString();
                recherche = newController.SelectNodes("//vertex_weights/vcount");

                for (int j = 0; j < vcount.Count; j++)
                {
                    recherche[0].InnerText+= vcount[j]+" ";
                }
                recherche[0].InnerText = recherche[0].InnerText.Remove(recherche[0].InnerText.Length - 1);

                recherche = newController.SelectNodes("//vertex_weights/v");

                for (int j = 0; j < v.Count; j++)
                {
                    recherche[0].InnerText += v[j] + " ";
                }
                recherche[0].InnerText = recherche[0].InnerText.Remove(recherche[0].InnerText.Length - 1);
                
                libraryControllers.AppendChild(newController);

                var newSceneNode = sceneNodeSample.CloneNode(true);

                recherche = newSceneNode.SelectNodes("//@*[contains(., '☺')]");

                for (int mR = 0; mR < recherche.Count; mR++)
                {
                    recherche[mR].Value = recherche[mR].Value.Replace("☺", i.ToString());
                }
                recherche = newSceneNode.SelectNodes("//@*[contains(., '@')]");
                for (int mR = 0; mR < recherche.Count; mR++)
                {
                    recherche[mR].Value = recherche[mR].Value.Replace("@", this.meshes[i].Binary.ReadInt(4, false).ToString());
                }

                visualScene.AppendChild(newSceneNode);
                matrices = null;
                infs = null;
                vcount = null;
                v = null;
            }

            var jointSample = visualScene.SelectNodes("node[@name = 'joint000']")[0];
            visualScene.RemoveChild(jointSample);

            for (int i=0;i<this.Skeleton.Bones.Length;i++)
            {
                var newJoint = jointSample.CloneNode(true);
                newJoint.Attributes[0].Value = "bone" + i.ToString("d3");
                newJoint.Attributes[1].Value = "bone" + i.ToString("d3");
                newJoint.Attributes[2].Value = "bone" + i.ToString("d3");

                recherche = newJoint.SelectNodes("matrix");
                recherche[0].InnerText = "";
                Microsoft.Xna.Framework.Matrix mat = this.Skeleton.Bones[i].Matrice;

                recherche[0].InnerText += mat.M11.ToString() + " ";
                recherche[0].InnerText += mat.M21.ToString() + " ";
                recherche[0].InnerText += mat.M31.ToString() + " ";
                recherche[0].InnerText += mat.M41.ToString() + " ";

                recherche[0].InnerText += mat.M12.ToString() + " ";
                recherche[0].InnerText += mat.M22.ToString() + " ";
                recherche[0].InnerText += mat.M32.ToString() + " ";
                recherche[0].InnerText += mat.M42.ToString() + " ";

                recherche[0].InnerText += mat.M13.ToString() + " ";
                recherche[0].InnerText += mat.M23.ToString() + " ";
                recherche[0].InnerText += mat.M33.ToString() + " ";
                recherche[0].InnerText += mat.M43.ToString() + " ";

                recherche[0].InnerText += mat.M14.ToString() + " ";
                recherche[0].InnerText += mat.M24.ToString() + " ";
                recherche[0].InnerText += mat.M34.ToString() + " ";
                recherche[0].InnerText += mat.M44.ToString() + " ";


                int parentID = this.Skeleton.Bones[i].ParentID;
                if (parentID<0)
                {
                    visualScene.AppendChild(newJoint);
                }
                else
                {
                    recherche = visualScene.SelectNodes("//node[@name='bone"+ parentID .ToString("d3")+ "']");
                    if (recherche.Count>0)
                    {
                        recherche[0].AppendChild(newJoint);
                    }
                    else
                    {
                        visualScene.AppendChild(newJoint);
                    }
                }

            }
            visualScene.AppendChild(visualScene.FirstChild);
            visualScene.AppendChild(visualScene.FirstChild);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.GetEncoding("ISO-8859-1");
            XmlWriter writer = XmlWriter.Create(fname, settings);

            doc.Save(writer);
        }

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            this.UpdateSkinCondition();
            this.ComputeMatricesWithChanges();

            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            for (int i=0;i<this.meshes.Count;i++)
            {
                if (be != null)
                {
                    be.TextureEnabled = true;
                    be.Texture = this.t7.getTexture(this.meshes[i].Binary.ReadInt(4, false));
                    be.CurrentTechnique.Passes[0].Apply();
                }
                if (rs != null)
                {
                    gcm.GraphicsDevice.RasterizerState = rs;
                }
                this.meshes[i].Draw(gcm);
            }
            this.Skeleton.GetGraphic();
            this.Skeleton.Draw(gcm, be, rs);
        }

        public void ComputeMatrices()
        {
            for (int i = 0; i < this.Skeleton.Bones.Length; i++)
            {
                this.Skeleton.BonesMatrices[i] = this.Skeleton.Bones[i].Matrice;
            }
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
        
        public void GetMatrices()
        {
            for (int i = 0; i < this.Skeleton.Bones.Length; i++)
            {
                Matrix m = Matrix.CreateFromAxisAngle(new Vector3(1,0,0), this.Skeleton.Bones[i].RotateX);
                m *= Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), this.Skeleton.Bones[i].RotateY);
                m *= Matrix.CreateFromAxisAngle(new Vector3(0, 0, 1), this.Skeleton.Bones[i].RotateZ);
                m *= Matrix.CreateTranslation(this.Skeleton.Bones[i].TranslateX, this.Skeleton.Bones[i].TranslateY, this.Skeleton.Bones[i].TranslateZ);
                this.Skeleton.Bones[i].Matrice = m;
            }
        }

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
                Matrix rotated = Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), rX + this.Skeleton.Bones[i].RotateX + this.Skeleton.Bones[i].RotateX_Addition);
                rotated *= Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), rY + this.Skeleton.Bones[i].RotateY + this.Skeleton.Bones[i].RotateY_Addition);
                rotated *= Matrix.CreateFromAxisAngle(new Vector3(0, 0, 1), rZ + this.Skeleton.Bones[i].RotateZ + this.Skeleton.Bones[i].RotateZ_Addition);
                rotated *= Matrix.CreateTranslation(this.Skeleton.Bones[i].TranslateX, this.Skeleton.Bones[i].TranslateY, this.Skeleton.Bones[i].TranslateZ);

                this.Skeleton.BonesReMatrices[i] = rotated;
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

        public void ParseMeshes()
        {
            int modelType = this.t4.binary.ReadInt(0, true); /* Normal = 3  || Shadow = 4 */
            
            int bonesCount = this.t4.binary.ReadShort(0x10, true);
            int skeletonOffset = this.t4.binary.ReadShort(0x14, true);
            int skeletonDefinitonsOffset = this.t4.binary.ReadShort(0x18, true);
            int meshCount = this.t4.binary.ReadInt(0x1C, true);

            if (modelType==3)
            {
                byte[] skeletonBytes = new byte[bonesCount*0x40];
                Array.Copy(this.t4.binary.Buffer, this.t4.binary.Pointer + skeletonOffset, skeletonBytes, 0 , skeletonBytes.Length);
                this.binarySkeleton = new SrkBinary(ref skeletonBytes);
                this.Skeleton = new Skeleton(this);
                this.Skeleton.Bones = new Bone[bonesCount];
                this.Skeleton.BonesMatrices = new Matrix[bonesCount];
                this.Skeleton.BonesReMatrices = new Matrix[bonesCount];
                for (int i=0;i< bonesCount;i++)
                {
                    short id = this.binarySkeleton.ReadShort(i * 0x40, false);
                    short parentId = this.binarySkeleton.ReadShort(i * 0x40 + 4, false);
                    float scaleX = this.binarySkeleton.ReadFloat(i * 0x40 + 0x10, false);
                    float scaleY = this.binarySkeleton.ReadFloat(i * 0x40 + 0x14, false);
                    float scaleZ = this.binarySkeleton.ReadFloat(i * 0x40 + 0x18, false);
                    float rotateX = this.binarySkeleton.ReadFloat(i * 0x40 + 0x20, false);
                    float rotateY = this.binarySkeleton.ReadFloat(i * 0x40 + 0x24, false);
                    float rotateZ = this.binarySkeleton.ReadFloat(i * 0x40 + 0x28, false);
                    float translateX = this.binarySkeleton.ReadFloat(i * 0x40 + 0x30, false);
                    float translateY = this.binarySkeleton.ReadFloat(i * 0x40 + 0x34, false);
                    float translateZ = this.binarySkeleton.ReadFloat(i * 0x40 + 0x38, false);

                    this.Skeleton.Bones[i] = new Bone(id);
                    this.Skeleton.Bones[i].ParentID = parentId;
                    this.Skeleton.Bones[i].ScaleX = scaleX;
                    this.Skeleton.Bones[i].ScaleY = scaleY;
                    this.Skeleton.Bones[i].ScaleZ = scaleZ;

                    this.Skeleton.Bones[i].RotateX = rotateX;
                    this.Skeleton.Bones[i].RotateY = rotateY;
                    this.Skeleton.Bones[i].RotateZ = rotateZ;

                    this.Skeleton.Bones[i].TranslateX = translateX;
                    this.Skeleton.Bones[i].TranslateY = translateY;
                    this.Skeleton.Bones[i].TranslateZ = translateZ;
                }
                //Bone.outpt = "";
                GetMatrices();
                ComputeMatrices();
                //File.WriteAllText("matsXNA.txt", Bone.outpt);
                

                byte[] skeletonDefinitionsBytes = new byte[0x110];
                Array.Copy(this.t4.binary.Buffer, this.t4.binary.Pointer + skeletonDefinitonsOffset, skeletonDefinitionsBytes, 0, skeletonDefinitionsBytes.Length);
                this.binarySkeletonDefinitons = new SrkBinary(ref skeletonBytes);
            }
            
            for (int currMeshIndex=0; currMeshIndex<meshCount; currMeshIndex++)
            {
                int dmaOffset = this.t4.binary.ReadInt(0x20+ currMeshIndex*0x20 + 0x10, true);
                int meshStartOffset = this.t4.binary.ReadInt(dmaOffset + 4, true);

                int matiOffset = this.t4.binary.ReadInt(0x20+ currMeshIndex*0x20 + 0x14, true);
                int matiCount = this.t4.binary.ReadInt(matiOffset, true);

                int meshEndOffset = matiOffset + matiCount * 4;
                SrkBinary.Align16(ref meshEndOffset);

                int meshSize = meshEndOffset- meshStartOffset;

                byte[] meshBytes = new byte[0x20 + meshSize];
                Array.Copy(this.t4.binary.Buffer, this.t4.binary.Pointer + 0x20 + currMeshIndex * 0x20, meshBytes, 0, 0x20);
                Array.Copy(this.t4.binary.Buffer, this.t4.binary.Pointer + meshStartOffset, meshBytes, 0x20, meshSize);
                Mesh currMesh = new Mesh(meshBytes);
                currMesh.Objet = this;
                currMesh.Binary = new SrkBinary(ref meshBytes);

                int dmaOffsetLocal = dmaOffset - meshStartOffset;
                int matiOffsetLocal = matiOffset - meshStartOffset;

                currMesh.Binary.WriteInt(0x10, dmaOffsetLocal, false);
                currMesh.Binary.WriteInt(0x14, matiOffsetLocal, false);
                currMesh.Binary.Pointer = 0x20;

                int dmaPosition = dmaOffsetLocal;

                bool readDMAs = true;
                while (readDMAs)
                {
                    int currVifOffset = currMesh.Binary.ReadInt(dmaPosition + 4, true) - meshStartOffset;
                    currMesh.Binary.WriteInt(dmaPosition + 4, currVifOffset, true); /* local dans fichier de sortie */
                    currVifOffset += 8;      /* skip 01 01 00 01     00 80 04 6C */

                    int type = currMesh.Binary.ReadInt(currVifOffset, true);
                    int matCountPerVert_count = currMesh.Binary.ReadInt(currVifOffset + ((type == 2) ? 0x2C : 0x3C), true);
                    
                    currMesh.DMA_Pointers.Add(dmaPosition);
                    dmaPosition += matCountPerVert_count * 0x10 + 0x20;

                    if (currMesh.Binary.ReadUInt(dmaPosition + 0x18, true) != 0x01000101)
                        readDMAs = false;
                }
                //File.WriteAllBytes("mesh" + currMeshIndex.ToString("d3") + ".bin", meshBytes);
                currMesh.GetData();
                if (modelType==3)
                    this.meshes.Add(currMesh);
                else
                    this.shadowMeshes.Add(currMesh);
            }
        }
    }
}
