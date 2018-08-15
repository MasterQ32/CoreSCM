
import corescm.test;
import corescm.generic;
import mq32.ics;

schematic minimal
{
    device lpc : lpc18xx { package(lbga-256); }
    
    device bat : Battery { package(generic-2); }
    
    lpc.SSP0_MISO -- bat.+;
    lpc.T0_CAP0   -- bat.-;
}