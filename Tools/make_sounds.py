import numpy as np, sys, os
from scipy.io import wavfile
from scipy.signal import butter, lfilter

SR = 44100
OUT = sys.argv[1]
rng = np.random.default_rng(7)

def t(sec): return np.arange(int(SR*sec))/SR
def env(n, attack=0.003, release=0.01):
    e = np.ones(n); a=int(SR*attack); r=int(SR*release)
    if a: e[:a]=np.linspace(0,1,a)
    if r: e[-r:]*=np.linspace(1,0,r)
    return e
def expdecay(n, tau): return np.exp(-np.arange(n)/SR/tau)
def sweep(f0, f1, sec, shape='sine'):
    tt=t(sec); f=np.geomspace(f0,f1,len(tt)); ph=2*np.pi*np.cumsum(f)/SR
    if shape=='sine': return np.sin(ph)
    if shape=='square': return np.sign(np.sin(ph))
    if shape=='saw': return 2*((ph/(2*np.pi))%1)-1
def tone(f, sec): return np.sin(2*np.pi*f*t(sec))
def lp(x, fc): b,a=butter(2, fc/(SR/2)); return lfilter(b,a,x)
def hp(x, fc): b,a=butter(2, fc/(SR/2), 'high'); return lfilter(b,a,x)
def noise(sec): return rng.standard_normal(len(t(sec)))
def save(name, x, gain_db=-3):
    x = x/ (np.max(np.abs(x))+1e-9) * 10**(gain_db/20)
    x = x*env(len(x), 0.002, 0.008)
    wavfile.write(os.path.join(OUT, name+'.wav'), SR, (x*32767).astype(np.int16))

# laser pew: falling sweep, square + sine, fast decay
d=0.18; x=0.6*sweep(1800,280,d,'square')+0.8*sweep(1800,280,d); x=lp(x,6000)*expdecay(len(x),0.06); save('laser_fire',x,-4)
# empty click: tiny filtered noise tick
d=0.05; x=hp(noise(d),1500)*expdecay(len(t(d)),0.006)+0.5*tone(2200,d)*expdecay(len(t(d)),0.004); save('empty_click',x,-8)
# hit marker body: short crisp tick
d=0.08; x=tone(2600,d)*expdecay(len(t(d)),0.015)+0.3*hp(noise(d),3000)*expdecay(len(t(d)),0.005); save('hit_body',x,-5)
# hit marker head: brighter double ding
d=0.14; x=(tone(3200,d)+0.7*tone(4800,d))*expdecay(len(t(d)),0.035); save('hit_head',x,-4)
# hurt: low thud with buzz
d=0.25; x=sweep(160,60,d)*expdecay(len(t(d)),0.08)+0.35*lp(sweep(220,120,d,'square'),1200)*expdecay(len(t(d)),0.05); save('hurt',x,-3)
# kill confirm: two rising bright notes
a=tone(880,0.12)*expdecay(len(t(0.12)),0.05); b=(tone(1320,0.2)+0.4*tone(2640,0.2))*expdecay(len(t(0.2)),0.08); save('kill_confirm',np.concatenate([a,b]),-4)
# death: power-down sweep + noise burst
d=0.6; x=lp(sweep(700,70,d,'saw'),3000)*expdecay(len(t(d)),0.25)+0.5*lp(noise(d),2500)*expdecay(len(t(d)),0.08); save('death',x,-3)
# spawn: soft rising shimmer, slow attack
d=0.7; tt=t(d); x=sum(sweep(f,f*2.2,d) for f in (300,450,600))*(1+0.4*np.sin(2*np.pi*14*tt))
e=np.minimum(1, tt/0.25)*np.exp(-np.maximum(0,tt-0.35)/0.18); save('spawn',x*e,-6)
# charge gained: small chime
d=0.3; x=(tone(1568,d)+0.6*tone(2349,d)+0.25*tone(3136,d))*expdecay(len(t(d)),0.09); save('charge_gained',x,-6)
# teleport denied: two low "uh-uh" buzzes
def buzz(f,sec): return lp(tone(f,sec)+0.5*np.sign(tone(f,sec)),1500)*env(len(t(sec)),0.005,0.03)
x=np.concatenate([buzz(220,0.1), np.zeros(int(SR*0.04)), buzz(165,0.13)]); save('teleport_denied',x,-5)
# countdown beep
d=0.12; x=tone(1000,d)*env(len(t(d)),0.004,0.03); save('countdown_beep',x,-6)
# match start: warm rising arpeggio (C5 E5 G5 C6), soft sine notes, gentle attack, nothing harsh
def soft_note(f, sec, tau):
    tt=t(sec); x=np.sin(2*np.pi*f*tt)+0.15*np.sin(2*np.pi*2*f*tt)
    return x*np.minimum(1, tt/0.015)*np.exp(-tt/tau)
parts=[]
for i,f in enumerate((523.25,659.25,783.99,1046.5)):
    sec=0.14 if i<3 else 0.6
    parts.append(soft_note(f,sec,0.12 if i<3 else 0.25))
save('match_start',lp(np.concatenate(parts),4000),-9)
# match end: soft falling chime (G5 E5 C5), bell-like decay
parts=[soft_note(f,0.22 if f!=523.25 else 0.8,0.15 if f!=523.25 else 0.35) for f in (783.99,659.25,523.25)]
save('match_end',lp(np.concatenate(parts),3500),-9)
print('done')
