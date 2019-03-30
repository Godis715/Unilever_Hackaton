using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PalletViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    class Box
    {
        private Point3D S { get; set; }
        private Point3D E { get; set; }

        public Box(Point3D start, Point3D end)
        {
            S = start; E = end;
        }

        public MeshGeometry3D GenerateMesh()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(S.X, S.Y, S.Z)); // 0
            mesh.Positions.Add(new Point3D(S.X, S.Y, E.Z)); // 1
            //mesh.Positions.Add(new Point3D(S.X, E.Y, S.Z)); // 2
           // mesh.Positions.Add(new Point3D(S.X, E.Y, E.Z)); // 3

            mesh.Positions.Add(new Point3D(E.X, S.Y, S.Z)); // 4
            mesh.Positions.Add(new Point3D(E.X, S.Y, E.Z)); // 5
           // mesh.Positions.Add(new Point3D(E.X, E.Y, S.Z)); // 6
           // mesh.Positions.Add(new Point3D(E.X, E.Y, E.Z)); // 7

            // S.X poly
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);

            // E.X poly
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);

            // S.Y poly
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);

            // E.Y poly
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(6);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(7);

            return mesh;
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            mesh.Positions.Add(new Point3D(0, 1, 0));
            mesh.Positions.Add(new Point3D(0, 0, 0));
            mesh.Positions.Add(new Point3D(-1, 0, 0));
            mesh.Positions.Add(new Point3D(-1, 1, 0));

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            
            DiffuseMaterial meshMat = new DiffuseMaterial((Brush)Application.Current.Resources["patternBrush"]);

            //TextBox foundTextBox = UIHelper.FindChild<TextBox>(Application.Current.MainWindow, "myTextBoxName");

            InitializeComponent();
        }
    }
}
