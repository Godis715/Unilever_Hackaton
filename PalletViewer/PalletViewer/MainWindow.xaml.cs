
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace PalletViewer
{
	class Helper
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
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

        public MainWindow()
        {
            InitializeComponent();

            BoxFactory boxFactory = new BoxFactory(0.23, 0.11, 0.13);

            Box box = boxFactory.GenBox();

            Box box1 = boxFactory.GenBox();

            Box box2 = boxFactory.GenBox();

            box.RotUp();

            box1.RotFront();

            MeshGeometry3D[] meshes = BoxToPolygons(new Box[] { box, box1, box2 });

            MyScene = new Scene(new Point3D(0, 0, 0))
            {

                UpBrush = new SolidColorBrush { Color = Colors.Red, Opacity = 1.0 },
                FrontBrush = new SolidColorBrush { Color = Colors.Yellow, Opacity = 1.0 },
                SideBrush = new SolidColorBrush { Color = Colors.Green, Opacity = 1.0 },

                UpMesh = meshes[0],
                FrontMesh = meshes[1],
                SideMesh = meshes[2],
            };

            this.DataContext = MyScene;
        }

        #region event handlers

        private void TestScene_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            double widthPallet = 12;
            double lengthPallet = 8;
            var layer = new LayerOnPallet(widthPallet, lengthPallet, TestScene);

            double widthBox = 0.4;
            double lengthBox = 2.2;
            double heigthBox = 2.3;

			layer.GenerateBoxs_On(new BoxFactory(widthBox, heigthBox, lengthBox));
            layer.CreateLayer(widthBox, lengthBox, heigthBox,
                LayerOnPallet.DirectionFilling.Right, LayerOnPallet.OrientationBox.Vertically);

            var meshes = BoxToPolygons(layer.Boxes.ToArray());

            MyScene.RestoreMesh(meshes);

        }

        void MouseDown_ViewPort(object sender, MouseButtonEventArgs e)
        {
            MyScene.StartMousePos = e.GetPosition(this);

            MyScene.LastMousePos = MyScene.StartMousePos;
        }

        void MouseMove_ViewPort(object sender, MouseEventArgs e)
        {
            if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if ((MyScene.LastMousePos - e.GetPosition(this)).Length > 10)
                {
                    Vector shift = e.GetPosition(this) - MyScene.StartMousePos;

                    MyScene.Camera.ShiftUVPosition(shift);

                    MyScene.LastMousePos = e.GetPosition(this);
                }
            }
        }

        void MouseUp_ViewPort(object sender, MouseButtonEventArgs e)
        {
            MyScene.Camera.RestorePosition();
        }
        #endregion
    }
}
