/*
 * This file demonstrates a schematic for a basic astable multivibrator
 */
 
import corescm;
import corescm.builtin;
import corescm.generic;
import corescm.transistors;

import mq32.ics;

// freely designed after http://www.dieelektronikerseite.de/Pics/Lections/Astabiler%20Multivibrator%20-%20Eine%20unruhige%20Schaltung%20S01.GIF 
schematic multivibrator
{
    signal VCC, GND;
    
    device D1, D2 : LED;
    device T1, T2 : BC458;
    device BAT    : Battery;
    device C1, C2 : E-Cap { capacity(10µ); }
    
    BAT.Plus  -- VCC;
    BAT.Minus -- GND;

    D1.A, D2.A -- VCC;
    
    // D1.C -- [R:470] -- T1.C;
    // D2.C -- [R:470] -- T2.C;
    D1.C, D2.C -- [R:470] -- T1.C, T2.C;
    
    T1.B -- C2.Minus;
    T2.B -- C1.Minus;
    
    T1.C -- C1.Plus;
    T2.C -- C2.Plus;
    
    C1.Plus -- [R:47k] -- VCC;
    C2.Plus -- [R:47k] -- VCC;
    
    C1.Plus, C2.Plus -- [R:47k] -- VCC;
    
    T1.E, T2.E -- GND;
}