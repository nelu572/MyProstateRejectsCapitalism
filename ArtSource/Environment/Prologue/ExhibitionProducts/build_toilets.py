"""ACT 2 product models, in metres and Unity axes (Y up, front -Z).

Standalone editable art source. Produces mesh data for import through Unity MCP;
does not edit Unity scenes, Prefabs, existing meshes, or project settings.
Material slots: porcelain, seat/lid, recessed bowl, satin hardware, controls.
"""
from pathlib import Path
import json
import math

OUT = Path(__file__).with_name("ToiletProducts.json")
SEGMENTS = 64


def vecsub(a, b):
    return [a[i] - b[i] for i in range(3)]


def cross(a, b):
    return [a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0]]


def smooth_profile(stations, steps=3):
    result = []
    for i in range(len(stations)-1):
        p0, p1 = stations[max(0, i-1)], stations[i]
        p2, p3 = stations[i+1], stations[min(len(stations)-1, i+2)]
        for k in range(steps):
            t = k / steps
            row = [.5*((2*p1[d])+(-p0[d]+p2[d])*t +
                   (2*p0[d]-5*p1[d]+4*p2[d]-p3[d])*t*t +
                   (-p0[d]+3*p1[d]-3*p2[d]+p3[d])*t*t*t) for d in range(4)]
            row[1], row[2] = max(.002, row[1]), max(.002, row[2])
            result.append(row)
    return result + [list(stations[-1])]


class Product:
    def __init__(self, name):
        self.name = name
        self.vertices = []
        self.normals = []
        self.colors = []
        self.triangles = [[] for _ in range(5)]
        self.parts = []

    def add(self, name, vertices, faces, material, transform=None):
        if transform:
            vertices = [transform(p) for p in vertices]
        normals = [[0., 0., 0.] for _ in vertices]
        for a, b, c in faces:
            n = cross(vecsub(vertices[b], vertices[a]), vecsub(vertices[c], vertices[a]))
            for index in (a, b, c):
                for d in range(3):
                    normals[index][d] += n[d]
        for n in normals:
            length = math.sqrt(sum(x*x for x in n))
            if length > 1e-12:
                for d in range(3):
                    n[d] /= length
        offset = len(self.vertices)
        self.vertices.extend(vertices)
        self.normals.extend(normals)
        low_y = min(p[1] for p in vertices)
        high_y = max(p[1] for p in vertices)
        for p in vertices:
            shade = 1. - .22 * max(0., min(1., (high_y-p[1])/max(.001,high_y-low_y))) if material == 2 else 1.
            self.colors.append([shade,shade,shade,1.])
        for f in faces:
            self.triangles[material].extend(offset + index for index in f)
        self.parts.append({"name": name, "vertices": len(vertices), "triangles": len(faces), "material": material})

    def loft(self, name, profile, material=0, power=1., cap_first=False, cap_last=False, transform=None):
        vertices, faces = [], []
        for y, rx, rz, cz in profile:
            for i in range(SEGMENTS):
                a = 2*math.pi*i/SEGMENTS
                sx, sz = math.sin(a), math.cos(a)
                vertices.append([math.copysign(abs(sx)**power, sx)*rx, y,
                                 math.copysign(abs(sz)**power, sz)*rz+cz])
        for j in range(len(profile)-1):
            for i in range(SEGMENTS):
                a, b = j*SEGMENTS+i, j*SEGMENTS+(i+1)%SEGMENTS
                c, d = a+SEGMENTS, b+SEGMENTS
                faces.extend([(a,d,c), (a,b,d)])
        if cap_first:
            center = len(vertices)
            vertices.append([0, profile[0][0], profile[0][3]])
            faces.extend((i, center, (i+1)%SEGMENTS) for i in range(SEGMENTS))
        if cap_last:
            center = len(vertices)
            vertices.append([0, profile[-1][0], profile[-1][3]])
            start = (len(profile)-1)*SEGMENTS
            faces.extend((start+i, start+(i+1)%SEGMENTS, center) for i in range(SEGMENTS))
        self.add(name, vertices, faces, material, transform)

    def rounded_box(self, name, center, size, bevel=.02, material=0, power=.30, transform=None):
        x, y, z = center
        w, h, d = size
        r = min(bevel, h*.42, w*.20, d*.28)
        profile = [(y-h/2,w/2-r,d/2-r,z), (y-h/2+r*.3,w/2-r*.3,d/2-r*.3,z),
                   (y-h/2+r,w/2,d/2,z), (y+h/2-r,w/2,d/2,z),
                   (y+h/2-r*.3,w/2-r*.3,d/2-r*.3,z), (y+h/2,w/2-r,d/2-r,z)]
        def place(p):
            p = [p[0]+x,p[1],p[2]]
            return transform(p) if transform else p
        self.loft(name, profile, material, power, True, True, place)

    def cylinder(self, name, center, radius, height, material=3, axis="Y"):
        x, y, z = center
        def place(p):
            if axis == "X":
                p = [p[1],p[0],-p[2]]
            return [p[0]+x,p[1]+y,p[2]+z]
        self.loft(name, [(-height/2,radius*.88,radius*.88,0),(-height*.3,radius,radius,0),
                         (height*.3,radius,radius,0),(height/2,radius*.88,radius*.88,0)],
                  material, 1., True, True, place)

    def seat(self, height, rx, rz, cz, power=1.):
        innerx, innerz, t = rx-.058, rz-.057, .028
        profile = [(height,rx-.008,rz-.008,cz), (height+.005,rx,rz,cz),
                   (height+t-.005,rx,rz,cz),(height+t,rx-.005,rz-.005,cz),
                   (height+t,innerx+.006,innerz+.006,cz),(height+t-.005,innerx,innerz,cz),
                   (height+.005,innerx,innerz,cz),(height,innerx+.006,innerz+.006,cz),
                   (height,rx-.008,rz-.008,cz)]
        self.loft("Separate open seat ring", profile, 1, power)
        for x in (-.18,.18):
            self.cylinder("Seat hinge", (x,height+.014,.18), .014,.052,3,"X")
        for x in (-rx*.86,rx*.86):
            self.rounded_box("Seat support pad",(x,height-.0065,-.14),(.027,.013,.043),.003,1)

    def lid(self, seat_height, rx, rz, angle, power=1.):
        hinge = (0,seat_height+.030,.18)
        a = math.radians(angle)
        def opened(p):
            dy, dz = p[1]-hinge[1], p[2]-hinge[2]
            return [p[0],hinge[1]+math.cos(a)*dy-math.sin(a)*dz,
                    hinge[2]+math.sin(a)*dy+math.cos(a)*dz]
        self.rounded_box("Separate raised lid",(0,hinge[1],-.105),(rx*2,.025,rz*2),.010,1,power,opened)

    def finish(self):
        low = [min(p[d] for p in self.vertices) for d in range(3)]
        high = [max(p[d] for p in self.vertices) for d in range(3)]
        return {"name":self.name,"vertices":self.vertices,"normals":self.normals,"colors":self.colors,
                "submeshes":self.triangles,"parts":self.parts,"bounds":{"min":low,"max":high},
                "triangleCount":sum(map(len,self.triangles))//3}


def build(kind):
    p = Product(kind)
    if kind == "Classic":
        outer=[(0,.135,.18,.10),(.025,.151,.195,.10),(.065,.137,.17,.10),(.17,.118,.145,.08),
               (.245,.17,.22,.025),(.32,.239,.315,-.035),(.375,.266,.347,-.045),(.408,.264,.345,-.045)]
        inner=[(.408,.264,.345,-.045),(.408,.225,.292,-.045),(.385,.216,.278,-.035),
               (.335,.19,.24,0),(.275,.143,.176,.035),(.22,.064,.072,.065),(.202,.025,.032,.07)]
        power, seat = 1., (.421,.277,.358,-.045)
        p.rounded_box("Rear structural spine",(0,.25,.34),(.22,.50,.12),.028)
        p.rounded_box("Rounded separate cistern",(0,.635,.32),(.43,.35,.18),.027)
        p.rounded_box("Cistern lid with thin joint",(0,.826,.32),(.447,.024,.199),.009,1)
        for x in (-.033,.033):
            p.cylinder("Dual flush button",(x,.843,.315),.027,.007)
        lid=(.257,.285,96,1.)
    elif kind == "Bidet":
        outer=[(0,.235,.294,.035),(.028,.261,.32,.03),(.14,.262,.327,.025),(.27,.274,.345,.005),
               (.365,.275,.354,-.025),(.401,.266,.345,-.045)]
        inner=[(.401,.266,.345,-.045),(.401,.223,.289,-.055),(.376,.214,.274,-.05),
               (.325,.184,.235,-.015),(.265,.13,.17,.025),(.205,.045,.055,.06)]
        power, seat = .77, (.414,.278,.350,-.06)
        p.rounded_box("Integrated low rear housing",(0,.46,.35),(.47,.16,.18),.045,0,.62)
        p.rounded_box("Rear access joint",(0,.541,.35),(.436,.015,.151),.006,1,.62)
        p.rounded_box("Small side control inset",(.274,.341,-.025),(.013,.036,.105),.004,3)
        for z in (-.055,-.015):
            p.rounded_box("Bidet side button",(.283,.343,z),(.009,.017,.024),.003,4)
        lid=(.258,.282,100,.77)
    elif kind == "Premium":
        outer=[(0,.19,.235,.035),(.025,.217,.256,.025),(.10,.21,.25,.025),(.21,.244,.285,.005),
               (.33,.301,.354,-.025),(.395,.312,.371,-.035),(.425,.302,.360,-.035)]
        inner=[(.425,.302,.360,-.035),(.425,.247,.299,-.04),(.395,.237,.282,-.03),
               (.345,.207,.244,.005),(.275,.145,.18,.04),(.22,.05,.06,.075)]
        power, seat = 1., (.438,.31,.367,-.035)
        p.rounded_box("Rounded concealed cistern",(0,.566,.385),(.435,.272,.17),.044,0,.72)
        p.rounded_box("Flush rocker",(0,.706,.38),(.095,.008,.043),.003,3,.6)
        lid=(.289,.284,98,1.)
    else:
        outer=[(0,.219,.267,.045),(.025,.238,.284,.035),(.12,.229,.277,.025),(.28,.25,.31,-.015),
               (.375,.269,.337,-.035),(.410,.264,.332,-.035)]
        inner=[(.410,.264,.332,-.035),(.410,.207,.275,-.04),(.38,.199,.261,-.03),
               (.325,.169,.218,.005),(.26,.105,.145,.035),(.205,.036,.047,.075)]
        power, seat = .45, (.423,.277,.344,-.035)
        p.rounded_box("Architectural slim rear column",(0,.45,.38),(.316,.72,.12),.024,0,.38)
        p.rounded_box("Column top cap",(0,.822,.38),(.328,.018,.132),.006,1,.38)
        for x in (-.043,.043):
            p.rounded_box("Split flush plate",(x,.715,.316),(.073,.042,.008),.003,3,.38)
        lid=(.260,.280,93,.48)
    p.loft("Sculpted porcelain body",smooth_profile(outer),0,power,True)
    p.loft("Recessed bowl and glazed rim",smooth_profile(inner,2),2,power,False,True)
    p.seat(*seat,power)
    p.lid(seat[0],*lid)
    return p.finish()


def main():
    products=[build(kind) for kind in ("Classic","Bidet","Premium","Design")]
    data={"units":"metres","up":"Y","front":"-Z","origin":"floor contact",
          "materialSlots":["Porcelain","SeatAndLid","BowlInterior","SatinHardware","Controls"],
          "products":products}
    OUT.write_text(json.dumps(data,separators=(",",":")),encoding="utf-8")
    for p in products:
        print(p["name"],p["triangleCount"],"triangles",p["bounds"])
    print("Wrote",OUT)


if __name__ == "__main__":
    main()
