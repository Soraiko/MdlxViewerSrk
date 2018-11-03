using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MdlxViewer
{
    public class ArcBallCamera
    {
        public ArcBallCamera(float aspectRation, Vector3 lookAt)
            : this(aspectRation, MathHelper.PiOver4, lookAt, Vector3.Up, 0.1f, float.MaxValue) { }

        public ArcBallCamera(float aspectRatio, float fieldOfView, Vector3 lookAt, Vector3 up, float nearPlane, float farPlane)
        {
            this.Animated = false;
            this.aspectRatio = aspectRatio;
            this.fieldOfView = fieldOfView;
            this.lookAt = lookAt;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;
			this.ReCreateProjectionMatrix();
		}
		
        private void ReCreateViewMatrix()
        {
            position = Vector3.Transform(Vector3.Backward, Matrix.CreateFromYawPitchRoll(yaw, pitch, 0));
            position += lookAt;
			
			viewMatrix = Matrix.CreateLookAt(position, lookAt, Vector3.Up);

			viewMatrixDirty = false;
        }

		public float RollStep = 1;

		private void ReCreateProjectionMatrix()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(this.fieldOfView, AspectRatio, nearPlane, farPlane);
			this.RollStep = 1;
			if (Math.Abs(this.pitch) > (Math.PI / 2))
			{
				this.RollStep = -1;
				projectionMatrix = projectionMatrix * Matrix.CreateRotationZ((float)Math.PI);
			}
			projectionMatrixDirty = false;
        }

        #region HelperMethods

        /// <summary>
        /// Moves the camera and lookAt at to the right,
        /// as seen from the camera, while keeping the same height
        /// </summary>        
        public void MoveCameraRight(float amount)
        {
            Vector3 right = Vector3.Normalize(LookAt - Position); //calculate forward
            right = Vector3.Cross(right, Vector3.Up); //calculate the real right

            right.Normalize();
            LookAt += right * amount;
        }

        public void MoveCameraUp(float amount)
        {
            Vector3 up = Vector3.Normalize(LookAt - Position);

            Vector3 output = up * amount;
            output = Vector3.Transform(output, Matrix.CreateRotationY(-yaw));
            output = Vector3.Transform(output, Matrix.CreateRotationX((float)((Math.PI / 2))));
            output = Vector3.Transform(output, Matrix.CreateRotationY(yaw));

            LookAt += output;
        }


        public void MoveCameraForward(float amount)
        {
            Vector3 forward = Vector3.Normalize(LookAt - Position);

            forward.Normalize();
            LookAt += forward * amount;
        }

		/*public Microsoft.Xna.Framework.Graphics.VertexPositionColor[] GetMouseSelectorVertexColor()
		{
			VertexPositionColor[] outpt = new VertexPositionColor[] { new VertexPositionColor(), new VertexPositionColor() };

			var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
			float xoff = (mouseState.Position.X - MainViewer.graphics.PreferredBackBufferWidth / 2);
			float yoff = (mouseState.Position.Y - MainViewer.graphics.PreferredBackBufferHeight / 2);

			xoff /= 1730f;
			yoff /= 1730f;
			xoff *= (float)MainViewer.ratio;
			yoff *= (float)MainViewer.ratio;

			ArcBallCamera newCam = new ArcBallCamera(this.aspectRatio, this.fieldOfView,
			this.lookAt, Microsoft.Xna.Framework.Vector3.Up, 1, Single.MaxValue);

			newCam.PitchBounds = false;
			newCam.Zoom = this.zoom;
			newCam.Yaw = this.yaw;
			newCam.Pitch = this.pitch;


			newCam.MoveCameraForward(-newCam.Zoom);
			//newCam.MoveCameraForward(1);

			newCam.MoveCameraRight((float)-xoff);
			newCam.MoveCameraUp((float)+yoff);

			Vector3 depart = new Vector3(newCam.Position.X, newCam.Position.Y, newCam.Position.Z);

			//newCam.MoveCameraForward(-1);
			//newCam.MoveCameraRight((float)-xoff * 9);
			//newCam.MoveCameraUp((float)+yoff * 9);


			newCam.MoveCameraForward(1000);

			newCam.MoveCameraRight((float)xoff * 1000f);
			newCam.MoveCameraUp((float)-yoff * 1000f);

			Vector3 destination = new Vector3(newCam.LookAt.X, newCam.LookAt.Y, newCam.LookAt.Z);


			outpt[0].Position = depart;
			outpt[0].Color = Color.Red;
			outpt[1].Position = destination;
			outpt[1].Color = Color.Green;
			return outpt;
		}*/
        
		public static ArcBallCamera newCamNearScreen;
		public static ArcBallCamera newCam;
        

		#endregion

		#region FieldsAndProperties
		//We don't need an update method because the camera only needs updating
		//when we change one of it's parameters.
		//We keep track if one of our matrices is dirty
		//and reacalculate that matrix when it is accesed.
		private bool viewMatrixDirty = true;
        private bool projectionMatrixDirty = true;
        public bool PitchBounds { get; set; }
        
        public float MinPitch = -MathHelper.PiOver2 + 0.3f;
        public float MaxPitch = MathHelper.PiOver2 - 0.3f;
        private float pitch;
        public float Pitch
        {
            get { return pitch; }
            set
            {
                viewMatrixDirty = true;
                float newVal = value;

                if (this.PitchBounds)
                    newVal = MathHelper.Clamp(value, MinPitch, MaxPitch);
                else
                    newVal = value;

				while (newVal < -Math.PI)
                    newVal += (float)(2 * Math.PI);
				while (newVal > Math.PI)
                    newVal -= (float)(2 * Math.PI);
                pitch = newVal;
				ReCreateProjectionMatrix();
			}
        }
        public Matrix GetMatrix()
        {
            return Matrix.CreateFromYawPitchRoll(this.Yaw, this.Pitch, 0);
        }

		private float yaw;
		public float Yaw
        {
            get { return yaw; }
            set
            {
                viewMatrixDirty = true;
                yaw = value;

                if (yaw < -Math.PI)
                    yaw += (float)(2 * Math.PI);
                if (yaw > Math.PI)
                    yaw -= (float)(2 * Math.PI);
            }
        }

        private float fieldOfView;
        public float FieldOfView
        {
            get { return fieldOfView; }
            set
			{
				if (value>0&&value<Math.PI)
				{
					projectionMatrixDirty = true;
					fieldOfView = value;
				}
            }
        }

        private float aspectRatio;
        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                projectionMatrixDirty = true;
                aspectRatio = value;
            }
        }

        private float nearPlane;
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                projectionMatrixDirty = true;
                nearPlane = value;
            }
        }

        private float farPlane;
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                projectionMatrixDirty = true;
                farPlane = value;
            }
        }

        public float MinZoom = 1;
        public float MaxZoom = float.MaxValue;
        private float zoom = 1;


        public float Zoom
        {
            get { return zoom; }
            set
            {
                viewMatrixDirty = true;
                zoom = MathHelper.Clamp(value, MinZoom, MaxZoom);
            }
        }

        private float zoomTarget = 1;

        public bool Animated { get; set; }

        float rotationX = 0;
        float rotationY = 0;

        public float RotationX
        {
            get
            {
                return this.rotationX;
            }
            set
            {
                rotateXFreeze = false;
                this.rotationX = value;
                if (!this.Animated)
                {
                    this.Yaw = value;
                }
            }
        }

        public float RotationY
        {
            get
            {
                return this.rotationY;
            }
            set
            {
                rotateYFreeze = false;
                this.rotationY = value;
                if (!this.Animated)
                {
                    this.Pitch = value;
                }
            }
        }


        float translationX = 0;
        float translationY = 0;
        float translationZ = 0;

        public float TranslationX
        {
            get
            {
                return this.translationX;
            }
            set
            {
                translateXFreeze = false;

                if (!this.Animated)
                    value -= this.translationX;
                this.translationX = value;
                if (!this.Animated)
                {
                    this.MoveCameraRight(value);
                }
            }
        }

        public float TranslationY
        {
            get
            {
                return this.translationY;
            }
            set
            {
                translateYFreeze = false;

                if (!this.Animated)
                    value -= this.translationY;

                    this.translationY = value;
                if (!this.Animated)
                {
                    this.MoveCameraUp(value);
                }
            }
        }

        public float TranslationZ
        {
            get
            {
                return this.translationZ;
            }
            set
            {
                translateZFreeze = false;

                if (!this.Animated)
                    value -= this.translationZ;
                this.translationZ = value;
                if (!this.Animated)
                {
                    this.MoveCameraForward(value);
                }
            }
        }



        //

        Vector3 globalPosition;

        public Vector3 GlobalPosition()
        {
            this.globalPosition = Vector3.Normalize(View.mainCamera.LookAt - View.mainCamera.Position);
            this.globalPosition = View.mainCamera.LookAt - this.globalPosition * View.mainCamera.Zoom;
            return this.globalPosition;
        }
        

        public void Animate()
        {
            /*if (this.Moving&&!moving)
            {
                if (thread == null || !thread.IsAlive)
                {
                    thread = new System.Threading.Thread(() =>
                    {
                        while (stopWatch.ElapsedMilliseconds < 200) { }
                        Object3D.orderZindex = true;
                        thread.Abort();
                    });
                    thread.Start();
                }
                stopWatch.Restart();
                //udpate skeletonselection
            }*/
            

            if (!this.Animated)
                return;
            
            if (zoomFreeze)
            {

            }
            else if (Math.Abs(this.Zoom - this.zoomTarget)<0.1f)
            {
                zoomFreeze = true;
            }
            else if (this.Zoom< this.zoomTarget)
            {
                this.Zoom += (this.zoomTarget - this.zoom) / 10f;
            }
            else
            {
                this.Zoom -= (this.zoom - this.zoomTarget) / 10f;
            }

            if (translateXFreeze)
            {

            }
            else
            if (Math.Abs(this.TranslationX) > 0.001f)
            {
                this.MoveCameraRight(this.TranslationX);
                this.TranslationX = this.TranslationX * 0.9f;
            }
            else
            {
                translateXFreeze = true;
            }

            if (translateYFreeze)
            {

            }
            else
            if (Math.Abs(this.TranslationY) > 0.001f)
            {
                this.MoveCameraUp(this.TranslationY);
                this.TranslationY = this.TranslationY * 0.9f;
            }
            else
            {
                translateYFreeze = true;
            }

            if (translateZFreeze)
            {

            }
            else
            if (Math.Abs(this.TranslationZ) > 0.001f)
            {
                this.MoveCameraForward(this.TranslationZ);
                this.TranslationZ = this.TranslationZ * 0.9f;
            }
            else
            {
                translateZFreeze = true;
            }

            if (rotateXFreeze)
            {

            }
            else
            if (Math.Abs(this.RotationX) > 0.001f)
            {
                this.Yaw += this.RotationX;
                this.RotationX = this.RotationX * 0.9f;
            }
            else
            {
                rotateXFreeze = true;
            }

            if (rotateYFreeze)
            {

            }
            if (Math.Abs(this.RotationY) > 0.001f)
            {
                this.Pitch += this.RotationY;
                this.RotationY = this.RotationY * 0.9f;
            }
            else
            {
                rotateYFreeze = true;
            }
        }

        bool zoomFreeze = false;
        bool translateXFreeze = false;
        bool translateYFreeze = false;
        bool translateZFreeze = false;
        bool rotateXFreeze = false;
        bool rotateYFreeze = false;

        public float ZoomTarget
        {
            get { return zoomTarget; }
            set
            {
                this.zoomTarget = MathHelper.Clamp(value, MinZoom, MaxZoom);

                zoomFreeze = false;

                if (!this.Animated)
                {
                    this.Zoom = value;
                }
            }
        }


        private Vector3 position;
        public Vector3 Position
        {
            get
            {
                if (viewMatrixDirty)
                {
                    ReCreateViewMatrix();
                }
                return position;
            }
        }

        private Vector3 lookAt;
        public Vector3 LookAt
        {
            get { return lookAt; }
            set
            {
                viewMatrixDirty = true;
                lookAt = value;
            }
        }
        #endregion

        #region ICamera Members        
        public Matrix ViewProjectionMatrix
        {
            get { return ViewMatrix * ProjectionMatrix; }
        }

        private Matrix viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                if (viewMatrixDirty)
                {
                    ReCreateViewMatrix();
                }

                Microsoft.Xna.Framework.Vector3 position = this.lookAt + Microsoft.Xna.Framework.Vector3.Normalize(this.lookAt - this.position) * -this.zoom;
                return Microsoft.Xna.Framework.Matrix.CreateLookAt(position, this.lookAt, Microsoft.Xna.Framework.Vector3.Up);
            }
        }
        
        private Matrix projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get
            {
                if (projectionMatrixDirty)
                {
                    ReCreateProjectionMatrix();
                }
                return projectionMatrix;
            }
        }
        #endregion
    }
}
