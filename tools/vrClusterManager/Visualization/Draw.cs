using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace vrClusterManager
{
    public static class Draw
    {
        public static GeometryModel3D GeometryBuilder(double width, double height, Color areaColor, Color backAreaColor)
        {
            MeshGeometry3D triangleMesh = new MeshGeometry3D();

            //Points coords
            double xmin = width / -2;
            double xmax = width / 2;
            double ymin = height / -2;
            double ymax = height / 2;
            //points
            Point3D point0 = new Point3D(xmin, ymin, 0);
            Point3D point1 = new Point3D(xmax, ymin, 0);
            Point3D point2 = new Point3D(xmax, ymax, 0);
            Point3D point3 = new Point3D(xmin, ymax, 0);

            //Add points
            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);
            triangleMesh.Positions.Add(point3);
            
            // Rectangular area
            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(1);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(3);

            Material material = new DiffuseMaterial(new SolidColorBrush(areaColor));
            Material backMaterial = new DiffuseMaterial(new SolidColorBrush(backAreaColor));
            GeometryModel3D mGeometry = new GeometryModel3D(triangleMesh, material);
            mGeometry.BackMaterial = backMaterial;
            return mGeometry;
        }

    }
}
