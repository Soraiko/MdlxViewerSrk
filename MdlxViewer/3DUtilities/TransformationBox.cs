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
    public class TransformationBox
    {
        Camembert cmbX;
        Camembert cmbY;
        Camembert cmbZ;
        Vector3 position;
        
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.cmbX.Position = value;
                this.cmbY.Position = value;
                this.cmbZ.Position = value;
                this.position = value;
            }
        }

        bool enabled;
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (!value)
                {
                    this.Position = new Vector3(1000000,1000000,100000);
                    View.Controls.MouseDragState = 0;
                }

                this.cmbX.Enabled = value;
                this.cmbY.Enabled = value;
                this.cmbZ.Enabled = value;
                this.enabled = value;
            }
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
        public void XRotChanged(object o, EventArgs e)
        {
            this.X = (float)o;
        }

        public void YRotChanged(object o, EventArgs e)
        {
            this.Y = (float)o;
        }

        public void ZRotChanged(object o, EventArgs e)
        {
            this.Z = (float)o;
        }

        public void Release(object o, EventArgs e)
        {
            View.focusedObject.Skeleton.SelectedBone.RotateX_Addition += this.X;
            View.focusedObject.Skeleton.SelectedBone.RotateY_Addition += this.Y;
            View.focusedObject.Skeleton.SelectedBone.RotateZ_Addition += this.Z;
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public TransformationBox()
        {
            cmbX = new Camembert(30);
            cmbX.Position = new Vector3(0, 0, 0);
            cmbX.ValueChanged += new EventHandler(this.XRotChanged);
            cmbX.Release += new EventHandler(this.Release);
            cmbX.Couleur = Microsoft.Xna.Framework.Color.Red;

            cmbY = new Camembert(30);
            cmbY.Position = new Vector3(0, 0, 0);
            cmbY.RotX = 1.5f;
            cmbY.ValueChanged += new EventHandler(this.YRotChanged);
            cmbY.Release += new EventHandler(this.Release);
            cmbY.Couleur = new Microsoft.Xna.Framework.Color(0,255,0,255);

            cmbZ = new Camembert(30);
            cmbZ.Position = new Vector3(0, 0, 0);
            cmbZ.RotY = 1.5f;
            cmbZ.ValueChanged += new EventHandler(this.ZRotChanged);
            cmbZ.Release += new EventHandler(this.Release);
            cmbZ.Couleur = Microsoft.Xna.Framework.Color.Blue;
            this.Enabled = false;
        }

        float rotX;
        public float RotX
        {
            get
            {
                return this.rotX;
            }
            set
            {
                this.cmbX.RotX = value;
                this.cmbY.RotX = value+1.5f;
                this.cmbZ.RotX = value;

                this.rotX = value;
            }
        }
        float rotY;
        public float RotY
        {
            get
            {
                return this.rotY;
            }
            set
            {
                this.cmbX.RotY = value;
                this.cmbY.RotY = value;
                this.cmbZ.RotY = value + 1.5f;

                this.rotY = value;
            }
        }
        float rotZ;
        public float RotZ
        {
            get
            {
                return this.rotZ;
            }
            set
            {
                this.cmbX.RotZ = value;
                this.cmbY.RotZ = value;
                this.cmbZ.RotZ = value;

                this.rotZ = value;
            }
        }

        float rayon;
        public float Rayon
        {
            get
            {
                return this.rayon;
            }
            set
            {
                this.cmbX.Rayon = value;
                this.cmbY.Rayon = value;
                this.cmbZ.Rayon = value;

                this.rayon = value;
            }
        }

        Microsoft.Xna.Framework.Input.MouseState oldMouseState;
        Microsoft.Xna.Framework.Input.KeyboardState oldKeyboardState;

        Microsoft.Xna.Framework.Input.MouseState mouseState;
        Microsoft.Xna.Framework.Input.KeyboardState keyboardState;
        int middleButtonPress = 0;

        public void UpdateMouse()
        {
            mouseState = Mouse.GetState(Program.MainView.Window);
            keyboardState = Keyboard.GetState();
            
            if (mouseState.LeftButton == ButtonState.Pressed)
            {

            }
            else if (oldMouseState.LeftButton == ButtonState.Pressed)
            {
                this.cmbX.Highlight = false;
                this.cmbY.Highlight = false;
                this.cmbZ.Highlight = false;
            }

            if (keyboardState.IsKeyDown(Keys.LeftShift))
            {
                if (middleButtonPress > 0)
                    middleButtonPress++;
                if (middleButtonPress < 0)
                    middleButtonPress--;
                
                if (mouseState.MiddleButton == ButtonState.Pressed)
                {
                    if (middleButtonPress < 0)
                    {
                        for (int i = 0; i < View.focusedObject.Skeleton.Bones.Length; i++)
                        {
                            View.focusedObject.Skeleton.Bones[i].RotateX_Addition = 0;
                            View.focusedObject.Skeleton.Bones[i].RotateY_Addition = 0;
                            View.focusedObject.Skeleton.Bones[i].RotateZ_Addition = 0;
                        }
                        middleButtonPress = 0;
                    }

                    if (middleButtonPress==0)
                    middleButtonPress = 1;
                }
                else
                {
                    if (middleButtonPress>0 && middleButtonPress < 8)
                        middleButtonPress = -1;
                    if (middleButtonPress>0 || middleButtonPress < -8)
                        middleButtonPress = 0;
                }
                if (middleButtonPress > 0)
                {
                    View.focusedObject.Skeleton.SelectedBone.RotateX_Addition = 0;
                    View.focusedObject.Skeleton.SelectedBone.RotateY_Addition = 0;
                    View.focusedObject.Skeleton.SelectedBone.RotateZ_Addition = 0;
                }
                
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    int xDiff = (mouseState.Position.X - oldMouseState.X);
                    int yDiff = (mouseState.Position.Y - oldMouseState.Y);
                    this.cmbX.Highlight = xDiff != 0;
                    this.cmbY.Highlight = yDiff != 0;
                    
                    View.focusedObject.Skeleton.SelectedBone.RotateX_Addition += xDiff / 100f;
                    View.focusedObject.Skeleton.SelectedBone.RotateY_Addition += yDiff / 100f;
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    int zDiff = (mouseState.Position.Y - oldMouseState.Y);
                    this.cmbZ.Highlight = zDiff != 0;
                    View.focusedObject.Skeleton.SelectedBone.RotateZ_Addition += zDiff / 100f;
                }
            }
            else if (oldKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                this.cmbX.Highlight = false;
                this.cmbY.Highlight = false;
                this.cmbZ.Highlight = false;
            }
            oldKeyboardState = keyboardState;
            oldMouseState = mouseState;
        }

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            Vector3 global = View.mainCamera.GlobalPosition();
            float dist = Vector3.Distance(this.position, global);
            this.Rayon = (dist / 10f);

            cmbX.Draw(gcm, be, rs);
            cmbY.Draw(gcm, be, rs);
            cmbZ.Draw(gcm, be, rs);
        }
    }
}
