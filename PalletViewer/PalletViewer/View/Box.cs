using System.Windows.Media.Media3D;

namespace PalletViewer
{
    public class Box
    {
        public Point3D S { get; set; }
        public Vector3D Dim { get; set; }
		public int[] Orient { get; set; }
		private Point3D[] points;

        public Box(Vector3D dim, int[] orient)
        {
            Dim = dim;
			S = new Point3D(0, 0, 0);
            Orient = new int[] { orient[0], orient[1], orient[2] };
        }

        public Box(Vector3D dim, Point3D start, int[] orient)
        {
            Dim = dim;
			S = start;
            Orient = new int[] { orient[0], orient[1], orient[2] };
        }

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

		public void Translate(Vector3D shift)
        {
            S += shift;

			points = null;
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

		public Box Copy()
		{
			return new Box(Dim, S, Orient);
		}
    }

    public class BoxFactory
    {
        private Vector3D Dimensions { get; set; }

        private int[] Orient { get; set; }

        private Point3D Start { get; set; }

        // params of standart box
        public BoxFactory(double sHeight, double sWidth, double sLength)
        {
            Dimensions = new Vector3D(sWidth, sHeight, sLength);

            Orient = new int[] { 0, 1, 2 };

            Start = new Point3D(0, 0, 0);
        }

        public void SetOrient(int[] orient)
        {
            Orient = orient;
        }

        public Box GenBox()
        {
            return new Box(Dimensions, Start, Orient);
        }
    }
}
