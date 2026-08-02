using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FAE.Foundation.App
{
    public static class Viewport3DDrawer
    {
        public static void Draw3D(Model3DGroup group, 
            double L_span_X, double L_cons_L, double L_cons_R, 
            double L_span_Y, double L_cons_T, double L_cons_B, 
            double h_ban, double b_dam, double h_dam, double b_cot, double D_f)
        {
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

            double L_mong = L_cons_L + L_span_X + L_cons_R;
            double B_mong = L_cons_T + L_span_Y + L_cons_B;
            
            // X is length (L_mong), Z is width (B_mong), Y is height (up)
            
            // 1. Slab
            double y_slab_center = h_ban / 2.0;
            AddBox(group, new Point3D(0, y_slab_center, 0), L_mong, h_ban, B_mong, Colors.LightGray);

            // 2. Ribs (Longitudinal - X direction)
            double y_rib_center = h_ban + h_dam / 2.0;
            double z_tam_top = B_mong/2 - L_cons_T;
            double z_tam_bot = z_tam_top - L_span_Y;
            
            AddBox(group, new Point3D(0, y_rib_center, -z_tam_top), L_mong, h_dam, b_dam, Colors.DarkGray); // -Z because WPF Z is towards viewer
            AddBox(group, new Point3D(0, y_rib_center, -z_tam_bot), L_mong, h_dam, b_dam, Colors.DarkGray);

            // 3. Ribs (Transverse - Z direction)
            double x_tam_trai = -L_mong/2 + L_cons_L;
            double x_tam_phai = x_tam_trai + L_span_X;

            AddBox(group, new Point3D(x_tam_trai, y_rib_center, 0), b_dam, h_dam, B_mong, Colors.DarkGray);
            AddBox(group, new Point3D(x_tam_phai, y_rib_center, 0), b_dam, h_dam, B_mong, Colors.DarkGray);

            // 4. Columns (4 corners)
            double H_col = D_f - (h_ban + h_dam) + 2.0; // Protrude above ground
            double y_col_center = h_ban + h_dam + H_col / 2.0;

            AddBox(group, new Point3D(x_tam_trai, y_col_center, -z_tam_top), b_cot, H_col, b_cot, Colors.DimGray); // Top-Left
            AddBox(group, new Point3D(x_tam_phai, y_col_center, -z_tam_top), b_cot, H_col, b_cot, Colors.DimGray); // Top-Right
            AddBox(group, new Point3D(x_tam_trai, y_col_center, -z_tam_bot), b_cot, H_col, b_cot, Colors.DimGray); // Bot-Left
            AddBox(group, new Point3D(x_tam_phai, y_col_center, -z_tam_bot), b_cot, H_col, b_cot, Colors.DimGray); // Bot-Right
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
