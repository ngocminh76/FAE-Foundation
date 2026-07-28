"""
Interactive 3D Viewport Widget with Soil Stress Contour Heatmap Visualization
"""

import numpy as np
from PySide6.QtWidgets import QWidget, QVBoxLayout
from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
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
        self.ax.set_title("KHUNG HIỂN THỊ MÓNG BÈ 3D TƯƠNG TÁC", color='#4ec9b0', fontsize=12, fontweight='bold')
        self.ax.set_xlabel("Trục X (m)", color='#cccccc')
        self.ax.set_ylabel("Trục Y (m)", color='#cccccc')
        self.ax.set_zlabel("Chiều Cao Z (m)", color='#cccccc')
        self.ax.tick_params(colors='#cccccc')
        self.canvas.draw()

    def update_scene(self, project: TowerFoundationProject, fea_results: dict = None):
        """Vẽ toàn bộ mô hình 3D móng, dầm sườn, cổ cột, lực tác dụng và Heatmap Ứng Suất Đất Nền"""
        self.ax.clear()
        self.ax.set_facecolor('#1e1e1e')
        self.ax.set_title(f"MÓNG BÈ 3D & HEATMAP ỨNG SUẤT ĐẤT NỀN: {project.name}", color='#4ec9b0', fontsize=11, fontweight='bold')
        
        Lx = project.slab.L_x
        Ly = project.slab.L_y
        h_slab = project.slab.h_slab
        h_beam = project.beam.h_beam
        b_beam = project.beam.b_beam
        Kz = project.soil.K_z

        # 1. Vẽ Đám Mây Ứng Suất Đất Nền (Soil Stress Heatmap Contour Surface)
        x_mesh = np.linspace(0, Lx, 25)
        y_mesh = np.linspace(0, Ly, 25)
        X, Y = np.meshgrid(x_mesh, y_mesh)

        # Tính phản lực đất nền P(x,y)
        total_N = sum(l.N_sls for l in project.loads)
        total_Mx = sum(l.M_x_sls + l.Q_y_sls * project.column.H_col for l in project.loads)
        total_My = sum(l.M_y_sls + l.Q_x_sls * project.column.H_col for l in project.loads)

        Area = Lx * Ly
        Wx = Lx * (Ly**2) / 6.0
        Wy = Ly * (Lx**2) / 6.0

        # Áp lực P(x,y) = N/A + My*(x - Lx/2)/Wy - Mx*(y - Ly/2)/Wx
        P_grid = (total_N / Area) + (total_My * (X - Lx/2.0) / Wy) - (total_Mx * (Y - Ly/2.0) / Wx)
        P_grid = np.maximum(0.0, P_grid) # Đất không chịu kéo (P >= 0)

        # Vẽ mặt Heatmap dưới đáy bản móng (Z = -0.05)
        Z_surface = np.full_like(X, -0.05)
        norm = cm.colors.Normalize(vmin=np.min(P_grid), vmax=np.max(P_grid))
        colors = cm.jet(norm(P_grid))

        surf = self.ax.plot_surface(X, Y, Z_surface, facecolors=colors, shade=False, alpha=0.85, rstride=1, cstride=1)

        # 2. Vẽ Bản Móng Bè Phẳng
        X_slab = [0, Lx, Lx, 0, 0]
        Y_slab = [0, 0, Ly, Ly, 0]
        Z_slab = [0, 0, 0, 0, 0]
        self.ax.plot(X_slab, Y_slab, Z_slab, color='#569cd6', linewidth=2, label='Bản móng bè')
        self.ax.plot(X_slab, Y_slab, [h_slab]*5, color='#569cd6', linestyle='--', linewidth=1)

        # 3. Vẽ 4 Dầm Sườn Chạy Suốt 2 Phương
        lcx = project.column.spacing_x
        lcy = project.column.spacing_y
        x1, x2 = (Lx - lcx) / 2.0, (Lx + lcx) / 2.0
        y1, y2 = (Ly - lcy) / 2.0, (Ly + lcy) / 2.0

        # Dầm X1 và X2
        self.ax.plot([0, Lx], [y1, y1], [h_beam, h_beam], color='#ce9178', linewidth=2.5, label='Dầm sườn X')
        self.ax.plot([0, Lx], [y2, y2], [h_beam, h_beam], color='#ce9178', linewidth=2.5)

        # Dầm Y1 và Y2
        self.ax.plot([x1, x1], [0, Ly], [h_beam, h_beam], color='#dcdcaa', linewidth=2.5, label='Dầm sườn Y')
        self.ax.plot([x2, x2], [0, Ly], [h_beam, h_beam], color='#dcdcaa', linewidth=2.5)

        # 4. Vẽ 4 Cổ Cột Điện Cao
        H_col = project.column.H_col
        col_points = [(x1, y1), (x2, y1), (x1, y2), (x2, y2)]
        
        for idx, (cx, cy) in enumerate(col_points):
            self.ax.plot([cx, cx], [cy, cy], [0, H_col], color='#4ec9b0', linewidth=4)
            load = project.loads[idx]
            if load.N < 0:
                # Kéo / Nhổ: Mũi tên đỏ hướng LÊN
                self.ax.quiver(cx, cy, H_col, 0, 0, 1.2, color='#f44747', arrow_length_ratio=0.3, linewidth=2.5)
                self.ax.text(cx, cy, H_col + 1.3, f"Leg{idx+1} Nhổ ({load.N:.0f}kN)", color='#f44747', fontsize=8)
            else:
                # Nén dồn: Mũi tên xanh hướng XUỐNG
                self.ax.quiver(cx, cy, H_col + 1.2, 0, 0, -1.2, color='#4fc1ff', arrow_length_ratio=0.3, linewidth=2.5)
                self.ax.text(cx, cy, H_col + 1.3, f"Leg{idx+1} Nén (+{load.N:.0f}kN)", color='#4fc1ff', fontsize=8)

        self.ax.set_xlabel("Trục X (m)", color='#cccccc')
        self.ax.set_ylabel("Trục Y (m)", color='#cccccc')
        self.ax.set_zlabel("Chiều Cao Z (m)", color='#cccccc')
        self.ax.tick_params(colors='#cccccc')
        self.canvas.draw()
