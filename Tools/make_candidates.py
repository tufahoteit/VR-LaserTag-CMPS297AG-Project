import numpy as np, sys, os
from scipy.io import wavfile
from scipy.signal import butter, lfilter
SR=44100; OUT=sys.argv[1]; rng=np.random.default_rng(11)
def t(s): return np.arange(int(SR*s))/SR
def dec(n,tau): return np.exp(-np.arange(n)/SR/tau)
def tone(f,s): return np.sin(2*np.pi*f*t(s))
def sweep(f0,f1,s,shape='sine'):
    f=np.geomspace(f0,f1,len(t(s))); ph=2*np.pi*np.cumsum(f)/SR
    return np.sin(ph) if shape=='sine' else np.sign(np.sin(ph))
def bp(x,lo,hi): b,a=butter(2,[lo/(SR/2),hi/(SR/2)],'band'); return lfilter(b,a,x)
def lp(x,fc): b,a=butter(2,fc/(SR/2)); return lfilter(b,a,x)
def noise(s): return rng.standard_normal(len(t(s)))
def fade(x,a=0.002,r=0.01):
    a=int(SR*a); r=int(SR*r); x=x.copy()
    if a: x[:a]*=np.linspace(0,1,a)
    if r: x[-r:]*=np.linspace(1,0,r)
    return x
def save(name,x,db):
    x=fade(x/(np.abs(x).max()+1e-9)*10**(db/20))
    wavfile.write(os.path.join(OUT,name+'.wav'),SR,(x*32767).astype(np.int16))
def cat(*xs): return np.concatenate(xs)
def gap(s): return np.zeros(int(SR*s))

# Hit marker (you hit someone, body)
save('hit_body_A_tick', bp(noise(0.03),3000,6000)*dec(len(t(0.03)),0.006)+0.4*tone(4000,0.03)*dec(len(t(0.03)),0.008), -8)   # CoD-style dry "tk"
save('hit_body_B_tink', (tone(3000,0.07)+0.4*tone(6100,0.07))*dec(len(t(0.07)),0.018), -9)                                     # small metallic tink
save('hit_body_C_pop',  sweep(1800,900,0.05)*dec(len(t(0.05)),0.015), -8)                                                     # soft pop
# Hit marker (headshot): same family, clearly bigger
hb=bp(noise(0.03),3000,6000)*dec(len(t(0.03)),0.006)
save('hit_head_A_tick_ting', cat(hb, gap(0.015), (tone(2600,0.18)+0.5*tone(5200,0.18))*dec(len(t(0.18)),0.06)), -7)            # tick + ringing ting
save('hit_head_B_double_tink', cat((tone(3000,0.06)+0.4*tone(6100,0.06))*dec(len(t(0.06)),0.015), gap(0.03), (tone(3600,0.12)+0.4*tone(7200,0.12))*dec(len(t(0.12)),0.04)), -8)
save('hit_head_C_glass_ping', (tone(2093,0.3)+0.6*tone(4186,0.3)+0.3*tone(6279,0.3))*dec(len(t(0.3)),0.09), -9)                # glassy ping (C7)
# Hurt (you got hit)
v=t(0.22); buzz=np.sign(np.sin(2*np.pi*140*v))*(0.6+0.4*np.sign(np.sin(2*np.pi*30*v)))
save('hurt_A_vest_buzz', lp(buzz,1800)*dec(len(v),0.09), -8)                                                                  # laser tag vest buzz
save('hurt_B_thump', sweep(180,55,0.2)*dec(len(t(0.2)),0.07)+0.25*lp(noise(0.2),800)*dec(len(t(0.2)),0.03), -6)               # dull body thump
save('hurt_C_zap', lp(sweep(900,200,0.15,'square'),3000)*dec(len(t(0.15)),0.05), -10)                                          # short downward zap
# Death (tagged out), no explosion
save('death_A_power_down', sweep(900,120,0.6)*np.exp(-np.maximum(0,t(0.6)-0.35)/0.1), -7)                                      # smooth power-down whine
def beep(f,s): return (tone(f,s)+0.2*tone(2*f,s))*np.minimum(1,t(s)/0.005)*np.exp(-t(s)/(s*0.7))
save('death_B_three_beeps', cat(beep(880,0.12),gap(0.05),beep(660,0.12),gap(0.05),beep(440,0.35)), -8)                       # classic laser tag "tagged out" beeps
save('death_C_fizzle', sweep(1200,200,0.45)*np.exp(-t(0.45)/0.2)+0.3*bp(noise(0.45),2000,6000)*np.exp(-t(0.45)/0.12), -8)      # electric fizzle out
print('done')

# Headshot options in the pop family (the body hit is pop C)
pop=sweep(1800,900,0.05)*dec(len(t(0.05)),0.015)
save('hit_head_D_pop_ting', cat(pop, gap(0.01), (tone(2637,0.2)+0.4*tone(5274,0.2))*dec(len(t(0.2)),0.06)), -7)      # pop + ringing ting
save('hit_head_E_double_pop', cat(pop, gap(0.035), sweep(2600,1300,0.06)*dec(len(t(0.06)),0.018)), -7)            # pop then a higher pop
save('hit_head_F_big_pop', sweep(2400,700,0.09)*dec(len(t(0.09)),0.03)+0.3*tone(3520,0.09)*dec(len(t(0.09)),0.04), -6)  # one bigger, brighter pop
print('heads done')
