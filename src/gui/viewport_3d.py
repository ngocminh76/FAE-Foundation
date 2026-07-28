"""
Interactive 3D Viewport Widget with 3 VIEW MODES:
Mode 1: Solid 3D Geometry & Load Vectors
Mode 2: 3D Deformed Shape & Settlement / Uplift Simulation (Phóng đại biến dạng 3D)
Mode 3: 3D Soil Stress Heatmap Contour Surface
"""

import numpy as np
from PySide6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton, QLabel, QComboBox
from matplotlib.backends.backend_qtagg import FigureCanvasQTAgg as FigureCanvas
from matplotlib.figure import Figure
from mpl_toolkits.mplot3d.art3d import Poly3DCollection
import matplotlib.cm as cm
from src.core.models import TowerFoundationProject

class Viewport3DWidget(QWidget):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.current_project = None
        self.current_fea_results = None
        self.init_ui()

    def init_ui(self):
        layout = QVBoxLayout(self)
        
        # Thanh Công Cụ Điều Khiển View Modes & Zoom
        tb_layout = QHBoxLayout()
        
        lbl_view_mode = QLabel("👁️ Chế độ View:")
        lbl_view_mode.setStyleSheet("color: #4ec9b0; font-weight: bold;")
        tb_layout.addWidget(lbl_view_mode)

        self.combo_view_mode = QComboBox()
        self.combo_view_mode.addItems([
            "1. Mô Hình 3D Đặc & Tải Trọng (Solid Geometry & Loads)",
            "2. Mô Phỏng Biến Dạng Lún & Nhổ 3D (3D Deformed Mesh & Settlement)",
            "3. Heatmap Ứng Suất Đất Nền (Soil Pressure Contour)"
        ])
        self.combo_view_mode.currentIndexChanged.connect(self.on_view_mode_changed)
        tb_layout.addWidget(self.combo_view_mode)

        tb_layout.addStretch()

        btn_reset = QPushButton("🔄 Reset 3D")
        btn_reset.clicked.connect(self.reset_camera_view)
        tb_layout.addWidget(btn_reset)

        btn_top = QPushButton("🔝 Mặt Bằng (Top)")
        btn_top.clicked.connect(self.set_top_view)
        tb_layout.addWidget(btn_top)

        btn_side = QPushButton("👁️ Mặt Đứng (Side)")
        btn_side.clicked.connect(self.set_side_view)
        tb_layout.addWidget(btn_side)

        layout.addLayout(tb_layout)

        # Matplotlib 3D Canvas
        self.fig = Figure(figsize=(6, 5), facecolor='#1e1e1e')
        self.canvas = FigureCanvas(self.fig)
        self.ax = self.fig.add_subplot(111, projection='3d')
        self.ax.set_facecolor('#1e1e1e')
        
        # Kết nối sự kiện con lăn chuột Zoom
        self.canvas.mpl_connect('scroll_event', self._on_scroll)

        layout.addWidget(self.canvas)
        self.plot_empty_scene()

    def _on_scroll(self, event):
        if event.inaxes != self.ax:
            return

        base_scale = 1.15
        scale_factor = (1.0 / base_scale) if event.button == 'up' else (base_scale if event.button == 'down' else 1.0)

        xlim, ylim, zlim = self.ax.get_xlim(), self.ax.get_ylim(), self.ax.get_zlim()
        x_mid, y_mid, z_mid = (xlim[0]+xlim[1])/2.0, (ylim[0]+ylim[1])/2.0, (zlim[0]+zlim[1])/2.0
        x_range, y_range, z_range = (xlim[1]-xlim[0])*scale_factor/2.0, (ylim[1]-ylim[0])*scale_factor/2.0, (zlim[1]-zlim[0])*scale_factor/2.0

        self.ax.set_xlim([x_mid - x_range, x_mid + x_range])
        self.ax.set_ylim([y_mid - y_range, y_mid + y_range])
        self.ax.set_zlim([z_mid - z_range, z_mid + z_range])
        self.canvas.draw_idle()

    def reset_camera_view(self):
        self.ax.view_init(elev=25, azim=-55)
        self.canvas.draw_idle()

    def set_top_view(self):
        self.ax.view_init(elev=90, azim=-90)
        self.canvas.draw_idle()

    def set_side_view(self):
        self.ax.view_init(elev=0, azim=-90)
        self.canvas.draw_idle()

    def on_view_mode_changed(self, index):
        if self.current_project:
            self.update_scene(self.current_project, self.current_fea_results)

    def plot_empty_scene(self):
        self.ax.clear()
        self.ax.set_title("KHUNG HIỂN THỊ MÓNG BÈ 3D ĐẶC TƯƠNG TÁC", color='#4ec9b0', fontsize=12, fontweight='bold')
        self.ax.set_xlabel("Trục X (m)", color='#cccccc')
        self.ax.set_ylabel("Trục Y (m)", color='#cccccc')
        self.ax.set_zlabel("Chiều Cao Z (m)", color='#cccccc')
        self.ax.tick_params(colors='#cccccc')
        self.canvas.draw()

    def _draw_solid_box(self, x0, y0, z0, dx, dy, dz, face_color='#808080', edge_color='#404040', alpha=0.9):
        x1, y1, z1 = x0 + dx, y0 + dy, z0 + dz
        vertices = np.array([
            [x0, y0, z0], [x1, y0, z0], [x1, y1, z0], [x0, y1, z0],
            [x0, y0, z1], [x1, y0, z1], [x1, y1, z1], [x0, y1, z1]
        ])
        faces = [
            [vertices[0], vertices[1], vertices[2], vertices[3]],
            [vertices[4], vertices[5], vertices[6], vertices[7]],
            [vertices[0], vertices[1], vertices[5], vertices[4]],
            [vertices[2], vertices[3], vertices[7], vertices[6]],
            [vertices[1], vertices[2], vertices[6], vertices[5]],
            [vertices[0], vertices[3], vertices[7], vertices[4]]
        ]
        poly = Poly3DCollection(faces, facecolors=face_color, edgecolors=edge_color, alpha=alpha, linewidths=0.5)
        self.ax.add_collection3d(poly)

    def update_scene(self, project: TowerFoundationProject, fea_results: dict = None):
        self.current_project = project
        self.current_fea_results = fea_results
        
        self.ax.clear()
        self.ax.set_facecolor('#1e1e1e')
        mode = self.combo_view_mode.currentIndex()

        Lx, Ly = project.slab.L_x, project.slab.L_y
        h_slab, h_lean = project.slab.h_slab, project.slab.h_lean
        h_beam, b_beam = project.beam.h_beam, project.beam.b_beam
        H_col, b_col, h_col = project.column.H_col, project.column.b_col, project.column.h_col
        lcx, lcy = project.column.spacing_x, project.column.spacing_y
        x1, x2 = (Lx - lcx) / 2.0, (Lx + lcx) / 2.0
        y1, y2 = (Ly - lcy) / 2.0, (Ly + lcy) / 2.0

        if mode == 1:
            # MODE 2: MÔ PHỎNG BIẾN DẠNG LÚN & NHỔ 3D (3D DEFORMED SHAPE)
            self.ax.set_title(f"📉 MÔ PHỎNG BIẾN DẠNG LÚN & NHỔ 3D (Phóng Đại Scale x300): {project.name}", color='#4ec9b0', fontsize=11, fontweight='bold')
            
            x_mesh = np.linspace(0, Lx, 25)
            y_mesh = np.linspace(0, Ly, 25)
            X, Y = np.meshgrid(x_mesh, y_mesh)

            total_N = sum(l.N_sls for l in project.loads)
            total_Mx = sum(l.M_x_sls + l.Q_y_sls * H_col for l in project.loads)
            total_My = sum(l.M_y_sls + l.Q_x_sls * H_col for l in project.loads)

            Area = Lx * Ly
            Wx = Lx * (Ly**2) / 6.0
            Wy = Ly * (Lx**2) / 6.0

            # Tính độ lún w(x,y) (mét)
            P_grid = (total_N / Area) + (total_My * (X - Lx/2.0) / Wy) - (total_Mx * (Y - Ly/2.0) / Wx)
            w_grid = P_grid / project.soil.K_z # mét

            # Phóng đại biến dạng x300 lần để mắt thường nhìn thấy rõ độ võng/nghiêng
            scale_factor = 300.0
            Z_deformed = h_slab + w_grid * scale_factor

            # Tô màu mặt biến dạng: Xanh dương (Lún), Vàng/Đỏ (Hẫng nhổ)
            norm = cm.colors.Normalize(vmin=np.min(w_grid*1000), vmax=np.max(w_grid*1000))
            colors = cm.coolwarm(norm(w_grid*1000))

            surf = self.ax.plot_surface(X, Y, Z_deformed, facecolors=colors, shade=True, alpha=0.9, rstride=1, cstride=1)

            # Vẽ vị trí nghiêng 4 cổ cột
            col_points = [(x1, y1), (x2, y1), (x1, y2), (x2, y2)]
            for idx, (cx, cy) in enumerate(col_points):
                w_col = ((total_N / Area) + (total_My * (cx - Lx/2.0) / Wy) - (total_Mx * (cy - Ly/2.0) / Wx)) / project.soil.K_z
                z_base_def = h_slab + w_col * scale_factor
                self.ax.plot([cx, cx], [cy, cy], [z_base_def, z_base_def + H_col], color='#ffcc00', linewidth=4, label='Cổ cột nghiêng' if idx==0 else "")

            # Ghi chú biến dạng max
            w_max_mm = np.max(w_grid) * 1000.0
            w_min_mm = np.min(w_grid) * 1000.0
            self.ax.text2D(0.05, 0.92, f"• Lún max: {w_max_mm:.2f} mm | Nhổ max: {max(0, -w_min_mm):.2f} mm", transform=self.ax.transAxes, color='#4ec9b0', fontsize=10, fontweight='bold')

        elif mode == 2:
            # MODE 3: HEATMAP ỨNG SUẤT ĐẤT NỀN
            self.ax.set_title(f"📊 HEATMAP ỨNG SUẤT ĐẤT NỀN (P_soil = Kz * w): {project.name}", color='#4ec9b0', fontsize=11, fontweight='bold')
            x_mesh = np.linspace(0, Lx, 25)
            y_mesh = np.linspace(0, Ly, 25)
            X, Y = np.meshgrid(x_mesh, y_mesh)

            total_N = sum(l.N_sls for l in project.loads)
            total_Mx = sum(l.M_x_sls + l.Q_y_sls * H_col for l in project.loads)
            total_My = sum(l.M_y_sls + l.Q_x_sls * project.column.H_col for l in project.loads)

            Area, Wx, Wy = Lx * Ly, Lx * (Ly**2) / 6.0, Ly * (Lx**2) / 6.0
            P_grid = np.maximum(0.0, (total_N / Area) + (total_My * (X - Lx/2.0) / Wy) - (total_Mx * (Y - Ly/2.0) / Wx))

            norm = cm.colors.Normalize(vmin=np.min(P_grid), vmax=np.max(P_grid))
            colors = cm.jet(norm(P_grid))
            self.ax.plot_surface(X, Y, np.full_like(X, h_slab), facecolors=colors, shade=False, alpha=0.85, rstride=1, cstride=1)

        else:
            # MODE 1: KẾT CẤU 3D ĐẶC & TẢI TRỌNG
            self.ax.set_title(f"🧱 MÓNG BÈ BÊ TÔNG 3D ĐẶC & TẢI TRỌNG: {project.name}", color='#4ec9b0', fontsize=11, fontweight='bold')
            
            # Lớp bê tông lót
            offset_lean = 0.1
            self._draw_solid_box(-offset_lean, -offset_lean, -h_lean, Lx + 2*offset_lean, Ly + 2*offset_lean, h_lean,
                                 face_color='#4a4a4a', edge_color='#2d2d2d', alpha=0.95)

            # Bản móng bè đặc
            self._draw_solid_box(0, 0, 0, Lx, Ly, h_slab, face_color='#969696', edge_color='#505050', alpha=0.7)

            # 4 Dầm sườn 3D đặc
            self._draw_solid_box(0, y1 - b_beam/2.0, h_slab, Lx, b_beam, h_beam - h_slab, face_color='#b0b0b0', edge_color='#333333', alpha=0.95)
            self._draw_solid_box(0, y2 - b_beam/2.0, h_slab, Lx, b_beam, h_beam - h_slab, face_color='#b0b0b0', edge_color='#333333', alpha=0.95)
            self._draw_solid_box(x1 - b_beam/2.0, 0, h_slab, b_beam, Ly, h_beam - h_slab, face_color='#a0a0a0', edge_color='#333333', alpha=0.95)
            self._draw_solid_box(x2 - b_beam/2.0, 0, h_slab, b_beam, Ly, h_beam - h_slab, face_color='#a0a0a0', edge_color='#333333', alpha=0.95)

            # 4 Cổ cột 3D đặc + Bu lông neo + Lực 3D
            col_points = [(x1, y1), (x2, y1), (x1, y2), (x2, y2)]
            for idx, (cx, cy) in enumerate(col_points):
                self._draw_solid_box(cx - b_col/2.0, cy - h_col/2.0, h_beam, b_col, h_col, H_col - (h_beam - h_slab),
                                     face_color='#c8c8c8', edge_color='#222222', alpha=1.0)
                
                z_top = h_slab + H_col
                bolt_offsets = [(-0.15, -0.15), (0.15, -0.15), (-0.15, 0.15), (0.15, 0.15)]
                for bx, by in bolt_offsets:
                    self.ax.plot([cx + bx, cx + bx], [cy + by, cy + by], [z_top, z_top + 0.15], color='#ffcc00', linewidth=3)

                load = project.loads[idx]
                if load.N < 0:
                    self.ax.quiver(cx, cy, z_top + 0.2, 0, 0, 1.2, color='#f44747', arrow_length_ratio=0.3, linewidth=3)
                    self.ax.text(cx, cy, z_top + 1.5, f"Leg{idx+1} Nhổ ({load.N:.0f}kN)", color='#f44747', fontsize=9, fontweight='bold')
                else:
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
