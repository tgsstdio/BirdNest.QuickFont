using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace QuickFont
{
    public static class Helper
    {
		public static void IsMatrixOrthogonal (out bool isOrthog, out float left, out float right, out float bottom, out float top, Matrix4 matrix)
		{		
			if (System.Math.Abs (matrix.M11) < float.Epsilon || System.Math.Abs (matrix.M22) < float.Epsilon)
			{
				isOrthog = false;
				left = right = bottom = top = 0;
				return;
			}
			left = -(1f + matrix.M41) / (matrix.M11);
			right = (1f - matrix.M41) / (matrix.M11);
			bottom = -(1 + matrix.M42) / (matrix.M22);
			top = (1 - matrix.M42) / (matrix.M22);
			isOrthog = Math.Abs (matrix.M12) < float.Epsilon && Math.Abs (matrix.M13) < float.Epsilon && Math.Abs (matrix.M14) < float.Epsilon && Math.Abs (matrix.M21) < float.Epsilon && Math.Abs (matrix.M23) < float.Epsilon && Math.Abs (matrix.M24) < float.Epsilon && Math.Abs (matrix.M31) < float.Epsilon && Math.Abs (matrix.M32) < float.Epsilon && Math.Abs (matrix.M34) < float.Epsilon && Math.Abs (matrix.M44 - 1f) < float.Epsilon;
		}

        public static T[] ToArray<T>(ICollection<T> collection)
        {
            T[] output = new T[collection.Count];
            collection.CopyTo(output, 0);
            return output;
        }

        /// <summary>
        /// Ensures GL.End() is called
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="code"></param>
        public static void SafeGLBegin(BeginMode mode, Action code)
        {
            GL.Begin(mode);

            code();

            GL.End();
        }

        /// <summary>
        /// Ensures that state is disabled
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="code"></param>
        public static void SafeGLEnable(EnableCap cap, Action code)
        {
            GL.Enable(cap);

            code();

            GL.Disable(cap);
        }

        /// <summary>
        /// Ensures that multiple states are disabled
        /// </summary>
        /// <param name="cap"></param>
        /// <param name="code"></param>
        public static void SafeGLEnable(EnableCap[] caps, Action code)
        {
            foreach(var cap in caps)
                GL.Enable(cap);

            code();

            foreach (var cap in caps)
                GL.Disable(cap);
        }

        public static void SafeGLEnableClientStates(ArrayCap[] caps, Action code)
        {
            foreach (var cap in caps)
                GL.EnableClientState(cap);

            code();

            foreach (var cap in caps)
                GL.DisableClientState(cap);
        }
    }
}
