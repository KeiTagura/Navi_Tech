using System;
using System.Collections.Generic;
using UnityEngine;

public static class SortedGizmos
    {
        static List<ICommand> commands = new List<ICommand>(1000);
        public static Color color { get; set; }

        public static void BatchCommit()
            {
    #if UNITY_EDITOR        
                Camera cam = null;
                var sv = UnityEditor.SceneView.currentDrawingSceneView;
                if (sv != null && sv.camera != null)
                    {
                    cam = sv.camera;
                    }
                else
                    {
                    cam = Camera.main;
                    }
                if (cam != null)
                    {
                    var mat = cam.worldToCameraMatrix;
                    for (int i = 0; i < commands.Count; ++i)
                        {
                        commands[i].Transform(mat);
                        }
                    // sort by z
                    var a = commands.ToArray();
                    Array.Sort<ICommand>(a, compareCommands);
                    // draw
                    for (int i = 0; i < a.Length; ++i)
                        {
                        a[i].Draw();
                        }
                    }
    #endif
                commands.Clear();
            }

        public static void DrawSphere(Vector3 _center, float _radius)
            {
                commands.Add(new DrawSolidSphereCommand
                    {
                        color = color,
                        position = _center,
                        radius = _radius
                    });
            }

        public static void DrawWireSphere(Vector3 _center, float _radius)
            {
                commands.Add(new DrawWireSphereCommand
                    {
                        color = color,
                        position = _center,
                        radius = _radius
                    });
            }

        public static void DrawCube(Vector3 _center, Vector3 _size)
            {
                commands.Add(new DrawSolidCubeCommand
                    {
                        color = color,
                        position = _center,
                        size = _size
                    });
            }

        public static void DrawWireCube(Vector3 _center, Vector3 _size)
            {
                commands.Add(new DrawWireCubeCommand
                    {
                        color = color,
                        position = _center,
                        size = _size
                    });
            }

        static int compareCommands(ICommand _a, ICommand _b)
            {
                float diff = _a.SortValue - _b.SortValue;
                if (diff < 0f) return -1;
                else if (diff > 0f) return 1;
                else return 0;
            }

        interface ICommand
            {
                void Transform(Matrix4x4 _worldToCamera);
                void Draw();
                float SortValue { get; }
            }

        struct DrawSolidSphereCommand : ICommand
            {
                public Color color;
                public Vector3 position;
                public float radius;
                private Vector3 transformedPosition;

                public void Transform(Matrix4x4 _mat)
                    {
                        transformedPosition = _mat.MultiplyPoint(position);
                    }

                public void Draw()
                    {
                        Gizmos.color = color;
                        Gizmos.DrawSphere(position, radius);
                    }

                public float SortValue { get { return transformedPosition.z; } }
            }

        struct DrawWireSphereCommand : ICommand
            {
                public Color color;
                public Vector3 position;
                public float radius;
                private Vector3 transformedPosition;

                public void Transform(Matrix4x4 _mat)
                    {
                        transformedPosition = _mat.MultiplyPoint(position);
                    }

                public void Draw()
                    {
                        Gizmos.color = color;
                        Gizmos.DrawWireSphere(position, radius);
                    }

                public float SortValue { get { return transformedPosition.z; } }
            }

        struct DrawSolidCubeCommand : ICommand
            {
                public Color color;
                public Vector3 position;
                public Vector3 size;
                private Vector3 transformedPosition;

                public void Transform(Matrix4x4 _mat)
                    {
                        transformedPosition = _mat.MultiplyPoint(position);
                    }

                public void Draw()
                    {
                        Gizmos.color = color;
                        Gizmos.DrawCube(position, size);
                    }

                public float SortValue { get { return transformedPosition.z; } }
            }

        struct DrawWireCubeCommand : ICommand
            {
                public Color color;
                public Vector3 position;
                public Vector3 size;
                private Vector3 transformedPosition;

                public void Transform(Matrix4x4 _mat)
                    {
                        transformedPosition = _mat.MultiplyPoint(position);
                    }

                public void Draw()
                    {
                        Gizmos.color = color;
                        Gizmos.DrawWireCube(position, size);
                    }

                public float SortValue { get { return transformedPosition.z; } }
            }

        struct DrawSolidMeshCommand : ICommand
            {
                public Color color;
                public Vector3 position;
                public Vector3 scale;
                public Quaternion rotation;
                public Mesh mesh;
                private Vector3 transformedPosition;

                public void Transform(Matrix4x4 _mat)
                    {
                        transformedPosition = _mat.MultiplyPoint(position);
                    }

                public void Draw()
                    {
                        Gizmos.color = color;
                        Gizmos.DrawMesh(mesh, position, rotation, scale);
                    }

                public float SortValue { get { return transformedPosition.z; } }
            }

        struct DrawWireMeshCommand : ICommand
            {
                public Color color;
                public Vector3 position;
                public Vector3 scale;
                public Quaternion rotation;
                public Mesh mesh;
                private Vector3 transformedPosition;

                public void Transform(Matrix4x4 _mat)
                    {
                     transformedPosition = _mat.MultiplyPoint(position);
                    }

                public void Draw()
                    {
                        Gizmos.color = color;
                        Gizmos.DrawWireMesh(mesh, position, rotation, scale);
                    }

                public float SortValue { get { return transformedPosition.z; } }
            }
    }