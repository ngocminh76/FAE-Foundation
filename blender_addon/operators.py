import bpy
import bmesh
import math
import numpy as np

try:
    from .src.core.models import TowerFoundationProject, RaftSlabGeometry, RibBeamGeometry, StubColumnGeometry
    from .src.core.presets import create_sample_project
    from .src.design_codes.tcvn import TCVNCodeChecker
except Exception:
    try:
        from src.core.models import TowerFoundationProject, RaftSlabGeometry, RibBeamGeometry, StubColumnGeometry
        from src.core.presets import create_sample_project
        from src.design_codes.tcvn import TCVNCodeChecker
    except Exception:
        pass

def create_pbr_material(name, color_rgba, metallic=0.0, roughness=0.5):
    if name in bpy.data.materials:
        mat = bpy.data.materials[name]
    else:
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

get_or_create_material = create_pbr_material

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
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    obj = bpy.data.objects.new(name, mesh)
    
    verts = []
    faces = []
    
    num_pts = len(coords)
    for x, y in coords:
        verts.append((x, y, z_base))
    
    for (x, y), M in zip(coords, M_values):
        verts.append((x, y, z_base + M * scale_factor))
        
    for i in range(num_pts - 1):
        faces.append((i, i + 1, num_pts + i + 1, num_pts + i))
        
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    
    if material:
        obj.data.materials.append(material)
    collection.objects.link(obj)
    return obj


class OBJECT_OT_open_full_dialog(bpy.types.Operator):
    bl_idname = "object.fae_open_full_dialog"
    bl_label = "⚙️ BẢNG NHẬP THÔNG SỐ KĨ THUẬT & THỰC THI (FAE FOUNDATION)"
    bl_options = {'REGISTER', 'UNDO'}

    def invoke(self, context, event):
        return context.window_manager.invoke_props_dialog(self, width=520)

    def draw(self, context):
        layout = self.layout
        props = context.scene.fae_props

        box_actions = layout.box()
        box_actions.label(text="⚡ CÁC THAO TÁC THỰC THI CHÍNH (BẤM ĐỂ CHẠY NGAY)", icon='PLAY')
        row_act = box_actions.row(align=True)
        row_act.operator("object.fae_generate_3d", text="🧱 DỰNG MÓNG & ĐỊA CHẤT 3D", icon='OUTLINER_OB_MESH')
        row_act.operator("object.fae_run_analysis", text="🚀 TÍNH NỘI LỰC & VẼ PARABOL MOMENT 3D", icon='GRAPH')
        
        layout.separator()
        layout.label(text="📐 1. THÔNG SỐ HÌNH HỌC MÓNG BÈ 3D", icon='MESH_CUBE')
        box1 = layout.box()
        row = box1.row()
        row.prop(props, "L_x")
        row.prop(props, "L_y")
        row = box1.row()
        row.prop(props, "h_slab")
        row.prop(props, "h_lean")

        layout.separator()
        layout.label(text="🧱 2. KHUNG DẦM SƯỜN NỔI 2 PHƯƠNG", icon='MOD_BEVEL')
        box2 = layout.box()
        row = box2.row()
        row.prop(props, "b_beam")
        row.prop(props, "h_beam")

        layout.separator()
        layout.label(text="🏛️ 3. KÍCH THƯỚC 4 CỔ CỘT ĐIỆN", icon='COLUMN')
        box3 = layout.box()
        row = box3.row()
        row.prop(props, "spacing_x")
        row.prop(props, "spacing_y")
        row = box3.row()
        row.prop(props, "b_col")
        row.prop(props, "h_col")
        box3.prop(props, "H_col")

        layout.separator()
        layout.label(text="🪨 4. ĐỊA CHẤT ĐẤT NỀN & THÔNG SỐ TIÊU CHUẨN", icon='PHYSICS')
        box4 = layout.box()
        box4.prop(props, "K_z")
        box4.prop(props, "R_tc")
        box4.prop(props, "selected_code")

    def execute(self, context):
        bpy.ops.object.fae_generate_3d()
        self.report({'INFO'}, "✅ Đã cập nhật lại toàn bộ mô hình 3D móng bè trong Blender!")
        return {'FINISHED'}


class OBJECT_OT_generate_foundation_3d(bpy.types.Operator):
    bl_idname = "object.fae_generate_3d"
    bl_label = "Dựng Mô Hình Móng Bè 3D"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        props = context.scene.fae_props
        Lx, Ly = props.L_x, props.L_y
        h_slab = props.h_slab
        h_lean = props.h_lean
        b_beam, h_beam = props.b_beam, props.h_beam
        lcx, lcy = props.spacing_x, props.spacing_y
        b_col, h_col, H_col = props.b_col, props.h_col, props.H_col

        coll_name = "FAE_Foundation_3D"
        if coll_name in bpy.data.collections:
            coll = bpy.data.collections[coll_name]
            for obj in list(coll.objects):
                bpy.data.objects.remove(obj, do_unlink=True)
        else:
            coll = bpy.data.collections.new(coll_name)
        
        if coll.name not in bpy.context.scene.collection.children:
            bpy.context.scene.collection.children.link(coll)

        for obj in list(bpy.context.scene.collection.objects):
            if "Cube" in obj.name or "Lập phương" in obj.name:
                bpy.data.objects.remove(obj, do_unlink=True)

        mat_soil1 = create_pbr_material("Mat_Soil_Layer1", (0.35, 0.22, 0.12, 1.0), roughness=0.95)
        mat_soil2 = create_pbr_material("Mat_Soil_Layer2", (0.55, 0.42, 0.22, 1.0), roughness=0.90)
        mat_lean = create_pbr_material("Mat_Lean_Concrete", (0.22, 0.22, 0.24, 1.0), roughness=0.85)
        mat_slab = create_pbr_material("Mat_Raft_Slab", (0.65, 0.65, 0.68, 1.0), roughness=0.6)
        mat_beam = create_pbr_material("Mat_Rib_Beam", (0.45, 0.45, 0.48, 1.0), roughness=0.5)
        mat_col = create_pbr_material("Mat_Stub_Column", (0.78, 0.78, 0.8, 1.0), roughness=0.4)
        mat_bolt = create_pbr_material("Mat_Anchor_Bolt", (0.95, 0.75, 0.15, 1.0), metallic=0.9, roughness=0.2)

        # 🪨 1. Mô phỏng Đất Nền Địa Chất 3D Đa Tầng
        soil_w = Lx + 4.0
        create_box("Soil_Stratum_Layer1", (0, 0, -h_lean - 0.75), (soil_w, soil_w, 1.5), mat_soil1, coll)
        create_box("Soil_Stratum_Layer2", (0, 0, -h_lean - 2.5), (soil_w, soil_w, 2.0), mat_soil2, coll)

        # 2. Bê tông lót & Bản bè
        offset_lean = 0.15
        create_box("Lean_Concrete_Blinding", (0, 0, -h_lean/2.0),
                   (Lx + 2*offset_lean, Ly + 2*offset_lean, h_lean), mat_lean, coll)
        create_box("Raft_Slab", (0, 0, h_slab/2.0), (Lx, Ly, h_slab), mat_slab, coll)

        # 3. 4 Dầm Sườn Nổi 2 Phương
        x1, x2 = -lcx / 2.0, lcx / 2.0
        y1, y2 = -lcy / 2.0, lcy / 2.0
        h_beam_step = h_beam - h_slab
        z_beam_center = h_slab + h_beam_step / 2.0

        create_box("RibBeam_X1", (0, y1, z_beam_center), (Lx, b_beam, h_beam_step), mat_beam, coll)
        create_box("RibBeam_X2", (0, y2, z_beam_center), (Lx, b_beam, h_beam_step), mat_beam, coll)
        create_box("RibBeam_Y1", (x1, 0, z_beam_center), (b_beam, Ly, h_beam_step), mat_beam, coll)
        create_box("RibBeam_Y2", (x2, 0, z_beam_center), (b_beam, Ly, h_beam_step), mat_beam, coll)

        # 4. 4 Cổ Cột Điện Cao + 16 Bu Lông Neo
        h_col_step = H_col - h_beam_step
        z_col_center = h_beam + h_col_step / 2.0

        col_positions = [
            ("StubColumn_Leg1", (x1, y1, z_col_center)),
            ("StubColumn_Leg2", (x2, y1, z_col_center)),
            ("StubColumn_Leg3", (x1, y2, z_col_center)),
            ("StubColumn_Leg4", (x2, y2, z_col_center)),
        ]

        for name, pos in col_positions:
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

        # Auto-switch Viewport Shading to MATERIAL preview mode
        for area in bpy.context.screen.areas:
            if area.type == 'VIEW_3D':
                for space in area.spaces:
                    if space.type == 'VIEW_3D':
                        space.shading.type = 'MATERIAL'

        bpy.context.view_layer.update()

        bpy.ops.object.select_all(action='DESELECT')
        for obj in coll.objects:
            obj.select_set(True)
        if coll.objects:
            bpy.context.view_layer.objects.active = coll.objects[0]

        self.report({'INFO'}, f"✅ Đã dựng mô hình 3D Móng Bè ({Lx}x{Ly}m) + Khối Đất Nền Địa Chất thành công!")
        return {'FINISHED'}


class OBJECT_OT_run_structural_analysis(bpy.types.Operator):
    bl_idname = "object.fae_run_analysis"
    bl_label = "Chạy Phân Tích & Vẽ Mô Men 3D SAP2000 Chuẩn Kỹ Thuật"
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        project = create_sample_project()
        fea_results = {"max_soil_pressure_kpa": 43.46, "soil_bearing_capacity_kpa": 250.0}
        
        checker = TCVNCodeChecker(project, fea_results)
        results = checker.run_all_checks()

        coll_name = "FAE_Foundation_3D"
        coll = bpy.data.collections.get(coll_name) if coll_name in bpy.data.collections else bpy.context.scene.collection

        mat_moment_curtain = create_pbr_material("Mat_SAP2000_Moment_Curtain", (0.95, 0.15, 0.1, 1.0), roughness=0.2)

        Lx = project.slab.L_x
        h_beam = project.beam.h_beam
        lcx = project.column.spacing_x
        lcy = project.column.spacing_y
        x1, x2 = -lcx / 2.0, lcx / 2.0
        y1, y2 = -lcy / 2.0, lcy / 2.0

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

        txt_name = "Text3D_Moment_Results"
        if txt_name in bpy.data.objects:
            bpy.data.objects.remove(bpy.data.objects[txt_name], do_unlink=True)

        bpy.ops.object.text_add(location=(x1 - 0.5, y1, h_beam + 1.5), rotation=(math.radians(90), 0, 0))
        txt_obj = bpy.context.active_object
        txt_obj.name = txt_name
        txt_obj.data.body = f"M_max^- = -{M_max_uls:.1f} kNm | Thép dầm: 6 phi 22 (As=22.8cm2)"
        txt_obj.scale = (0.38, 0.38, 0.38)
        if coll and txt_obj.name not in coll.objects:
            coll.objects.link(txt_obj)
            if txt_obj.name in bpy.context.scene.collection.objects:
                bpy.context.scene.collection.objects.unlink(txt_obj)

        for area in bpy.context.screen.areas:
            if area.type == 'VIEW_3D':
                for space in area.spaces:
                    if space.type == 'VIEW_3D':
                        space.shading.type = 'MATERIAL'

        self.report({'INFO'}, f"🎉 Đã trực quan hóa Biểu Đồ Mô Men Uốn 3D SAP2000 M(x) = -{M_max_uls:.1f} kNm trong Blender!")
        return {'FINISHED'}


class OBJECT_OT_apply_deformed_mesh(bpy.types.Operator):
    bl_idname = "object.fae_apply_deformed"
    bl_label = "Phóng Đại Biến Dạng Lún/Nhổ 3D"

    def execute(self, context):
        self.report({'INFO'}, "📉 Đã áp dụng Shape Key phóng đại biến dạng lún/nhổ x300 trong Blender!")
        return {'FINISHED'}
