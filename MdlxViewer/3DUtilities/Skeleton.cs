using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;

namespace MdlxViewer
{
    public class Skeleton
    {
        public Bone[] Bones;
        public Matrix[] BonesMatrices;
        public Matrix[] BonesReMatrices;

        public Object3D objet;
		public VertexPositionColorTexture[] bonesTexture;
		public VertexPositionColorTexture[] bonesTextureCircles;
        public VertexPositionColorTexture[] bonesTextureLines;

        public bool ShowIndices { get; set; }

        public void SelectBoneHierarchy(int index)
        {
            if (index > this.Bones.Length-1)
                return;
            this.Bones[index].Selected = true;
            for (int i = 0; i < this.Bones.Length; i++)
            {
                if (this.Bones[i].ParentID == index)
                {
                    SelectBoneHierarchy(i);
                }
            }
        }

        public void SelectBone(int index)
        {
            if (index > this.Bones.Length - 1)
                return;
            if (!this.AllowBoneSelect)
            {
                throw new Exception("Error: Bone selection not allowed");
            }

            for (int i = 0; i < this.Bones.Length; i++)
            {
                this.Bones[i].Selected = false;
            }

            if (index < 0)
            {
                this.selectedBone = -1;
                View.transformationBox.Enabled = false;
                return;
            }
            else
            {
                if (this.objet is MDLX)
                    (this.objet as MDLX).ComputeMatricesWithChanges();
                if (this.objet is DAE)
                    (this.objet as DAE).ComputeMatricesWithChanges();

                SelectBoneHierarchy(index);

                this.selectedBone = index;
                View.transformationBox.Enabled = true;

                //View.tb.RotX = this.Bones[selectedBone].RotateX;
                //View.tb.RotY = this.Bones[selectedBone].RotateZ;
                //View.tb.RotZ = this.Bones[selectedBone].RotateY;

            }
        }

        public static VertexPositionColorTexture[] boneTextureBuffer = new VertexPositionColorTexture[24];

        public static VertexPositionColorTexture[] GetBoneTexture(int boneIndex, bool skinned)
		{
			boneTextureBuffer[0].Position = new Vector3(1f, 0.4f, 0f);
			boneTextureBuffer[0].Color = (skinned ? Color.Green * 2f : Color.White);

			boneTextureBuffer[0].TextureCoordinate = new Vector2((1 / 382f), (59 / 382f));

			boneTextureBuffer[2].Position = new Vector3(3f, -0.4f, 0f);
			boneTextureBuffer[2].Color = boneTextureBuffer[0].Color;
			boneTextureBuffer[2].TextureCoordinate = new Vector2((161 / 382f), (111 / 382f));

			boneTextureBuffer[1].Position = new Vector3(3f, 0.4f, 0f);
			boneTextureBuffer[1].Color = boneTextureBuffer[0].Color;
			boneTextureBuffer[1].TextureCoordinate = new Vector2((161 / 382f), (59 / 382f));

			boneTextureBuffer[3].Position = new Vector3(3f, -0.4f, 0f);
			boneTextureBuffer[3].Color = boneTextureBuffer[0].Color;
			boneTextureBuffer[3].TextureCoordinate = new Vector2((161 / 382f), (111 / 382f));

			boneTextureBuffer[5].Position = new Vector3(1f, 0.4f, 0f);
			boneTextureBuffer[5].Color = boneTextureBuffer[0].Color;
			boneTextureBuffer[5].TextureCoordinate = new Vector2((1 / 382f), (59 / 382f));

			boneTextureBuffer[4].Position = new Vector3(1f, -0.4f, 0f);
			boneTextureBuffer[4].Color = boneTextureBuffer[0].Color;
			boneTextureBuffer[4].TextureCoordinate = new Vector2((1 / 382f), (111 / 382f));

			int centaines = boneIndex / 100;
			int dizaines = (boneIndex / 10) % 10;
			int unites = boneIndex % 10;
			for (int i = 6; i < boneTextureBuffer.Length; i += 6)
			{
				boneTextureBuffer[i].Position = new Vector3(1f, 0.4f, 0f);
				boneTextureBuffer[i].Color = boneTextureBuffer[0].Color;
				boneTextureBuffer[i].TextureCoordinate = new Vector2((1 / 382f), (1 / 382f));

				boneTextureBuffer[i + 2].Position = new Vector3(1.5f, -0.4f, 0f);
				boneTextureBuffer[i + 2].Color = boneTextureBuffer[0].Color;
				boneTextureBuffer[i + 2].TextureCoordinate = new Vector2((39 / 382f), (57 / 382f));

				boneTextureBuffer[i + 1].Position = new Vector3(1.5f, 0.4f, 0f);
				boneTextureBuffer[i + 1].Color = boneTextureBuffer[0].Color;
				boneTextureBuffer[i + 1].TextureCoordinate = new Vector2((39 / 382f), (1 / 382f));

				boneTextureBuffer[i + 3].Position = new Vector3(1.5f, -0.4f, 0f);
				boneTextureBuffer[i + 3].Color = boneTextureBuffer[0].Color;
				boneTextureBuffer[i + 3].TextureCoordinate = new Vector2((39 / 382f), (57 / 382f));

				boneTextureBuffer[i + 5].Position = new Vector3(1f, 0.4f, 0f);
				boneTextureBuffer[i + 5].Color = boneTextureBuffer[0].Color;
				boneTextureBuffer[i + 5].TextureCoordinate = new Vector2((1 / 382f), (1 / 382f));

				boneTextureBuffer[i + 4].Position = new Vector3(1f, -0.4f, 0f);
				boneTextureBuffer[i + 4].Color = boneTextureBuffer[0].Color;
				boneTextureBuffer[i + 4].TextureCoordinate = new Vector2((1 / 382f), (57 / 382f));
			}
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					boneTextureBuffer[6 + i * 6 + j].Position.X += 2.3f + (0.5f * i);
					int currDigit = centaines;
					if (i == 1)
						currDigit = dizaines;
					if (i == 2)
						currDigit = unites;

					boneTextureBuffer[6 + i * 6 + j].TextureCoordinate.X += ((38 / 382f) * currDigit);
				}
			}

			//for (int i=0;i< output.Length;i++)
			//output[i].Position *= 50f;

			return boneTextureBuffer;
		}


        public int selectedBone = -1;

        public Bone SelectedBone
        {
            get
            {
                if (selectedBone < 0)
                    return Bone.Empty;
                return this.Bones[selectedBone];
            }
        }


        public static Color UnselectedColor = new Color(38, 0, 67);
        public static Color SelectedColor = new Color(67, 255, 163);

        public static Color HighligthedColor = new Color(38+(67/2), 0+(255/2), 67+(163));

        public bool HighlightedBone;
        Vector3 HighlightedBoneMiddle;
        int HighlightedBoneParentID;

        public void DrawLineChildren(Bone currBone)
        {
            Vector3 bonePos = Vector3.Transform(Vector3.Zero, this.BonesReMatrices[currBone.ID]);
            Vector3 position = View.mainCamera.GlobalPosition();

            Vector2 mouseV2 = new Vector2(this.mouseState.Position.X, this.mouseState.Position.Y);

            Vector2 startOrtho = Object3D.GetOrtho(bonePos);

            if (Vector2.Distance(mouseV2, startOrtho)<10)
            {
                bool hasChildren = false;
                for (int i=0;i<this.Bones.Length;i++)
                {
                    if (this.Bones[i].ParentID == currBone.ID)
                    {
                        hasChildren = true;
                        break;
                    }
                }
                if (!hasChildren)
                {
                    this.HighlightedBoneParentID = currBone.ID;
                    this.HighlightedBoneMiddle = bonePos;
                    this.HighlightedBone = true;
                }
            }

            float diffX = position.X - (bonePos.X + objet.Position.X);
            float diffY = position.Y - (bonePos.Y + objet.Position.Y);
            float diffZ = position.Z - (bonePos.Z + objet.Position.Z);
            double hypo = Math.Pow(diffX * diffX + diffY * diffY + diffZ * diffZ, 0.5f);


            for (int i=0;i< this.Bones.Length;i++)
			{
                if (this.Bones[i].ParentID == currBone.ID)
                {
                    Vector3 boneChildrenPos = Vector3.Transform(Vector3.Zero, this.BonesReMatrices[i]);
                    if (Vector3.Distance(bonePos, boneChildrenPos) > 0.001f)
                    for (int j=0;j<1000;j++)
					{
						if (!this.bonesTextureLinesUsed[j])
                        {
                            this.bonesTextureLinesUsed[j] = true;
                            this.bonesTextureLinesUsed[j + 1] = true;
                            this.bonesTextureLinesUsed[j + 2] = true;
                            this.bonesTextureLinesUsed[j + 3] = true;
                            
                            this.bonesTextureLines[j].Position = new Vector3(0, -(float)hypo / 220f, 0);
                            this.bonesTextureLines[j].Position = Vector3.Transform(this.bonesTextureLines[j].Position, Matrix.CreateFromYawPitchRoll(View.mainCamera.Yaw, View.mainCamera.Pitch, 0));
                            this.bonesTextureLines[j].Position += bonePos;

                            this.bonesTextureLines[j].Color = UnselectedColor;
                            if (this.Bones[i].Selected&& this.Bones[i].ParentID>-1&& this.Bones[this.Bones[i].ParentID].Selected)
                                    this.bonesTextureLines[j].Color = SelectedColor;

                            this.bonesTextureLines[j].TextureCoordinate = new Vector2(279 / 382f, 243 / 382f);


                            this.bonesTextureLines[j + 1].TextureCoordinate = this.bonesTextureLines[j].TextureCoordinate;
                            this.bonesTextureLines[j + 1].Position = boneChildrenPos;
                            
                            this.bonesTextureLines[j + 2].Position = new Vector3(0, (float)hypo / 220f, 0);
                            this.bonesTextureLines[j + 2].Position = Vector3.Transform(this.bonesTextureLines[j + 2].Position, Matrix.CreateFromYawPitchRoll(View.mainCamera.Yaw, View.mainCamera.Pitch, 0));
                            this.bonesTextureLines[j + 2].Position += bonePos;

                                
                            this.bonesTextureLines[j + 2].TextureCoordinate = new Vector2(279 / 382f, 243 / 382f);

                            this.bonesTextureLines[j + 3].TextureCoordinate = this.bonesTextureLines[j].TextureCoordinate;
                            this.bonesTextureLines[j + 3].Position = boneChildrenPos;

                            bool boneZero = currBone.Grayed || Vector3.DistanceSquared(bonePos, Vector3.Zero) < 0.001f;
                            bool boneChildZero = this.Bones[i].Grayed || Vector3.DistanceSquared(boneChildrenPos, Vector3.Zero) < 0.001f;

                            if (boneZero || boneChildZero)
                            {
                                if (boneZero)
                                    currBone.Grayed = true;
                                if (boneChildZero)
                                    this.Bones[i].Grayed = true;
                                this.bonesTextureLines[j].Color = this.bonesTextureLines[j].Color * 0.5f;
                                this.bonesTextureLines[j].Color.A = 50;
                            }

                            this.bonesTextureLines[j + 1].Color = this.bonesTextureLines[j].Color;
                            this.bonesTextureLines[j + 2].Color = this.bonesTextureLines[j].Color;
                            this.bonesTextureLines[j + 3].Color = this.bonesTextureLines[j].Color;

                            Vector3 start = bonePos;// this.bonesTextureLines[j + 0].Position + this.objet.Position;
                            Vector3 end = boneChildrenPos;
                            float sansEnfant = Vector2.Distance(mouseV2, Object3D.GetOrtho(boneChildrenPos));

                            Vector3 middle = (start + end) / 2f;

                            if (View.Controls.MouseDragState == 0)
                            {
                                if (sansEnfant > 10 && !this.HighlightedBone)
                                {
                                    Vector2 childPosOtrho = Object3D.GetOrtho(middle);

                                    /* Optimisation */
                                    if (Vector2.Distance(childPosOtrho, mouseV2) < Program.MainView.graphics.PreferredBackBufferWidth / 3)
                                    {
                                        Vector3 diff = end - start;

                                        for (int h = 0; h < 101; h++)
                                        {
                                            Vector3 toDist = start + diff * (h / 100f);
                                            Vector2 toDistOrtho = Object3D.GetOrtho(toDist);
                                            if (Vector2.Distance(toDistOrtho, mouseV2) < 10)
                                            {
                                                this.HighlightedBoneParentID = currBone.ID;
                                                this.HighlightedBoneMiddle = middle;
                                                this.HighlightedBone = true;
                                                break;
                                            }
                                        }
                                    }
                                    //this.mousePos
                                }

                                if (this.HighlightedBone && Vector3.Distance(middle, this.HighlightedBoneMiddle) < 0.01 && currBone.ID == this.HighlightedBoneParentID)
                                {
                                    this.bonesTextureLines[j + 0].Color = HighligthedColor;
                                    this.bonesTextureLines[j + 1].Color = HighligthedColor;
                                    this.bonesTextureLines[j + 2].Color = HighligthedColor;
                                    this.bonesTextureLines[j + 3].Color = HighligthedColor;
                                }
                            }

                            this.bonesTextureLinesLength += 4;
                            break;
						}
					}

                    DrawLineChildren(this.Bones[i]);
				}
			}
		}
		public bool[] bonesTextureLinesUsed;
		public int bonesTextureLinesLength;
        long[][] longs;

        public void GetGraphic()
		{
            if (this.bonesTexture.Length == 0)
            {
                this.bonesTexture = new VertexPositionColorTexture[this.Bones.Length * 24];
                this.bonesTextureCircles = new VertexPositionColorTexture[this.Bones.Length * 6];
                this.bonesTextureLines = new VertexPositionColorTexture[1000];
                this.bonesTextureLinesUsed = new bool[1000];
                for (int j = 0; j < this.bonesTextureCircles.Length; j += 6)
                {
                    for (int k = 0; k < 6; k++)
                    {
                        this.bonesTextureCircles[j + k] = Object3D.CUBEVPC[k];
                        this.bonesTextureCircles[j + k].TextureCoordinate *= (191f / 382f);
                        this.bonesTextureCircles[j + k].TextureCoordinate.X += (1f / 382f);
                        this.bonesTextureCircles[j + k].TextureCoordinate.Y += (154f / 382f);
                    }
                }
                longs = new long[this.Bones.Length][];
                for (int j = 0; j < this.Bones.Length; j++)
                {
                    Vector3 bonePos = Vector3.Transform(Vector3.Zero, this.BonesReMatrices[j]);
                    VertexPositionColorTexture[] currBoneTex = GetBoneTexture(this.Bones[j].ID, this.Bones[j].IsSkinned);

                    long[] currLong = new long[]
                    { (long)(bonePos.X * 100),
                            (long)(bonePos.Y * 100),
                            (long)(bonePos.Z * 100)};

                    longs[j] = currLong;
                    for (int k = 0; k < currBoneTex.Length; k++)
                        this.bonesTexture[j * 24 + k] = currBoneTex[k];
                    currBoneTex = null;
                }
                int[] deja = new int[this.Bones.Length];

                for (int j = 0; j < longs.Length; j++)
                {
                    deja[j] = 0;
                    for (int k = 0; k < longs.Length; k++)
                    {
                        if (longs[k][0] == longs[j][0] &&
                        longs[k][1] == longs[j][1] &&
                        longs[k][2] == longs[j][2])
                            deja[j]++;
                    }
                }
                for (int j = 0; j < longs.Length; j++)
                {
                    for (int k = j + 1; k < longs.Length; k++)
                    {
                        if (longs[k][0] == longs[j][0] &&
                        longs[k][1] == longs[j][1] &&
                        longs[k][2] == longs[j][2])
                            for (int l = 0; l < 24; l++)
                                this.bonesTexture[k * 24 + l].Position.Y -= 1f;
                    }
                    for (int l = 0; l < 24; l++)
                        this.bonesTexture[j * 24 + l].Position.Y += -0.5f + deja[j] * 0.4f + deja[j] * 0.1f;
                }
            }


            


            this.bonesTextureLinesLength = 0;
            for (int i = 0; i < this.bonesTextureLinesUsed.Length; i++)
            {
                this.bonesTextureLinesUsed[i] = false;
            }
            this.HighlightedBone = false;
            this.HighlightedBoneMiddle = Vector3.Zero;
            DrawLineChildren(this.Bones[0]);
		}

		public Skeleton(Object3D objet)
		{
            this.AllowBoneSelect = true;

            this.objet = objet;
            this.bonesTexture = new VertexPositionColorTexture[0];
            this.Bones = new Bone[0];
            this.BonesMatrices = new Matrix[0];
            this.BonesReMatrices = new Matrix[0];
        }

        bool allowBoneSelect;

        public bool AllowBoneSelect
        {
            get
            {
                return this.allowBoneSelect;
            }
            set
            {
                this.allowBoneSelect = value;
                if (!this.allowBoneSelect)
                {
                    this.SelectBone(-1);
                }
            }
        }

        Microsoft.Xna.Framework.Input.MouseState oldMouseState;
        Microsoft.Xna.Framework.Input.KeyboardState oldKeyboardState;
        Microsoft.Xna.Framework.Input.MouseState mouseState;
        Microsoft.Xna.Framework.Input.KeyboardState keyboardState;

        int leftKeyCount = 0;
        int rightKeyCount = 0;

        bool cancelUnselectedBone = false;

        public void UpdateControls()
        {
            this.mouseState = Mouse.GetState(Program.MainView.Window);
            this.keyboardState = Keyboard.GetState();
            if (this.mouseState.Position!=this.oldMouseState.Position)
            {
                cancelUnselectedBone = true;
            }
            if (this.mouseState.LeftButton == ButtonState.Pressed)
            {
                if (this.oldMouseState.LeftButton == ButtonState.Released)
                {
                    if (this.HighlightedBone)
                    {
                        if (selectedBone != this.HighlightedBoneParentID)
                        {
                            SelectBone(this.HighlightedBoneParentID);
                        }
                    }
                    else
                        cancelUnselectedBone = false;
                }
            }
            else if (!cancelUnselectedBone)
            {
                SelectBone(-1);
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                if (leftKeyCount==0)
                leftKeyCount = 1;
            }
            else
            {
                leftKeyCount = 0;
            }


            if (keyboardState.IsKeyDown(Keys.Right))
            {
                if (rightKeyCount == 0)
                    rightKeyCount = 1;
            }
            else
            {
                rightKeyCount = 0;
            }


            if (leftKeyCount == 1 || leftKeyCount>20)
            {
                View.focusedObject.Skeleton.SelectBone(View.focusedObject.Skeleton.SelectedBone.ID - 1);
            }
            
            if (rightKeyCount == 1 || rightKeyCount > 20)
            {
                View.focusedObject.Skeleton.SelectBone(View.focusedObject.Skeleton.SelectedBone.ID + 1);
            }

            if (leftKeyCount > 0)
                leftKeyCount++;

            if (rightKeyCount > 0)
                rightKeyCount++;

            this.oldKeyboardState = keyboardState;
            this.oldMouseState = mouseState;
        }

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            //this.SkeletonCache.ValidateSelectionCache(gcm);
            UpdateControls();
            Vector3 position = View.mainCamera.GlobalPosition();
			double hypo = 1;
			float diffX = 0;
			float diffY = 0;
			float diffZ = 0;
			VertexPositionColorTexture[] toDraw;
            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;

            float diffWithZoom = Vector3.Distance(position, View.focusedObject.Position + View.focusedObject.MiddleCoord) / (View.focusedObject.Zoom * 1.5f);

            if (this.ShowIndices && diffWithZoom < 1)
            {
                if (this.bonesTexture.Length > 0/*&&MainViewer.form.skeletonIndCheckBox.Enabled && MainViewer.form.skeletonIndCheckBox.Checked &&
					(!MainViewer.form.swapCheckBox.Checked||objet.MeshCount==0)*/)
                {
                    toDraw = new VertexPositionColorTexture[this.bonesTexture.Length];

                    Vector3 bonePos = Vector3.Zero;
                    for (int j = 0; j < toDraw.Length; j++)
                    {
                        if (j % 24 == 0)
                        {
                            bonePos = Vector3.Transform(Vector3.Zero, this.BonesReMatrices[j / 24]);

                            diffX = position.X - (bonePos.X + objet.Position.X);
                            diffY = position.Y - (bonePos.Y + objet.Position.Y);
                            diffZ = position.Z - (bonePos.Z + objet.Position.Z);
                            hypo = Math.Pow(diffX * diffX + diffY * diffY + diffZ * diffZ, 0.5f);
                        }

                        toDraw[j] = this.bonesTexture[j];
                        //if (MainViewer.form.rotateIndCheckBox.Enabled && MainViewer.form.rotateIndCheckBox.Checked)
                        toDraw[j].Position = Vector3.Transform(toDraw[j].Position, Matrix.CreateFromYawPitchRoll(View.mainCamera.Yaw, View.mainCamera.Pitch, 0));
                        //if (!MainViewer.form.scaleIndCheckBox.Enabled || MainViewer.form.scaleIndCheckBox.Checked)
                        toDraw[j].Position = Vector3.Transform(toDraw[j].Position, Matrix.CreateScale((float)hypo / 80f));
                        //else
                        //toDraw[j].Position = Vector3.Transform(toDraw[j].Position, Matrix.CreateScale(objet.Zoom / 1000f));

                        toDraw[j].Position += bonePos;
                        toDraw[j].Position += objet.Position;

                        if (j%24==23 && this.selectedBone == j / 24)
                        {
                            for (int k = 6; k < 24; k++)
                            {
                                toDraw[j - 23 + k].Color.R = 255;
                                toDraw[j - 23 + k].Color.G = 0;
                                toDraw[j - 23 + k].Color.B = 0;
                            }
                        }

                        toDraw[j].Color.A = (byte)(255 - diffWithZoom * 255);
                    }

                    if (be != null)
                    {
                        be.TextureEnabled = true;
                        be.Texture = SrkBinary.GetT2D(View.GetCurrentDirectory() + @"\Content\boneTexture.png");
                        be.CurrentTechnique.Passes[0].Apply();
                    }
                    if (rs != null)
                    {
                        gcm.GraphicsDevice.RasterizerState = rs;
                    }

                    gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, toDraw, 0, toDraw.Length / 3);
                }
            }

			if (this.bonesTextureCircles.Length>0/*&&MainViewer.form.skeletonCheckBox.Checked*/)
			{
				toDraw = new VertexPositionColorTexture[this.bonesTextureCircles.Length];
                Vector3 bonePos = Vector3.Zero;
                for (int j = 0; j < toDraw.Length; j++)
				{
					if (j % 6 == 0)
                    {
                        bonePos = Vector3.Transform(Vector3.Zero, this.BonesReMatrices[j / 6]);

                        diffX = position.X - (bonePos.X + objet.Position.X);
						diffY = position.Y - (bonePos.Y + objet.Position.Y);
						diffZ = position.Z - (bonePos.Z + objet.Position.Z);
						hypo = Math.Pow(diffX * diffX + diffY * diffY + diffZ * diffZ, 0.5f);
					}
					toDraw[j] = this.bonesTextureCircles[j];
					toDraw[j].Position = Vector3.Transform(toDraw[j].Position, Matrix.CreateFromYawPitchRoll(View.mainCamera.Yaw, View.mainCamera.Pitch, 0));
					
					//if (!MainViewer.form.scaleIndCheckBox.Enabled||MainViewer.form.scaleIndCheckBox.Checked)
						toDraw[j].Position = Vector3.Transform(toDraw[j].Position, Matrix.CreateScale(((float)hypo / 60f)));
					//else
						//toDraw[j].Position = Vector3.Transform(toDraw[j].Position, Matrix.CreateScale((objet.Zoom / 1000f) / 1));

					toDraw[j].Position += bonePos;
                    if (this.HighlightedBone && j / 6 == this.HighlightedBoneParentID)
                    {
                        toDraw[j].TextureCoordinate.X += (192f / 382f);
                        toDraw[j].Color = HighligthedColor;
                    }
                    else
                    if (this.Bones[j / 6].Selected)
                    {
                        if (this.Bones[j / 6].ParentID < 0 || !this.Bones[this.Bones[j / 6].ParentID].Selected)
                            toDraw[j].TextureCoordinate.X += (192f / 382f);
                        toDraw[j].Color = SelectedColor;
                    }
                    else
                    {
                        toDraw[j].Color = UnselectedColor;
                    }

                    toDraw[j].Position += objet.Position;
				}

                if (be != null)
                {
                    be.TextureEnabled = true;
                    be.Texture = SrkBinary.GetT2D(View.GetCurrentDirectory() + @"\Content\boneTexture.png");
                    be.CurrentTechnique.Passes[0].Apply();
                }
                if (rs != null)
                {
                    gcm.GraphicsDevice.RasterizerState = rs;
                }

                gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, toDraw, 0, toDraw.Length / 3);
                
				VertexPositionColorTexture[] toDrawLines = new VertexPositionColorTexture[this.bonesTextureLinesLength];
				if (this.bonesTextureLinesLength>0)
                {
                    for (int i = 0; i < toDrawLines.Length; i ++)
                    {
                        toDrawLines[i] = this.bonesTextureLines[i];
                        toDrawLines[i].Position += objet.Position;

                    }
                    gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, toDrawLines, 0, toDrawLines.Length / 2);
                }
                
			}
		}

	}
}
