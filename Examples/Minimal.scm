
import corescm.test;

schematic minimal
{
    device A : SingleThing
    {
        package(generic-2);
    }
    device B : MultiFun
    {
        package(generic-2);
    }
    
    A.X[0,1] -- B.X[0,1];
}