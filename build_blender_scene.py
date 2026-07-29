"""
Standalone Blender Script to Automatedly Build, Materialize, Light & Render the FAE Foundation 3D Model
and Save project to FAE_Foundation_3D_Model.blend
"""

import bpy
import bmesh
import math
import sys
import os

# Set up project path
workspace_dir = r"d:\03.MINH\MyApp"
if workspace_dir not in sys.path:
    sys.path.insert(0, workspace_dir)

from src.core.presets import create_sample_project
from src.design_codes.tcvn import TCVNCodeChecker

def clean_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)

def create_concrete_material():
    mat = bpy.data.materials.new(name="PBR_Concrete")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Base Color'].default_value = (0.7, 0.7, 0.72, 1.0) # Concrete light grey
        bsdf.inputs['Roughness'].default_value = 0.65
    return mat

def create_steel_material():
    mat = bpy.data.materials.new(name="PBR_Steel_Gold")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Base Color'].default_value = (0.9, 0.75, 0.2, 1.0) # Metallic Gold/Steel
        bsdf.inputs['Metallic'].default_value = 0.9
        bsdf.inputs['Roughness'].default_value = 0.3
    return mat

def create_heatmap_material():
    mat = bpy.data.materials.new(name="Soil_Heatmap_Mat")
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    bsdf = nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs['Base Color'].default_value = (0.9, 0.2, 0.1, 1.0) # Stress Heatmap Red
        bsdf.inputs['Roughness'].default_value = 0.4
    return mat

def build_foundation():
    clean_scene()
    project = create_sample_project()
    
    Lx, Ly = project.slab.L_x, project.slab.L_y
    h_slab = project.slab.h_slab
    h_lean = project.slab.h_lean
    b_beam, h_beam = project.beam.b_beam, project.beam.h_beam
    H_col = project.column.H_col
    b_col, h_col = project.column.b_col, project.column.h_col
    lcx, lcy = project.column.spacing_x, project.column.spacing_y
    x1, x2 = -lcx/2.0, lcx/2.0
    y1, y2 = -lcy/2.0, lcy/2.0

    mat_concrete = create_concrete_material()
    mat_steel = create_steel_material()
    mat_heatmap = create_heatmap_material()

    coll = bpy.data.collections.new("FAE_Foundation_3D")
    bpy.context.scene.collection.children.link(coll)

    # 1. Bê tông lót
    offset_lean = 0.15
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, -h_lean/2.0))
    obj_lean = bpy.context.active_object
    obj_lean.name = "Lean_Concrete_Blinding"
    obj_lean.scale = (Lx + 2*offset_lean, Ly + 2*offset_lean, h_lean)
    obj_lean.data.materials.append(mat_concrete)

    # 2. Bản móng bè
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, h_slab/2.0))
    obj_slab = bpy.context.active_object
    obj_slab.name = "Raft_Slab"
    obj_slab.scale = (Lx, Ly, h_slab)
    obj_slab.data.materials.append(mat_concrete)

    # 3. Mặt Heatmap ứng suất đất phủ trên bản bè
    bpy.ops.mesh.primitive_plane_add(size=1.0, location=(0, 0, h_slab + 0.005))
    obj_heat = bpy.context.active_object
    obj_heat.name = "Soil_Stress_Heatmap_Surface"
    obj_heat.scale = (Lx, Ly, 1.0)
    obj_heat.data.materials.append(mat_heatmap)

    # 4. 4 Dầm sườn nổi chạy suốt
    beam_configs = [
        ("RibBeam_X1", (0, y1, h_slab + (h_beam - h_slab)/2.0), (Lx, b_beam, h_beam - h_slab)),
        ("RibBeam_X2", (0, y2, h_slab + (h_beam - h_slab)/2.0), (Lx, b_beam, h_beam - h_slab)),
        ("RibBeam_Y1", (x1, 0, h_slab + (h_beam - h_slab)/2.0), (b_beam, Ly, h_beam - h_slab)),
        ("RibBeam_Y2", (x2, 0, h_slab + (h_beam - h_slab)/2.0), (b_beam, Ly, h_beam - h_slab)),
    ]

    for name, pos, size in beam_configs:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=pos)
        o_beam = bpy.context.active_object
        o_beam.name = name
        o_beam.scale = size
        o_beam.data.materials.append(mat_concrete)

    # 5. 4 Cổ cột cao + Bu lông neo
    col_positions = [
        ("StubColumn_Leg1", (x1, y1, h_beam + (H_col - (h_beam - h_slab))/2.0)),
        ("StubColumn_Leg2", (x2, y1, h_beam + (H_col - (h_beam - h_slab))/2.0)),
        ("StubColumn_Leg3", (x1, y2, h_beam + (H_col - (h_beam - h_slab))/2.0)),
        ("StubColumn_Leg4", (x2, y2, h_beam + (H_col - (h_beam - h_slab))/2.0)),
    ]

    for name, pos in col_positions:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=pos)
        o_col = bpy.context.active_object
        o_col.name = name
        o_col.scale = (b_col, h_col, H_col - (h_beam - h_slab))
        o_col.data.materials.append(mat_concrete)

        # Cụm 4 Bu lông neo M36
        z_top = h_slab + H_col
        bolt_offsets = [(-0.15, -0.15), (0.15, -0.15), (-0.15, 0.15), (0.15, 0.15)]
        for b_idx, (bx, by) in enumerate(bolt_offsets):
            bpy.ops.mesh.primitive_cylinder_add(radius=0.02, depth=0.2, location=(pos[0]+bx, pos[1]+by, z_top + 0.1))
            o_bolt = bpy.context.active_object
            o_bolt.name = f"AnchorBolt_{name}_{b_idx+1}"
            o_bolt.data.materials.append(mat_steel)

    # 6. Thiết lập Camera & Sun Light
    bpy.ops.object.light_add(type='SUN', location=(10, -10, 15))
    sun = bpy.context.active_object
    sun.data.energy = 4.5

    bpy.ops.object.camera_add(location=(12, -14, 10), rotation=(math.radians(55), 0, math.radians(40)))
    cam = bpy.context.active_object
    bpy.context.scene.camera = cam

    # 7. Render hình ảnh 3D và Lưu file .blend
    blend_filepath = os.path.join(workspace_dir, "FAE_Foundation_3D_Model.blend")
    bpy.ops.wm.save_as_mainfile(filepath=blend_filepath)
    print(f"✅ Saved Blender project to {blend_filepath}")

    render_path = r"C:\Users\qnbk1\.gemini\antigravity\brain\532fcab8-feeb-4929-b8e3-e0fffa788c40\blender_foundation_render.png"
    bpy.context.scene.render.filepath = render_path
    bpy.context.scene.render.image_settings.file_format = 'PNG'
    bpy.ops.render.render(write_still=True)
    print(f"📸 Rendered image to {render_path}")

if __name__ == "__main__":
    build_foundation()
