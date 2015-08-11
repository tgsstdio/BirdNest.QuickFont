using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;


namespace QuickFont
{

    public struct Viewport
    {
        public int X, Y, Width, Height;
        public Viewport(int X, int Y, int Width, int Height) { this.X = X; this.Y = Y; this.Width = Width; this.Height = Height; }
    }

    public struct TransformViewport
    {
        public float X, Y, Width, Height;
        public TransformViewport(float X, float Y, float Width, float Height) { this.X = X; this.Y = Y; this.Width = Width; this.Height = Height; }
    }

    class ProjectionStack
    {

        //The currently set viewport
        public static Viewport? CurrentViewport {
            get {

                if (currentViewport == null)
                {
                    UpdateCurrentViewport();
                }

                return currentViewport; 
            }
        }
        private static Viewport? currentViewport = null;

        public static void UpdateCurrentViewport()
        {
            GraphicsContext.Assert();
            Viewport viewport = new Viewport();
            GL.GetInteger(GetPName.Viewport, out viewport.X);
            currentViewport = viewport;
        }


        public static void InvalidateViewport()
        {
            currentViewport = null;
        }

        public static void GetCurrentOrthogProjection(out bool isOrthog, out float left, out float right, out float bottom, out float top)
        {
            Matrix4 matrix = new Matrix4();
            GL.GetFloat(GetPName.ProjectionMatrix, out matrix.Row0.X);

			Helper.IsMatrixOrthogonal (out isOrthog, out left, out right, out bottom, out top, matrix);
        }


        public static void Begin()
        {

            GraphicsContext.Assert();

            Viewport currentVp = (Viewport)CurrentViewport;

            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix(); //push projection matrix
            GL.LoadIdentity();
            GL.Ortho(currentVp.X, currentVp.Width, currentVp.Height, currentVp.Y, -1.0, 1.0);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();  //push modelview matrix
            GL.LoadIdentity();

        }

        public static void End()
        {
            GraphicsContext.Assert();

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix(); //pop modelview

            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix(); //pop projection

            GL.MatrixMode(MatrixMode.Modelview);
        }






    }
}
