"""
Interactive 3D Viewport Widget with SOLID CONCRETE GEOMETRY (Extruded 3D Blocks)
and 3D Soil Stress Heatmap Surface Contour
"""

import numpy as np
from PySide6.QtWidgets import QWidget, QVBoxLayout
from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
from mpl_toolkits.mplot3d.art3d import Poly3DCollection
import matplotlib.cm as cm
from src.core.models import TowerFoundationProject

class Viewport3DWidget(QWidget):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.init_ui()

    def init_ui(self):
        layout = QVBoxLayout(self)
        
        self.fig = Figure(figsize=(6, 5), facecolor='#1e1e1e')
        self.canvas = FigureCanvas(self.fig)
        self.ax = self.fig.add_subplot(111, projection='3d')
        self.ax.set_facecolor('#1e1e1e')
        
        layout.addWidget(self.canvas)
        self.plot_empty_scene()

    def plot_empty_scene(self):
        self.ax.clear()
        self.ax.set_title("KHUNG HIỂN THỊ MÓNG BÈ 3D ĐẶC TƯƠNG TÁC", color='#4ec9b0', fontsize=12, fontweight='bold')
        self.ax.set_xlabel("Trục X (m)", color='#cccccc')
        self.ax.set_ylabel("Trục Y (m)", color='#cccccc')
        self.ax.set_zlabel("Chiều Cao Z (m)", color='#cccccc')
        self.ax.tick_params(colors='#cccccc')
        self.canvas.draw()

    def _draw_solid_box(self, x0, y0, z0, dx, dy, dz, face_color='#808080', edge_color='#404040', alpha=0.9):
        """Vẽ một khối hộp 3D bê tông đặc (Solid 3D Concrete Box)"""
        x1, y1, z1 = x0 + dx, y0 + dy, z0 + dz
        vertices = np.array([
            [x0, y0, z0], [x1, y0, z0], [x1, y1, z0], [x0, y1, z0], # Đáy dưới
            [x0, y0, z1], [x1, y0, z1], [x1, y1, z1], [x0, y1, z1]  # Đáy trên
        ])
        faces = [
            [vertices[0], vertices[1], vertices[2], vertices[3]], # Đáy dưới
            [vertices[4], vertices[5], vertices[6], vertices[7]], # Đáy trên
            [vertices[0], vertices[1], vertices[5], vertices[4]], # Mặt trước
            [vertices[2], vertices[3], vertices[7], vertices[6]], # Mặt sau
            [vertices[1], vertices[2], vertices[6], vertices[5]], # Mặt phải
            [vertices[0], vertices[3], vertices[7], vertices[4]]  # Mặt trái
        ]
        poly = Poly3DCollection(faces, facecolors=face_color, edgecolors=edge_color, alpha=alpha, linewidths=0.5)
        self.ax.add_collection3d(poly)

    def update_scene(self, project: TowerFoundationProject, fea_results: dict = None):
        """Vẽ toàn bộ Mô hình Bê tông 3D Đặc, 4 Cổ cột, Dầm sườn, Bu lông neo & Heatmap Ứng Suất Đất"""
        self.ax.clear()
        self.ax.set_facecolor('#1e1e1e')
        self.ax.set_title(f"MÓNG BÈ BÊ TÔNG 3D ĐẶC & HEATMAP ĐẤT NỀN: {project.name}", color='#4ec9b0', fontsize=11, fontweight='bold')
        
        Lx = project.slab.L_x
        Ly = project.slab.L_y
        h_slab = project.slab.h_slab
        h_lean = project.slab.h_lean
        h_beam = project.beam.h_beam
        b_beam = project.beam.b_beam
        H_col = project.column.H_col
        b_col = project.column.b_col
        h_col = project.column.h_col

        # 1. Vẽ Lớp Bê Tông Lót (Lean Concrete Blinding 3D Box)
        offset_lean = 0.1 # Nhô rộng 10cm
        self._draw_solid_box(-offset_lean, -offset_lean, -h_lean, Lx + 2*offset_lean, Ly + 2*offset_lean, h_lean,
                             face_color='#4a4a4a', edge_color='#2d2d2d', alpha=0.95)

        # 2. Vẽ Đám Mây Ứng Suất Đất Nền (3D Soil Stress Contour Heatmap)
        x_mesh = np.linspace(0, Lx, 20)
        y_mesh = np.linspace(0, Ly, 20)
        X, Y = np.meshgrid(x_mesh, y_mesh)

        total_N = sum(l.N_sls for l in project.loads)
        total_Mx = sum(l.M_x_sls + l.Q_y_sls * project.column.H_col for l in project.loads)
        total_My = sum(l.M_y_sls + l.Q_x_sls * project.column.H_col for l in project.loads)

        Area = Lx * Ly
        Wx = Lx * (Ly**2) / 6.0
        Wy = Ly * (Lx**2) / 6.0

        P_grid = (total_N / Area) + (total_My * (X - Lx/2.0) / Wy) - (total_Mx * (Y - Ly/2.0) / Wx)
        P_grid = np.maximum(0.0, P_grid)

        # Mặt Heatmap phủ trên mặt trên bản móng bè
        norm = cm.colors.Normalize(vmin=np.min(P_grid), vmax=np.max(P_grid))
        colors = cm.jet(norm(P_grid))
        self.ax.plot_surface(X, Y, np.full_like(X, h_slab + 0.01), facecolors=colors, shade=False, alpha=0.75, rstride=1, cstride=1)

        # 3. Vẽ Bản Móng Bè Phẳng 3D Đặc (Solid Raft Slab Box)
        self._draw_solid_box(0, 0, 0, Lx, Ly, h_slab, face_color='#969696', edge_color='#505050', alpha=0.7)

        # 4. Vẽ 4 Dầm Sườn Nổi 3D Đặc Chạy Suốt 2 Phương
        lcx = project.column.spacing_x
        lcy = project.column.spacing_y
        x1, x2 = (Lx - lcx) / 2.0, (Lx + lcx) / 2.0
        y1, y2 = (Ly - lcy) / 2.0, (Ly + lcy) / 2.0

        # 2 Dầm phương X chạy suốt từ 0 sang Lx
        self._draw_solid_box(0, y1 - b_beam/2.0, h_slab, Lx, b_beam, h_beam - h_slab, face_color='#b0b0b0', edge_color='#333333', alpha=0.95)
        self._draw_solid_box(0, y2 - b_beam/2.0, h_slab, Lx, b_beam, h_beam - h_slab, face_color='#b0b0b0', edge_color='#333333', alpha=0.95)

        # 2 Dầm phương Y chạy suốt từ 0 sang Ly
        self._draw_solid_box(x1 - b_beam/2.0, 0, h_slab, b_beam, Ly, h_beam - h_slab, face_color='#a0a0a0', edge_color='#333333', alpha=0.95)
        self._draw_solid_box(x2 - b_beam/2.0, 0, h_slab, b_beam, Ly, h_beam - h_slab, face_color='#a0a0a0', edge_color='#333333', alpha=0.95)

        # 5. Vẽ 4 Cổ Cột Điện Cao 3D Đặc + Bu Lông Neo + Mũi Tên Lực 3D
        col_points = [(x1, y1), (x2, y1), (x1, y2), (x2, y2)]
        
        for idx, (cx, cy) in enumerate(col_points):
            # Cổ cột bê tông 3D đặc
            self._draw_solid_box(cx - b_col/2.0, cy - h_col/2.0, h_beam, b_col, h_col, H_col - (h_beam - h_slab),
                                 face_color='#c8c8c8', edge_color='#222222', alpha=1.0)
            
            # Cụm 4 Bu lông neo trên đỉnh cổ cột
            z_top = h_slab + H_col
            bolt_offsets = [(-0.15, -0.15), (0.15, -0.15), (-0.15, 0.15), (0.15, 0.15)]
            for bx, by in bolt_offsets:
                self.ax.plot([cx + bx, cx + bx], [cy + by, cy + by], [z_top, z_top + 0.15], color='#ffcc00', linewidth=3)

            # Mũi tên Lực tác dụng 3D
            load = project.loads[idx]
            if load.N < 0:
                # Kéo / Nhổ: Mũi tên đỏ hướng LÊN
                self.ax.quiver(cx, cy, z_top + 0.2, 0, 0, 1.2, color='#f44747', arrow_length_ratio=0.3, linewidth=3)
                self.ax.text(cx, cy, z_top + 1.5, f"Leg{idx+1} Nhổ ({load.N:.0f}kN)", color='#f44747', fontsize=9, fontweight='bold')
            else:
                # Nén dồn: Mũi tên xanh hướng XUỐNG
                self.ax.quiver(cx, cy, z_top + 1.4, 0, 0, -1.2, color='#4fc1ff', arrow_length_ratio=0.3, linewidth=3)
                self.ax.text(cx, cy, z_top + 1.5, f"Leg{idx+1} Nén (+{load.N:.0f}kN)", color='#4fc1ff', fontsize=9, fontweight='bold')

        self.ax.set_xlim([-1, Lx + 1])
        self.ax.set_ylim([-1, Ly + 1])
        self.ax.set_zlim([-h_lean - 0.5, H_col + h_slab + 2.5])
        self.ax.set_xlabel("Trục X (m)", color='#cccccc')
        self.ax.set_ylabel("Trục Y (m)", color='#cccccc')
        self.ax.set_zlabel("Chiều Cao Z (m)", color='#cccccc')
        self.ax.tick_params(colors='#cccccc')
        self.canvas.draw()
