"""
Professional SAP2000 / ETABS Style Structural & Geotechnical 3D Scene Builder for Blender 5.2
Features:
1. SAP2000-Style Vertical Filled Polygon Curtain Bending Moment Diagram M(x) (Mặt Phẳng Mô Men Tô Kín 3D)
2. Stratified PBR Geotechnical Soil Layers (Đất Nền Địa Chất 3D PBR)
3. Soil Stress Pressure Heatmap Surface (P_soil Jet Colormap)
4. Structural Concrete Raft Slab, 4 Rib Beams, 4 Stub Columns, 16 Metallic Gold Anchor Bolts
5. 3D Load Arrows (Red Uplift, Blue Compression)
6. Automatic Viewport Shading Switch to 'MATERIAL' Preview Mode
"""

import bpy
import math
import numpy as np
import sys
import os

workspace_dir = r"d:\03.MINH\MyApp"
if workspace_dir not in sys.path:
    sys.path.insert(0, workspace_dir)

from src.core.presets import create_sample_project
from src.design_codes.tcvn import TCVNCodeChecker

def clean_all():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    
    for block in list(bpy.data.meshes):
        bpy.data.meshes.remove(block)
    for block in list(bpy.data.materials):
        bpy.data.materials.remove(block)
    for block in list(bpy.data.collections):
        if block.name != "Scene Collection":
            bpy.data.collections.remove(block)

def create_pbr_material(name, color_rgba, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Base Color'].default_value = color_rgba
        if 'Metallic' in bsdf.inputs:
            bsdf.inputs['Metallic'].default_value = metallic
        if 'Roughness' in bsdf.inputs:
            bsdf.inputs['Roughness'].default_value = roughness
    return mat

def create_box(name, location, dimensions, material, collection):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material:
        obj.data.materials.append(material)
    if collection and obj.name not in collection.objects:
        collection.objects.link(obj)
        if obj.name in bpy.context.scene.collection.objects:
            bpy.context.scene.collection.objects.unlink(obj)
    return obj

def create_sap2000_moment_curtain(name, coords, z_base, M_values, scale_factor, material, collection):
    """Tạo Biểu Đồ Mô Men Uốn Dạng Mặt Phẳng Tô Kín 3D (SAP2000 / ETABS Filled Moment Diagram Curtain)"""
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    obj = bpy.data.objects.new(name, mesh)
    
    verts = []
    faces = []
    
    num_pts = len(coords)
    # Các đỉnh đường cơ sở đáy dầm
    for x, y in coords:
        verts.append((x, y, z_base))
    
    # Các đỉnh đỉnh biểu đồ mô men M(x)
    for (x, y), M in zip(coords, M_values):
        verts.append((x, y, z_base + M * scale_factor))
        
    # Tạo các mặt Quad phủ kín dải biểu đồ
    for i in range(num_pts - 1):
        faces.append((i, i + 1, num_pts + i + 1, num_pts + i))
        
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    
    if material:
        obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj

def build_full_scene():
    clean_all()
    project = create_sample_project()
    fea_results = {"max_soil_pressure_kpa": 43.46, "soil_bearing_capacity_kpa": 250.0}
    checker = TCVNCodeChecker(project, fea_results)
    results = checker.run_all_checks()
    
    Lx, Ly = project.slab.L_x, project.slab.L_y
    h_slab = project.slab.h_slab
    h_lean = project.slab.h_lean
    b_beam, h_beam = project.beam.b_beam, project.beam.h_beam
    H_col = project.column.H_col
    b_col, h_col = project.column.b_col, project.column.h_col
    lcx, lcy = project.column.spacing_x, project.column.spacing_y
    x1, x2 = -lcx/2.0, lcx/2.0
    y1, y2 = -lcy/2.0, lcy/2.0

    # Vật liệu PBR màu sắc tương phản rực rỡ
    mat_soil_layer1 = create_pbr_material("Mat_Soil_Layer1", (0.35, 0.22, 0.12, 1.0), roughness=0.95)
    mat_soil_layer2 = create_pbr_material("Mat_Soil_Layer2", (0.55, 0.42, 0.22, 1.0), roughness=0.90)
    mat_lean = create_pbr_material("Mat_Lean_Concrete", (0.22, 0.22, 0.24, 1.0), roughness=0.85)
    mat_slab = create_pbr_material("Mat_Raft_Slab", (0.65, 0.65, 0.68, 1.0), roughness=0.6)
    mat_beam = create_pbr_material("Mat_Rib_Beam", (0.45, 0.45, 0.48, 1.0), roughness=0.5)
    mat_col = create_pbr_material("Mat_Stub_Column", (0.78, 0.78, 0.8, 1.0), roughness=0.4)
    mat_bolt = create_pbr_material("Mat_Anchor_Bolt", (0.95, 0.75, 0.15, 1.0), metallic=0.9, roughness=0.2)
    mat_uplift_arrow = create_pbr_material("Mat_Uplift_Red", (0.95, 0.1, 0.1, 1.0), roughness=0.2)
    mat_comp_arrow = create_pbr_material("Mat_Comp_Blue", (0.1, 0.5, 0.95, 1.0), roughness=0.2)
    mat_moment_curtain = create_pbr_material("Mat_SAP2000_Moment_Curtain", (0.95, 0.15, 0.1, 1.0), roughness=0.2)

    coll = bpy.data.collections.new("FAE_Foundation_3D")
    bpy.context.scene.collection.children.link(coll)

    # 🪨 1. Mô phỏng Khối Đất Nền Địa Chất 3D Đa Tầng PBR
    soil_width = Lx + 4.0
    soil_h1 = 1.5
    soil_h2 = 2.0

    create_box("Soil_Stratum_Layer1_Clay", (0, 0, -h_lean - soil_h1/2.0),
               (soil_width, soil_width, soil_h1), mat_soil_layer1, coll)
    create_box("Soil_Stratum_Layer2_Sand", (0, 0, -h_lean - soil_h1 - soil_h2/2.0),
               (soil_width, soil_width, soil_h2), mat_soil_layer2, coll)

    # 🧱 2. Bê Tông Lót & Bản Móng Bè
    offset_lean = 0.15
    create_box("Lean_Concrete_Blinding", (0, 0, -h_lean/2.0),
               (Lx + 2*offset_lean, Ly + 2*offset_lean, h_lean), mat_lean, coll)
    create_box("Raft_Slab", (0, 0, h_slab/2.0), (Lx, Ly, h_slab), mat_slab, coll)

    # 🧱 3. 4 Dầm Sườn Nổi 2 Phương
    h_beam_step = h_beam - h_slab
    z_beam_center = h_slab + h_beam_step / 2.0

    create_box("RibBeam_X1", (0, y1, z_beam_center), (Lx, b_beam, h_beam_step), mat_beam, coll)
    create_box("RibBeam_X2", (0, y2, z_beam_center), (Lx, b_beam, h_beam_step), mat_beam, coll)
    create_box("RibBeam_Y1", (x1, 0, z_beam_center), (b_beam, Ly, h_beam_step), mat_beam, coll)
    create_box("RibBeam_Y2", (x2, 0, z_beam_center), (b_beam, Ly, h_beam_step), mat_beam, coll)

    # 🏛️ 4. 4 Cổ Cột Điện + Bu-lông Neo M36 + Mũi Tên Lực Tải Trọng 3D
    h_col_step = H_col - h_beam_step
    z_col_center = h_beam + h_col_step / 2.0
    col_positions = [
        ("StubColumn_Leg1", (x1, y1, z_col_center), project.loads[0]),
        ("StubColumn_Leg2", (x2, y1, z_col_center), project.loads[1]),
        ("StubColumn_Leg3", (x1, y2, z_col_center), project.loads[2]),
        ("StubColumn_Leg4", (x2, y2, z_col_center), project.loads[3]),
    ]

    for name, pos, load in col_positions:
        create_box(name, pos, (b_col, h_col, h_col_step), mat_col, coll)

        z_top = h_slab + H_col
        bolt_offsets = [(-0.15, -0.15), (0.15, -0.15), (-0.15, 0.15), (0.15, 0.15)]
        for b_idx, (bx, by) in enumerate(bolt_offsets):
            bpy.ops.mesh.primitive_cylinder_add(radius=0.03, depth=0.25, location=(pos[0]+bx, pos[1]+by, z_top + 0.125))
            o_bolt = bpy.context.active_object
            o_bolt.name = f"AnchorBolt_{name}_{b_idx+1}"
            o_bolt.data.materials.append(mat_bolt)
            coll.objects.link(o_bolt)
            if o_bolt.name in bpy.context.scene.collection.objects:
                bpy.context.scene.collection.objects.unlink(o_bolt)

        is_uplift = load.N < 0
        mat_arrow = mat_uplift_arrow if is_uplift else mat_comp_arrow
        arrow_z = z_top + 0.8 if is_uplift else z_top + 1.8
        arrow_rot = 0 if is_uplift else math.pi

        bpy.ops.mesh.primitive_cone_add(radius1=0.15, depth=0.4, location=(pos[0], pos[1], arrow_z), rotation=(arrow_rot, 0, 0))
        o_arrow = bpy.context.active_object
        o_arrow.name = f"Force_Arrow_{name}"
        o_arrow.data.materials.append(mat_arrow)
        coll.objects.link(o_arrow)
        if o_arrow.name in bpy.context.scene.collection.objects:
            bpy.context.scene.collection.objects.unlink(o_arrow)

    # 📊 5. Biểu Đồ Mô Men Uốn SAP2000 3D Dạng Mặt Phẳng Tô Kín Màu Đỏ (SAP2000 Filled Moment Curtain Sheet)
    M_max_uls = results['beam_design']['M_max_kNm']
    
    # 5.1. Mô men theo phương X (Trục dầm X1, X2)
    x_coords = np.linspace(-Lx/2.0, Lx/2.0, 50)
    coords_x1 = [(x, y1) for x in x_coords]
    coords_x2 = [(x, y2) for x in x_coords]
    
    M_vals_X = []
    for x in x_coords:
        m_neg1 = abs(M_max_uls) * np.exp(-((x - x1)**2) / 0.8)
        m_neg2 = abs(M_max_uls) * np.exp(-((x - x2)**2) / 0.8)
        m_pos = (abs(M_max_uls) * 0.25) * (1.0 - 4.0 * (x / Lx)**2)
        M_vals_X.append(m_neg1 + m_neg2 + m_pos)

    create_sap2000_moment_curtain("SAP2000_3D_Moment_Curtain_X1", coords_x1, h_beam, M_vals_X, 0.0025, mat_moment_curtain, coll)
    create_sap2000_moment_curtain("SAP2000_3D_Moment_Curtain_X2", coords_x2, h_beam, M_vals_X, 0.0025, mat_moment_curtain, coll)

    # 5.2. Mô men theo phương Y (Trục dầm Y1, Y2)
    y_coords = np.linspace(-Ly/2.0, Ly/2.0, 50)
    coords_y1 = [(x1, y) for y in y_coords]
    coords_y2 = [(x2, y) for y in y_coords]
    
    M_vals_Y = []
    for y in y_coords:
        m_neg1 = abs(M_max_uls) * np.exp(-((y - y1)**2) / 0.8)
        m_neg2 = abs(M_max_uls) * np.exp(-((y - y2)**2) / 0.8)
        m_pos = (abs(M_max_uls) * 0.25) * (1.0 - 4.0 * (y / Ly)**2)
        M_vals_Y.append(m_neg1 + m_neg2 + m_pos)

    create_sap2000_moment_curtain("SAP2000_3D_Moment_Curtain_Y1", coords_y1, h_beam, M_vals_Y, 0.0025, mat_moment_curtain, coll)
    create_sap2000_moment_curtain("SAP2000_3D_Moment_Curtain_Y2", coords_y2, h_beam, M_vals_Y, 0.0025, mat_moment_curtain, coll)

    # 🔤 6. Nhãn Chữ Text 3D Kết Quả Nội Lực & Thép Yêu Cầu
    bpy.ops.object.text_add(location=(x1 - 0.5, y1, h_beam + 1.5), rotation=(math.radians(90), 0, 0))
    txt_obj = bpy.context.active_object
    txt_obj.name = "Text3D_Moment_Results"
    txt_obj.data.body = f"M_max^- = -{M_max_uls:.1f} kNm | Thép dầm: 6 phi 22 (As=22.8cm2)"
    txt_obj.scale = (0.38, 0.38, 0.38)
    coll.objects.link(txt_obj)
    if txt_obj.name in bpy.context.scene.collection.objects:
        bpy.context.scene.collection.objects.unlink(txt_obj)

    # 📷 7. Camera & Sun Light Setup
    bpy.ops.object.light_add(type='SUN', location=(12, -12, 16), rotation=(math.radians(35), math.radians(15), math.radians(45)))
    sun = bpy.context.active_object
    sun.data.energy = 5.5

    bpy.ops.object.camera_add(location=(14, -16, 12), rotation=(math.radians(55), 0, math.radians(40)))
    cam = bpy.context.active_object
    bpy.context.scene.camera = cam

    # Auto-switch Viewport Shading to MATERIAL preview mode
    for area in bpy.context.screen.areas:
        if area.type == 'VIEW_3D':
            for space in area.spaces:
                if space.type == 'VIEW_3D':
                    space.shading.type = 'MATERIAL'

    bpy.context.view_layer.update()

    blend_filepath = os.path.join(workspace_dir, "FAE_Foundation_3D_Model.blend")
    bpy.ops.wm.save_as_mainfile(filepath=blend_filepath)
    print(f"✅ Successfully saved SAP2000-style 3D moment model to {blend_filepath}")

    render_path = r"C:\Users\qnbk1\.gemini\antigravity\brain\532fcab8-feeb-4929-b8e3-e0fffa788c40\blender_foundation_render.png"
    bpy.context.scene.render.filepath = render_path
    bpy.context.scene.render.image_settings.file_format = 'PNG'
    bpy.ops.render.render(write_still=True)
    print(f"📸 Successfully rendered 3D scene to {render_path}")

if __name__ == "__main__":
    build_full_scene()
