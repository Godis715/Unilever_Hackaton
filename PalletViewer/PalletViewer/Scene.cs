﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PalletViewer
{

    public class Scene : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        private Point3D SceneCenter { get; set; }

        public MyCamera Camera { get; set; }

        public void RestoreMesh(MeshGeometry3D[] meshes)
        {
            UpMesh = meshes[0];
            FrontMesh = meshes[1];
            SideMesh = meshes[2];

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UpMesh)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrontMesh)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SideMesh)));
        }

        public Scene(Point3D _scenter)
        {
            SceneCenter = _scenter;
            Camera = new MyCamera(50.0, _scenter, new Point(Math.PI / 4, Math.PI / 4));
        }

        public Point StartMousePos { get; set; }

        public Point LastMousePos { get; set; }
    }
}