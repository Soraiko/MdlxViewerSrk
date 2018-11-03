using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BAR_Editor;
using System;
using System.IO;
using System.Collections.Generic;

namespace MdlxViewer
{
    public partial class View
    {
        public static class Controls
        {
            public static bool ShowLookAt = false;
            public static int MouseDragState = 0;

            public static Microsoft.Xna.Framework.Input.MouseState mouseState;
            public static Microsoft.Xna.Framework.Input.MouseState oldMouseState;

            public static KeyboardState keyboardState;
            public static KeyboardState oldKeyboardState;

            public static void MouseControls(GameWindow win)
            {
                mouseState = Mouse.GetState(win);
                keyboardState = Keyboard.GetState();


                if (keyboardState.IsKeyDown(Keys.S) && !oldKeyboardState.IsKeyDown(Keys.S))
                {
                    mainCamera.Animated = !mainCamera.Animated;
                    if (mainCamera.Animated)
                    {
                        mainCamera.RotationX = 0;
                        mainCamera.RotationY = 0;
                    }
                }
                if (keyboardState.IsKeyDown(Keys.T) && !oldKeyboardState.IsKeyDown(Keys.T))
                {
                    View.AllowTextures = !View.AllowTextures;
                }
                if (keyboardState.IsKeyDown(Keys.I) && !oldKeyboardState.IsKeyDown(Keys.I))
                {
                    focusedObject.Skeleton.ShowIndices = !focusedObject.Skeleton.ShowIndices;
                }
                if (keyboardState.IsKeyDown(Keys.Enter) && !oldKeyboardState.IsKeyDown(Keys.Enter))
                {
                    if (focusedObject is MDLX)
                    {
                        MDLX mdl = focusedObject as MDLX;
                        if (!Directory.Exists(View.GetCurrentDirectory() + @"\" + Path.GetFileNameWithoutExtension(mdl.FileName).ToUpper() + "-export"))
                            Directory.CreateDirectory(View.GetCurrentDirectory() + @"\" + Path.GetFileNameWithoutExtension(mdl.FileName).ToUpper() + "-export");
                        mdl.ExportDAE(View.GetCurrentDirectory() + @"\" + Path.GetFileNameWithoutExtension(mdl.FileName).ToUpper() + @"-export\" + Path.GetFileNameWithoutExtension(mdl.FileName).ToUpper() + ".dae");
                    }
                }

                int wheelVal = mouseState.ScrollWheelValue;
                int oldWheelVal = oldMouseState.ScrollWheelValue;
                

                bool allowCameraRotation = MouseDragState == 0 && !View.focusedObject.Skeleton.HighlightedBone && keyboardState.IsKeyUp(Keys.LeftShift);
                bool allowCameraTranslation = MouseDragState == 0 && keyboardState.IsKeyUp(Keys.LeftShift);
                bool allowCameraZoom = MouseDragState == 0 && keyboardState.IsKeyUp(Keys.LeftShift);

                if (allowCameraZoom)
                {
                    float step = focusedObject.Zoom / 10f;

                    if (wheelVal < oldWheelVal)
                    {
                        mainCamera.ZoomTarget += step;
                    }
                    else if (wheelVal > oldWheelVal)
                    {
                        mainCamera.ZoomTarget -= step;
                    }
                    if (mouseState.MiddleButton == ButtonState.Pressed)
                    {
                        ResetCamToTarget();
                    }
                }
                
                ShowLookAt = false;
                float factor = mainCamera.Animated ? 0.3333f : 1;


                float diffY = oldMouseState.Position.Y - mouseState.Position.Y;
                float diffX = oldMouseState.Position.X - mouseState.Position.X;

                if (allowCameraTranslation)
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    ShowLookAt = true;
                    float dist = (focusedObject.Perimetre) / 500f;

                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        if (diffY != 0)
                            mainCamera.TranslationZ += diffY * factor * dist;
                        //mainCamera.MoveCameraForward((oldMouseState.Position.Y - mouseState.Position.Y) * dist * mainCamera.RollStep);
                    }
                    else
                    {
                        if (diffY != 0)
                            mainCamera.TranslationY += -diffY * factor * dist;
                        if (diffX != 0)
                            mainCamera.TranslationX += diffX * factor * dist * mainCamera.RollStep;

                        //mainCamera.MoveCameraUp((mouseState.Position.Y - oldMouseState.Position.Y) * dist);
                        //mainCamera.MoveCameraRight((oldMouseState.Position.X - mouseState.Position.X) * dist * mainCamera.RollStep);
                    }
                }

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (oldMouseState.LeftButton == ButtonState.Released)
                        cameraTurnPress = allowCameraRotation;
                }
                else
                    cameraTurnPress = false;


                if (cameraTurnPress)
                {
                    ShowLookAt = true;
                    /*mainCamera.Pitch += MathHelper.ToRadians(oldMouseState.Position.Y - mouseState.Position.Y);
                    mainCamera.Yaw += MathHelper.ToRadians(oldMouseState.Position.X - mouseState.Position.X) * mainCamera.RollStep;*/
                    if (diffY!=0)
                    mainCamera.RotationY += factor * MathHelper.ToRadians(diffY);

                    if (diffX!=0)
                    mainCamera.RotationX += factor * MathHelper.ToRadians(diffX) * mainCamera.RollStep;
                }
                
                oldMouseState = mouseState;
                oldKeyboardState = keyboardState;
            }

            public static bool cameraTurnPress = false;

            public static VertexPositionColor[] LookAt = new VertexPositionColor[12];

            private static byte lookAtOpacity = 0;
            private static bool firstLookAt = true;

            public static void DrawLookAt(GraphicsDeviceManager gcm, BasicEffect be, RasterizerState rs)
            {
                if (ShowLookAt && lookAtOpacity < 255)
                    lookAtOpacity+=15;
                if (!ShowLookAt && lookAtOpacity > 0)
                    lookAtOpacity-=15;

                if (lookAtOpacity==0 || mainCamera.Zoom < 1.1)
                    return;

                LookAt[0].Position = mainCamera.LookAt;
                for (int i=0;i<12;i++)
                {
                    LookAt[i].Position = LookAt[0].Position;
                }

                LookAt[0].Position.X -= 1000;
                LookAt[3].Position.X += 1000;

                LookAt[4].Position.Y -= 1000;
                LookAt[7].Position.Y += 1000;

                LookAt[8].Position.Z -= 1000;
                LookAt[11].Position.Z += 1000;

                if (firstLookAt)
                {
                    LookAt[0].Color = Color.Red;
                    for (int i=1;i<12;i++)
                    LookAt[i].Color = LookAt[0].Color;
                    LookAt[0].Color.A = 0;
                    LookAt[3].Color.A = 0;
                    LookAt[4].Color.A = 0;
                    LookAt[7].Color.A = 0;
                    LookAt[8].Color.A = 0;
                    LookAt[11].Color.A = 0;

                    firstLookAt = false;
                }

                LookAt[1].Color.A = lookAtOpacity;
                LookAt[2].Color.A = lookAtOpacity;
                LookAt[5].Color.A = lookAtOpacity;
                LookAt[6].Color.A = lookAtOpacity;
                LookAt[9].Color.A = lookAtOpacity;
                LookAt[10].Color.A = lookAtOpacity;

                be.TextureEnabled = false;
                be.CurrentTechnique.Passes[0].Apply();
                gcm.GraphicsDevice.RasterizerState = rs;
                gcm.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                gcm.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, LookAt, 0, LookAt.Length / 2);
            }

            public static void DefaultCamera()
            {
                mainCamera = new ArcBallCamera(16f / 9f, MathHelper.ToRadians(60f),
                Vector3.Zero, Microsoft.Xna.Framework.Vector3.Up, 1, Single.MaxValue);
                mainCamera.PitchBounds = false;
                mainCamera.Zoom = 1000f;
                mainCamera.ZoomTarget = mainCamera.Zoom;
                mainCamera.Yaw = MathHelper.ToRadians(45);
                mainCamera.Pitch = MathHelper.ToRadians(-33);
                mainCamera.RotationX = mainCamera.Yaw;
                mainCamera.RotationY = mainCamera.Pitch;
                //mainCamera.Animated = false;
            }

            public static void ResetCamToTarget()
            {
                if (Object3D.objectListCDs.Count>0 && focusedObject.CreationDate.ToString() == Object3D.objectListCDs[0])
                {
                    DefaultCamera();
                    return;
                }
                mainCamera.Pitch = 0;
                mainCamera.Yaw = 0;

                mainCamera.RotationX = 0;
                mainCamera.RotationY = 0;

                mainCamera.TranslationX = 0;
                mainCamera.TranslationY = 0;
                mainCamera.TranslationZ = 0;

                mainCamera.LookAt = focusedObject.Position + focusedObject.MiddleCoord;
                mainCamera.Zoom = focusedObject.Zoom * 1.5f;
                mainCamera.ZoomTarget = mainCamera.Zoom;
            }
        }
    }
}
