"""Clean, shallow non-flush squat-latrine reconstruction for ACT 2.

The section has a closed bottom; it never cuts a hole in the game floor.
No historical date, provenance, or maker is claimed.
"""
from pathlib import Path
import json
import sys
sys.dont_write_bytecode = True
from build_toilets import Product

p = Product("MuseumPitToilet")
# White section frame, distinct from the ceramic squat slab. Geometry is in metres.
p.loft("White shallow section frame",[
    (0,.56,.42,0),(.006,.56,.42,0),(.026,.56,.42,0),
    (.032,.55,.41,0),(.032,.453,.362,0),(.004,.453,.362,0)
],0,.26,True)
# A planar slab around a narrow opening, rather than a buried sitting toilet.
p.loft("Non-flush squat slab",[
    (.004,.445,.355,0),(.075,.445,.355,0),(.090,.434,.344,0),
    (.090,.126,.252,0),(.083,.116,.242,0)
],1,.30)
p.loft("Closed-bottom open shaft section",[
    (.083,.116,.242,0),(.014,.107,.231,0),(.006,.107,.231,0)
],2,.30,False,True)
for x in (-.278,.278):
    p.rounded_box("Raised footrest",(x,.105,0),(.188,.030,.43),.008,3,.25)
    for z in (-.16,-.10,-.04,.02,.08,.14):
        p.rounded_box("Footrest grip rib",(x,.122,z),(.151,.004,.012),.001,3,.30)
data=p.finish()
data["units"]="metres"
data["up"]="Y"
data["front"]="-Z"
data["displayBaseWorldY"]=.17
data["reconstruction"]=True
path=Path(__file__).with_name("MuseumPitToilet.json")
path.write_text(json.dumps(data,separators=(",",":")),encoding="utf-8")
print(data["name"],data["triangleCount"],"triangles",data["bounds"])
print("Wrote",path)
