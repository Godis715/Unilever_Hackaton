
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PalletViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public static class Helper
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }

    public class Box
    {
        private Point3D S { get; set; }

        private Vector3D Dim { get; set; }

        private Point3D[] points;

        public Point3D[] Points
        {
            get
            {
                if (points == null)
                {
                    var E = S + Dim;

                    points = new Point3D[]
                        {
                            new Point3D(S.X, S.Y, S.Z),
                            new Point3D(S.X, S.Y, E.Z),
                            new Point3D(S.X, E.Y, S.Z),
                            new Point3D(S.X, E.Y, E.Z),

                            new Point3D(E.X, S.Y, S.Z),
                            new Point3D(E.X, S.Y, E.Z),
                            new Point3D(E.X, E.Y, S.Z),
                            new Point3D(E.X, E.Y, E.Z)
                        };
                }

                return points;
            }
            set
            {
                points = value;
            }
        }

        public int[] Orient { get; set; }

        public Box(Vector3D dim)
        {
            Dim = dim; S = new Point3D(0, 0, 0);
            Orient = new int[] { 0, 1, 2 };
        }

        public Box(Vector3D dim, Point3D start)
        {
            Dim = dim; S = start;
            Orient = new int[] { 0, 1, 2 };
        }

        public void Translate(Vector3D shift)
        {
            S += shift;
        }

        public void RotUp()
        {
            Dim = new Vector3D(Dim.Z, Dim.Y, Dim.X);
            Helper.Swap(ref Orient[0], ref Orient[2]);

            points = null;
        }

        public void RotFront()
        {
            Dim = new Vector3D(Dim.Y, Dim.X, Dim.Z);
            Helper.Swap(ref Orient[0], ref Orient[1]);

            points = null;
        }

        public void RotSide()
        {
            Dim = new Vector3D(Dim.X, Dim.Z, Dim.Y);
            Helper.Swap(ref Orient[1], ref Orient[2]);

            points = null;
        }
    }

    public class BoxFactory
    {
        private Vector3D Dimensions { get; set; }

        private Point3D Start { get; set; }

        // params of standart box
        public BoxFactory(double sWidth, double sHeight, double sLength)
        {
            Dimensions = new Vector3D(sWidth, sHeight, sLength);

            Start = new Point3D(0, 0, 0);
        }

        public Box GenBox()
        {
            return new Box(Dimensions, Start);
        }
    }

    public class Scene
    {
        #region brushes
        public SolidColorBrush UpBrush { get; set; }

        public SolidColorBrush FrontBrush { get; set; }

        public SolidColorBrush SideBrush { get; set; }
        #endregion

        #region meshes
        public MeshGeometry3D UpMesh { get; set; }

        public MeshGeometry3D FrontMesh { get; set; }

        public MeshGeometry3D SideMesh { get; set; }
        #endregion

        public Point3D SceneCenter { get; set; }

        public Point3D CameraPosition { get; set; }

        public Vector3D CameraOrient { get; set; }

        public void RestoreCamera()
        {
            CameraOrient = SceneCenter - CameraPosition;
        }

        private Vector LastMousePos { get; set; }
    }

    public partial class MainWindow : Window
    {

        public MeshGeometry3D[] BoxToPolygons(Box[] boxes)
        {
            var PolyIndecies = new int[][]
            {
            new int[] { 4, 1, 0,   5, 1, 4,   2, 3, 6,   3, 7, 6 }, // up indecies
            new int[] { 6, 0, 2,   4, 0, 6,   1, 7, 3,   1, 5, 7 }, // front
            new int[] { 2, 1, 3,   0, 1, 2,   5, 6, 7,   5, 4, 6 }, // side

            };

            var meshes = new MeshGeometry3D[3];

            for (int i = 0; i < 3; ++i)
            {
                meshes[i] = new MeshGeometry3D();
            }

            for (int i = 0; i < boxes.Length; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    for (int k = 0; k < 8; ++k)
                    {
                        meshes[j].Positions.Add(boxes[i].Points[k]);
                    }
                }

                for (int j = 0; j < 3; ++j)
                {
                    int orient = boxes[i].Orient[j];
                    for (int k = 0; k < PolyIndecies[j].Length; ++k)
                    {
                        meshes[j].TriangleIndices.Add(PolyIndecies[orient][k] + i * 8);
                    }
                }
            }

            return meshes;
        }

        public Scene MyScene { get; set; }

        void MouseDownEventHandler()
        {

        }

        public MainWindow()
        {
            InitializeComponent();

            BoxFactory boxFactory = new BoxFactory(2.3, 1.1, 1.3);

            Box box = boxFactory.GenBox();

            Box box1 = boxFactory.GenBox();

            box.RotUp();

            MeshGeometry3D[] meshes = BoxToPolygons(new Box[] { box, box1 });

            /* mesh.Positions.Add(new Point3D(0, 1, 0));
            mesh.Positions.Add(new Point3D(0, 0, 0));
            mesh.Positions.Add(new Point3D(-1, 0, 0));
            mesh.Positions.Add(new Point3D(-1, 1, 0));

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3); */

            MyScene = new Scene {

                UpBrush = new SolidColorBrush { Color = Colors.Red, Opacity = 1.0 },
                FrontBrush = new SolidColorBrush { Color = Colors.Yellow, Opacity = 1.0 },
                SideBrush = new SolidColorBrush { Color = Colors.Green, Opacity = 1.0 },

                UpMesh = meshes[0],
                FrontMesh = meshes[1],
                SideMesh = meshes[2],

                CameraPosition = new Point3D(3, 5, 3)
            };

            MyScene.RestoreCamera();

            this.DataContext = MyScene;
        }
    }
}
