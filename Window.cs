using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Text;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;



namespace Pertemuan1
{
    static class Constants
    {
        public const string path = "../../../Shaders/";
        public const string objPath = "../../../Wavefront/";
    }
    internal class Window : GameWindow
    {
        
        double _time;
        float degr = 0;
        Camera _camera;
        bool _firstMove = true;
        Vector2 _lastPos;
        Vector3 _objecPost = new Vector3(0.0f, 0.0f, 0.0f);
        float _rotationSpeed = 1f;

        //
        
        Asset3d[] suns = new Asset3d[4];
        Asset3d mainObj = new Asset3d();
        Asset3d plane = new Asset3d();
        Asset3d latar = new Asset3d();
        // point lights
        private readonly Vector3[] _pointLightPositions ={
            new Vector3(-0.37f, 0.5f, -0.6f),
            new Vector3(1.56f, 0.5f, -0.6f),
            new Vector3(0.65f, 0.0f, 2.4f),
        };
        Vector3 _torchLightPosition = new Vector3(0, 0, 0);
        // arrays

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }
        public Matrix4 generateArbRotationMatrix(Vector3 axis, Vector3 center, float degree)
        {
            var rads = MathHelper.DegreesToRadians(degree);

            var secretFormula = new float[4, 4] {
                { (float)Math.Cos(rads) + (float)Math.Pow(axis.X, 2) * (1 - (float)Math.Cos(rads)), axis.X* axis.Y * (1 - (float)Math.Cos(rads)) - axis.Z * (float)Math.Sin(rads),    axis.X * axis.Z * (1 - (float)Math.Cos(rads)) + axis.Y * (float)Math.Sin(rads),   0 },
                { axis.Y * axis.X * (1 - (float)Math.Cos(rads)) + axis.Z * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Y, 2) * (1 - (float)Math.Cos(rads)), axis.Y * axis.Z * (1 - (float)Math.Cos(rads)) - axis.X * (float)Math.Sin(rads),   0 },
                { axis.Z * axis.X * (1 - (float)Math.Cos(rads)) - axis.Y * (float)Math.Sin(rads),   axis.Z * axis.Y * (1 - (float)Math.Cos(rads)) + axis.X * (float)Math.Sin(rads),   (float)Math.Cos(rads) + (float)Math.Pow(axis.Z, 2) * (1 - (float)Math.Cos(rads)), 0 },
                { 0, 0, 0, 1}
            };
            var secretFormulaMatix = new Matrix4
            (
                new Vector4(secretFormula[0, 0], secretFormula[0, 1], secretFormula[0, 2], secretFormula[0, 3]),
                new Vector4(secretFormula[1, 0], secretFormula[1, 1], secretFormula[1, 2], secretFormula[1, 3]),
                new Vector4(secretFormula[2, 0], secretFormula[2, 1], secretFormula[2, 2], secretFormula[2, 3]),
                new Vector4(secretFormula[3, 0], secretFormula[3, 1], secretFormula[3, 2], secretFormula[3, 3])
            );

            return secretFormulaMatix;
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            //ganti background
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);

            //suns
            for (int i = 0; i < 3; i++)
            {
                suns[i] = new Asset3d();
                //suns[i].createBoxVertices(_pointLightPositions[i].X, _pointLightPositions[i].Y, _pointLightPositions[i].Z, 0.25f);
                suns[i].F_createPrism(_pointLightPositions[i], 10, 0.1f, 0f, 0.4f);
                suns[i].rotate(suns[i]._centerPosition, suns[i]._euler[0], 90);
                suns[i].setColor(new Vector3(255, 255, 0));
                suns[i].load(Constants.path + "shader.vert", Constants.path + "shader.frag", Size.X, Size.Y);
            }
            _camera = new Camera(new Vector3(0.7f, 0, -2.25f), Size.X / Size.Y);
            suns[3] = new Asset3d();
            //suns[i].createBoxVertices(_pointLightPositions[i].X, _pointLightPositions[i].Y, _pointLightPositions[i].Z, 0.25f);
            suns[3].F_createPrism(_torchLightPosition, 10, 0.1f, 0f, 0.4f);
            suns[3].rotate(suns[3]._centerPosition, suns[3]._euler[0], 90);
            suns[3].setColor(new Vector3(255, 255, 0));
            suns[3].load(Constants.path + "shader.vert", Constants.path + "shader.frag", Size.X, Size.Y);
            //end suns



            // blender object
            plane = plane.loadObject(Constants.objPath + "Plane.obj", Constants.objPath + "Final.mtl", new Vector3(0, 0, 0));
            latar = latar.loadObject(Constants.objPath + "Final.obj", Constants.objPath + "Final.mtl", new Vector3(0, 0, 0));
            mainObj.addChild(plane);
            mainObj.addChild(latar);
            mainObj.load_withnormal(Constants.path + "shaderwithNormal.vert", Constants.path + "objectnew.frag", Size.X, Size.Y);
            mainObj.translate(new Vector3(0, -0.5f, 0));
            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            //
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //_time += 9.0 * args.Time;
            Matrix4 temp = Matrix4.Identity;
            _torchLightPosition = _camera.Position;
            suns[2]._centerPosition = _torchLightPosition;
            for (int i = 0; i < 3; i++)
            {
                suns[i].render(0, _time, temp, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());
            }
            mainObj.render(0, _time, temp, _camera.GetViewMatrix(), _camera.GetProjectionMatrix());
            mainObj.setFragVariable(_camera.Position);
            mainObj.setMaterial();
            mainObj.setDirectionalLight(new Vector3(0.2f, -1.3f, -0.5f), new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0f, 0f, 0f), new Vector3(1, 1, 1));
            mainObj.setSpotLight(new Vector3(1, 4.0f, 1), new Vector3(0f, -1.0f, 0f), new Vector3(0.2f, 0.2f, 0.2f), new Vector3(1.0f, 0.0f, 0.5f), new Vector3(1.0f, 1.0f, 1.0f),
                1.0f, 0.09f, 0.032f, MathF.Cos(MathHelper.DegreesToRadians(12.5f)), MathF.Cos(MathHelper.DegreesToRadians(25f))) ;
            
            mainObj.setPointLights(_pointLightPositions, new Vector3(0.3f, 0.3f, 0.05f), new Vector3(0.2f, 0.2f, 0.2f), new Vector3(1.0f, 1.0f, 1.0f), 1.0f, 0.09f, 0.032f);
            mainObj.setPointLight(_torchLightPosition, new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0f, 0f), new Vector3(0.5f, .0f, .0f), 1.0f, 0.09f, 0.032f);
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            float cameraSpeed = 0.5f;
            if (KeyboardState.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time;
            }
            var mouse = MouseState;
            var sensitivity = 0.2f;
            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }

            if (KeyboardState.IsKeyDown(Keys.N))
            {
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objecPost;
                _camera.Yaw += _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objecPost, _rotationSpeed).ExtractRotation());
                _camera.Position += _objecPost;

                _camera._front = -Vector3.Normalize(_camera.Position - _objecPost);
            }
            if (KeyboardState.IsKeyDown(Keys.Comma))
            {
                var axis = new Vector3(0, 1, 0);
                _camera.Position -= _objecPost;
                _camera.Yaw -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objecPost, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objecPost;

                _camera._front = -Vector3.Normalize(_camera.Position - _objecPost);
            }
            if (KeyboardState.IsKeyDown(Keys.K))
            {
                var axis = new Vector3(1, 0, 0);
                _camera.Position -= _objecPost;
                _camera.Pitch -= _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objecPost, _rotationSpeed).ExtractRotation());
                _camera.Position += _objecPost;
                _camera._front = -Vector3.Normalize(_camera.Position - _objecPost);
            }
            if (KeyboardState.IsKeyDown(Keys.M))
            {
                var axis = new Vector3(1, 0, 0);
                _camera.Position -= _objecPost;
                _camera.Pitch += _rotationSpeed;
                _camera.Position = Vector3.Transform(_camera.Position,
                    generateArbRotationMatrix(axis, _objecPost, -_rotationSpeed).ExtractRotation());
                _camera.Position += _objecPost;
                _camera._front = -Vector3.Normalize(_camera.Position - _objecPost);
            }

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButton.Left)
            {
                float _x = (MousePosition.X - Size.X / 2) / (Size.X / 2);
                float _y = -(MousePosition.Y - Size.Y / 2) / (Size.Y / 2);

                Console.WriteLine("x = " + _x + "y = " + _y);
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)

        {
            base.OnMouseWheel(e);
            Console.WriteLine("Offset Y: " + e.OffsetY);
            Console.WriteLine("Offset X: " + e.OffsetX);
            _camera.Fov = _camera.Fov - e.OffsetY;
        }

    }
}
