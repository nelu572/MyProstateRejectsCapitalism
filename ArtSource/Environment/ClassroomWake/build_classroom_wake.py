"""Scene-specific classroom sample. Reuses the project's authored bevel/PBR pipeline.
Only new files are written. Original corridor art remains intact except the functional
second door leaf and its shallow interior proxy, replaced in the candidate scene.
"""
from pathlib import Path
import json, math

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
OUT = ROOT / 'Assets/Models/Environment/Prologue/ClassroomWake'
OUT.mkdir(parents=True, exist_ok=True)
old = HERE.parent / 'KoreanSchoolArt02/build_school_rhythm.py'
source = old.read_text(encoding='utf-8')
source = source[:source.index("env['write_obj'](OUT/")]
source = source.replace('    for i,dz in enumerate((-.32,.32)):',
    '    if abs(z-9.9)<.01: return\n    for i,dz in enumerate((-.32,.32)):')
ns = {'__file__': str(old)}
exec(compile(source, str(old), 'exec'), ns)
env = ns['env']
for mat, faces in env['groups'].items():
    env['groups'][mat] = [f for f in faces if not
        (all(p[0] < -2.25 for p in f[0]) and all(7.8 <= p[2] <= 16.9 for p in f[0]))]
env['write_obj'](OUT/'Corridor_OnlyOpening.obj')

env['groups'] = {}
box, face = env['box'], env['face']
colliders = []
def solid(name, mat, c, s, bevel=.005):
    box(mat, c, s, bevel)
    colliders.append({'name': name, 'center': c, 'size': s})

# 8.1 x 9m room, 3m ceiling. The existing corridor facade is the eastern wall.
solid('Classroom floor', 'ClassFloor', (-6.2,-.09,12.3),(8.1,.18,9.0))
solid('Front wall', 'Paint', (-6.2,1.5,7.75),(8.1,3,.20))
solid('Rear wall', 'Paint', (-6.2,1.5,16.85),(8.1,3,.20))
solid('Exterior window wall base', 'PaintLower', (-10.3,.45,12.3),(.20,.9,9.2))
solid('Exterior window wall head', 'Paint', (-10.3,2.78,12.3),(.20,.44,9.2))
for lo,hi in [(7.8,10.0),(15.7,16.8)]:
    solid('Window end pier', 'Paint',(-10.3,1.7,(lo+hi)/2),(.20,1.6,hi-lo))
solid('Window collision', 'ExteriorGlass', (-10.31,1.73,12.85),(.025,1.65,5.7),.001)
for z in [10,11.425,12.85,14.275,15.7]:
    box('Metal',(-10.16,1.73,z),(.065,1.72,.05),.002)
for y in [.91,2.1,2.56]:
    box('Metal',(-10.15,y,12.85),(.12,.055,5.82),.002)
for z in [7.87,16.72]:
    box('PaintLower',(-6.2,.45,z),(8.0,.9,.018),.001)
    box('Skirting',(-6.2,.055,z),(8,.11,.022),.002)
box('Skirting',(-10.16,.055,12.3),(.035,.11,8.8),.002)
for x in [-9.65,-8.45,-7.25,-6.05,-4.85,-3.65,-2.6]:
    for z in [8.4,9.6,10.8,12,13.2,14.4,15.6,16.45]:
        width=min(1.197,2*(x+10.2),2*(-2.2-x))
        depth=min(1.197,2*(z-7.85),2*(16.75-z))
        box('Ceiling',(x,3.018,z),(width,.035,depth),.001)
solid('Classroom ceiling collision','CeilingGap',(-6.2,3.10,12.3),(8.1,.14,9))

# Twenty recognizable desks/chairs; a front aisle and an eastern door aisle remain open.
for col,x in enumerate([-9.1,-7.6,-6.1,-4.6]):
    for row,z in enumerate([10.5,11.7,12.9,14.1,15.3]):
        box('Gasket',(x,.735,z),(.66,.026,.49),.017)
        box('HPL',(x,.758,z),(.65,.032,.48),.018)
        for dx in [-.25,.25]:
            for dz in [-.17,.17]:
                box('Metal',(x+dx,.37,z+dz),(.028,.72,.028),.005)
                box('Gasket',(x+dx,.025,z+dz),(.045,.035,.045),.008)
        box('Metal',(x,.25,z-.17),(.52,.025,.025),.004)
        box('HPL',(x,.51,z+.02),(.52,.018,.33),.005)
        for dx in [-.25,.25]:box('Metal',(x+dx,.61,z+.02),(.025,.21,.34),.004)
        solid('Student desk %d %d'%(col,row),'Gasket',(x,.40,z),(.65,.76,.48),.001)
        # Collision proxy is hidden during integration; visible geometry is the model above.
        env['groups']['Gasket'] = env['groups']['Gasket'][:-26]
        cz=z+.55
        box('HPL',(x,.438,cz),(.42,.032,.40),.025)
        box('HPL',(x,.72,cz+.18),(.42,.31,.028),.016)
        for dx in [-.17,.17]:
            for dz in [-.15,.15]:
                box('Metal',(x+dx,.22,cz+dz),(.026,.43,.026),.005)
                box('Gasket',(x+dx,.02,cz+dz),(.044,.03,.044),.007)
            box('Metal',(x+dx,.62,cz+.17),(.026,.48,.028),.005)
        colliders.append({'name':'Chair %d %d'%(col,row),'center':[x,.42,cz],'size':[.44,.84,.42]})

# Teaching wall: framed green board, chalk tray, teacher desk and clock.
box('Metal',(-6.8,1.56,7.89),(4.3,1.32,.045),.006)
box('BoardGreen',(-6.8,1.56,7.919),(4.18,1.20,.025),.004)
box('Metal',(-6.8,.92,7.99),(4.32,.045,.17),.005)
solid('Teacher desk','HPL',(-6.9,.78,8.9),(1.2,.055,.58),.018)
box('PaintLower',(-6.9,.41,9.10),(1.06,.67,.035),.004)
for x in [-7.4,-6.4]:box('Metal',(x,.39,8.9),(.035,.75,.49),.005)
for k in range(40):
    a=k*2*math.pi/40;b=(k+1)*2*math.pi/40
    face('Paper',[(-3.3,2.42,7.90),(-3.3+.18*math.cos(a),2.42+.18*math.sin(a),7.90),(-3.3+.18*math.cos(b),2.42+.18*math.sin(b),7.90)],(0,0,1))
box('Metal',(-3.335,2.445,7.925),(.085,.015,.012),.002)
box('Metal',(-3.25,2.36,7.926),(.015,.13,.012),.002)
box('Felt',(-7.5,1.65,16.72),(3.2,1.10,.04),.006)
for x,z in [(-8.45,16.69),(-7.55,16.69),(-6.65,16.69)]:box('Paper',(x,1.65,z),(.50,.69,.008),.001)

# Draped folds, kept away from the walk route. Corridor glazing is partly screened.
def curtain(x,lo,hi,bottom,top):
    for k in range(math.ceil((hi-lo)/.04)):
        a=lo+k*.04;b=min(a+.04,hi)
        x0=x+.045*math.sin(k*math.pi/2);x1=x+.045*math.sin((k+1)*math.pi/2)
        face('Curtain',[(x0,bottom,a),(x1,bottom,b),(x1,top,b),(x0,top,a)],(1,0,0))
curtain(-10.02,10.02,10.82,.68,2.70);curtain(-10.02,14.9,15.72,.68,2.70)
curtain(-2.47,11.08,12.05,.96,2.55);curtain(-2.47,14.10,14.63,.96,2.55)

# Two simple ceiling fixtures. Only one is fully lit in the scene.
for x,z,lit in [(-6.3,13.8,True),(-6.3,10.1,False),(-8.8,13.8,False),(-3.5,13.8,False)]:
    box('Metal',(x,2.95,z),(.5,.10,1.22),.015)
    for dx in [-.12,.12]:box('Diffuser' if lit else 'Ceiling',(x+dx,2.887,z),(.18,.025,1.12),.006)

# The wake desk has an open exercise book, fine rules and a single pen.
box('BoardGreen',(-6.1,.788,14.04),(.30,.012,.23),.004)
box('Paper',(-6.1,.799,14.04),(.286,.012,.217),.002)
box('Gasket',(-6.10,.806,14.04),(.0015,.001,.21),.0002)
for side in [-1,1]:
    for n in range(6):box('Metal',(-6.1+side*.075,.806,13.97+n*.024),(.116,.0008,.00065),.0001)
box('Metal',(-5.88,.795,14.10),(.009,.009,.15),.003)
env['write_obj'](OUT/'Classroom_FurnitureAndShell.obj')

# Same leaf proportions as Art02, with actual upper glazing and perimeter gaskets.
env['groups']={}
z=9.9
for i,dz in enumerate([-.32,.32]):
    x=-2.025-i*.025
    box('HPL',(x+.027,.59,z+dz),(.018,1.02,.595),.002)
    box('Metal',(x+.037,.12,z+dz),(.008,.14,.595),.001)
    box('Glass',(x,1.66,z+dz),(.025,.97,.58),.0005)
    box('FrostBand',(x+.002,1.26,z+dz),(.025,.22,.56),.0004)
    for off in [-.318,.318]:box('Metal',(x+.016,1.13,z+dz+off),(.05,2.18,.035),.0015)
    for y in [.08,1.13,2.18]:box('Metal',(x+.016,y,z+dz),(.05,.045,.65),.0015)
box('Gasket',(-1.994,1.03,z+.24),(.009,.20,.045),.005)
box('Metal',(-1.982,1.03,z+.251),(.012,.155,.012),.003)
# Matching classroom-side recessed pull.
box('Metal',(-2.085,1.03,z+.251),(.015,.16,.025),.005)
env['write_obj'](OUT/'Classroom_SlidingLeaves.obj')
for p in OUT.glob('*.obj'):(HERE/p.name).write_bytes(p.read_bytes())
(HERE/'colliders.json').write_text(json.dumps(colliders,indent=2),encoding='utf-8')
print('Classroom meshes and',len(colliders),'simple colliders authored. Door opening z9.25..10.55.')
