using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Media3D;

namespace PalletViewer
{
    public class MyCamera : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Point SavedUVPos { get; set; }

        private Point UVPos { get; set; }

        public Point3D CameraPos { get; set; }

        public Vector3D CameraOrient { get; set; }

        public double Rad { get; set; }

        public Point3D Center { get; set; }

        public MyCamera(double _rad, Point3D _center, Point _uvpos)
        {
            Rad = _rad;
            Center = _center;
            SavedUVPos = _uvpos;
            UVPos = _uvpos;

            ShiftUVPosition(new Vector(0, 0));
        }

        public void ShiftUVPosition(Vector shift)
        {
            shift /= 100;

            UVPos = SavedUVPos - shift;

            if (UVPos.Y < 0.1)
            {
                UVPos = new Point(UVPos.X, 0.1);
            }
            else if (UVPos.Y > Math.PI / 2)
            {
                UVPos = new Point(UVPos.X, Math.PI / 2);
            }

            double RsinU = Rad * Math.Sin(UVPos.Y);
            double RcosU = Rad * Math.Cos(UVPos.Y);
            double cosV = Math.Cos(UVPos.X);
            double sinV = Math.Sin(UVPos.X);

            Vector3D sphereCoords = new Vector3D
            {
                Z = RsinU * cosV,
                X = RsinU * sinV,
                Y = RcosU
            };

            CameraPos = Center + sphereCoords;

            CameraOrient = Center - CameraPos;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraPos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraOrient)));

        }

        public void RestorePosition()
        {
            SavedUVPos = UVPos;
        }
    }
}
