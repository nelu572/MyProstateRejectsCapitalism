"""Candidate 02: revise only the z=-1..17m school facade rhythm.
Read-only reuse of Art01's authored finishes, furniture, floor and courtyard.
No texture generation and no runtime system. Metres, Unity X/Y/Z.
"""
from pathlib import Path
import ast
import json
import math

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
PREV=HERE.parent/'KoreanSchoolArt01'
OUT=ROOT/'Assets/Models/Environment/KoreanSchoolArt02'
OUT.mkdir(parents=True,exist_ok=True)
env={'math':math,'groups':{}}
tree=ast.parse((HERE.parent/'StartLiminal/build_start_visual.py').read_text(encoding='utf-8'))
for node in tree.body:
    if isinstance(node,ast.FunctionDef) and node.name in ('cross','sub','face','box','write_obj'):
        exec(compile(ast.Module(body=[node],type_ignores=[]),'<authored bevel pipeline>','exec'),env)
box=env['box']

# Keep Art01's non-facade pieces byte-for-byte in geometry terms.
previous=(PREV/'build_school_art.py').read_text(encoding='utf-8')
previous_tree=ast.parse(previous)
env['source']=[r for r in json.loads((PREV/'source_boxes.json').read_text(encoding='utf-8'))
               if not r['root'].startswith(('02 -','03 -'))]
env['hidden']=[]
for node in previous_tree.body:
    if isinstance(node,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='mapping' for t in node.targets):
        exec(compile(ast.Module(body=[node],type_ignores=[]),'<Art01 material map>','exec'),env)
    if isinstance(node,ast.For) and isinstance(node.target,ast.Name) and node.target.id=='record':
        exec(compile(ast.Module(body=[node],type_ignores=[]),'<Art01 furniture and lighting>','exec'),env)
start=previous.index("box('Grout'")
end=previous.index("path=OUT/'SchoolArt01_Detail.obj'")
retained=previous[start:end].replace('for lo in (1.2,5.15,10.2,14.15):',
                                   'for lo in (2.4,4.55,11.2,14.15):')
exec(compile(retained,'<Art01 retained floor ceiling context>','exec'),env)

def solid_wall(x,lo,hi):
    box('Paint',(x,1.5,(lo+hi)/2),(.20,3,hi-lo),.0002)
    inner=x+(.108 if x<0 else -.108)
    box('PaintLower',(inner,.46,(lo+hi)/2),(.016,.92,hi-lo),.0002)

def sliding_set_door(z):
    box('Paint',(-2.1,2.65,z),(.20,.70,1.30),.0002)
    for dz in (-.69,.69):
        box('Metal',(-1.995,1.15,z+dz),(.08,2.3,.06),.0015)
    box('Metal',(-1.99,2.31,z),(.09,.10,1.48),.002)
    for i,dz in enumerate((-.32,.32)):
        x=-2.025-i*.025
        box('Gasket',(x+.021,1.12,z+dz),(.012,2.14,.625),.001)
        box('HPL',(x+.027,.59,z+dz),(.018,1.02,.595),.002)
        box('Metal',(x+.037,.12,z+dz),(.008,.14,.595),.001)
        box('Glass',(x,1.66,z+dz),(.025,.97,.58),.0005)
        box('FrostBand',(x+.002,1.26,z+dz),(.025,.22,.56),.0004)
        for off in (-.318,.318):
            box('Metal',(x+.016,1.13,z+dz+off),(.05,2.18,.035),.0015)
        for y in (1.13,2.18):
            box('Metal',(x+.016,y,z+dz),(.05,.045,.65),.0015)
    box('Gasket',(-1.994,1.03,z+.24),(.009,.20,.045),.005)
    box('Metal',(-1.982,1.03,z+.251),(.012,.155,.012),.003)

def classroom_window(lo,hi,panes):
    c=(lo+hi)/2;w=hi-lo
    box('PaintLower',(-2.1,.46,c),(.20,.92,w),.0002)
    box('Paint',(-2.1,2.725,c),(.20,.55,w),.0002)
    box('Glass',(-2.045,1.685,c),(.025,1.53,w),.0005)
    box('FrostBand',(-2.0435,1.26,c),(.025,.22,w-.035),.0004)
    for j in range(panes+1):
        z=lo+j*w/panes
        box('Gasket',(-1.988,1.685,z),(.012,1.57,.057),.001)
        box('Metal',(-1.995,1.685,z),(.07,1.57,.045),.0015)
    for y in (.92,2.1,2.45):
        box('Gasket',(-1.988,y,c),(.012,.057,w+.06),.001)
        box('Metal',(-1.995,y,c),(.07,.045,w+.06),.0015)
    box('Metal',(-1.96,.91,c),(.12,.055,w+.10),.002)

# Two classroom sets separated by a broad facility wall, rather than four doors.
doors=[(.55,1.85),(9.25,10.55)]
windows=[(2.3,5.0),(11.05,14.65)]
openings=sorted(doors+windows)
cursor=-1
for lo,hi in openings:
    if lo>cursor:solid_wall(-2.1,cursor,lo)
    cursor=hi
if cursor<17:solid_wall(-2.1,cursor,17)
for lo,hi in doors:sliding_set_door((lo+hi)/2)
for lo,hi in windows:classroom_window(lo,hi,3)

# Attached structural returns project only 12-14cm into the 4m corridor.
piers=[{'center':[-2.03,1.5,5.4],'size':[.34,3,.8]},
       {'center':[-2.04,1.5,15.025],'size':[.32,3,.75]}]
for p in piers:
    box('Paint',p['center'],p['size'],.003)
    x,y,z=p['center'];sx,sy,sz=p['size']
    box('PaintLower',(x+sx/2+.004,.46,z),(.012,.92,sz),.001)
    box('Skirting',(x+sx/2+.008,.055,z),(.018,.11,sz),.002)

# Exterior glazing is grouped into three banks of different lengths with solid piers.
exterior=[(-.5,3.8,3),(5.4,10.9,3),(12.9,16.7,2)]
cursor=-1
for lo,hi,n in exterior:
    if lo>cursor:solid_wall(2.1,cursor,lo)
    c=(lo+hi)/2;w=hi-lo
    box('ExteriorGlass',(2.10,1.88,c),(.025,1.82,w-.10),.0005)
    for j in range(n+1):
        z=lo+.045+j*(w-.09)/n
        box('Gasket',(2.028,1.88,z),(.012,1.88,.057),.001)
        box('Metal',(2.035,1.88,z),(.07,1.88,.045),.0015)
    for y in (.97,2.27,2.79):
        box('Gasket',(2.028,y,c),(.012,.057,w),.001)
        box('Metal',(2.035,y,c),(.07,.045,w),.0015)
    cursor=hi
if cursor<17:solid_wall(2.1,cursor,17)

# Perpendicular school blade signs. Panel lies in XY; classroom doors lie in YZ.
signs=[{'name':'2-1','subtitle':'2학년 1반','center':[-1.60,2.48,1.95]},
       {'name':'2-2','subtitle':'2학년 2반','center':[-1.60,2.48,10.65]}]
for sign in signs:
    x,y,z=sign['center']
    box('Metal',(-1.985,2.73,z),(.035,.28,.14),.004)
    box('Metal',(-1.64,2.77,z),(.70,.035,.045),.004)
    for dx in (-.25,.25):box('Metal',(x+dx,2.65,z),(.018,.17,.018),.003)
    box('Metal',(x,y,z),(.68,.32,.055),.005)
    for dz in (-.031,.031):
        box('Paper',(x,y,z+dz),(.645,.285,.010),.002)
        box('Felt',(x-.285,y,z+dz*1.18),(.045,.285,.009),.001)

env['write_obj'](OUT/'SchoolArt02_Rhythm.obj')
(HERE/'SchoolArt02_Rhythm.obj').write_bytes((OUT/'SchoolArt02_Rhythm.obj').read_bytes())
(HERE/'layout.json').write_text(json.dumps({'scope_z':[-1,17],
    'classroom_doors':doors,'classroom_windows':windows,'piers':piers,
    'exterior_windows':exterior,'signs':signs},ensure_ascii=False,indent=2),encoding='utf-8')
print('Candidate 02 facade mesh:',sum(len(v) for v in env['groups'].values()),'faces')
