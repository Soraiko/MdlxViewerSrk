using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MdlxViewer
{
    public class Bone
    {
        public static Bone Empty = new Bone(-1);
        public bool Grayed;

        public bool Selected
        {
            get;set;
        }
            
        public float ScaleX
        {
            get; set;
        }
        public float ScaleY
        {
            get; set;
        }
        public float ScaleZ
        {
            get; set;
        }

        public float RotateX
        {
            get; set;
        }
        public float RotateY
        {
            get; set;
        }
        public float RotateZ
        {
            get; set;
        }

        public float RotateX_Addition
        {
            get; set;
        }
        public float RotateY_Addition
        {
            get; set;
        }
        public float RotateZ_Addition
        {
            get; set;
        }

        public float TranslateX
        {
            get; set;
        }
        public float TranslateY
        {
            get; set;
        }
        public float TranslateZ
        {
            get;set;
        }

        public Quaternion Quaternion;
        public Vector3 Vecteur;
        public Matrix Matrice;
        public bool IsSkinned { get; set; }

        public short ParentID { get; set; }
        public short ID { get; set; }
        public static string outpt = "";
        

        public static string ToString(Matrix m)
        {
            string s = "";
            s += m.M11.ToString("0.000000") + " " + m.M12.ToString("0.000000") + " " + m.M13.ToString("0.000000") + " " + m.M14.ToString("0.000000") + "\r\n";
            s += m.M12.ToString("0.000000") + " " + m.M22.ToString("0.000000") + " " + m.M23.ToString("0.000000") + " " + m.M24.ToString("0.000000") + "\r\n";
            s += m.M13.ToString("0.000000") + " " + m.M32.ToString("0.000000") + " " + m.M33.ToString("0.000000") + " " + m.M34.ToString("0.000000") + "\r\n";
            s += m.M14.ToString("0.000000") + " " + m.M42.ToString("0.000000") + " " + m.M43.ToString("0.000000") + " " + m.M44.ToString("0.000000") + "\r\n";
            return s;
        }

        public static Matrix SlimMatrix(Matrix mat)
        {
                /*float m11 = mat.M11;
                float m12 = mat.M12;
                float m13 = mat.M13;
                float m14 = mat.M14;

                float m21 = mat.M21;
                float m22 = mat.M22;
                float m23 = mat.M23;
                float m24 = mat.M24;

                float m31 = mat.M31;
                float m32 = mat.M32;
                float m33 = mat.M33;
                float m34 = mat.M34;

                float m41 = mat.M41;
                float m42 = mat.M42;
                float m43 = mat.M43;
                float m44 = mat.M44;

                mat.M11 = m11;
                mat.M12 = m12;
                mat.M13 = m13;
                mat.M14 = m14;

                mat.M21 = m21;
                mat.M22 = m22;
                mat.M23 = m23;
                mat.M24 = m24;

                mat.M31 = m31;
                mat.M32 = m32;
                mat.M33 = m33;
                mat.M34 = m34;

                mat.M41 = m41;
                mat.M42 = m42;
                mat.M43 = m43;
                mat.M44 = m44;*/
            return mat;
        }

        public Bone(short id_)
        {
            this.ID = id_;
            this.ParentID = -1;
            this.Quaternion = Quaternion.Identity;
            this.Vecteur = Vector3.Zero;
            this.Matrice = Matrix.Identity;
        }

    }
}
