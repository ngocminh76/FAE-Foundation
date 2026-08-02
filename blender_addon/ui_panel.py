import bpy

class FAEExtensionProperties(bpy.types.PropertyGroup):
    # Kích thước móng
    L_x: bpy.props.FloatProperty(name="L_x - Chiều dài bản (m)", default=8.0, min=2.0, max=30.0)
    L_y: bpy.props.FloatProperty(name="L_y - Chiều rộng bản (m)", default=8.0, min=2.0, max=30.0)
    h_slab: bpy.props.FloatProperty(name="h_slab - Chiều dày bản bè (m)", default=0.4, min=0.1, max=3.0)
    h_lean: bpy.props.FloatProperty(name="h_lean - Chiều dày lót (m)", default=0.1, min=0.05, max=1.0)
    
    # Dầm sườn
    b_beam: bpy.props.FloatProperty(name="b_beam - Bề rộng dầm (m)", default=0.4, min=0.1, max=2.0)
    h_beam: bpy.props.FloatProperty(name="h_beam - Chiều cao dầm (m)", default=0.8, min=0.2, max=4.0)

    # Cổ cột
    spacing_x: bpy.props.FloatProperty(name="lcx - Khoảng cách cột X (m)", default=3.5, min=1.0, max=20.0)
    spacing_y: bpy.props.FloatProperty(name="lcy - Khoảng cách cột Y (m)", default=3.5, min=1.0, max=20.0)
    b_col: bpy.props.FloatProperty(name="b_col - Bề rộng cổ cột (m)", default=0.6, min=0.2, max=2.0)
    h_col: bpy.props.FloatProperty(name="h_col - Bề sâu cổ cột (m)", default=0.6, min=0.2, max=2.0)
    H_col: bpy.props.FloatProperty(name="H_col - Chiều cao cổ cột (m)", default=1.8, min=0.5, max=10.0)

    # 地質 Soil
    K_z: bpy.props.FloatProperty(name="Kz - Hệ số nền Winkler (kN/m³)", default=22500.0, min=1000.0, max=200000.0)
    R_tc: bpy.props.FloatProperty(name="Rtc - Sức chịu tải tiêu chuẩn (kPa)", default=250.0, min=10.0, max=2000.0)

    # Tiêu chuẩn
    selected_code: bpy.props.EnumProperty(
        name="Tiêu chuẩn thiết kế",
        items=[
            ('TCVN', '🇻🇳 TCVN 5574:2018 / TCVN 9362:2012 (Việt Nam)', 'Tiêu chuẩn Việt Nam'),
            ('ACI', '🇺🇸 ACI 318-19 LRFD (Mỹ)', 'Tiêu chuẩn Mỹ ACI 318'),
            ('EUROCODE', '🇪🇺 Eurocode 2 / Eurocode 7 (Châu Âu)', 'Tiêu chuẩn Châu Âu Eurocode')
        ],
        default='TCVN'
    )


class FAEExtensionPanel(bpy.types.Panel):
    bl_label = "⚡ FAE Foundation 3D"
    bl_idname = "VIEW3D_PT_fae_foundation"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'FAE Foundation'

    def draw(self, context):
        layout = self.layout
        props = context.scene.fae_props

        # 🚀 ĐẶT 3 NÚT THỰC THI CHÍNH LÊN TRÊN CÙNG ĐỂ THẤY NGAY LẬP TỨC
        layout.operator("object.fae_open_full_dialog", text="⚙️ Mở Cửa Sổ Nhập Đầy Đủ (Full Dialog)", icon='WINDOW')
        
        box_actions = layout.box()
        box_actions.label(text="⚡ THỰC THI & TÍNH TOÁN 3D", icon='PLAY')
        col_act = box_actions.column(align=True)
        col_act.operator("object.fae_generate_3d", text="🧱 1. DỰNG MÓNG BÈ 3D", icon='OUTLINER_OB_MESH')
        col_act.operator("object.fae_run_analysis", text="🚀 2. TÍNH NỘI LỰC & VẼ MÔ MEN 3D", icon='GRAPH')
        col_act.operator("object.fae_apply_deformed", text="📉 3. PHÓNG ĐẠI BIẾN DẠNG 3D", icon='MOD_DISPLACE')

        layout.separator()

        # Group 1: Kích thước móng
        box_geo = layout.box()
        box_geo.label(text="📐 Kích Thước Móng Bè 3D", icon='MESH_CUBE')
        col = box_geo.column(align=True)
        col.prop(props, "L_x")
        col.prop(props, "L_y")
        col.prop(props, "h_slab")
        col.prop(props, "h_lean")

        # Group 2: Dầm sườn
        box_beam = layout.box()
        box_beam.label(text="🧱 Dầm Sườn Nổi 2 Phương", icon='MOD_BEVEL')
        col = box_beam.column(align=True)
        col.prop(props, "b_beam")
        col.prop(props, "h_beam")

        # Group 3: 4 Cổ Cột
        box_col = layout.box()
        box_col.label(text="🏛️ 4 Cổ Cột Điện Truyền Tải", icon='COLUMN')
        col = box_col.column(align=True)
        col.prop(props, "spacing_x")
        col.prop(props, "spacing_y")
        col.prop(props, "b_col")
        col.prop(props, "h_col")
        col.prop(props, "H_col")

        layout.separator()
        box_code = layout.box()
        box_code.label(text="📋 Tiêu Chuẩn Phân Tích", icon='PHYSICS')
        box_code.prop(props, "selected_code", text="")
