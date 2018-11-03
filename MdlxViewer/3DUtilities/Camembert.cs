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
    public class Camembert
    {
        Vector3 position;

        public bool Enabled
        {
            get;set;
        }

        public Vector3 Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (Vector3.Distance(value, this.position)>0.01)
                {
                    this.position = value;
                    UpdateRayon();
                }
            }
        }

        VertexPositionColor[] vpc;

        public EventHandler ValueChanged;
        public EventHandler Release;

        
        Microsoft.Xna.Framework.Color couleur;

        public Microsoft.Xna.Framework.Color Couleur
        {
            get
            {
                return this.couleur;
            }
            set
            {
                this.couleur = value;
                this.contour = new Microsoft.Xna.Framework.Color(this.couleur.R, this.couleur.G, this.couleur.B, (byte)255);
                this.interieur = new Microsoft.Xna.Framework.Color(this.couleur.R, this.couleur.G, this.couleur.B, (byte)130);
                UpdateRayon();
            }
        }

        Microsoft.Xna.Framework.Color contour;
        Microsoft.Xna.Framework.Color interieur;

        

        Microsoft.Xna.Framework.Input.MouseState oldMouseState;
        Microsoft.Xna.Framework.Input.KeyboardState oldKeyboardState;

        Microsoft.Xna.Framework.Input.MouseState mouseState;
        Microsoft.Xna.Framework.Input.KeyboardState keyboardState;

        void UpdateMouse()
        {
            this.mouseState = Mouse.GetState(Program.MainView.Window);
            this.keyboardState = Keyboard.GetState();

            if (View.Controls.keyboardState.IsKeyDown(Keys.LeftShift))
            {
                return;
            }
            
            
            if (View.Controls.MouseDragState == 0)
                dragInstance = -1;

            if (dragInstance>-1)
            {
                if (this.ID != dragInstance)
                {
                    this.Highlight = false;
                    return;
                }
            }

            Vector2 centre = Object3D.GetOrtho(this.position);

            double xDiff = mouseState.X - centre.X;
            double yDiff = mouseState.Y - centre.Y;

            
            Vector2 angle0 = Object3D.GetOrtho(this.position+ Vector3.Transform(new Vector3(this.rayon,0,0),this.Matrice));
            Vector2 anglePIsur2 = Object3D.GetOrtho(this.position+ Vector3.Transform(new Vector3(0, this.rayon, 0),this.Matrice));
            
            if (angle0.X < centre.X)
                xDiff = -xDiff;
            if (anglePIsur2.Y < centre.Y)
                yDiff = -yDiff;

            double hypo = Math.Pow(xDiff * xDiff + yDiff * yDiff, 0.5);
            float angleMouse = (float)Math.Atan2(yDiff/hypo,xDiff/hypo);
            Object3D.MesurePositive(ref angleMouse);


            double smallest = Single.MaxValue;
            double realAngleMouse = 0;
            int smallestI = 0;

            for (int i=0;i<100;i++)
            {
                double angle = (i / 100d) * Math.PI * 2;
                Vector2 posOrtho = Object3D.GetOrtho(this.position + Vector3.Transform(new Vector3((float)(this.rayon * Math.Cos(angle)),
                    (float)(this.rayon * Math.Sin(angle)), 0), this.Matrice));

                double xDiffSeek = posOrtho.X - centre.X;
                double yDiffSeek = posOrtho.Y - centre.Y;

                if (angle0.X < centre.X)
                    xDiffSeek = -xDiffSeek;
                if (anglePIsur2.Y < centre.Y)
                    yDiffSeek = -yDiffSeek;
                double hypoSeek = Math.Pow(xDiffSeek * xDiffSeek + yDiffSeek * yDiffSeek, 0.5);
                
                float angleOrtho = (float)Math.Atan2(yDiffSeek / hypoSeek, xDiffSeek / hypoSeek);
                Object3D.MesurePositive(ref angleOrtho);

                double diff = Math.Abs(angleOrtho - angleMouse);

                if (diff < smallest)
                {
                    realAngleMouse = angle;
                    smallest = diff;
                    smallestI = i;
                }
            }

            Vector2 posRayonOrthoAtMouseAngle = Object3D.GetOrtho(this.position + Vector3.Transform(new Vector3((float)(this.rayon * Math.Cos(realAngleMouse)),
                    (float)(this.rayon * Math.Sin(realAngleMouse)), 0), this.Matrice));

            xDiff = posRayonOrthoAtMouseAngle.X - centre.X;
            yDiff = posRayonOrthoAtMouseAngle.Y - centre.Y;

            if (angle0.X < centre.X)
                xDiff = -xDiff;
            if (anglePIsur2.Y < centre.Y)
                yDiff = -yDiff;

            double hypo2 = Math.Pow(xDiff * xDiff + yDiff * yDiff, 0.5);
            bool highlight = View.Controls.keyboardState.IsKeyUp(Keys.LeftShift) && Math.Abs(hypo - hypo2) <= hypo2 * 0.1f;
            

            if (highlight)
            {
                dragInstance = this.ID;
            }

            this.Highlight = View.Controls.MouseDragState > 0;

            if (View.Controls.mouseState.LeftButton == ButtonState.Released && View.Controls.MouseDragState == 2)
            {
                this.Release?.Invoke(null, null);
                this.endAngle = 0;
                this.startAngle = 0;
                View.Controls.MouseDragState = 0;
            }

            if (highlight)
            {
                if (View.Controls.MouseDragState == 0 && View.Controls.mouseState.LeftButton == ButtonState.Released)
                    View.Controls.MouseDragState = 1;

                if (View.Controls.mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (View.Controls.MouseDragState == 1)
                    {
                        this.endAngle = (float)realAngleMouse;
                        this.startAngle = (float)realAngleMouse;
                        View.Controls.MouseDragState = 2;
                    }
                }
                else
                {
                    View.Controls.MouseDragState = 1;
                }
            }
            else
            {
                if (View.Controls.MouseDragState == 1)
                    View.Controls.MouseDragState = 0;
            }
            
            if (View.Controls.MouseDragState == 2)
            {
                this.endAngle = (float)realAngleMouse;
                float start = this.startAngle;
                float end = this.endAngle;

                float val = end- start;
                this.ValueChanged?.Invoke(val, null);
            }
            oldKeyboardState = keyboardState;
            oldMouseState = mouseState;
        }

        public Camembert() : this(100)
        {

        }

        static int dragInstance = -1;
        static int lastInstance = -1;

        public int ID;

        public Camembert(int ray)
        {
            this.position = Vector3.Zero;
            vpc = new VertexPositionColor[999];
            for (int i = 0; i < vpc.Length; i++)
                vpc[i] = new VertexPositionColor();

            this.Matrice = Matrix.CreateFromYawPitchRoll(0, 0, 0);
            this.startAngle = 0;
            this.endAngle = 0;
            this.Couleur = Microsoft.Xna.Framework.Color.White;
            this.Rayon = ray;
            lastInstance++;
            this.ID = lastInstance;
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
                if (Math.Abs(this.rayon - value) > 0.0001)
                {
                    this.rayon = value;
                    UpdateRayon();
                }
            }
        }
        float rotX;
        float rotY;
        float rotZ;

        public float RotX
        {
            get
            {
                return this.rotX;
            }
            set
            {
                this.rotX = value;
                this.Matrice = Matrix.CreateFromYawPitchRoll(this.rotX, this.rotY, this.rotZ);
            }
        }
        public float RotY
        {
            get
            {
                return this.rotY;
            }
            set
            {
                this.rotY = value;
                this.Matrice = Matrix.CreateFromYawPitchRoll(this.rotX, this.rotY, this.rotZ);
            }
        }
        public float RotZ
        {
            get
            {
                return this.rotZ;
            }
            set
            {
                this.rotZ = value;
                this.Matrice = Matrix.CreateFromYawPitchRoll(this.rotX, this.rotY, this.rotZ);
            }
        }


        Matrix Matrice;

        float startAngle;
        float endAngle;

        public float StartAngle
        {
            get
            {
                return this.startAngle;
            }
            set
            {
                float newVal = value;
                Object3D.MesurePositive(ref newVal);

                if (Math.Abs(newVal - this.startAngle)>0.0001)
                {
                    this.startAngle = value;
                    UpdateRayon();
                }
            }
        }

        public float EndAngle
        {
            get
            {
                return this.endAngle;
            }
            set
            {
                float newVal = value;
                Object3D.MesurePositive(ref newVal);

                if (Math.Abs(newVal - this.endAngle) > 0.0001)
                {
                    this.endAngle = value;
                    UpdateRayon();
                }
            }
        }

        public void UpdateRayon()
        {
            float start = this.startAngle;
            float end = this.endAngle;

            List<float[]> regions = new List<float[]>(0);

            if (start <= end)
            {
                if (end > start + Math.PI)
                {
                    regions.Add(new float[] { end, (float)(Math.PI * 2) });
                    regions.Add(new float[] { 0, start });
                }
                else
                {
                    regions.Add(new float[] { start, end });
                }
            }
            else
            {
                if (end + Math.PI > start )
                {
                    regions.Add(new float[] { end, start });
                }
                else
                {
                    regions.Add(new float[] { 0 ,end});
                    regions.Add(new float[] { start , (float)(Math.PI * 2) });
                }
            }

            for (int i = 0; i < 100; i++)
            {
                double angleA = (i / 100f) * Math.PI * 2;
                double angleB = ((i + 1) / 100f) * Math.PI * 2;

                double middle = angleA + ((angleB - angleA) / 2);

                bool condition = false;
                
                for (int j=0;j< regions.Count;j++)
                {
                    if (middle >= regions[j][0]&& middle <= regions[j][1])
                    {
                        condition = true;
                        break;
                    }
                }

                if (condition)
                {
                    vpc[i * 3 + 0].Color = this.interieur;
                    vpc[i * 3 + 1].Color = this.interieur;
                    vpc[i * 3 + 2].Color = this.interieur;
                }
                else
                {
                    vpc[i * 3 + 0].Color = Microsoft.Xna.Framework.Color.Transparent;
                    vpc[i * 3 + 1].Color = Microsoft.Xna.Framework.Color.Transparent;
                    vpc[i * 3 + 2].Color = Microsoft.Xna.Framework.Color.Transparent;
                }

                vpc[i * 3 + 0].Position.X = 0;
                vpc[i * 3 + 0].Position.Y = 0;
                vpc[i * 3 + 0].Position.Z = 0;

                vpc[i * 3 + 1].Position.X = (float)(this.rayon * Math.Cos(angleA));
                vpc[i * 3 + 1].Position.Y = (float)(this.rayon * Math.Sin(angleA));
                vpc[i * 3 + 1].Position.Z = 0;

                vpc[i * 3 + 2].Position.X = (float)(this.rayon * Math.Cos(angleB));
                vpc[i * 3 + 2].Position.Y = (float)(this.rayon * Math.Sin(angleB));
                vpc[i * 3 + 2].Position.Z = 0;

                vpc[i * 3 + 0].Position = Vector3.Transform(vpc[i * 3 + 0].Position, this.Matrice);
                vpc[i * 3 + 1].Position = Vector3.Transform(vpc[i * 3 + 1].Position, this.Matrice);
                vpc[i * 3 + 2].Position = Vector3.Transform(vpc[i * 3 + 2].Position, this.Matrice);

                vpc[i * 3 + 0].Position += this.position;
                vpc[i * 3 + 1].Position += this.position;
                vpc[i * 3 + 2].Position += this.position;
            }

            Matrix camMat = Matrix.Identity;
            Vector3 camPos = Vector3.Zero;

            if (View.mainCamera != null)
            {
                camMat = View.mainCamera.GetMatrix();
                camPos = View.mainCamera.GlobalPosition();
            }


            for (int i = 0; i < 100; i++)
            {
                double angleA = (i / 100f) * Math.PI * 2;
                double angleB = ((i + 1) / 100f) * Math.PI * 2;
                
                Vector3 positionA = new Vector3((float)(this.rayon * Math.Cos(angleA)), (float)(this.rayon * Math.Sin(angleA)), 0);
                Vector3 positionB = new Vector3((float)(this.rayon * Math.Cos(angleB)), (float)(this.rayon * Math.Sin(angleB)), 0);
                /*float distA = Vector3.Distance(camPos, positionA) /100f;
                float distB = Vector3.Distance(camPos, positionB)/100f;*/

                var color = this.highlight ? Microsoft.Xna.Framework.Color.Yellow : this.contour;
                



                vpc[300 + i * 6 + 0].Position = positionA * 0.97f;
                vpc[300 + i * 6 + 0].Color = color;

                vpc[300 + i * 6 + 1].Position = positionA * 1.03f;
                vpc[300 + i * 6 + 1].Color = color;

                vpc[300 + i * 6 + 2].Position = positionB * 0.97f;
                vpc[300 + i * 6 + 2].Color = color;

                vpc[300 + i * 6 + 3].Position = positionB * 0.97f;
                vpc[300 + i * 6 + 3].Color = color;

                vpc[300 + i * 6 + 4].Position = positionA * 1.03f;
                vpc[300 + i * 6 + 4].Color = color;

                vpc[300 + i * 6 + 5].Position = positionB * 1.03f;
                vpc[300 + i * 6 + 5].Color = color;


                vpc[300 + i * 6 + 0].Position = Vector3.Transform(vpc[300 + i * 6 + 0].Position, this.Matrice);
                vpc[300 + i * 6 + 1].Position = Vector3.Transform(vpc[300 + i * 6 + 1].Position, this.Matrice);
                vpc[300 + i * 6 + 2].Position = Vector3.Transform(vpc[300 + i * 6 + 2].Position, this.Matrice);
                vpc[300 + i * 6 + 3].Position = Vector3.Transform(vpc[300 + i * 6 + 3].Position, this.Matrice);
                vpc[300 + i * 6 + 4].Position = Vector3.Transform(vpc[300 + i * 6 + 4].Position, this.Matrice);
                vpc[300 + i * 6 + 5].Position = Vector3.Transform(vpc[300 + i * 6 + 5].Position, this.Matrice);
                
                vpc[300 + i * 6 + 0].Position += this.position;
                vpc[300 + i * 6 + 1].Position += this.position;
                vpc[300 + i * 6 + 2].Position += this.position;
                vpc[300 + i * 6 + 3].Position += this.position;
                vpc[300 + i * 6 + 4].Position += this.position;
                vpc[300 + i * 6 + 5].Position += this.position;
            }
        }

        public bool highlight;

        public bool Highlight
        {
            get
            {
                return this.highlight;
            }
            set
            {
                if (value!= this.highlight)
                {
                    this.highlight = value;
                    var color = value ? Microsoft.Xna.Framework.Color.Yellow : this.contour;
                    for (int i = 0; i < 200; i++)
                        vpc[300 + i].Color = color;
                }
            }
        }

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            if (!this.Enabled)
            {
                return;
            }
            UpdateMouse();
            UpdateRayon();
            be.TextureEnabled = false;
            be.VertexColorEnabled = true;
            be.CurrentTechnique.Passes[0].Apply();
            gcm.GraphicsDevice.RasterizerState = rs;
            gcm.GraphicsDevice.DepthStencilState = DepthStencilState.None;
            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, this.vpc, 0, 300);
        }
    }
}
