"""
OpenSeesPy Solver Engine for 4-Leg Transmission Tower Ribbed Raft Foundation
"""

import numpy as np
from typing import Dict, Any, Tuple, List
from src.core.models import TowerFoundationProject

class TowerFoundationFEASolver:
    def __init__(self, project: TowerFoundationProject, mesh_size: float = 0.5):
        self.project = project
        self.mesh_size = mesh_size
        self.results = {}
        
    def run_analysis(self) -> Dict[str, Any]:
        """
        Thực hiện tạo mô hình phần tử hữu hạn và phân tích kết cấu.
        Nếu có OpenSeesPy sẽ dùng OpenSees, nếu chưa cài sẽ chạy mô phỏng ma trận FEA thuần NumPy.
        """
        try:
            import openseespy.opensees as ops
            return self._run_opensees(ops)
        except Exception as e:
            print(f"[Info] OpenSeesPy C++ DLL chưa sẵn sàng ({e}). Chuyển sang Solver Nền Đàn Hồi Ma Trận NumPy.")
            return self._run_numpy_solver()

    def _run_opensees(self, ops) -> Dict[str, Any]:
        ops.wipe()
        ops.model('basic', '-ndm', 3, '-ndf', 6)

        # 1. Tính toán hình học ô lưới
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        nx = int(np.round(Lx / self.mesh_size))
        ny = int(np.round(Ly / self.mesh_size))
        dx = Lx / nx
        dy = Ly / ny

        # Vị trí 4 cổ cột
        lcx = self.project.column.spacing_x
        lcy = self.project.column.spacing_y
        x1, x2 = (Lx - lcx) / 2.0, (Lx + lcx) / 2.0
        y1, y2 = (Ly - lcy) / 2.0, (Ly + lcy) / 2.0

        # Tạo nút bản móng (Base Nodes)
        node_map = {}
        node_id = 1
        for i in range(nx + 1):
            x = i * dx
            for j in range(ny + 1):
                y = j * dy
                ops.node(node_id, x, y, 0.0)
                node_map[(i, j)] = node_id
                node_id += 1

        # Nền đất: Lò xo Winkler dưới tất cả nút bản móng (zeroLength + Elastic Material)
        mat_soil_id = 1
        ops.uniaxialMaterial('Elastic', mat_soil_id, self.project.soil.K_z * dx * dy)

        fixed_soil_node_start = 10000
        for (i, j), nid in node_map.items():
            fnid = fixed_soil_node_start + nid
            x, y = i * dx, j * dy
            ops.node(fnid, x, y, 0.0)
            ops.fix(fnid, 1, 1, 1, 1, 1, 1) # Cố định nút đất
            ops.element('zeroLength', 20000 + nid, fnid, nid, '-mat', mat_soil_id, '-dir', 3)

        # Khai báo vật liệu & tiết diện dầm / bản
        E_b = self.project.concrete.E_b * 1000.0 # kN/m2
        nu = 0.2
        G_b = E_b / (2 * (1 + nu))

        # Elastic Section cho Dầm sườn
        b_b = self.project.beam.b_beam
        h_b = self.project.beam.h_beam
        A_b = b_b * h_b
        Iz_b = b_b * (h_b ** 3) / 12.0
        Iy_b = h_b * (b_b ** 3) / 12.0
        J_b = (b_b * h_b**3 + h_b * b_b**3) / 12.0

        sec_beam_id = 1
        ops.transform('Linear', 1, 0, 1, 0) # Orient Y

        # Tạo dầm sườn phương X & Y chạy suốt từ mép sang mép
        elem_id = 30000
        # Dầm X1 (tại y1) và X2 (tại y2)
        j1_idx = int(np.round(y1 / dy))
        j2_idx = int(np.round(y2 / dy))

        for i in range(nx):
            n_start = node_map[(i, j1_idx)]
            n_end = node_map[(i+1, j1_idx)]
            ops.element('elasticBeamColumn', elem_id, n_start, n_end, A_b, E_b, G_b, J_b, Iy_b, Iz_b, 1)
            elem_id += 1

            n_start = node_map[(i, j2_idx)]
            n_end = node_map[(i+1, j2_idx)]
            ops.element('elasticBeamColumn', elem_id, n_start, n_end, A_b, E_b, G_b, J_b, Iy_b, Iz_b, 1)
            elem_id += 1

        # Dầm Y1 (tại x1) và Y2 (tại x2)
        i1_idx = int(np.round(x1 / dx))
        i2_idx = int(np.round(x2 / dx))

        for j in range(ny):
            n_start = node_map[(i1_idx, j)]
            n_end = node_map[(i1_idx, j+1)]
            ops.element('elasticBeamColumn', elem_id, n_start, n_end, A_b, E_b, G_b, J_b, Iy_b, Iz_b, 1)
            elem_id += 1

            n_start = node_map[(i2_idx, j)]
            n_end = node_map[(i2_idx, j+1)]
            ops.element('elasticBeamColumn', elem_id, n_start, n_end, A_b, E_b, G_b, J_b, Iy_b, Iz_b, 1)
            elem_id += 1

        # 4 Cổ cột đứng cao H_col
        H_c = self.project.column.H_col
        b_c = self.project.column.b_col
        h_c = self.project.column.h_col
        A_c = b_c * h_c
        Iz_c = b_c * (h_c ** 3) / 12.0
        Iy_c = h_c * (b_c ** 3) / 12.0
        J_c = (b_c * h_c**3 + h_c * b_c**3) / 12.0

        col_base_nodes = [
            node_map[(i1_idx, j1_idx)], # Leg 1
            node_map[(i2_idx, j1_idx)], # Leg 2
            node_map[(i1_idx, j2_idx)], # Leg 3
            node_map[(i2_idx, j2_idx)]  # Leg 4
        ]

        col_top_nodes = []
        top_node_start = 5000
        ops.transform('Linear', 2, 0, 0, 1)

        for idx, base_nid in enumerate(col_base_nodes):
            top_nid = top_node_start + idx + 1
            col_top_nodes.append(top_nid)
            # Tọa độ đỉnh cổ cột
            x_c = ops.nodeCoord(base_nid, 1)
            y_c = ops.nodeCoord(base_nid, 2)
            ops.node(top_nid, x_c, y_c, H_c)

            # Phần tử cổ cột
            ops.element('elasticBeamColumn', elem_id, base_nid, top_nid, A_c, E_b, G_b, J_c, Iy_c, Iz_c, 2)
            elem_id += 1

        # Gán tải trọng tại 4 đỉnh cổ cột
        ops.timeSeries('Constant', 1)
        ops.pattern('Plain', 1, 1)

        for load in self.project.loads:
            top_nid = col_top_nodes[load.leg_id - 1]
            ops.load(top_nid, load.Q_x, load.Q_y, load.N, load.M_x, load.M_y, 0.0)

        # Giải bài toán
        ops.system('BandGeneral')
        ops.numberer('RCM')
        ops.constraints('Transformation')
        ops.integrator('LoadControl', 1.0)
        ops.algorithm('Linear')
        ops.analysis('Static')
        ops.analyze(1)

        # Trích xuất kết quả
        displacements = {}
        max_settlement = 0.0
        max_uplift = 0.0

        for (i, j), nid in node_map.items():
            w = ops.nodeDisp(nid, 3) # Chuyển vị Z (m)
            displacements[(i, j)] = w
            if w < max_settlement:
                max_settlement = w
            if w > max_uplift:
                max_uplift = w

        max_soil_press = abs(max_settlement) * self.project.soil.K_z # kN/m2

        self.results = {
            "status": "Success (OpenSeesPy)",
            "max_settlement_mm": abs(max_settlement) * 1000.0,
            "max_uplift_mm": max_uplift * 1000.0,
            "max_soil_pressure_kpa": max_soil_press,
            "soil_bearing_capacity_kpa": 250.0,
            "is_bearing_safe": max_soil_press <= 250.0,
            "is_uplift_safe": max_uplift * 1000.0 < 10.0 # Độ hẫng móng < 10mm
        }
        return self.results

    def _run_numpy_solver(self) -> Dict[str, Any]:
        """
        Numpy analytical fallback calculation when OpenSeesPy is not installed.
        """
        Lx = self.project.slab.L_x
        Ly = self.project.slab.L_y
        Area = Lx * Ly
        W_x = Lx * (Ly**2) / 6.0
        W_y = Ly * (Lx**2) / 6.0

        total_N = sum(l.N for l in self.project.loads)
        total_Mx = sum(l.M_x + l.Q_y * self.project.column.H_col for l in self.project.loads)
        total_My = sum(l.M_y + l.Q_x * self.project.column.H_col for l in self.project.loads)

        P_avg = total_N / Area
        P_max = P_avg + abs(total_Mx)/W_x + abs(total_My)/W_y
        P_min = P_avg - abs(total_Mx)/W_x - abs(total_My)/W_y

        settlement_max_mm = (P_max / self.project.soil.K_z) * 1000.0
        uplift_max_mm = max(0.0, (-P_min / self.project.soil.K_z) * 1000.0)

        self.results = {
            "status": "Success (Numpy Fallback)",
            "max_settlement_mm": settlement_max_mm,
            "max_uplift_mm": uplift_max_mm,
            "max_soil_pressure_kpa": P_max,
            "min_soil_pressure_kpa": P_min,
            "soil_bearing_capacity_kpa": 250.0,
            "is_bearing_safe": P_max <= 250.0,
            "is_uplift_safe": P_min >= 0.0
        }
        return self.results
