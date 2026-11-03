"""Small additive architectural candidate; reuses StartVisual meshes and PBR maps.
No original output is overwritten. Unity world metres, X right, Y up, Z forward.
"""
from pathlib import Path
import ast, math

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
tree=ast.parse((HERE/'build_start_visual.py').read_text())
env={'math':math,'groups':{}}
for node in tree.body:
    if isinstance(node,ast.FunctionDef) and node.name in ['cross','sub','face','box','write_obj']:
        exec(compile(ast.Module(body=[node],type_ignores=[]),'<existing mesh functions>','exec'),env)
box=env['box']

# Offset opening, 1.92m clear, then a short compression to a 2.45m ceiling.
box('PortalStone',(.96,1.225,2.0),(1.68,2.45,5.6),.045)
box('TransitionPaint',(0,2.56,3.0),(3.82,.22,8.0),.008)
box('TransitionPaint',(0,3.005,-1.02),(3.82,1.11,.16),.008)
box('TransitionPaint',(0,2.755,7.0),(3.82,.61,.16),.008)
for z in [1.4,5.6]:
    box('Reveal',(-.85,2.442,z),(.16,.024,1.20),.003)
    box('Metal',(-.85,2.431,z),(.13,.016,1.16),.002)
    box('Diffuser',(-.85,2.420,z),(.085,.012,1.10),.002)
box('Reveal',(-.84,2.445,-1.10),(1.68,.025,.075),.002)
box('Diffuser',(-.84,2.428,-1.10),(1.58,.012,.035),.002)

# Replace the false end-panel read with an upper light aperture, just before the turn.
box('TransitionPaint',(0,1.53,11.86),(3.61,3.06,.12),.008)
box('Metal',(0,2.65,11.785),(3.27,.67,.035),.003)
box('FrostedGlass',(0,2.65,11.758),(3.20,.60,.025),.002)

# The landing reuses its walls; only the higher window, lintels and floor finish are added.
box('TransitionPaint',(4.1,3.275,12.13),(4.2,.25,.26),.004)
box('TransitionPaint',(2.02,3.28,10.6),(.12,.44,2.82),.004)
box('Porcelain',(6.10,-.025,10.6),(.23,.05,2.80),.0015)
box('LandingPaint',(9.1,4.625,13.3),(5.8,.35,.20),.006)
box('LandingPaint',(6.6,3.8,13.3),(.8,1.30,.20),.006)
box('LandingPaint',(11.75,3.8,13.3),(.5,1.30,.20),.006)
box('LandingPaint',(6.1,4.1,10.6),(.20,1.40,2.8),.008)
box('Metal',(9.25,3.80,13.20),(4.53,1.28,.06),.004)
box('FrostedGlass',(9.25,3.80,13.16),(4.45,1.20,.025),.002)
for x in [7.025,8.5,10,11.475]:
    box('Metal',(x,3.8,13.135),(.027,1.20,.035),.002)
for row in range(9):
    z0=8+row*.6;z1=min(13.2,z0+.6)
    for col in range(10):
        x0=6.2+col*.6;x1=min(12,x0+.6)
        box('Porcelain',((x0+x1)/2,.012,(z0+z1)/2),(x1-x0-.004,.024,z1-z0-.004),.0015)
# Two unobtrusive upper slots at the high ceiling, not a new prop set.
for x in [7.6,10.6]:
    box('Reveal',(x,4.79,10.6),(.14,.025,2.4),.003)
    box('Diffuser',(x,4.77,10.6),(.08,.012,2.3),.002)

out=ROOT/'Assets/Models/Environment/StartLiminal/DiscoveryAdditions.obj'
env['write_obj'](out)
(HERE/'DiscoveryAdditions.obj').write_bytes(out.read_bytes())
print('Added',sum(len(v) for v in env['groups'].values()),'faces; original architecture and all textures preserved')
