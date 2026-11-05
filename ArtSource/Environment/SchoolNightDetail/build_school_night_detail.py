"""Small additive 18m architectural joints; existing school facade stays intact.
Uses the repository's original bevelled OBJ pipeline, metre-scale Unity XYZ.
"""
from pathlib import Path
import ast, math
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
OUT=ROOT/'Assets/Models/Environment/Prologue/SchoolNightDetail'
OUT.mkdir(parents=True,exist_ok=True)
env={'math':math,'groups':{}}
tree=ast.parse((HERE.parent/'StartLiminal/build_start_visual.py').read_text(encoding='utf-8'))
for node in tree.body:
    if isinstance(node,ast.FunctionDef) and node.name in ('cross','sub','face','box','write_obj'):
        exec(compile(ast.Module(body=[node],type_ignores=[]),'<existing bevel pipeline>','exec'),env)
box=env['box']
# Perimeter downstand joins existing piers; central ceiling plane stays at 3m.
for x in (-1.95,1.96):
    box('Paint',(x,2.935,8),(.11,.13,18),.004)
    box('Gasket',(x,2.865,8),(.055,.008,18),.001)
# Column capitals / returned plaster joints remain against the wall.
for z,w in ((5.4,.8),(15.025,.75)):
    box('Paint',(-1.965,2.84,z),(.20,.22,w+.12),.006)
    for edge in (z-w/2,z+w/2):
        box('PaintLower',(-1.965,.46,edge),(.18,.92,.038),.002)
        box('Skirting',(-1.932,.055,edge),(.23,.11,.045),.002)
# Classroom window reveals / stone sills: a real recess, not a frame on a plane.
for lo,hi in ((2.3,5.0),(11.05,14.65)):
    c=(lo+hi)/2;w=hi-lo
    box('Sill',(-1.948,.881,c),(.235,.06,w+.15),.004)
    box('Paint',(-2.10,2.49,c),(.22,.07,w+.10),.002)
    for edge in (lo-.025,hi+.025):
        box('Paint',(-2.085,1.69,edge),(.23,1.59,.065),.002)
    box('Gasket',(-1.996,.848,c),(.025,.010,w+.08),.001)
# Exterior window banks, front and back faces across the wall thickness.
for lo,hi in ((-.5,3.8),(5.4,10.9),(12.9,16.7)):
    c=(lo+hi)/2;w=hi-lo
    box('Sill',(2.04,.939,c),(.36,.058,w+.10),.004)
    box('Metal',(2.188,.92,c),(.04,.09,w+.13),.002)
    box('Paint',(2.12,2.84,c),(.25,.07,w+.10),.002)
    for edge in (lo-.02,hi+.02):
        box('Paint',(2.125,1.88,edge),(.23,1.83,.055),.002)
# Door tracks tied into wall head; leave 1.3m opening and moving leaf sweep untouched.
for z in (1.2,9.9):
    box('Paint',(-2.10,2.41,z),(.23,.10,1.53),.003)
    box('Metal',(-1.982,2.345,z),(.08,.018,1.49),.001)
# School blade sign fixing plates: no new wording.
for z in (1.95,10.65):
    box('Metal',(-1.985,2.73,z),(.035,.30,.19),.003)
    for dz in (-.064,.064):
        for y in (2.64,2.82):
            box('Gasket',(-1.963,y,z+dz),(.009,.018,.018),.004)
env['write_obj'](OUT/'School_JunctionDetails.obj')
(HERE/'School_JunctionDetails.obj').write_bytes((OUT/'School_JunctionDetails.obj').read_bytes())
# Only a few visible courtyard lamps, not a new exterior level.
env['groups']={}
for x,z in ((17,1.5),(17,21),(7,25)):
    box('OutdoorMetal',(x,-1.95,z),(.055,2.5,.055),.006)
    box('OutdoorMetal',(x,-.67,z),(.26,.09,.22),.008)
    box('ExteriorLamp',(x,-.72,z),(.20,.025,.16),.004)
env['write_obj'](OUT/'Courtyard_LampSilhouettes.obj')
(HERE/'Courtyard_LampSilhouettes.obj').write_bytes((OUT/'Courtyard_LampSilhouettes.obj').read_bytes())
print('Additive school details and 3 courtyard lamp silhouettes exported.')

