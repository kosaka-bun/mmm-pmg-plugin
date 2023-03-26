using NUnit.Framework;
using PianoPlayingMotionGenerator.Util.FingeringCalculator;

namespace DllTest.AllTest {

[TestFixture]
public class FingeringCalculatorAlgorithmTest {

    [Test]
    public void highToLowTest() {
        calcLowerByHigher(59, 3, 58);
    }

    void calcLowerByHigher(int high, int highFinger, int low) {
        var h = new SingleNote(0, 0, high);
        h.finger = highFinger;
        var l = new SingleNote(0, 0, low);
        fc.calcLowerByHigher(h, l);
    }
    
    private FingeringCalculator fc = new FingeringCalculator("", null);
}

}