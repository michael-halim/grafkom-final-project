using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pertemuan1
{
    internal class Asset3d
    {
        List<Vector3> _vertices = new List<Vector3>();
        List<uint> _indices = new List<uint>();
        int _vertexBufferObject;
        int _vertexArrayObject;
        int _elementBufferObject;
        Shader _shader;
        Matrix4 _view;
        Matrix4 _projection;
        Matrix4 _model;
        public Vector3 _centerPosition;
        public List<Vector3> _euler;
        public List<Asset3d> Child;
        // base color
        public Vector3 _color;
        // Materials
        public Vector3 _Ka;
        public Vector3 _Kd; 
        public Vector3 _Ks;
        public float _Ns;

        // testing purposes
        public string _name;

        // bounding box purposes
        List<Vector3> bbox_coordinates;
        public Asset3d(List<Vector3> vertices, List<uint> indices)
        {
            _vertices = vertices;
            _indices = indices;
            setdefault();
        }
        public Asset3d()
        {
            _vertices = new List<Vector3>();
            setdefault();
        }

        public void translate(Vector3 position)
        {
            _model *= Matrix4.CreateTranslation(position);
            _centerPosition += position;

            foreach (var i in Child)
            {
                i.translate(position);
            }
        }

        public void setdefault()
        {
            _euler = new List<Vector3>();
            //sumbu X
            _euler.Add(new Vector3(1, 0, 0));
            //sumbu y
            _euler.Add(new Vector3(0, 1, 0));
            //sumbu z
            _euler.Add(new Vector3(0, 0, 1));

            _model = Matrix4.Identity;
            _centerPosition = new Vector3(0, 0, 0);
            _color = new Vector3(0, 0, 0);
            _Ka = new Vector3(0, 0, 0);
            _Kd = new Vector3(1,1,0);
            _Ks = new Vector3(0, 0, 0);
            _Ns = 256 / 4f;
            Child = new List<Asset3d>();
            bbox_coordinates = new List<Vector3>();
            _name = "I am unnamed!!!";

        }
        public void setColor(Vector3 color)
        {
            _color = returnColor(color);
        }

        public void setColorRaw(Vector3 color)
        {
            _color = color;
        }
        public Vector3 returnColor(Vector3 color)
        {
            return new Vector3(color.X / 255f, color.Y / 255f, color.Z / 255f);
        }
        public void load(string shadervert, string shaderfrag, float Size_x, float Size_y)
        {
            //Buffer
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count
                * Vector3.SizeInBytes, _vertices.ToArray(), BufferUsageHint.StaticDraw);
            //VAO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            //kalau mau bikin object settingannya beda dikasih if
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            //ada data yang disimpan di _indices
            if (_indices.Count != 0)
            {
                _elementBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count
                    * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);
            }
            _shader = new Shader(shadervert, shaderfrag);
            _shader.SetVector3("objectColor", _color);
            _shader.Use();

            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size_x / (float)Size_y, 0.1f, 100.0f);
            foreach (var item in Child)
            {
                item.load(shadervert, shaderfrag, Size_x, Size_y);
            }
        }

        public void render(int _lines, double time, Matrix4 temp, Matrix4 camera_view, Matrix4 camera_projection)
        {
            _shader.Use();
            GL.BindVertexArray(_vertexArrayObject);
            //_model = _model * Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(time));
            //_model = temp;

            _shader.SetMatrix4("model", _model);
            _shader.SetMatrix4("view", camera_view);
            _shader.SetMatrix4("projection", camera_projection);


            if (_indices.Count != 0)
            {
                GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedInt, 0);
            }
            else
            {

                if (_lines == 0)
                {
                    GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Count);
                }
                else if (_lines == 1)
                {
                    GL.DrawArrays(PrimitiveType.TriangleFan, 0, _vertices.Count);
                }
                else if (_lines == 2)
                {

                }
                else if (_lines == 3)
                {
                    GL.DrawArrays(PrimitiveType.LineStrip, 0, _vertices.Count);
                }
            }
            foreach (var item in Child)
            {
                item.render(_lines, time, temp, camera_view, camera_projection);
            }
        }
        public void createBoxVertices(float x, float y, float z, float length)
        {
            _centerPosition.X = x;
            _centerPosition.Y = y;
            _centerPosition.Z = z;
            Vector3 temp_vector;

            //TITIK 1
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 2
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 3
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 4
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z - length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 5
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 6
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y + length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 7
            temp_vector.X = x - length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);
            //TITIK 8
            temp_vector.X = x + length / 2.0f;
            temp_vector.Y = y - length / 2.0f;
            temp_vector.Z = z + length / 2.0f;
            _vertices.Add(temp_vector);

            _indices = new List<uint>
            {
                //SEGITIGA DEPAN 1
                0,1,2,
                //SEGITIGA DEPAN 2
                1,2,3,
                //SEGITIGA ATAS 1
                0,4,5,
                //SEGITIGA ATAS 2
                0,1,5,
                //SEGITIGA KANAN 1
                1,3,5,
                //SEGITIGA KANAN 2
                3,5,7,
                //SEGITIGA KIRI 1
                0,2,4,
                //SEGITIGA KIRI 2
                2,4,6,
                //SEGITIGA BELAKANG 1
                4,5,6,
                //SEGITIGA BELAKANG 2
                5,6,7,
                //SEGITIGA BAWAH 1
                2,3,6,
                //SEGITIGA BAWAH 2
                3,6,7
            };


        }
        public void createBoxVertices2(Vector3 position, float length)
        {
            _centerPosition = position;
            Vector3 temp_vector;

            //FRONT FACE

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));



            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            //BACK FACE
            //TITIK 5
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            //TITIK 6
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));
            //TITIK 7
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            //TITIK 6
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));
            //TITIK 7
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            //TITIK 8
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            //LEFT FACE
            //TITIK 1
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            //TITIK 3
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            //TITIK 5
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            //TITIK 3
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            //TITIK 5
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(-1.0f, 0.0f, 0.0f));
            //TITIK 7
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(-1.0f, 0.0f, 0.0f));

            //RIGHT FACE
            //TITIK 2
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            //TITIK 4
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            //TITIK 6
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            //TITIK 4
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            //TITIK 6
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            //TITIK 8
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));

            //BOTTOM FACES
            //TITIK 3
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));
            //TITIK 4
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));
            //TITIK 7
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));
            //TITIK 4
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));
            //TITIK 7
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));
            //TITIK 8
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, -1.0f, 0.0f));

            //TOP FACES
            //TITIK 1
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            //TITIK 2
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            //TITIK 5
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            //TITIK 2
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            //TITIK 5
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            //TITIK 6
            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + length / 2.0f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
        }
        public void createRectVertices(Vector3 position, float length)
        {
            _centerPosition = position;
            //FRONT FACE
            Vector3 temp_vector;
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - 0.005f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z - 0.005f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z - 0.005f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + -0.005f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + -0.005f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z + -0.005f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, -1.0f));

            // back face
            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z + 0.00001f;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y + length / 2.0f;
            temp_vector.Z = position.Z;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            temp_vector.X = position.X - length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

            temp_vector.X = position.X + length / 2.0f;
            temp_vector.Y = position.Y - length / 2.0f;
            temp_vector.Z = position.Z;
            _vertices.Add(temp_vector);
            _vertices.Add(new Vector3(0.0f, 0.0f, 1.0f));

        }

        public void loadMaterials(string path, string mtl_name="")
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }
            
            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    List<string> words = new List<string>(streamReader.ReadLine().ToLower().Split(' '));
                    words.RemoveAll(s => s == string.Empty);
                   // if it's empty, skip
                   if(words.Count == 0)
                    {
                        continue;
                    }
                    
                   // check if mtl
                   if(words[0] == "newmtl")
                    {
                        // check if the mtl
                        if(words[1] == mtl_name)
                        {
                            for (int i = 0; i < 4; i++) {
                                List<string> mtl = new List<string>(streamReader.ReadLine().ToLower().Split(' '));
                                
                                // take the letter
                                string type = mtl[0];
                                mtl.RemoveAt(0);

                                // determine
                                switch (type) {
                                    case "ns":
                                        _Ns = float.Parse(mtl[0]) / 1000 * 256;
                                        break;
                                    case "ka":
                                        _Ka = new Vector3(float.Parse(mtl[0]), float.Parse(mtl[1]), float.Parse(mtl[2]));
                                        break;
                                    case "kd":
                                        _Kd = new Vector3(float.Parse(mtl[0]), float.Parse(mtl[1]), float.Parse(mtl[2]));
                                        break;
                                    case "ks":
                                        _Ks = new Vector3(float.Parse(mtl[0]), float.Parse(mtl[1]), float.Parse(mtl[2]));
                                        break;
                                }

                            }
                        }
                    }
                    
                }
            }
            

        }
        public Asset3d loadObject(string path, string mtl_path, Vector3 position, float scale = 0.1f)
        {
            _centerPosition = position;
            List<Vector3> temporary_vertices = new List<Vector3>();
            List<Vector3> temporary_textures = new List<Vector3>();
            List<Vector3> temporary_normals = new List<Vector3>();
            Asset3d mother = new Asset3d();
            Asset3d sub_object = new Asset3d();
            bool isFirst = true;
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Unable to open \"" + path + "\", does not exist.");
            }

            using (StreamReader streamReader = new StreamReader(path))
            {
                while (!streamReader.EndOfStream)
                {
                    // get one line and separate by space
                    List<string> words = new List<string>(streamReader.ReadLine().ToLower().Split(' '));
                    words.RemoveAll(s => s == string.Empty);
               
                    // if it's empty, skip
                    if (words.Count == 0)
                    {
                        continue;
                    }

                    // take the letter
                    string type = words[0];
                    words.RemoveAt(0);

                    // determine
                    switch (type)
                    {
                        case "o":
                            // if not the first, swallow it first
                            if (!isFirst)
                            {

                                mother.Child.Add(sub_object);
                            }
                            else {
                                isFirst = false;
                            }
                            // reset the vertices list and the child
                            sub_object = new Asset3d();
                            sub_object._name = words[0];
                            
                            // update index offset

                            break;
                        case "v":
                            temporary_vertices.Add(new Vector3(float.Parse(words[0]) * scale, float.Parse(words[1]) * scale, float.Parse(words[2]) * scale));
                            break;

                        case "vt":
                            temporary_textures.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]),
                                                            words.Count < 3 ? 0 : float.Parse(words[2])));
                            break;

                        case "vn":
                            temporary_normals.Add(new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2])));
                            break;

                        case "f":
                            foreach (string w in words)
                            {
                                if (w.Length == 0)
                                    continue;

                                string[] comps = w.Split('/');

                                // vertex
                                Vector3 vertex = temporary_vertices[(int)uint.Parse(comps[0]) - 1] + _centerPosition;
                                sub_object._vertices.Add(vertex);
                                
                                // normals
                                sub_object._vertices.Add(temporary_normals[(int)uint.Parse(comps[2]) - 1]);
                            }
                            
                            break;

                        case "usemtl":
                            sub_object.loadMaterials(mtl_path, words[0]);
                            break;
                        default:
                            break;
                    }
                    if (streamReader.EndOfStream)
                    {
                        mother.Child.Add(sub_object);
                    }
                }
            }
            return mother;
        }
        public void createNPrismEpic(Vector3 position, float rBot = 0.1f, float rTop = 0.1f, float height  = 0.1f, int sides = 3)
        {
            _centerPosition = position;
            float step = 360 / sides;
            Vector3 tempvec = new Vector3();
            List<Vector3> temp_vertices = new List<Vector3>();
            List<Vector3> temp_normals = new List<Vector3>();

            for (float i = 0, i2 = step; i < 360; i += step, i2 += step) 
            {
                double degInRad = i * Math.PI / 180;
                double deg2InRad = i2 * Math.PI / 180;

                // each iteration makes a rectangle -- that's 6 vertices, 2 faces, and 1 normal.

                // first triangle
                Vector3 a = new Vector3();
                a.X = (float)Math.Round((double)position.X + Math.Cos(degInRad) * rBot, 3);
                a.Y = (float)Math.Round((double)position.Y + Math.Sin(degInRad) * rBot, 3);
                a.Z = position.Z;

                Vector3 b = new Vector3();
                b.X = (float)Math.Round((double)position.X + Math.Cos(degInRad) * rTop, 3);
                b.Y = (float)Math.Round((double)position.Y + Math.Sin(degInRad) * rTop, 3);
                b.Z = position.Z + height;

                Vector3 c = new Vector3();
                c.X = (float)Math.Round((double)position.X + Math.Cos(deg2InRad) * rBot, 3);
                c.Y = (float)Math.Round((double)position.Y + Math.Sin(deg2InRad) * rBot, 3);
                c.Z = position.Z;

                // make normals
                Vector3 ba = a - b;
                Vector3 bc = c - b;
                Vector3 normal = Vector3.Normalize(Vector3.Cross(ba, bc));

                // the other triangle
                Vector3 d = c;
                Vector3 e = b;
                Vector3 f = new Vector3();
                f.X = (float)Math.Round((double)position.X + Math.Cos(deg2InRad) * rTop, 3);
                f.Y = (float)Math.Round((double)position.Y + Math.Sin(deg2InRad) * rTop, 3);
                f.Z = position.Z + height;

                // append
                _vertices.Add(a);
                _vertices.Add(normal);
                _vertices.Add(b);
                _vertices.Add(normal);
                _vertices.Add(c);
                _vertices.Add(normal);
                _vertices.Add(d);
                _vertices.Add(normal);
                _vertices.Add(e);
                _vertices.Add(normal);
                _vertices.Add(f);
                _vertices.Add(normal);
            }
        }
        public void createNGonEpic(Vector3 position, float radius, int sides = 3)
        {
            _centerPosition = position;
            float step = 360 / sides;
            Vector3 tempvec = new Vector3();
            List<Vector3> temp_vertices = new List<Vector3>();
           


            // we draw many overlapping vertices here.
            for (float i = 0, i2 = step; i < 360; i += step, i2 += step)
            {
                double degInRad = i * Math.PI / 180;
                double deg2InRad = i2 * Math.PI / 180;
                // origin (A)
                tempvec.X = (float)Math.Round((double)position.X + Math.Cos(degInRad) * radius, 3); 
                tempvec.Y = (float)Math.Round((double)position.Y + Math.Sin(degInRad) * radius, 3); 
                tempvec.Z = position.Z;
                //Console.WriteLine(tempvec + " A, theta = " + i);
                temp_vertices.Add(tempvec);

                // center (O)
                //Console.WriteLine(position + " O") ;
                temp_vertices.Add(position);

                //// neighbor (B)
                tempvec.X = (float)Math.Round((double)position.X + Math.Cos(deg2InRad) * radius, 3); Console.WriteLine("COS (" + i2 + ") -> " + tempvec.X);
                tempvec.Y = (float)Math.Round((double)position.Y + Math.Sin(deg2InRad) * radius, 3); Console.WriteLine("SIN (" + i2 + ") -> " + tempvec.Y);
                tempvec.Z = position.Z;
                //Console.WriteLine(tempvec + " B, theta = " + i2);
                temp_vertices.Add(tempvec);
                Console.WriteLine();
            }
            // ABC is CCW already, so N = OX x OY
            for (int i = 0; i < temp_vertices.Count; i += 3)
            {
                // gather tree vertices
                Vector3 a = temp_vertices[i];
                Vector3 o = temp_vertices[i + 1];
                Vector3 b = temp_vertices[i + 2];

                // create a couple of vectors
                
                Vector3 oa = o - a; 
                Vector3 ob = o - b; 
                Vector3 normal = Vector3.Normalize(Vector3.Cross(oa, ob)); // cross and 
                // append them
                
                
                _vertices.Add(a);
                _vertices.Add(normal);
                _vertices.Add(o);
                _vertices.Add(normal);
                _vertices.Add(b);
                _vertices.Add(normal);
            }
        }
        public void F_createPrism(Vector3 position, float sides, float rBot, float rTop, float height)
        {
            _centerPosition = position;
            float step = 360 / sides; // the angular interval of each side
            Vector3 tempVec;

            // plot sides points
            for (float i = 0; i <= 360; i += step)
            {
                double degInRad = i * Math.PI / 180;

                // bottom
                tempVec.X = position.X + (float)Math.Cos(degInRad) * rBot;
                tempVec.Y = position.Y + (float)Math.Sin(degInRad) * rBot;
                tempVec.Z = position.Z;
                _vertices.Add(tempVec);
                // top
                tempVec.X = position.X + (float)Math.Cos(degInRad) * rTop;
                tempVec.Y = position.Y + (float)Math.Sin(degInRad) * rTop;
                tempVec.Z = position.Z + height;
                _vertices.Add(tempVec);
            }

            // add indices
            for (uint i = 0; i < _vertices.Count; i++)
            {
                if (i == _vertices.Count - 2)
                {
                    _indices.Add(i);
                    _indices.Add(i + 1);
                    _indices.Add(0);
                }
                else if (i == _vertices.Count - 1)
                {
                    _indices.Add(i);
                    _indices.Add(0);
                    _indices.Add(1);
                }
                else
                {
                    _indices.Add(i);
                    _indices.Add(i + 1);
                    _indices.Add(i + 2);
                }
            }
        }

        public void makeTorch(Vector3 position)
        {
            Asset3d torchHead = new Asset3d();
            torchHead.F_createPrism(position, 0.1f, 0.0f, 0.1f, 10);
            torchHead.rotate(torchHead._centerPosition, torchHead._euler[0], 90);
            addChild(torchHead);
        }
        public void testNormals()
        {
            Vector3 ab = new Vector3(1, 2, 3) * 3; Console.WriteLine(ab);
            Vector3 bc = new Vector3(3, 4, 5) * 3; Console.WriteLine(bc);
            Vector3 cross = Vector3.Cross(ab, bc);
            Console.WriteLine("cross result : " + cross);
            Console.WriteLine("normalized : " + Vector3.Normalize(cross));
        }
        public void createEllipsoid(float radiusX,float radiusY, float radiusZ,float _x,float _y,float _z)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;
            float pi = (float)Math.PI;
            Vector3 temp_vector;
            for(float u = -pi;u<=pi;u+= pi / 300)
            {
                for(float v = -pi / 2; v <= pi / 2; v += pi / 300)
                {
                    temp_vector.X = _x + (float)Math.Cos(v) * (float)Math.Cos(u) * radiusX;
                    temp_vector.Y = _y + (float)Math.Cos(v) * (float)Math.Sin(u) * radiusY;
                    temp_vector.Z = _z + (float)Math.Sin(v) * radiusZ;
                    _vertices.Add(temp_vector);
                }
            }
        }
        public void createEllipsoid2(float radiusX, float radiusY, float radiusZ, float _x, float _y, float _z, int sectorCount, int stackCount)
        {
            _centerPosition.X = _x;
            _centerPosition.Y = _y;
            _centerPosition.Z = _z;
            float pi = (float)Math.PI;
            Vector3 temp_vector;
            float sectorStep = 2 * (float)Math.PI / sectorCount;
            float stackStep = (float)Math.PI / stackCount;
            float sectorAngle, StackAngle, x, y, z;

            for (int i = 0; i <= stackCount; ++i)
            {
                StackAngle = pi / 2 - i * stackStep;
                x = radiusX * (float)Math.Cos(StackAngle);
                y = radiusY * (float)Math.Cos(StackAngle);
                z = radiusZ * (float)Math.Sin(StackAngle);

                for (int j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;

                    temp_vector.X = x * (float)Math.Cos(sectorAngle);
                    temp_vector.Y = y * (float)Math.Sin(sectorAngle);
                    temp_vector.Z = z;
                    _vertices.Add(temp_vector);
                }
            }

            uint k1, k2;
            for (int i = 0; i < stackCount; ++i)
            {
                k1 = (uint)(i * (sectorCount + 1));
                k2 = (uint)(k1 + sectorCount + 1);
                for (int j = 0; j < sectorCount; ++j, ++k1, ++k2)
                {
                    if (i != 0)
                    {
                        _indices.Add(k1);
                        _indices.Add(k2);
                        _indices.Add(k1 + 1);
                    }
                    if (i != (stackCount - 1))
                    {
                        _indices.Add(k1 + 1);
                        _indices.Add(k2);
                        _indices.Add(k2 + 1);
                    }
                }
            }
        }
        public void rotatede(Vector3 pivot, Vector3 vector, float angle)
        {
            var radAngle = MathHelper.DegreesToRadians(angle);

            var arbRotationMatrix = new Matrix4
                (
                new Vector4((float)(Math.Cos(radAngle) + Math.Pow(vector.X, 2.0f) * (1.0f - Math.Cos(radAngle))), (float)(vector.X * vector.Y * (1.0f - Math.Cos(radAngle)) + vector.Z * Math.Sin(radAngle)), (float)(vector.X * vector.Z * (1.0f - Math.Cos(radAngle)) - vector.Y * Math.Sin(radAngle)), 0),
                new Vector4((float)(vector.X * vector.Y * (1.0f - Math.Cos(radAngle)) - vector.Z * Math.Sin(radAngle)), (float)(Math.Cos(radAngle) + Math.Pow(vector.Y, 2.0f) * (1.0f - Math.Cos(radAngle))), (float)(vector.Y * vector.Z * (1.0f - Math.Cos(radAngle)) + vector.X * Math.Sin(radAngle)), 0),
                new Vector4((float)(vector.X * vector.Z * (1.0f - Math.Cos(radAngle)) + vector.Y * Math.Sin(radAngle)), (float)(vector.Y * vector.Z * (1.0f - Math.Cos(radAngle)) - vector.X * Math.Sin(radAngle)), (float)(Math.Cos(radAngle) + Math.Pow(vector.Z, 2.0f) * (1.0f - Math.Cos(radAngle))), 0),
                Vector4.UnitW
                );

            _model *= Matrix4.CreateTranslation(-pivot);
            _model *= arbRotationMatrix;
            _model *= Matrix4.CreateTranslation(pivot);

            for (int i = 0; i < 3; i++)
            {
                _euler[i] = Vector3.Normalize(getRotationResult(pivot, vector, radAngle, _euler[i], true));
            }

            _centerPosition = getRotationResult(pivot, vector, radAngle, _centerPosition);


            foreach (var i in Child)
            {
                i.rotate(pivot, vector, angle);
            }
        }
        public void rotate(Vector3 pivot, Vector3 vector, float angle)
        {
            //pivot -> mau rotate di titik mana
            //vector -> mau rotate di sumbu apa? (x,y,z)
            //angle -> rotatenya berapa derajat?
            var real_angle = angle;
            angle = MathHelper.DegreesToRadians(angle);

            //mulai ngerotasi
            for (int i = 0; i < _vertices.Count; i++)
            {
                _vertices[i] = getRotationResult(pivot, vector, angle, _vertices[i]);
            }
            //rotate the euler direction
            for (int i = 0; i < 3; i++)
            {
                _euler[i] = getRotationResult(pivot, vector, angle, _euler[i], true);

                //NORMALIZE
                //LANGKAH - LANGKAH
                //length = akar(x^2+y^2+z^2)
                float length = (float)Math.Pow(Math.Pow(_euler[i].X, 2.0f) + Math.Pow(_euler[i].Y, 2.0f) + Math.Pow(_euler[i].Z, 2.0f), 0.5f);
                Vector3 temporary = new Vector3(0, 0, 0);
                temporary.X = _euler[i].X / length;
                temporary.Y = _euler[i].Y / length;
                temporary.Z = _euler[i].Z / length;
                _euler[i] = temporary;
            }
            _centerPosition = getRotationResult(pivot, vector, angle, _centerPosition);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count * Vector3.SizeInBytes,
                _vertices.ToArray(), BufferUsageHint.StaticDraw);
            foreach (var item in Child)
            {
                item.rotate(pivot, vector, real_angle);
            }
        }
        public Vector3 getRotationResult(Vector3 pivot, Vector3 vector, float angle, Vector3 point, bool isEuler = false)
        {
            Vector3 temp, newPosition;

            if (isEuler)
            {
                temp = point;
            }
            else
            {
                temp = point - pivot;
            }

            newPosition.X =
                temp.X * (float)(Math.Cos(angle) + Math.Pow(vector.X, 2.0f) * (1.0f - Math.Cos(angle))) +
                temp.Y * (float)(vector.X * vector.Y * (1.0f - Math.Cos(angle)) - vector.Z * Math.Sin(angle)) +
                temp.Z * (float)(vector.X * vector.Z * (1.0f - Math.Cos(angle)) + vector.Y * Math.Sin(angle));

            newPosition.Y =
                temp.X * (float)(vector.X * vector.Y * (1.0f - Math.Cos(angle)) + vector.Z * Math.Sin(angle)) +
                temp.Y * (float)(Math.Cos(angle) + Math.Pow(vector.Y, 2.0f) * (1.0f - Math.Cos(angle))) +
                temp.Z * (float)(vector.Y * vector.Z * (1.0f - Math.Cos(angle)) - vector.X * Math.Sin(angle));

            newPosition.Z =
                temp.X * (float)(vector.X * vector.Z * (1.0f - Math.Cos(angle)) - vector.Y * Math.Sin(angle)) +
                temp.Y * (float)(vector.Y * vector.Z * (1.0f - Math.Cos(angle)) + vector.X * Math.Sin(angle)) +
                temp.Z * (float)(Math.Cos(angle) + Math.Pow(vector.Z, 2.0f) * (1.0f - Math.Cos(angle)));

            if (isEuler)
            {
                temp = newPosition;
            }
            else
            {
                temp = newPosition + pivot;
            }
            return temp;
        }

        public void resetEuler()
        {
            _euler[0] = new Vector3(1, 0, 0);
            _euler[1] = new Vector3(0, 1, 0);
            _euler[2] = new Vector3(0, 0, 1);
        }
        public void addChild(Vector3 position, float length)
        {
            Asset3d newChild = new Asset3d();
            newChild.createBoxVertices2(position, length);
            Child.Add(newChild);
        }
        public void addChild(Asset3d 子)
        {
            Child.Add(子);
        }
        public void setFragVariable(Vector3 viewPos)
        {
            
           //_shader.SetVector3("objectColor", _color); // set color as the object's own color
            _shader.SetVector3("viewPos", viewPos); // view position as given (from camera position)
            //_shader.SetVector3("lightColor", returnColor(LightColor));
            foreach (var item in Child)
            {
                item.setFragVariable(viewPos);
            }
        }
        // ambient, specular, diffuse in that order
        public void setMaterial()
        {

            _shader.SetVector3("material.ambient", _Ka);
            _shader.SetVector3("material.diffuse", _Kd);
            _shader.SetVector3("material.specular", _Ks);
            _shader.SetFloat("material.shine_coefficient", _Ns);
           
            foreach (var item in Child)
            {
                item.setMaterial();
            }
        }
        public void setSpecularDiffuseVariable(Vector3 LightPos)
        {
            //Console.Write(LightPos);
            _shader.SetVector3("lightPos", LightPos);
            
            foreach (var item in Child)
            {
                item.setSpecularDiffuseVariable(LightPos);
            }
        }
        public void setDirectionalLight(Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular)
        {
            _shader.SetVector3("dirLight.direction", direction);
            _shader.SetVector3("dirLight.ambient", ambient);
            _shader.SetVector3("dirLight.diffuse", diffuse);
            _shader.SetVector3("dirLight.specular", specular);

            foreach(var item in Child)
            {
                item.setDirectionalLight(direction, ambient, diffuse, specular);
            }
        }
        public void setPointLight(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular, float constant, float linear, float quadratic)
        {
            _shader.SetVector3("torchLight.position", position);
            _shader.SetVector3("torchLight.ambient", ambient);
            _shader.SetVector3("torchLight.diffuse", diffuse);
            _shader.SetVector3("torchLight.specular", specular);
            _shader.SetFloat("torchLight.constant", constant);
            _shader.SetFloat("torchLight.linear", linear);
            _shader.SetFloat("torchLight.quadratic", quadratic);
            foreach (var item in Child)
            {
                item.setPointLight(position, ambient, diffuse, specular, constant, linear, quadratic);
            }
        }
        public void setSpotLight(Vector3 position, Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular, float constant, float linear, float quadratic, float cutOff, float outerCutOff)
        {
            _shader.SetVector3("spotLight.position", position);
            _shader.SetVector3("spotLight.direction", direction);
            _shader.SetVector3("spotLight.ambient", ambient);
            _shader.SetVector3("spotLight.diffuse", diffuse);
            _shader.SetVector3("spotLight.specular", specular);
            _shader.SetFloat("spotLight.constant", constant);
            _shader.SetFloat("spotLight.linear", linear);
            _shader.SetFloat("spotLight.quadratic", quadratic);
            _shader.SetFloat("spotLight.cutOff", cutOff);
            _shader.SetFloat("spotLight.outerCutOff", outerCutOff);
            foreach (var item in Child)
            {
                item.setSpotLight(position, direction, ambient, diffuse, specular, constant, linear, quadratic, cutOff, outerCutOff);
            }
        }
        public void setPointLights(Vector3[] position, Vector3 ambient, Vector3 diffuse, Vector3 specular, float constant, float linear, float quadratic)
        {
            for (int i = 0; i < position.Length; i++)
            {
                _shader.SetVector3($"pointLight[{i}].position", position[i]);
                _shader.SetVector3($"pointLight[{i}].ambient", ambient);
                _shader.SetVector3($"pointLight[{i}].diffuse", diffuse);
                _shader.SetVector3($"pointLight[{i}].specular", specular);
                _shader.SetFloat($"pointLight[{i}].constant", 1.0f);
                _shader.SetFloat($"pointLight[{i}].linear", 0.14f);
                _shader.SetFloat($"pointLight[{i}].quadratic", 0.07f);
            }

            foreach (var item in Child)
            {
                item.setPointLights(position, ambient, diffuse, specular, constant, linear, quadratic);
            }
        }

        public void setBoundingBox()
        {
            float maxX = 0;
            float maxY = 0;
            float maxZ = 0;

            for (int i=0; i<_vertices.Count; i++)
            {
                // take the even ones
                if(i % 2 == 0)
                {
                    
                    if(Math.Abs(_vertices[i].X) > maxX)
                    {
                        maxX = Math.Abs(_vertices[i].X);
                    }
                    if (Math.Abs(_vertices[i].Y) > maxX)
                    {
                        maxY = Math.Abs(_vertices[i].Y);
                    }
                    if (Math.Abs(_vertices[i].Z) > maxX)
                    {
                        maxZ = Math.Abs(_vertices[i].Z);

                    }
                }
            }
            float radX = _centerPosition.X + maxX;
            float radY = _centerPosition.X + maxY;
            float radZ = _centerPosition.X + maxZ;

            bbox_coordinates.Add(_centerPosition + new Vector3(radX, radY, radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(radX, radY, -radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(radX, -radY, radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(radX, -radY, -radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(-radX, radY, radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(-radX, radY, -radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(-radX, -radY, radZ));
            bbox_coordinates.Add(_centerPosition + new Vector3(-radX, -radY, -radZ));


            Console.WriteLine("Nama : " + _name + "in " + _centerPosition);
            foreach(var item in bbox_coordinates)
            {
                Console.WriteLine(item);
            }
        }
        public void load_withnormal(string shadervert, string shaderfrag, float Size_x, float Size_y)
        {
            //Buffer
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Count
                * Vector3.SizeInBytes, _vertices.ToArray(), BufferUsageHint.StaticDraw);
            //VAO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float,
                false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float,
                false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            //if (_indices.Count != 0)
            //{
            //    _elementBufferObject = GL.GenBuffer();
            //    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            //    GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Count
            //        * sizeof(uint), _indices.ToArray(), BufferUsageHint.StaticDraw);
            //}

            _shader = new Shader(shadervert, shaderfrag);
            _shader.Use();
            //Console.WriteLine(shaderfrag);
            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);

            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size_x / (float)Size_y, 0.1f, 100.0f);

            setBoundingBox();
            foreach (var item in Child)
            {
                item.load_withnormal(shadervert, shaderfrag, Size_x, Size_y);
            }
        }
    }
}
