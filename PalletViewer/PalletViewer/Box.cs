using System.Windows.Media.Media3D;

namespace PalletViewer
{
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
            Helper.Swap(ref Orient[1], ref Orient[2]);

            points = null;
        }

        public void RotFront()
        {
            Dim = new Vector3D(Dim.Y, Dim.X, Dim.Z);
            Helper.Swap(ref Orient[0], ref Orient[2]);

            points = null;
        }

        public void RotSide()
        {
            Dim = new Vector3D(Dim.X, Dim.Z, Dim.Y);
            Helper.Swap(ref Orient[0], ref Orient[1]);

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
}
