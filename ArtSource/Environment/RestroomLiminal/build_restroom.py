"""Restroom discovery-room candidate. Reuses authored StartLiminal PBR maps.
Room OBJ: Unity world metres. Entrance OBJ: local to existing PaidToilet (12,0,10.6), Y=90.
Only new paths are written; no original model or texture is modified.
"""
from pathlib import Path
import ast, math
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
tree=ast.parse((HERE.parent/'StartLiminal/build_start_visual.py').read_text())
env={'math':math,'groups':{}}
for node in tree.body:
    if isinstance(node,ast.FunctionDef) and node.name in ['cross','sub','face','box','write_obj']:
        exec(compile(ast.Module(body=[node],type_ignores=[]),'<mesh helpers>','exec'),env)
box=env['box'];face=env['face']

def cylinder(mat,c,r,length,axis=0,n=24):
    others=[i for i in range(3) if i!=axis]
    rings=[]
    for end in [-1,1]:
        ring=[]
        for k in range(n):
            a=2*math.pi*k/n;p=list(c);p[axis]+=end*length/2
            p[others[0]]+=r*math.cos(a);p[others[1]]+=r*math.sin(a);ring.append(tuple(p))
        rings.append(ring)
    for k in range(n):
        a,b=rings[0][k],rings[0][(k+1)%n];d,e=rings[1][k],rings[1][(k+1)%n]
        normal=[0,0,0]
        for i in others:normal[i]=(a[i]+b[i])/2-c[i]
        face(mat,[a,b,e,d],normal)
    for i,ring in enumerate(rings):
        normal=[0,0,0];normal[axis]=[-1,1][i];face(mat,ring,normal)

def slab(poly,mat,x0,x1):
    # Cross section in Z,Y, extruded along X.
    p0=[(x0,y,z) for z,y in poly];p1=[(x1,y,z) for z,y in poly]
    face(mat,p0,(-1,0,0));face(mat,p1,(1,0,0))
    for k in range(len(poly)):
        j=(k+1)%len(poly);face(mat,[p0[k],p0[j],p1[j],p1[k]])

def wall_z(z,x0,x1,lo=0,hi=4.8):
    # Slab wainscot and upper paint use metre-scale UVs and true recessed joints.
    if lo<1.1:
        end=min(hi,1.1)
        for k in range(math.ceil((x1-x0)/1.2)):
            a=x0+k*1.2;b=min(a+1.2,x1)
            box('Stone',((a+b)/2,(lo+end)/2,z),(b-a-.004,end-lo,.18),.004)
    if hi>1.1:
        a=max(lo,1.1);box('Plaster',((x0+x1)/2,(a+hi)/2,z),(x1-x0,hi-a,.18),.006)
    if lo==0:box('Skirting',((x0+x1)/2,.065,z-.012),(x1-x0,.13,.205),.002)

# South wall, north wall with a real service-door opening and upper clerestory.
wall_z(7.91,6.2,12)
wall_z(13.29,6.2,7.575,hi=3.15)
wall_z(13.29,8.825,12,hi=3.15)
wall_z(13.29,7.575,8.825,lo=2.20,hi=3.15)
wall_z(13.29,6.2,7.0,lo=3.15,hi=4.45)
wall_z(13.29,11.5,12,lo=3.15,hi=4.45)
wall_z(13.29,6.2,12,lo=4.45,hi=4.8)
# West approach portal remains in its original location, at its original 3.4m height.
box('Plaster',(6.1,2.4,8.6),(.20,4.8,1.2),.006)
box('Plaster',(6.1,2.4,12.6),(.20,4.8,1.2),.006)
box('Plaster',(6.1,4.1,10.6),(.20,1.40,2.8),.006)
# East wall has a tall recess, not an independent toilet box.
for a,b in [(8,9.25),(11.95,13.2)]:
    box('Plaster',(12.12,2.4,(a+b)/2),(.24,4.8,b-a),.007)
    box('Stone',(11.982,.55,(a+b)/2),(.045,1.09,b-a-.006),.003)
    box('Skirting',(11.957,.065,(a+b)/2),(.025,.13,b-a),.002)
box('Plaster',(12.12,4.625,10.6),(.24,.35,2.70),.007)
# Ceiling: side service soffits, central high panel, structural ribs aligned with entry piers.
box('Ceiling',(9.1,4.90,10.6),(6.0,.20,5.4),.006)
for z in [8.30,12.90]:
    box('Ceiling',(9.1,4.625,z),(5.8,.35,.60),.008)
for z in [9.25,11.95]:
    box('Plaster',(9.1,4.62,z),(5.8,.36,.20),.008)
# North clerestory; each mullion and reveal is separate geometry.
box('Shadow',(9.25,3.8,13.34),(4.55,1.32,.16),.003)
box('Glass',(9.25,3.8,13.22),(4.45,1.20,.024),.002)
for x in [7.0,8.5,10,11.5]:box('Metal',(x,3.8,13.17),(.026,1.26,.04),.002)
for y in [3.17,4.43]:box('Metal',(9.25,y,13.17),(4.52,.026,.04),.002)
# Integrated luminaires, not a dense grid of ceiling lamps.
for x,y,span in [(8.35,4.79,2.5),(11.55,4.43,2.10)]:
    box('Shadow',(x,y,10.6),(.16,.025,span+.10),.003)
    box('Metal',(x,y-.009,10.6),(.13,.016,span+.06),.002)
    box('Diffuser',(x,y-.020,10.6),(.085,.012,span),.002)
# Flush service leaf retains the original next-ACT interactable behind this mesh.
box('ExitLeaf',(8.2,1.1,13.26),(1.18,2.15,.045),.003)
for x in [7.59,8.81]:box('Metal',(x,1.11,13.225),(.028,2.22,.06),.002)
box('Metal',(8.2,2.22,13.225),(1.25,.028,.06),.002)
cylinder('Metal',(8.65,1.03,13.232),.023,.012,axis=2)
box('Metal',(8.60,1.03,13.205),(.14,.022,.040),.007)
box('Graphite',(8.2,2.45,13.185),(.42,.14,.018),.003)

MODEL=ROOT/'Assets/Models/Environment/RestroomLiminal'
env['write_obj'](MODEL/'RestroomArchitecture.obj')
(HERE/'RestroomArchitecture.obj').write_bytes((MODEL/'RestroomArchitecture.obj').read_bytes())
print('Room faces:',sum(len(v) for v in env['groups'].values()))
env['groups']={}

# Full-height stone cheeks, a recessed human-scale door, and a transom above it.
for z in [9.40,11.80]:
    box('Stone',(12.015,2.225,z),(.39,4.45,.30),.006)
    box('Shadow',(11.824,2.225,z+.17),(.018,4.44,.018),.001)
for a,b in [(9.55,10.10),(11.20,11.65)]:
    box('Stone',(12.045,1.375,(a+b)/2),(.39,2.75,b-a),.004)
box('Stone',(12.045,2.49,10.65),(.39,.52,1.10),.005)
box('Glass',(12.17,3.52,10.60),(.025,1.54,2.12),.002)
for z in [9.535,10.60,11.665]:box('Metal',(12.13,3.52,z),(.045,1.55,.026),.002)
for y in [2.75,4.29]:box('Metal',(12.13,y,10.60),(.045,.026,2.16),.002)
box('Stone',(12.045,4.375,10.60),(.39,.15,2.10),.005)
# Door frame: 35mm jamb, 45mm leaf, 2.15m leaf height, 980mm clear leaf width.
for z in [10.115,11.185]:box('Metal',(12.17,1.12,z),(.10,2.24,.035),.002)
box('Metal',(12.17,2.24,10.65),(.10,.035,1.105),.002)
box('Shadow',(12.25,1.10,10.65),(.035,2.19,1.025),.001)
box('DoorLeaf',(12.205,1.10,10.65),(.045,2.15,.98),.003)
box('Metal',(12.176,.15,10.65),(.009,.22,.92),.002)
# Small opaque vision panel, gasket and satin metal trim.
for z in [10.801,10.959]:box('Metal',(12.173,1.66,z),(.014,.63,.012),.002)
for y in [1.351,1.969]:box('Metal',(12.173,y,10.88),(.014,.012,.17),.002)
box('Glass',(12.174,1.66,10.88),(.009,.61,.15),.001)
for y in [.30,1.10,1.95]:cylinder('Metal',(12.155,y,11.15),.009,.065,axis=1)
# Lever, spindle and rose. No oversized decorative frame.
cylinder('Metal',(12.178,1.04,10.26),.025,.012)
cylinder('Metal',(12.143,1.04,10.26),.011,.060)
cylinder('Metal',(12.113,1.04,10.325),.011,.14,axis=2)
cylinder('Metal',(12.113,1.04,10.395),.011,.022)
box('Threshold',(12.04,.025,10.65),(.48,.05,1.11),.003)
# Recess floor joins the existing landing tiles.
box('Porcelain',(12.05,-.005,10.60),(.54,.05,2.1),.002)
box('Shadow',(12.0,2.227,10.65),(.08,.012,1.05),.002)
box('Diffuser',(12.0,2.216,10.65),(.05,.008,.98),.002)

# Pictogram plaque, 600 x 280mm; figure icons are geometry, not giant lettering.
box('Graphite',(11.833,2.46,10.65),(.025,.28,.60),.004)
def person(z,female=False):
    x=11.817
    cylinder('Icon',(x,2.545,z),.022,.006,n=32)
    box('Icon',(x,2.490,z),(.006,.066,.040),.002)
    box('Icon',(x,2.516,z),(.006,.014,.086),.002)
    if female:slab([(z-.043,2.435),(z+.043,2.435),(z+.024,2.505),(z-.024,2.505)],'Icon',x-.003,x+.003)
    for dz in [-.019,.019]:box('Icon',(x,2.430,z+dz),(.006,.055,.012),.002)
    for dz in [-.037,.037]:box('Icon',(x,2.482,z+dz),(.006,.065,.012),.002)
person(10.755);person(10.545,True)
box('Icon',(11.814,2.49,10.65),(.007,.14,.004),.001)

# Payment unit and price plate on the south/right side of the door.
box('Metal',(11.842,1.39,9.825),(.025,.42,.26),.004)
box('Graphite',(11.773,1.39,9.825),(.12,.38,.225),.012)
box('Screen',(11.706,1.47,9.825),(.009,.135,.165),.004)
box('Reader',(11.704,1.29,9.825),(.011,.12,.165),.004)
# Contactless arcs, following a small circular wave rather than a toy-like button.
for r in [.017,.028,.039]:
    for k in range(15):
        a=-.85+k/15*1.7;b=-.85+(k+1)/15*1.7
        face('Icon',[(11.697,1.29+(r-.0015)*math.sin(a),9.825+(r-.0015)*math.cos(a)),(11.697,1.29+(r+.0015)*math.sin(a),9.825+(r+.0015)*math.cos(a)),(11.697,1.29+(r+.0015)*math.sin(b),9.825+(r+.0015)*math.cos(b)),(11.697,1.29+(r-.0015)*math.sin(b),9.825+(r-.0015)*math.cos(b))],(-1,0,0))
box('PricePlate',(11.832,1.805,9.825),(.018,.19,.30),.003)
box('Indicator',(11.700,1.585,9.825),(.008,.004,.050),.001)
# Card outline on the screen.
for y in [1.437,1.503]:box('Icon',(11.699,y,9.825),(.004,.003,.10),.001)
for z in [9.775,9.875]:box('Icon',(11.699,1.47,z),(.004,.066,.003),.001)

# Convert entrance groups to the existing PaidToilet local coordinate system.
for name,faces in env['groups'].items():
    converted=[]
    for pts,uv,n in faces:
        converted.append(([(10.6-p[2],p[1],p[0]-12) for p in pts],uv,(-n[2],n[1],n[0])))
    env['groups'][name]=converted
env['write_obj'](MODEL/'PaidToiletEntrance.obj')
(HERE/'PaidToiletEntrance.obj').write_bytes((MODEL/'PaidToiletEntrance.obj').read_bytes())
print('Entrance faces:',sum(len(v) for v in env['groups'].values()))
