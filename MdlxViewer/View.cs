using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BAR_Editor;
using System;
using System.IO;
using System.Collections.Generic;

namespace MdlxViewer
{
    public partial class View : Game
    {
        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public View()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(SetToReference);
            Content.RootDirectory = "Content";
        }

        void SetToReference(object sender, PreparingDeviceSettingsEventArgs eventargs)
        {
            eventargs.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
        }

        protected override void Initialize()
        {
            base.Initialize();
            InitializeGraphics();
        }
        public static TransformationBox transformationBox;

        void InitializeGraphics()
        {
            graphics.PreferMultiSampling = true;
            graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.Projection = Microsoft.Xna.Framework.Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60f), GraphicsDevice.Viewport.AspectRatio, 1f, 1000);
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = true;

            rasterSolid = new RasterizerState();
            rasterSolid.FillMode = FillMode.Solid;
            rasterSolid.MultiSampleAntiAlias = true;

            rasterSolidNoCull = new RasterizerState();
            rasterSolidNoCull.FillMode = FillMode.Solid;
            rasterSolidNoCull.MultiSampleAntiAlias = true;
            rasterSolidNoCull.CullMode = CullMode.None;

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
        }

        public void OnResize(Object sender, EventArgs e)
        {
            mainCamera.AspectRatio = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;
        }

        public static string GetCurrentDirectory()
        {
            return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Object3D.emptyObject = new Object3D();
            Object3D.emptyObject.MinCoords = new Vector3(-500,-500,-500);
            Object3D.emptyObject.MaxCoords = new Vector3(500,500,500);
            Object3D.emptyObject.Skeleton = new Skeleton(Object3D.emptyObject);
            focusedObject = Object3D.emptyObject;

            SrkBinary.InitEmptyData();

            backgroundColor = new Color(30, 30, 30);
            transformationBox = new TransformationBox();

            for (int i=0;i< Program.args_.Length;i++)
            {
                string fname = Program.args_[i].ToLower();
                if (Path.GetExtension(fname) == ".dae")
                {
                    DAE dae = new DAE(fname);
                    dae.Parse();
                    dae.Skeleton.AllowBoneSelect = true;
                    dae.Skeleton.ShowIndices = false;
                    focusedObject = dae;
                }
                if (Path.GetExtension(fname) == ".obj")
                {
                    OBJ obj = new OBJ(fname);
                    obj.Parse();
                    focusedObject = obj;
                }
                if (Path.GetExtension(fname) == ".mdlx")
                {
                    MDLX mdl = new MDLX(fname);
                    mdl.Parse();
                    mdl.Skeleton.AllowBoneSelect = true;
                    mdl.Skeleton.ShowIndices = false;
                    focusedObject = mdl;
                }
                focusedObject.FileName = fname;
            }
            Controls.DefaultCamera();
            Controls.ResetCamToTarget();
        }


        public static Object3D focusedObject;
        public static ArcBallCamera mainCamera;

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            mainCamera.Animate();
            Controls.MouseControls(Window);
            transformationBox.UpdateMouse();

            base.Update(gameTime);
        }

        public static BasicEffect basicEffect;
        public static RasterizerState rasterSolid;
        public static RasterizerState rasterSolidNoCull;
        public static Microsoft.Xna.Framework.Color backgroundColor;
        public static bool AllowTextures = true;

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(backgroundColor);

            basicEffect.View = mainCamera.ViewMatrix;
            basicEffect.Projection = mainCamera.ProjectionMatrix;
            
            Object3D.Draw3DGrid(graphics, basicEffect, rasterSolidNoCull);
            Controls.DrawLookAt(graphics, basicEffect, rasterSolidNoCull);

            foreach (Object3D obj in Object3D.objectList)
                obj.Draw(graphics, basicEffect, rasterSolid);

            transformationBox.Draw(graphics, basicEffect, rasterSolidNoCull);
            
            base.Draw(gameTime);
        }
    }
}
