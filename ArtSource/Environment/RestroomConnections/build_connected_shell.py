"""Connection-only candidate: union the wall/ceiling volume, then cut real openings.
Metres, Unity X/Y/Z. Existing doors, hardware, lighting and finish assets are reused.
All internal box faces are discarded. A common grid keeps shared boundary edges aligned.
"""
from pathlib import Path
import json, math
HERE=Path(__file__).resolve().parent
solids=[];voids=[]
def solid(x0,x1,y0,y1,z0,z1):solids.append((x0,x1,y0,y1,z0,z1))
def opening(x0,x1,y0,y1,z0,z1):voids.append((x0,x1,y0,y1,z0,z1))

# Only the junction beyond Z=8 and the restroom room are replaced.
solid(-2.02,-1.8,0,3.30,8,12.02)
solid(-2.02,6.2,0,3.60,11.8,12.02)
solid(1.8,2.02,0,3.60,8,9.2)
solid(1.8,6.2,0,3.60,9.0,9.2)
solid(-1.8,-1.7925,.11,1.05,8,12.02)
solid(1.7925,1.8,.11,1.05,8,9.2)
solid(-2.02,1.86,3.06,3.30,8,12.02)
solid(1.8,6.3,3.40,3.60,9.0,12.02)
solid(1.74,1.86,3.06,3.60,9.0,12.02)
# Closed joint to the unchanged, lower approach ceiling at Z=8.
solid(-2.02,2.02,3.06,3.30,8,8.04)

# Room perimeter and approach opening form one wall shell.
solid(6.0,12.32,0,5.0,7.82,8.0)
solid(6.0,12.32,0,5.0,13.2,13.38)
solid(6.0,6.2,0,5.0,8.0,9.2)
solid(6.0,6.2,0,5.0,11.8,13.2)
solid(6.0,6.2,3.4,5.0,9.2,11.8)
# Flat east wall replaces the independent tall cheeks and protruding block piers.
solid(11.85,12.32,0,5.0,8.0,13.2)
solid(6.0,12.32,4.8,5.0,7.82,13.38)
# Existing service soffits become continuous with the wall and ceiling volumes.
solid(6.2,12.32,4.45,5.0,8.0,8.6)
solid(6.2,12.32,4.45,5.0,12.6,13.2)
solid(11.45,12.32,4.44,5.0,8.0,13.2)

# Door, transom and clerestory apertures match the existing retained assemblies.
opening(11.7,12.08,0,2.23,10.10,11.20)
opening(12.08,12.5,0,2.265,10.09,11.21)
opening(11.7,12.5,2.75,4.30,9.535,11.665)
opening(7.575,8.825,0,2.20,13.0,13.5)
opening(7.0,11.5,3.15,4.45,13.0,13.5)
opening(-1.6,1.6,2.35,2.95,11.7,12.2)

coords=[]
for axis in range(3):
    a={v for b in solids+voids for v in (b[axis*2],b[axis*2+1])}
    if axis==1:a.update([.11,1.05])
    coords.append(sorted(a))
xs,ys,zs=coords;nx,ny,nz=[len(a)-1 for a in coords]
lookup=[{v:i for i,v in enumerate(a)} for a in coords]
cells=set()
def cell_range(b):
    return [range(lookup[a][b[a*2]],lookup[a][b[a*2+1]]) for a in range(3)]
for b in solids:
    rx,ry,rz=cell_range(b)
    for i in rx:
        for j in ry:
            for k in rz:cells.add((i,j,k))
for b in voids:
    rx,ry,rz=cell_range(b)
    for i in rx:
        for j in ry:
            for k in rz:cells.discard((i,j,k))

faces=[]
def material(center,n):
    x,y,z=center
    if (n[1]<-.5 and y>=3.0) or y>=4.439:return 'Ceiling'
    if y<.110001:return 'Skirting'
    if y<1.050001:return 'Stone'
    if x>11.849 and 10.089<z<11.211 and y<2.266 and (abs(n[2])>.5 or n[1]<-.5):return 'Stone'
    if x<6.2:
        return 'CrossPaint' if n[2]>.5 and abs(z-9.2)<.001 else 'CornerPaint'
    return 'Plaster'
def add(mat,pts,n):
    u=[pts[1][a]-pts[0][a] for a in range(3)];v=[pts[2][a]-pts[0][a] for a in range(3)]
    cr=(u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0])
    if sum(cr[a]*n[a] for a in range(3))<0:pts=list(reversed(pts))
    faces.append((mat,pts,n))
for i,j,k in sorted(cells):
    lo=(xs[i],ys[j],zs[k]);hi=(xs[i+1],ys[j+1],zs[k+1])
    for axis in range(3):
        axes=[a for a in range(3) if a!=axis]
        for sign in [-1,1]:
            neighbor=[i,j,k];neighbor[axis]+=sign
            if tuple(neighbor) in cells:continue
            pts=[]
            for a,b in [(0,0),(1,0),(1,1),(0,1)]:
                p=list(lo);p[axis]=hi[axis] if sign==1 else lo[axis]
                p[axes[0]]=hi[axes[0]] if a else lo[axes[0]]
                p[axes[1]]=hi[axes[1]] if b else lo[axes[1]];pts.append(tuple(p))
            n=[0,0,0];n[axis]=sign;center=tuple(sum(p[a] for p in pts)/4 for a in range(3))
            add(material(center,n),pts,tuple(n))

# The existing small corner aperture gets a true recessed glass/frame connection.
def cuboid(mat,c,s):
    lo=tuple(c[a]-s[a]/2 for a in range(3));hi=tuple(c[a]+s[a]/2 for a in range(3))
    for axis in range(3):
        rest=[a for a in range(3) if a!=axis]
        for sign in [-1,1]:
            pts=[]
            for u,v in [(0,0),(1,0),(1,1),(0,1)]:
                p=list(lo);p[axis]=hi[axis] if sign==1 else lo[axis]
                p[rest[0]]=hi[rest[0]] if u else lo[rest[0]];p[rest[1]]=hi[rest[1]] if v else lo[rest[1]];pts.append(tuple(p))
            n=[0,0,0];n[axis]=sign;add(mat,pts,tuple(n))
for x in [-1.62,1.62]:cuboid('CornerMetal',(x,2.65,11.78),(.035,.67,.04))
for y in [2.325,2.975]:cuboid('CornerMetal',(0,y,11.78),(3.275,.025,.04))
cuboid('CornerGlass',(0,2.65,11.815),(3.22,.63,.025))

# One indexed mesh, with material submeshes and shared coplanar boundary vertices.
positions=[];normals=[];uvs=[];indices={};submeshes={}
for mat,pts,n in faces:
    dominant=max(range(3),key=lambda a:abs(n[a]));axes=[(2,1),(0,2),(0,1)][dominant]
    ids=[]
    for p in pts:
        uv=(p[axes[0]],p[axes[1]])
        key=tuple(round(v,7) for v in p)+n+tuple(round(v,7) for v in uv)
        if key not in indices:
            indices[key]=len(positions);positions.append(p);normals.append(n);uvs.append(uv)
        ids.append(indices[key])
    submeshes.setdefault(mat,[]).extend([ids[0],ids[1],ids[2],ids[0],ids[2],ids[3]])
data={'positions':positions,'normals':normals,'uv':uvs,'submeshes':[{'material':m,'triangles':t} for m,t in submeshes.items()]}
(HERE/'ConnectedShell.mesh.json').write_text(json.dumps(data,separators=(',',':')))

# Editable OBJ source shares position indices across all finish boundaries.
objpos={};objverts=[]
for _,pts,_ in faces:
    for p in pts:
        if p not in objpos:objpos[p]=len(objverts)+1;objverts.append(p)
lines=['# Connected architectural shell, metres, OBJ right handed','o ConnectedArchitecture']
lines += [f'v {-p[0]:.7f} {p[1]:.7f} {p[2]:.7f}' for p in objverts]
for _,pts,n in faces:
    dominant=max(range(3),key=lambda a:abs(n[a]));axes=[(2,1),(0,2),(0,1)][dominant]
    lines += [f'vt {p[axes[0]]:.7f} {p[axes[1]]:.7f}' for p in pts]
    lines += [f'vn {-n[0]} {n[1]} {n[2]}' for p in pts]
last=None;corner=1
for mat,pts,n in faces:
    if mat!=last:lines.append('usemtl '+mat);last=mat
    lines.append('f '+' '.join(f'{objpos[pts[j]]}/{corner+j}/{corner+j}' for j in reversed(range(len(pts)))))
    corner+=len(pts)
(HERE/'ConnectedShell.obj').write_text('\n'.join(lines),encoding='ascii')
print('Continuous union:',len(cells),'solid cells,',len(faces),'external quads,',len(positions),'vertices;',len(submeshes),'finish groups')
