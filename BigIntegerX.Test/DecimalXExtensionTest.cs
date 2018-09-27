using BigNumberX;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigNumberX.Test
{
    [TestFixture]
    public class DecimalXExtensionTest
    {
        [TestCase]
        public void SqrtTest()
        {
            DecimalX dec1;
            string s;

            dec1 = DecimalX.Create(16);
            s = dec1.Sqrt(1).ToString();
            Assert.IsTrue(s == "4.0");

            dec1 = DecimalX.Create("0.0");
            s = dec1.Sqrt(2).ToString();
            Assert.IsTrue(s == "0.00");

            dec1 = DecimalX.Create("2.0");
            s = dec1.Sqrt(200).ToString();
            Assert.IsTrue
              (s ==  "1.41421356237309504880168872420969807856967187537694807317667973799073247846210703885038753432764157273501384623091229702492483605585073721264412149709993583141322266592750559275579995050115278206057147");

            dec1 = DecimalX.Create("25.0");
            s = dec1.Sqrt(4).ToString();
            Assert.IsTrue(s == "5.0000");

            dec1 = DecimalX.Create("1.000");
            s = dec1.Sqrt(1).ToString();
            Assert.IsTrue(s == "1.0");

            dec1 = DecimalX.Create(6);
            s = dec1.Sqrt(4).ToString();
            Assert.IsTrue(s == "2.4494");

            dec1 = DecimalX.Create("0.5");
            s = dec1.Sqrt(6).ToString();
            Assert.IsTrue(s == "0.707106");

            dec1 = DecimalX.Create("5113.51315");
            s = dec1.Sqrt(4).ToString();
            Assert.IsTrue(s == "71.5088");

            dec1 = DecimalX.Create("15112345");
            s = dec1.Sqrt(6).ToString();
            Assert.IsTrue(s == "3887.459967");

            dec1 = DecimalX.Create("783648276815623658365871365876257862874628734627835648726");
            s = dec1.Sqrt(58).ToString();
            Assert.IsTrue(s == "27993718524262253829858552106.4622387227347572406137833208384678543897305217402364794553");
        }

        [TestCase]
        public void IntRootTest()
        {
            var dec1 = DecimalX.Create("4.2345");
            var s = dec1.IntRoot(2, 30).ToString();
            Assert.IsTrue(s == "2.0577900767571020629770974914148");
        }

        [TestCase]
        public void ExpTest()
        {
            DecimalX dec1;
            string s;
            dec1 = DecimalX.Create("1");
            s = dec1.Exp(46).ToString();
            Assert.IsTrue(s == "2.7182818284590452353602874713526624977572470937");
            dec1 = DecimalX.Create("-0.5");
            s = dec1.Exp(32).ToString();
            Assert.IsTrue(s == "0.60653065971263342360379953499118");
        }

        [TestCase]
        public void LnTest()
        {
            var dec1 = DecimalX.Create("2.65");
            var s = dec1.Ln(32).ToString();
            Assert.IsTrue(s == "0.97455963999813084070924556288652");
        }
    }
}
