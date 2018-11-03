using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MdlxViewer
{
    public class Object3D : IDisposable
    {
        public Skeleton Skeleton;
        public Vector3 Position = new Vector3(0,0,0);
        public Vector3 MinCoords;
        public Vector3 MaxCoords;
        public string FileName;

        public static void MesurePrincipale(ref float angle)
        {
            while (angle < -Math.PI)
                angle += (float)(2 * Math.PI);

            while (angle > Math.PI)
                angle -= (float)(2 * Math.PI);
        }

        public static void MesurePositive(ref float angle)
        {
            while (angle < 0)
                angle += (float)(2 * Math.PI);

            while (angle > 2 * Math.PI)
                angle -= (float)(2 * Math.PI);
        }

        public static Vector2 GetOrtho(Vector3 v3)
        {
            Vector2 output = Vector2.Zero;
            IEffectMatrices eff = View.basicEffect;
            Matrix worldViewProjection = eff.World * eff.View * eff.Projection;
            Vector4 result = Vector4.Transform(v3, worldViewProjection);
            result /= result.W;
            Viewport vp = Program.MainView.graphics.GraphicsDevice.Viewport;
            Matrix invClient = Matrix.Invert(Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, -1, 1));
            output = Vector2.Transform(new Vector2(result.X, result.Y), invClient);
            return output;
        }

        Vector3 middle;
        public Vector3 MiddleCoord
        {
            get
            {
                this.middle.X = (this.MinCoords.X + this.MaxCoords.X) / 2f;
                this.middle.Y = (this.MinCoords.Y + this.MaxCoords.Y) / 2f;
                this.middle.Z = (this.MinCoords.Z + this.MaxCoords.Z) / 2f;
                return this.middle;
            }
        }
        

        public float Width
        {
            get
            {
                return (this.MaxCoords.X - this.MinCoords.X);
            }
        }

        public float Height
        {
            get
            {
                return (this.MaxCoords.Y - this.MinCoords.Y);
            }
        }

        public float Zoom
        {
            get
            {
                float zoom = this.Width;
                if (this.Height > zoom)
                    zoom = this.Height;
                return zoom;
            }
        }

        public float Perimetre
        {
            get
            {
                float peri = (this.MaxCoords.X - this.MinCoords.X) +
                    (this.MaxCoords.Y - this.MinCoords.Y) +
                    (this.MaxCoords.Z - this.MinCoords.Z);
                if (peri == 0)
                    peri = 100;
                return peri;
            }
        }

        public static List<Object3D> objectList = new List<Object3D>(0);
        public static List<string> objectListCDs = new List<string>(0);

        public string CreationDate = "";

        public Object3D()
        {
            this.CreationDate = DateTime.Now.ToString() + DateTime.Now.Millisecond.ToString();
            this.middle = Vector3.Zero;
            this.MinCoords = Vector3.Zero;
            this.MaxCoords = Vector3.Zero;
            if (!objectListCDs.Contains(this.CreationDate))
            {
                objectListCDs.Add(this.CreationDate);
                objectList.Add(this);
            }
        }

        public void Draw(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            if (this is DAE)
                (this as DAE).Draw(gcm, be, rs);
            if (this is OBJ)
                (this as OBJ).Draw(gcm, be, rs);
            if (this is MDLX)
                (this as MDLX).Draw(gcm, be, rs);
        }

        public static VertexPositionColor[] Grid;

        public static readonly VertexPositionColorTexture[] CUBEVPC = new VertexPositionColorTexture[]
        {
            new VertexPositionColorTexture(new Vector3(-0.5f,0.5f,0f), Color.White,new Vector2(1,0)),
            new VertexPositionColorTexture(new Vector3(0.5f,-0.5f,0f), Color.White,new Vector2(0,1)),
            new VertexPositionColorTexture(new Vector3(-0.5f,-0.5f,0f), Color.White,new Vector2(0,0)),

            new VertexPositionColorTexture(new Vector3(0.5f,-0.5f,0f), Color.White,new Vector2(0,1)),
            new VertexPositionColorTexture(new Vector3(-0.5f,0.5f,0f), Color.White,new Vector2(1,0)),
            new VertexPositionColorTexture(new Vector3(0.5f,0.5f,0f), Color.White,new Vector2(1,1))
        };

        public static void Draw3DGrid(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
        {
            if (Grid == null)
            {
                int nbLignes = 24;
                float ecart = 232f;
                float step = ecart / (float)(nbLignes * 2);
                if (Grid == null)
                {
                    Grid = new VertexPositionColor[nbLignes * 4 + 4];
                    Grid[0].Color = Color.White;
                    for (int i = 0; i < Grid.Length; i++)
                        Grid[i].Color = Grid[0].Color;
                }

                for (int i = 0; i < Grid.Length; i += 4)
                {
                    Grid[i].Position.X = -ecart + i * step;
                    Grid[i].Position.Y = 0;
                    Grid[i].Position.Z = -ecart;

                    Grid[i + 1].Position.X = -ecart + i * step;
                    Grid[i + 1].Position.Y = 0;
                    Grid[i + 1].Position.Z = ecart;

                    Grid[i + 2].Position.X = -ecart;
                    Grid[i + 2].Position.Y = 0;
                    Grid[i + 2].Position.Z = -ecart + i * step;

                    Grid[i + 3].Position.X = ecart;
                    Grid[i + 3].Position.Y = 0;
                    Grid[i + 3].Position.Z = -ecart + i * step;
                    
                    if (i / 4 == nbLignes / 2)
                    {
                        Grid[i].Color.G = 100;
                        Grid[i].Color.B = 100;
                        Grid[i + 1].Color.G = 100;
                        Grid[i + 1].Color.B = 100;
                        Grid[i + 2].Color.G = 100;
                        Grid[i + 2].Color.B = 100;
                        Grid[i + 3].Color.G = 100;
                        Grid[i + 3].Color.B = 100;
                    }
                }
            }
            be.TextureEnabled = false;
            be.CurrentTechnique.Passes[0].Apply();
            gcm.GraphicsDevice.RasterizerState = rs;
            gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, Grid, 0, Grid.Length / 2);
        }

        public void Dispose()
        {
            int index = objectListCDs.IndexOf(this.CreationDate);
            objectListCDs.RemoveAt(index);
            objectList.RemoveAt(index);

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(2000);
                this.Dispose();
            }).Start();
        }
        public static Object3D emptyObject;
    }
}
