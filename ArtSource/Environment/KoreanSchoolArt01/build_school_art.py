"""School art candidate 01: render-only replacements in z=-1..17 metres.
Reuses this project's authored bevel/OBJ pipeline and PBR surfaces. No external assets.
Original scene colliders and all source assets remain untouched.
"""
from pathlib import Path
import ast
import json
import math
import random
import struct
import zlib

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
OUT = ROOT / 'Assets/Models/Environment/Prologue/KoreanSchoolArt01'
OUT.mkdir(parents=True, exist_ok=True)
tree = ast.parse((HERE.parent / 'StartLiminal/build_start_visual.py').read_text(encoding='utf-8'))
env = {'math': math, 'groups': {}, 'struct': struct, 'zlib': zlib}
for node in tree.body:
    if isinstance(node, ast.FunctionDef) and node.name in ('cross', 'sub', 'face', 'box', 'write_obj', 'png'):
        exec(compile(ast.Module(body=[node], type_ignores=[]), '<existing bevel pipeline>', 'exec'), env)
box, face = env['box'], env['face']
source = json.loads((HERE / 'source_boxes.json').read_text(encoding='utf-8'))
hidden = []
mapping = {
    'Plaster': 'Paint', 'LowerWall': 'PaintLower', 'Aluminium': 'Metal',
    'DoorWood': 'HPL', 'ClassGlass': 'Glass', 'ExteriorGlass': 'ExteriorGlass',
    'Light': 'Diffuser', 'ShoeCabinet': 'Cabinet', 'Notice': 'Felt',
    'Paper': 'Paper', 'Red': 'Red',
}

for record in source:
    name, pos, size = record['name'], record['position'], record['scale']
    low, high = pos[2] - size[2] / 2, pos[2] + size[2] / 2
    if record['mesh'] != 'Cube' or low < -1.001 or high > 17.001:
        continue
    if record['root'].startswith(('06 -', '07 -')):
        continue
    mat = record['material'].replace('School_Blockout_', '')
    if mat not in mapping:
        continue
    hidden.append(record['instance'])
    if name == 'Recessed pull':
        # Slim recessed finger cup and inner grip, rather than a bar across circulation.
        box('Gasket', (-1.994, pos[1], pos[2]), (.009, .20, .045), .005)
        box('Metal', (-1.982, pos[1], pos[2] + .011), (.012, .155, .012), .003)
        continue
    if name in ('Sliding leaf glass upper', 'Recessed classroom glazing', 'Exterior glass'):
        box(mapping[mat], pos, size, .0005)
        if name != 'Exterior glass':
            # A narrow obscured band reads as glass, leaving sight into the depth proxy.
            box('FrostBand', (pos[0] + .0015, 1.26, pos[2]), (size[0], .22, size[2]-.035), .0004)
        continue
    bevel = .0015 if mat == 'Aluminium' else .003
    if mat in ('Plaster', 'LowerWall'):
        bevel = .0002
    if name == 'Fluorescent housing '+name.split()[-1]:
        bevel = .005
    box(mapping[mat], pos, size, bevel)
    if name in ('Classroom window mullion', 'Classroom window horizontal',
                'Aluminium vertical', 'Aluminium horizontal',
                'Leaf vertical stile', 'Leaf horizontal rail'):
        # Black glazing gasket sits behind each extrusion's exposed edge.
        gasket = list(size)
        gasket[0] = .012
        if size[1] > size[2]:
            gasket[2] += .012
        else:
            gasket[1] += .012
        box('Gasket', (pos[0] + (.007 if pos[0] < 0 else -.007), pos[1], pos[2]), gasket, .001)

# Floating timber panels receive a restrained perimeter reveal and lower protection kickplate.
for z in (0.0, 6.7, 9.0, 15.7):
    box('Gasket', (-2.009, .57, z), (.01, 1.07, 1.24), .001)
    for dz in (-.305, .305):
        box('HPL', (-1.998, .59, z+dz), (.012, 1.02, .575), .002)
        box('Metal', (-1.988, .12, z+dz), (.008, .14, .575), .001)
    box('Metal', (-2.002, 2.325, z), (.045, .022, 1.48), .002)
    box('Gasket', (-2.006, 2.285, z), (.018, .012, 1.37), .001)

# Floor: 600mm porcelain, real 3mm recessed grout, cut edge tiles. Top remains y=0.
box('Grout', (0, -.016, 8), (4, .03, 18), .0005)
edges = [-2, -1.8, -1.2, -.6, 0, .6, 1.2, 1.8, 2]
for row in range(30):
    for left, right in zip(edges, edges[1:]):
        box('Floor', ((left+right)/2, -.012, -.7+row*.6),
            (right-left-.003, .024, .597), .0011)

# 600x1200 acoustic ceiling panels with fine reveals. Underside remains y=3.0.
box('CeilingGap', (0, 3.045, 8), (4, .025, 18), .001)
for row in range(15):
    for left, right in zip(edges, edges[1:]):
        box('Ceiling', ((left+right)/2, 3.016, -.4+row*1.2),
            (right-left-.003, .032, 1.197), .001)
box('PaintLower', (1.994, .46, 8), (.009, .92, 18), .001)
box('Metal', (2.029, .947, 8), (.306, .064, 18), .002)
for x in (-1.977, 1.977):
    box('Skirting', (x, .055, 8), (.018, .11, 18), .002)

# Quiet school life: two pairs of indoor slippers, not a prop scatter.
def slipper(c, color):
    outline = [(-.045,-.11),(.045,-.11),(.053,.045),(.036,.11),(-.036,.11),(-.053,.045)]
    # X is corridor depth, Z is the shoe's long axis.
    top = [(c[0]+z,c[1]+.018,c[2]+x) for x,z in outline]
    bottom = [(c[0]+z,c[1],c[2]+x) for x,z in outline]
    face('Sole', bottom, (0,-1,0));face(color, top, (0,1,0))
    for i in range(len(outline)):
        j=(i+1)%len(outline);face('Sole',[bottom[i],bottom[j],top[j],top[i]])
    box(color, (c[0]+.035,c[1]+.038,c[2]), (.09,.055,.10), .022)
for z, color in ((3.25,'SlipperWhite'),(4.15,'SlipperNavy')):
    for offset in (-.065,.065):
        slipper((-1.80,.055,z+offset),color)

# Depth belongs to a shared proxy, not separately authored classroom interiors.
for z in (3.1,4.4,12.1,13.4):
    box('Desk',(-3.45,.74,z),(.58,.035,.95),.012)
    for xx in (-3.66,-3.24):
        for dz in (-.36,.36):box('Metal',(xx,.36,z+dz),(.025,.72,.025),.004)
    box('Desk',(-3.9,.45,z),(.38,.03,.38),.014)
    box('Desk',(-4.04,.72,z),(.025,.37,.38),.009)
for z in (5.3,14.3):
    box('BoardGreen',(-4.365,1.6,z),(.025,1.05,2.1),.006)
    box('Metal',(-4.34,1.055,z),(.09,.035,2.15),.003)
for lo in (1.2,5.15,10.2,14.15):
    # Pleated fabric is 0.5m behind the glass, with visible air and a soft silhouette.
    for k in range(12):
        z0=lo+k*.035;z1=z0+.035
        x0=-2.60+.035*math.sin(k*math.pi/2);x1=-2.60+.035*math.sin((k+1)*math.pi/2)
        face('Curtain',[(x0,.99,z0),(x1,.99,z1),(x1,2.43,z1),(x0,2.43,z0)],(1,0,0))

# Low-cost courtyard and facing wing: only the context visible from the sample windows.
box('Paving',(11,-3.2,8),(18,.16,52),.02)
box('Court',(12,-3.105,13),(9,.03,22),.001)
for x in (7.7,16.3):box('CourtLine',(x,-3.085,13),(.045,.008,21),.001)
for z in (2.5,13,23.5):box('CourtLine',(12,-3.085,z),(8.6,.008,.045),.001)
box('OutdoorPaint',(22,2.6,10),(5,11.6,44),.025)
for row in range(4):
    y=-1.8+row*2.65
    for col in range(13):
        z=-8+col*3
        box('OutdoorReveal',(19.46,y,z),(.09,1.65,2.55),.008)
        box('OutdoorGlass',(19.40,y,z),(.025,1.48,2.37),.002)
        for off in (-1.19,0,1.19):box('OutdoorMetal',(19.36,y,z+off),(.055,1.53,.038),.002)
        box('OutdoorMetal',(19.34,y-.78,z),(.17,.055,2.65),.005)
    box('OutdoorPaint',(19.30,y-1.02,10),(.40,.12,44),.01)
box('OutdoorPaint',(21.4,8.48,10),(6.3,.20,45),.02)

path=OUT/'SchoolArt01_Detail.obj'
env['write_obj'](path)
(HERE/'SchoolArt01_Detail.obj').write_bytes(path.read_bytes())
(HERE/'hidden_renderers.json').write_text(json.dumps(hidden),encoding='utf-8')
(HERE/'build_summary.json').write_text(json.dumps({'art_z':[-1,17],
    'materials':list(env['groups']),'faces':sum(len(f) for f in env['groups'].values()),
    'replacement_renderers':len(hidden)},indent=2),encoding='utf-8')
print('School art render mesh:',path,'faces',sum(len(f) for f in env['groups'].values()))

# Original authored light ash laminate grain, deliberately subtle rather than distressed.
texdir=ROOT/'Assets/Textures/Environment/Prologue/KoreanSchoolArt01'
texdir.mkdir(parents=True,exist_ok=True)
rng=random.Random(431)
pixels=bytearray()
for y in range(512):
    v=y/512
    for x in range(512):
        u=x/512
        grain=math.sin(u*math.pi*114 + .65*math.sin(v*math.pi*2))
        grain+=.28*math.sin(u*math.pi*278 + .3*math.sin(v*math.pi*4))
        delta=grain*2.0+rng.uniform(-1.2,1.2)
        pixels.extend([max(0,min(255,round(c+delta))) for c in (188,164,126)]+[255])
env['png'](texdir/'AuthoredAshLaminate_Base.png',512,512,pixels)
