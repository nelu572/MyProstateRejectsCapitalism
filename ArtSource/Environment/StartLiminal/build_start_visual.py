"""Editable metre-scale architecture and original, seamless PBR surfaces.
Unity world coordinates: X right, Y up, Z forward. No external source assets.
Run from the project root. Only writes this candidate's new output paths.
"""
from pathlib import Path
import math, random, struct, zlib

ROOT = Path(__file__).resolve().parents[3]
MODEL = ROOT / 'Assets/Models/Environment/StartLiminal'
TEX = ROOT / 'Assets/Textures/StartLiminal'
MODEL.mkdir(parents=True, exist_ok=True)
TEX.mkdir(parents=True, exist_ok=True)
groups = {}

def cross(a,b): return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def face(mat, pts, normal=None):
    n = cross(sub(pts[1],pts[0]),sub(pts[2],pts[0]))
    length = math.sqrt(sum(x*x for x in n))
    if length < 1e-9: return
    n = tuple(x/length for x in n)
    if normal and sum(x*y for x,y in zip(n,normal))<0:
        pts=list(reversed(pts));n=tuple(-x for x in n)
    axis=max(range(3),key=lambda i:abs(n[i]))
    uvaxes=[(2,1),(0,2),(0,1)][axis]
    uv=[(p[uvaxes[0]],p[uvaxes[1]]) for p in pts]
    groups.setdefault(mat,[]).append((pts,uv,n))

def box(mat,c,s,b=.004):
    h=[v/2 for v in s]; b=min(b,min(h)*.48)
    # Six flats, twelve bevel strips, eight corner facets: no overlapping cubes.
    for axis in range(3):
        others=[i for i in range(3) if i!=axis]
        for sign in [-1,1]:
            pts=[]
            for u,v in [(-1,-1),(1,-1),(1,1),(-1,1)]:
                p=list(c);p[axis]+=sign*h[axis]
                p[others[0]]+=u*(h[others[0]]-b);p[others[1]]+=v*(h[others[1]]-b)
                pts.append(tuple(p))
            n=[0,0,0];n[axis]=sign;face(mat,pts,n)
    for free in range(3):
        a,d=[i for i in range(3) if i!=free]
        for sa in [-1,1]:
            for sd in [-1,1]:
                pts=[]
                for which,sg in [(0,-1),(0,1),(1,1),(1,-1)]:
                    p=list(c);p[free]+=sg*(h[free]-b)
                    p[a]+=sa*(h[a]-(b if which else 0));p[d]+=sd*(h[d]-(0 if which else b));pts.append(tuple(p))
                n=[0,0,0];n[a]=sa;n[d]=sd;face(mat,pts,n)
    for sx in [-1,1]:
        for sy in [-1,1]:
            for sz in [-1,1]:
                signs=[sx,sy,sz];pts=[]
                for axis in range(3):
                    pts.append(tuple(c[i]+signs[i]*(h[i]-(0 if i==axis else b)) for i in range(3)))
                face(mat,pts,signs)

def stripwall(x,z0,z1,height=3.15,door=None):
    segments=[(z0,z1)] if not door else [(z0,door[0]),(door[1],z1)]
    for a,b in segments:
        box('Paint',(x,2.1,(a+b)/2),(.22,height-1.05,b-a),.008)
        for iz in range(math.ceil((b-a)/1.2)):
            lo=a+iz*1.2;hi=min(b,lo+1.2)
            box('Limestone',(x,.53,(lo+hi)/2),(.235,1.034,hi-lo-.004),.003)
        box('Reveal',(x-math.copysign(.008,x),.055,(a+b)/2),(.25,.11,b-a),.002)
    if door:
        a,b=door
        box('Paint',(x,2.71,(a+b)/2),(.22,.88,b-a),.006)

# Six 600mm porcelain tiles across, a physically recessed 4mm grout grid.
box('Grout',(0,-.075,0),(3.82,.11,24),.001)
for row in range(40):
    for col in range(6):
        box('Porcelain',(-1.5+col*.6,-.025,-11.7+row*.6),(.596,.05,.596),.0015)
# Cross-corridor extension only, matching the original turn and route.
for row in range(5):
    for col in range(7):
        box('Porcelain',(2.1+col*.6,-.025,9.3+row*.6),(.596,.05,.596),.0015)

stripwall(-1.91,-12,12,door=(-7.4,-6.3))
stripwall(1.91,-12,-8.7)
stripwall(1.91,-4.3,9.2)
# Long frosted service-light well: real recess, sill, reveals and slender mullions.
box('Paint',(2.35,1.575,-6.5),(.2,3.15,4.4),.008)
box('Limestone',(2.03,.53,-6.5),(.44,1.034,4.396),.004)
box('Paint',(2.03,2.96,-6.5),(.44,.38,4.4),.006)
box('Metal',(1.795,1.065,-6.5),(.10,.035,4.42),.002)
box('Metal',(1.795,2.77,-6.5),(.075,.035,4.42),.002)
for zz in [-8.7,-7.6,-6.5,-5.4,-4.3]:
    box('Metal',(1.825,1.92,zz),(.075,1.72,.026),.002)
for zz in [-8.15,-7.05,-5.95,-4.85]:
    box('FrostedGlass',(1.91,1.92,zz),(.024,1.67,1.07),.002)
    box('Diffuser',(2.20,1.92,zz),(.025,1.6,1.02),.001)

# Flush maintenance door with 35mm steel frame and lever; no decorative casing.
for zz in [-7.4,-6.3]:
    box('Metal',(-1.79,1.13,zz),(.08,2.26,.035),.002)
box('Metal',(-1.79,2.25,-6.85),(.08,.035,1.13),.002)
box('Reveal',(-1.8,1.11,-6.85),(.03,2.20,1.065),.001)
box('Door',(-1.77,1.11,-6.85),(.045,2.16,1.025),.003)
box('Metal',(-1.727,1.03,-6.49),(.025,.16,.035),.003)
box('Metal',(-1.70,1.04,-6.54),(.025,.025,.14),.007)
box('Metal',(-1.75,1.72,-6.85),(.03,.28,.16),.002)
box('Reveal',(-1.73,1.72,-6.85),(.008,.24,.12),.001)

# Back and blind end. The toilet remains occluded until the existing right turn.
box('Paint',(0,1.575,-12.12),(4.04,3.15,.24),.008)
box('Paint',(2.15,1.575,12.13),(8.1,3.15,.26),.012)
box('Limestone',(0,.55,11.975),(3.6,1.1,.035),.003)
box('Door',(-.28,1.83,11.956),(1.66,1.42,.035),.004)
box('Reveal',(.59,1.83,11.94),(.045,1.50,.03),.002)
box('Diffuser',(.63,1.83,11.934),(.018,1.45,.02),.002)

# Stepped ceiling with a raised central plenum, not a single box lid.
box('Ceiling',(0,3.15,-9),(4.04,.18,6),.006)
box('Ceiling',(0,3.65,0),(2.7,.18,12),.006)
for zz in [-6,6]:
    box('Paint',(0,3.4,zz),(2.72,.5,.14),.006)
for xx in [-1.63,1.63]:
    box('Ceiling',(xx,3.15,0),(.56,.18,12),.006)
    box('Paint',(math.copysign(1.36,xx),3.39,0),(.08,.50,12),.006)
    box('Reveal',(math.copysign(1.38,xx),3.10,0),(.10,.025,11.9),.001)
    box('Diffuser',(math.copysign(1.35,xx),3.22,0),(.035,.025,11.8),.002)
box('Ceiling',(0,3.15,9),(4.04,.18,6),.006)
# Rounded soffit portals, three separated structural bays. Quarter-circle inner corners.
for zz in [-5.8,1.0,7.4]:
    for side in [-1,1]:
        pts=[]
        for k in range(13):
            angle=k/12*math.pi/2
            pts.append((side*(1.40+.4*math.cos(angle)),2.61+.4*math.sin(angle)))
        # Fill between rounded interior and square exterior to make actual curved soffit.
        outer=[(side*1.94,2.61),(side*1.94,3.16),(side*1.40,3.16)]
        loop=pts+list(reversed(outer))
        for i in range(len(loop)):
            a=loop[i];b=loop[(i+1)%len(loop)]
            face('Paint',[(a[0],a[1],zz-.15),(b[0],b[1],zz-.15),(b[0],b[1],zz+.15),(a[0],a[1],zz+.15)])
        for dz in [-.15,.15]:
            for k in range(12):
                a=pts[k];b=pts[k+1];anchor=(side*1.94,3.16)
                face('Paint',[(a[0],a[1],zz+dz),(b[0],b[1],zz+dz),(anchor[0],anchor[1],zz+dz)],(0,0,math.copysign(1,dz)))
            face('Paint',[(pts[0][0],pts[0][1],zz+dz),(side*1.94,2.61,zz+dz),(side*1.94,3.16,zz+dz)],(0,0,math.copysign(1,dz)))
            face('Paint',[(pts[-1][0],pts[-1][1],zz+dz),(side*1.94,3.16,zz+dz),(side*1.40,3.16,zz+dz)],(0,0,math.copysign(1,dz)))
    box('Paint',(0,3.085,zz),(2.80,.15,.30),.006)

# Recessed luminaires, with a black gasket and aluminium lip.
for zz in [-10,-7.4,-3.7,3.6,8.8]:
    yy=3.046 if zz < -6 or zz > 6 else 3.546
    box('Reveal',(-.35,yy,zz),(1.22,.028,.19),.003)
    box('Metal',(-.35,yy-.009,zz),(1.18,.023,.16),.003)
    box('Diffuser',(-.35,yy-.022,zz),(1.10,.013,.115),.003)
# A recessed return grille, not free-floating decoration.
box('Reveal',(.65,3.041,-8.35),(.42,.025,.68),.002)
for k in range(14):
    box('Metal',(.65,3.025,-8.65+k*.046),(.38,.028,.014),.001)

def write_obj(path):
    lines=['# Start Liminal candidate | metres | Unity X/Y/Z | authored source']
    idx=1
    for mat,faces in groups.items():
        lines += ['o '+mat, 'g '+mat, 'usemtl '+mat]
        for pts,uv,n in faces:
            # OBJ is right-handed; Unity imports with X mirrored. Convert here.
            for p in pts: lines.append('v '+' '.join(f'{a:.6f}' for a in (-p[0],p[1],p[2])))
            for u,v in uv:lines.append(f'vt {u:.6f} {v:.6f}')
            for p in pts:lines.append('vn '+' '.join(f'{a:.6f}' for a in (-n[0],n[1],n[2])))
            lines.append('f '+' '.join(f'{i}/{i}/{i}' for i in reversed(range(idx,idx+len(pts)))))
            idx+=len(pts)
    path.write_text('\n'.join(lines),encoding='ascii')
write_obj(MODEL/'StartLiminalArchitecture.obj')
(Path(__file__).parent/'StartLiminalArchitecture.obj').write_bytes((MODEL/'StartLiminalArchitecture.obj').read_bytes())

def png(path, w, h, pixels):
    def chunk(t,b):return struct.pack('>I',len(b))+t+b+struct.pack('>I',zlib.crc32(t+b)&0xffffffff)
    data=b''.join(b'\0'+bytes(pixels[y*w*4:(y+1)*w*4]) for y in range(h))
    path.write_bytes(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(data,6))+chunk(b'IEND',b''))

# Tileable microtexture, normal and metallic/smoothness. No artificial dirt or distress.
N=512
for name,base,smooth,metal,relief in [
    ('Porcelain',(222,220,211),.76,0,.025),
    ('Limestone',(220,215,201),.31,0,.09),
    ('Paint',(239,237,228),.13,0,.055),
    ('Metal',(177,182,181),.63,.9,.13),
    ('FrostedGlass',(171,195,189),.54,.12,.48),
]:
    rng=random.Random(name);values=[]
    for y in range(N):
        for x in range(N):
            grain=rng.random()-.5
            broad=math.sin(x*2*math.pi/N*3)*math.sin(y*2*math.pi/N*5)
            if name=='Metal': grain=.5*math.sin(y*math.pi*2/8)+grain*.3
            if name=='FrostedGlass':grain=.5*math.cos(x*math.pi*2/8)
            values.append(grain*.7+broad*.3)
    albedo=bytearray();normal=bytearray();mask=bytearray()
    for y in range(N):
        for x in range(N):
            v=values[y*N+x]
            albedo.extend([max(0,min(255,int(c+v*(8 if name!='Paint' else 4)))) for c in base]+[255])
            dx=(values[y*N+(x+1)%N]-values[y*N+(x-1)%N])*relief
            dy=(values[((y+1)%N)*N+x]-values[((y-1)%N)*N+x])*relief
            norm=math.sqrt(1+dx*dx+dy*dy)
            normal.extend([int(127.5-dx/norm*127.5),int(127.5-dy/norm*127.5),int(127.5+127.5/norm),255])
            mask.extend([int(metal*255),0,0,int((smooth+v*.04)*255)])
    png(TEX/(name+'_Base.png'),N,N,albedo)
    png(TEX/(name+'_Normal.png'),N,N,normal)
    png(TEX/(name+'_MetalSmooth.png'),N,N,mask)
print('Wrote',len(groups),'mesh groups;',sum(len(f) for f in groups.values()),'faces; 15 original PBR maps')
