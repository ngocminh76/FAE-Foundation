import bpy

class FAEExtensionProperties(bpy.types.PropertyGroup):
    # Kích thước móng
    L_x: bpy.props.FloatProperty(name="L_x (m)", default=8.0, min=2.0, max=30.0)
    L_y: bpy.props.FloatProperty(name="L_y (m)", default=8.0, min=2.0, max=30.0)
    h_slab: bpy.props.FloatProperty(name="Chiều dày bản h_slab (m)", default=0.4, min=0.1, max=3.0)
    
    # Dầm sườn
    b_beam: bpy.props.FloatProperty(name="Bề rộng dầm b_beam (m)", default=0.4, min=0.1, max=2.0)
    h_beam: bpy.props.FloatProperty(name="Chiều cao dầm h_beam (m)", default=0.8, min=0.2, max=4.0)

    # Cổ cột
    spacing_x: bpy.props.FloatProperty(name="Khoảng cách lcx (m)", default=3.5, min=1.0, max=20.0)
    spacing_y: bpy.props.FloatProperty(name="Khoảng cách lcy (m)", default=3.5, min=1.0, max=20.0)
    b_col: bpy.props.FloatProperty(name="Bề rộng cổ cột b_col (m)", default=0.6, min=0.2, max=2.0)
    h_col: bpy.props.FloatProperty(name="Bề sâu cổ cột h_col (m)", default=0.6, min=0.2, max=2.0)
    H_col: bpy.props.FloatProperty(name="Chiều cao cổ cột H_col (m)", default=1.8, min=0.5, max=10.0)

    # Tiêu chuẩn
    selected_code: bpy.props.EnumProperty(
        name="Tiêu chuẩn",
        items=[
            ('TCVN', 'TCVN 5574:2018 / 9362:2012', 'Tiêu chuẩn Việt Nam'),
            ('ACI', 'ACI 318-19 (Mỹ)', 'Tiêu chuẩn Mỹ ACI 318'),
            ('EUROCODE', 'Eurocode 2 / 7 (Châu Âu)', 'Tiêu chuẩn Châu Âu Eurocode')
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

        box_geo = layout.box()
        box_geo.label(text="📐 Kích Thước Móng Bè 3D", icon='MESH_CUBE')
        row = box_geo.row()
        row.prop(props, "L_x")
        row.prop(props, "L_y")
        box_geo.prop(props, "h_slab")

        box_beam = layout.box()
        box_beam.label(text="🧱 Dầm Sườn Nổi 2 Phương", icon='MOD_BEVEL')
        row = box_beam.row()
        row.prop(props, "b_beam")
        row.prop(props, "h_beam")

        box_col = layout.box()
        box_col.label(text="🏛️ 4 Cổ Cột Điện", icon='COLUMN')
        row = box_col.row()
        row.prop(props, "spacing_x")
        row.prop(props, "spacing_y")
        row = box_col.row()
        row.prop(props, "b_col")
        row.prop(props, "h_col")
        box_col.prop(props, "H_col")

        layout.separator()
        layout.operator("object.fae_generate_3d", text="🧱 Dựng Mô Hình 3D Trong Blender", icon='OUTLINER_OB_MESH')

        layout.separator()
        box_code = layout.box()
        box_code.label(text="📋 Phân Tích Kết Cấu Multi-Standard", icon='PHYSICS')
        box_code.prop(props, "selected_code")
        box_code.operator("object.fae_run_analysis", text="🚀 Chạy Phân Tích & Tô Màu Heatmap", icon='PLAY')
        box_code.operator("object.fae_apply_deformed", text="📉 Phóng Đại Biến Dạng Lún/Nhổ 3D", icon='MOD_DISPLACE')
