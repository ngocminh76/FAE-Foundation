using System.Windows.Media;
using System.Windows.Media.Media3D;
using FAE.Foundation.App.Features.RibbedRaft;

namespace FAE.Foundation.App.Features.RibbedRaft.Drawers
{
    public static class Viewport3DDrawer
    {
        public static void Draw3D(Model3DGroup group, RibbedRaftModel foundation)
        {
            if (foundation == null) return;
            
            // Keep the lights and rotation transform, only clear geometry models
            var newChildren = new Model3DCollection();
            foreach (var child in group.Children)
            {
                if (child is Light) newChildren.Add(child);
            }
            if (newChildren.Count == 0) // fallback if lights were cleared
            {
                newChildren.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -1)));
                newChildren.Add(new AmbientLight((Color)ColorConverter.ConvertFromString("#404040")));
            }
            group.Children = newChildren;

            double L_mong = foundation.TotalLength;
            double B_mong = foundation.TotalWidth;
            
            // X is length (L_mong), Z is width (B_mong), Y is height (up)
            
            // 1. Slab
            double y_slab_center = foundation.SlabThickness / 2.0;
            if (foundation.HoleSize > 0)
            {
                double h = foundation.HoleSize;
                double wX_side = (L_mong - h) / 2.0;
                double cX_side = (L_mong + h) / 4.0;
                
                double wZ_topbot = (B_mong - h) / 2.0;
                double cZ_topbot = (B_mong + h) / 4.0;

                // Left and Right parts (full depth in Z)
                AddBox(group, new Point3D(-cX_side, y_slab_center, 0), wX_side, foundation.SlabThickness, B_mong, Colors.LightGray);
                AddBox(group, new Point3D(cX_side, y_slab_center, 0), wX_side, foundation.SlabThickness, B_mong, Colors.LightGray);

                // Top and Bottom parts (only between left and right parts)
                AddBox(group, new Point3D(0, y_slab_center, -cZ_topbot), h, foundation.SlabThickness, wZ_topbot, Colors.LightGray);
                AddBox(group, new Point3D(0, y_slab_center, cZ_topbot), h, foundation.SlabThickness, wZ_topbot, Colors.LightGray);
            }
            else
            {
                AddBox(group, new Point3D(0, y_slab_center, 0), L_mong, foundation.SlabThickness, B_mong, Colors.LightGray);
            }

            // 2. Ribs (Longitudinal - X direction)
            double y_rib_center = foundation.SlabThickness + foundation.RibHeight / 2.0;
            double z_tam_top = B_mong/2 - foundation.ConsTY;
            double z_tam_bot = z_tam_top - foundation.SpanY;
            
            AddBox(group, new Point3D(0, y_rib_center, -z_tam_top), L_mong, foundation.RibHeight, foundation.RibWidth, Colors.DarkGray); // -Z because WPF Z is towards viewer
            AddBox(group, new Point3D(0, y_rib_center, -z_tam_bot), L_mong, foundation.RibHeight, foundation.RibWidth, Colors.DarkGray);

            // 3. Ribs (Transverse - Z direction)
            double x_tam_trai = -L_mong/2 + foundation.ConsLX;
            double x_tam_phai = x_tam_trai + foundation.SpanX;

            AddBox(group, new Point3D(x_tam_trai, y_rib_center, 0), foundation.RibWidth, foundation.RibHeight, B_mong, Colors.DarkGray);
            AddBox(group, new Point3D(x_tam_phai, y_rib_center, 0), foundation.RibWidth, foundation.RibHeight, B_mong, Colors.DarkGray);

            // 4. Columns (4 corners)
            double H_col = foundation.Depth - (foundation.SlabThickness + foundation.RibHeight) + 2.0; // Protrude above ground
            double y_col_center = foundation.SlabThickness + foundation.RibHeight + H_col / 2.0;

            AddBox(group, new Point3D(x_tam_trai, y_col_center, -z_tam_top), foundation.ColumnWidth, H_col, foundation.ColumnWidth, Colors.DimGray); // Top-Left
            AddBox(group, new Point3D(x_tam_phai, y_col_center, -z_tam_top), foundation.ColumnWidth, H_col, foundation.ColumnWidth, Colors.DimGray); // Top-Right
            AddBox(group, new Point3D(x_tam_trai, y_col_center, -z_tam_bot), foundation.ColumnWidth, H_col, foundation.ColumnWidth, Colors.DimGray); // Bot-Left
            AddBox(group, new Point3D(x_tam_phai, y_col_center, -z_tam_bot), foundation.ColumnWidth, H_col, foundation.ColumnWidth, Colors.DimGray); // Bot-Right
        }

        private static void AddBox(Model3DGroup group, Point3D center, double wX, double hY, double dZ, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            double x = wX/2; double y = hY/2; double z = dZ/2;

            Point3D p0 = new Point3D(center.X - x, center.Y - y, center.Z + z);
            Point3D p1 = new Point3D(center.X + x, center.Y - y, center.Z + z);
            Point3D p2 = new Point3D(center.X + x, center.Y - y, center.Z - z);
            Point3D p3 = new Point3D(center.X - x, center.Y - y, center.Z - z);
            Point3D p4 = new Point3D(center.X - x, center.Y + y, center.Z + z);
            Point3D p5 = new Point3D(center.X + x, center.Y + y, center.Z + z);
            Point3D p6 = new Point3D(center.X + x, center.Y + y, center.Z - z);
            Point3D p7 = new Point3D(center.X - x, center.Y + y, center.Z - z);

            mesh.Positions.Add(p0); mesh.Positions.Add(p1); mesh.Positions.Add(p2); mesh.Positions.Add(p3);
            mesh.Positions.Add(p4); mesh.Positions.Add(p5); mesh.Positions.Add(p6); mesh.Positions.Add(p7);

            AddTriangle(mesh, 0, 1, 5); AddTriangle(mesh, 0, 5, 4); // Front
            AddTriangle(mesh, 1, 2, 6); AddTriangle(mesh, 1, 6, 5); // Right
            AddTriangle(mesh, 2, 3, 7); AddTriangle(mesh, 2, 7, 6); // Back
            AddTriangle(mesh, 3, 0, 4); AddTriangle(mesh, 3, 4, 7); // Left
            AddTriangle(mesh, 4, 5, 6); AddTriangle(mesh, 4, 6, 7); // Top
            AddTriangle(mesh, 3, 2, 1); AddTriangle(mesh, 3, 1, 0); // Bottom

            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            group.Children.Add(model);
        }

        private static void AddTriangle(MeshGeometry3D mesh, int p1, int p2, int p3)
        {
            mesh.TriangleIndices.Add(p1); mesh.TriangleIndices.Add(p2); mesh.TriangleIndices.Add(p3);
        }
    }
}
