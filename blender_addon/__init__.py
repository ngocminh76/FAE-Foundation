bl_info = {
    "name": "FAE Foundation - Móng Bè Cột Điện 3D",
    "author": "Antigravity & Minh",
    "version": (1, 0, 0),
    "blender": (4, 0, 0),
    "location": "View3D > Sidebar (N) > FAE Foundation",
    "description": "Addon Phân Tích Kết Cấu & Dựng Mô Hình 3D Móng Bè Phẳng Có Sườn Cột Điện Truyền Tải (TCVN, ACI 318, Eurocode)",
    "category": "3D View",
}

import sys
import os

addon_dir = os.path.dirname(os.path.abspath(__file__))
if addon_dir not in sys.path:
    sys.path.insert(0, addon_dir)

workspace_dir = r"d:\03.MINH\MyApp"
if os.path.exists(workspace_dir) and workspace_dir not in sys.path:
    sys.path.insert(0, workspace_dir)

import bpy
from .ui_panel import FAEExtensionPanel, FAEExtensionProperties
from .operators import (
    OBJECT_OT_open_full_dialog,
    OBJECT_OT_generate_foundation_3d,
    OBJECT_OT_run_structural_analysis,
    OBJECT_OT_apply_deformed_mesh
)

classes = (
    FAEExtensionProperties,
    FAEExtensionPanel,
    OBJECT_OT_open_full_dialog,
    OBJECT_OT_generate_foundation_3d,
    OBJECT_OT_run_structural_analysis,
    OBJECT_OT_apply_deformed_mesh
)

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.fae_props = bpy.props.PointerProperty(type=FAEExtensionProperties)
    print("✅ Registered FAE Foundation Blender Addon successfully!")

def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)
    del bpy.types.Scene.fae_props
    print("❌ Unregistered FAE Foundation Blender Addon.")

if __name__ == "__main__":
    register()
