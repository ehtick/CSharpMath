using System.Linq;
using Xunit;
using AngouriMath;

namespace CSharpMath {
  using Atom;
  public partial class EvluationTests {
    MathList ParseLaTeX(string latex) =>
      LaTeXParser.MathListFromLaTeX(latex).Match(list => list, e => throw new Xunit.Sdk.XunitException(e));
    Evaluation.MathItem ParseMath(string latex) =>
      Evaluation.Evaluate(ParseLaTeX(latex)).Match(entity => entity, e => throw new Xunit.Sdk.XunitException(e));
    void Test(string input, string converted, string? result) {
      var math = ParseMath(input);
      Assert.NotNull(math);
      Assert.Equal(converted, LaTeXParser.MathListToLaTeX(Evaluation.Parse(math)).ToString());
      // Ensure that the converted entity is valid by simplifying it
      if (result != null)
        Assert.Equal(result,
          LaTeXParser.MathListToLaTeX(Evaluation.Parse(Assert.IsType<Evaluation.MathItem.Entity>(math).Content.Simplify())).ToString());
      else Assert.IsNotType<Evaluation.MathItem.Entity>(result);
    }
    [Theory]
    [InlineData("1", "1")]
    [InlineData("1234", "1234")]
    [InlineData("0123456789", "123456789")]
    [InlineData("1234.", "1234")]
    [InlineData(".5678", "0.5678")]
    [InlineData(".9876543210", "0.987654321")]
    [InlineData("1234.5678", "1234.5678")]
    public void Numbers(string number, string output) =>
      Test(number, output, output);
    [Theory]
    [InlineData("a", "a", "a")]
    [InlineData("ab", @"a\times b", @"a\times b")]
    [InlineData("abc", @"a\times b\times c", @"a\times b\times c")]
    [InlineData("3a", @"3\times a", @"3\times a")]
    [InlineData("3ab", @"3\times a\times b", @"3\times a\times b")]
    [InlineData("3a3", @"3\times a\times 3", @"9\times a")]
    [InlineData("3aa", @"3\times a\times a", @"3\times a^2")]
    [InlineData(@"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
      @"a\times b\times c\times d\times e\times f\times g\times h\times i\times j\times k\times l\times m\times " +
      @"n\times o\times p\times q\times r\times s\times t\times u\times v\times w\times x\times y\times z\times " +
      @"A\times B\times C\times D\times E\times F\times G\times H\times I\times J\times K\times L\times M\times " +
      @"N\times O\times P\times Q\times R\times S\times T\times U\times V\times W\times X\times Y\times Z",
      // i is considered as a number instead of a variable like other alphabets, so it is sorted to the front
      @"i\times a\times A\times b\times B\times c\times C\times d\times D\times e\times E\times f\times F\times " +
      @"g\times G\times h\times H\times I\times j\times J\times k\times K\times l\times L\times m\times M\times " +
      @"n\times N\times o\times O\times p\times P\times q\times Q\times r\times R\times s\times S\times t\times " +
      @"T\times u\times U\times v\times V\times w\times W\times x\times X\times y\times Y\times z\times Z")]
    [InlineData(@"\alpha\beta\gamma\delta\epsilon\varepsilon\zeta\eta\theta\iota\kappa\varkappa" +
      @"\lambda\mu\nu\xi\omicron\pi\varpi\rho\varrho\sigma\varsigma\tau\upsilon\phi\varphi\chi" +
      @"\psi\omega\Gamma\Delta\Theta\Lambda\Xi\Pi\Sigma\Upsilon\Phi\Psi\Omega",
      @"\alpha \times \beta \times \gamma \times \delta \times \epsilon \times \varepsilon \times \zeta " +
      @"\times \eta \times \theta \times \iota \times \kappa \times \varkappa \times \lambda \times \mu " +
      @"\times \nu \times \xi \times \omicron \times \pi \times \varpi \times \rho \times \varrho " +
      @"\times \sigma \times \varsigma \times \tau \times \upsilon \times \phi \times \varphi \times \chi " +
      @"\times \psi \times \omega \times \Gamma \times \Delta \times \Theta \times \Lambda \times \Xi " +
      @"\times \Pi \times \Sigma \times \Upsilon \times \Phi \times \Psi \times \Omega ",
      @"\alpha \times \beta \times \chi \times \delta \times \Delta \times \epsilon \times \eta " +
      @"\times \gamma \times \Gamma \times \iota \times \kappa \times \lambda \times \Lambda \times \mu " +
      @"\times \nu \times \omega \times \Omega \times \omicron \times \phi \times \Phi \times \pi " +
      @"\times \Pi \times \psi \times \Psi \times \rho \times \sigma \times \Sigma \times \tau " +
      @"\times \theta \times \Theta \times \upsilon \times \Upsilon \times \varepsilon \times \varkappa " +
      @"\times \varphi \times \varpi \times \varrho \times \varsigma \times \xi \times \Xi \times \zeta ")]
    [InlineData(@"a_2", @"a_2", @"a_2")]
    [InlineData(@"a_2+a_2", @"a_2+a_2", @"2\times a_2")]
    [InlineData(@"a_{23}", @"a_{23}", @"a_{23}")]
    [InlineData(@"\pi_a", @"\pi _a", @"\pi _a")]
    public void Variables(string input, string converted, string result) => Test(input, converted, result);
    [Theory]
    [InlineData("a + b", @"a+b", "a+b")]
    [InlineData("a - b", @"a-b", "a-b")]
    [InlineData("a * b", @"a\times b", @"a\times b")]
    [InlineData(@"a\times b", @"a\times b", @"a\times b")]
    [InlineData(@"a\cdot b", @"a\times b", @"a\times b")]
    [InlineData(@"a / b", @"\frac{a}{b}", @"\frac{a}{b}")]
    [InlineData(@"a\div b", @"\frac{a}{b}", @"\frac{a}{b}")]
    [InlineData(@"\frac ab", @"\frac{a}{b}", @"\frac{a}{b}")]
    [InlineData("a + b + c", @"a+b+c", "a+b+c")]
    [InlineData("a + b - c", @"a+b-c", "a+b-c")]
    [InlineData("a + b * c", @"a+b\times c", @"a+b\times c")]
    [InlineData("a + b / c", @"a+\frac{b}{c}", @"a+\frac{b}{c}")]
    [InlineData("a - b + c", @"a-b+c", "a+c-b")]
    [InlineData("a - b - c", @"a-b-c", @"a-\left( b+c\right) ")]
    [InlineData("a - b * c", @"a-b\times c", @"a-b\times c")]
    [InlineData("a - b / c", @"a-\frac{b}{c}", @"a-\frac{b}{c}")]
    [InlineData("a * b + c", @"a\times b+c", @"a\times b+c")]
    [InlineData("a * b - c", @"a\times b-c", @"a\times b-c")]
    [InlineData("a * b * c", @"a\times b\times c", @"a\times b\times c")]
    [InlineData("a * b / c", @"\frac{a\times b}{c}", @"\frac{a\times b}{c}")]
    [InlineData("a / b + c", @"\frac{a}{b}+c", @"\frac{a}{b}+c")]
    [InlineData("a / b - c", @"\frac{a}{b}-c", @"\frac{a}{b}-c")]
    [InlineData("a / b * c", @"\frac{a}{b}\times c", @"\frac{a\times c}{b}")]
    [InlineData("a / b / c", @"\frac{\frac{a}{b}}{c}", @"\frac{\frac{a}{b}}{c}")]
    [InlineData(@"2+\frac ab", @"2+\frac{a}{b}", @"2+\frac{a}{b}")]
    [InlineData(@"\frac ab+2", @"\frac{a}{b}+2", @"2+\frac{a}{b}")]
    [InlineData(@"2-\frac ab", @"2-\frac{a}{b}", @"2-\frac{a}{b}")]
    [InlineData(@"\frac ab-2", @"\frac{a}{b}-2", @"\frac{a}{b}-2")]
    [InlineData(@"2*\frac ab", @"2\times \frac{a}{b}", @"\frac{2\times a}{b}")]
    [InlineData(@"\frac ab*2", @"\frac{a}{b}\times 2", @"\frac{2\times a}{b}")]
    [InlineData(@"2/\frac ab", @"\frac{2}{\frac{a}{b}}", @"\frac{2\times b}{a}")]
    [InlineData(@"\frac ab/2", @"\frac{\frac{a}{b}}{2}", @"\frac{\frac{a}{2}}{b}")]
    public void BinaryOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData("+a", "a", "a")]
    [InlineData("-a", "-a", "-a")]
    [InlineData("++a", "a", "a")]
    [InlineData("+-a", "-a", "-a")]
    [InlineData("-+a", "-a", "-a")]
    [InlineData("--a", "--a", "a")]
    [InlineData("+++a", "a", "a")]
    [InlineData("---a", "---a", "-a")]
    [InlineData("a++a", "a+a", @"2\times a")]
    [InlineData("a+-a", "a-a", "0")]
    [InlineData("a-+a", "a-a", "0")]
    [InlineData("a--a", "a--a", @"2\times a")]
    [InlineData("a+++a", "a+a", @"2\times a")]
    [InlineData("a---a", "a---a", "0")]
    [InlineData("a*+a", @"a\times a", "a^2")]
    [InlineData("a*-a", @"a\times -a", "-a^2")]
    [InlineData("+a*+a", @"a\times a", "a^2")]
    [InlineData("-a*-a", @"-a\times -a", "a^2")]
    [InlineData("a/+a", @"\frac{a}{a}", "1")]
    [InlineData("a/-a", @"\frac{a}{-a}", "-1")]
    [InlineData("+a/+a", @"\frac{a}{a}", "1")]
    [InlineData("-a/-a", @"\frac{-a}{-a}", "1")]
    [InlineData("-2+-2+-2", @"-2-2-2", "-6")]
    [InlineData("-2--2--2", @"-2--2--2", "2")]
    [InlineData("-2*-2*-2", @"-2\times -2\times -2", "-8")]
    [InlineData("-2/-2/-2", @"\frac{\frac{-2}{-2}}{-2}", "-0.5")]
    public void UnaryOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"9\%", @"\frac{9}{100}", "0.09")]
    [InlineData(@"a\%", @"\frac{a}{100}", @"0.01\times a")]
    [InlineData(@"\pi\%", @"\frac{\pi }{100}", @"0.01\times \pi ")]
    [InlineData(@"a\%\%", @"\frac{\frac{a}{100}}{100}", @"0.0001\times a")]
    [InlineData(@"9\%+3", @"\frac{9}{100}+3", "3.09")]
    [InlineData(@"-9\%+3", @"-\frac{9}{100}+3", "2.91")]
    [InlineData(@"2^2\%", @"\frac{2^2}{100}", "0.04")]
    [InlineData(@"2\%^2", @"\left( \frac{2}{100}\right) ^2", "0.0004")]
    [InlineData(@"2\%2", @"\frac{2}{100}\times 2", "0.04")]
    [InlineData(@"1+2\%^2", @"1+\left( \frac{2}{100}\right) ^2", "1.0004")]
    [InlineData(@"9\degree", @"\frac{9\times \pi }{180}", @"0.05\times \pi ")]
    [InlineData(@"a\degree", @"\frac{a\times \pi }{180}", @"0.005555555555555556\times a\times \pi ")]
    [InlineData(@"\pi\degree", @"\frac{\pi \times \pi }{180}", @"0.005555555555555556\times \pi ^2")]
    [InlineData(@"a\%\degree", @"\frac{\frac{a}{100}\times \pi }{180}", @"5.555555555555556E-05\times a\times \pi ")]
    [InlineData(@"a\degree\degree", @"\frac{\frac{a\times \pi }{180}\times \pi }{180}", @"3.08641975308642E-05\times a\times \pi ^2")]
    [InlineData(@"9\degree+3", @"\frac{9\times \pi }{180}+3", @"3+0.05\times \pi ")]
    [InlineData(@"-9\degree+3", @"-\frac{9\times \pi }{180}+3", @"3-0.05\times \pi ")]
    [InlineData(@"2^2\degree", @"\frac{2^2\times \pi }{180}", @"0.022222222222222223\times \pi ")]
    [InlineData(@"2\degree^2", @"\left( \frac{2\times \pi }{180}\right) ^2", @"0.0001234567901234568\times \pi ^2")]
    [InlineData(@"2\degree2", @"\frac{2\times \pi }{180}\times 2", @"0.022222222222222223\times \pi ")]
    [InlineData(@"1+2\degree^2", @"1+\left( \frac{2\times \pi }{180}\right) ^2", @"1+0.0001234567901234568\times \pi ^2")]
    public void PostfixOperators(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData("2^2", "2^2", "4")]
    [InlineData(".2^2", "0.2^2", "0.04000000000000001")]
    [InlineData("2.^2", "2^2", "4")]
    [InlineData("2.1^2", "2.1^2", "4.41")]
    [InlineData("a^a", "a^a", "a^a")]
    [InlineData("a^{a+b}", "a^{a+b}", "a^{a+b}")]
    [InlineData("a^{-2}", "a^{-2}", "a^{-2}")]
    [InlineData("2^{3^4}", "2^{3^4}", "2.4178516392292583E+24")]
    [InlineData("4^{3^2}", "4^{3^2}", "262144")]
    [InlineData("4^3+2", "4^3+2", "66")]
    [InlineData("2+3^4", "2+3^4", "83")]
    [InlineData("4^3*2", @"4^3\times 2", "128")]
    [InlineData("2*3^4", @"2\times 3^4", "162")]
    [InlineData("1/x", @"\frac{1}{x}", @"x^{-1}")]
    [InlineData("2/x", @"\frac{2}{x}", @"\frac{2}{x}")]
    [InlineData("0^x", @"0^x", @"0")]
    [InlineData("1^x", @"1^x", @"1")]
    [InlineData("x^0", @"x^0", @"1")]
    [InlineData(@"{\frac 12}^4", @"\left( \frac{1}{2}\right) ^4", "0.0625")]
    [InlineData(@"\sqrt2", @"\sqrt{2}", "1.4142135623730951")]
    [InlineData(@"\sqrt2^2", @"\left( \sqrt{2}\right) ^2", "2.0000000000000004")]
    [InlineData(@"\sqrt[3]2", @"2^{\frac{1}{3}}", "1.2599210498948732")]
    [InlineData(@"\sqrt[3]2^3", @"\left( 2^{\frac{1}{3}}\right) ^3", "2")]
    [InlineData(@"\sqrt[3]2^{1+1+1}", @"\left( 2^{\frac{1}{3}}\right) ^{1+1+1}", "2")]
    [InlineData(@"\sqrt[1+1+1]2^{1+1+1}", @"\left( 2^{\frac{1}{1+1+1}}\right) ^{1+1+1}", "2")]
    public void Exponents(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"\sin x", @"\sin \left( x\right) ", @"\sin \left( x\right) ")]
    [InlineData(@"\cos x", @"\cos \left( x\right) ", @"\cos \left( x\right) ")]
    [InlineData(@"\tan x", @"\tan \left( x\right) ", @"\tan \left( x\right) ")]
    [InlineData(@"\cot x", @"\cot \left( x\right) ", @"\cot \left( x\right) ")]
    [InlineData(@"\sec x", @"\frac{1}{\cos \left( x\right) }", @"\cos \left( x\right) ^{-1}")]
    [InlineData(@"\csc x", @"\frac{1}{\sin \left( x\right) }", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\arcsin x", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\arccos x", @"\arccos \left( x\right) ", @"\arccos \left( x\right) ")]
    [InlineData(@"\arctan x", @"\arctan \left( x\right) ", @"\arctan \left( x\right) ")]
    [InlineData(@"\arccot x", @"\arccot \left( x\right) ", @"\arccot \left( x\right) ")]
    [InlineData(@"\arcsec x", @"\arccos \left( \frac{1}{x}\right) ", @"\arccos \left( x^{-1}\right) ")]
    [InlineData(@"\arccsc x", @"\arcsin \left( \frac{1}{x}\right) ", @"\arcsin \left( x^{-1}\right) ")]
    [InlineData(@"\ln x", @"\ln \left( x\right) ", @"\ln \left( x\right) ")]
    [InlineData(@"\log x", @"\log \left( x\right) ", @"\log \left( x\right) ")]
    [InlineData(@"\log_3 x", @"\log _3\left( x\right) ", @"\log _3\left( x\right) ")]
    [InlineData(@"\log_{10} x", @"\log \left( x\right) ", @"\log \left( x\right) ")]
    [InlineData(@"\log_e x", @"\ln \left( x\right) ", @"\ln \left( x\right) ")]
    [InlineData(@"\ln x^2", @"\ln \left( x^2\right) ", @"\ln \left( x^2\right) ")]
    [InlineData(@"\log x^2", @"\log \left( x^2\right) ", @"\log \left( x^2\right) ")]
    [InlineData(@"\log_{10} x^2", @"\log \left( x^2\right) ", @"\log \left( x^2\right) ")]
    [InlineData(@"\log_3 x^2", @"\log _3\left( x^2\right) ", @"\log _3\left( x^2\right) ")]
    [InlineData(@"\log_e x^2", @"\ln \left( x^2\right) ", @"\ln \left( x^2\right) ")]
    [InlineData(@"\ln x^{-1}", @"\ln \left( x^{-1}\right) ", @"\ln \left( x^{-1}\right) ")]
    [InlineData(@"\log x^{-1}", @"\log \left( x^{-1}\right) ", @"\log \left( x^{-1}\right) ")]
    [InlineData(@"\log_{10} x^{-1}", @"\log \left( x^{-1}\right) ", @"\log \left( x^{-1}\right) ")]
    [InlineData(@"\log_3 x^{-1}", @"\log _3\left( x^{-1}\right) ", @"\log _3\left( x^{-1}\right) ")]
    [InlineData(@"\log_e x^{-1}", @"\ln \left( x^{-1}\right) ", @"\ln \left( x^{-1}\right) ")]
    [InlineData(@"2\sin x", @"2\times \sin \left( x\right) ", @"2\times \sin \left( x\right) ")]
    [InlineData(@"\sin 2x", @"\sin \left( 2\times x\right) ", @"\sin \left( 2\times x\right) ")]
    [InlineData(@"\sin xy", @"\sin \left( x\times y\right) ", @"\sin \left( x\times y\right) ")]
    [InlineData(@"\cos +x", @"\cos \left( x\right) ", @"\cos \left( x\right) ")]
    [InlineData(@"\cos -x", @"\cos \left( -x\right) ", @"\cos \left( -x\right) ")]
    [InlineData(@"\tan x\%", @"\tan \left( \frac{x}{100}\right) ", @"\tan \left( 0.01\times x\right) ")]
    [InlineData(@"\tan x\%^2", @"\tan \left( \left( \frac{x}{100}\right) ^2\right) ", @"\tan \left( 0.0001\times x^2\right) ")]
    [InlineData(@"\cot x*y", @"\cot \left( x\right) \times y", @"\cot \left( x\right) \times y")]
    [InlineData(@"\cot x/y", @"\frac{\cot \left( x\right) }{y}", @"\frac{\cot \left( x\right) }{y}")]
    [InlineData(@"\cos \arccos x", @"\cos \left( \arccos \left( x\right) \right) ", @"x")]
    [InlineData(@"\sin^2 x", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin^2 xy+\cos^2 yx", @"\sin \left( x\times y\right) ^2+\cos \left( y\times x\right) ^2", @"1")]
    [InlineData(@"\log^2 x", @"\log \left( x\right) ^2", @"\log \left( x\right) ^2")]
    [InlineData(@"\ln^2 x", @"\ln \left( x\right) ^2", @"\ln \left( x\right) ^2")]
    [InlineData(@"\log_{10}^2 x", @"\log \left( x\right) ^2", @"\log \left( x\right) ^2")]
    [InlineData(@"\log_3^2 x", @"\log _3\left( x\right) ^2", @"\log _3\left( x\right) ^2")]
    public void Functions(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"\sin^{-1} x", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\cos^{-1} x", @"\arccos \left( x\right) ", @"\arccos \left( x\right) ")]
    [InlineData(@"\tan^{-1} x", @"\arctan \left( x\right) ", @"\arctan \left( x\right) ")]
    [InlineData(@"\cot^{-1} x", @"\arccot \left( x\right) ", @"\arccot \left( x\right) ")]
    [InlineData(@"\sec^{-1} x", @"\arccos \left( \frac{1}{x}\right) ", @"\arccos \left( x^{-1}\right) ")]
    [InlineData(@"\csc^{-1} x", @"\arcsin \left( \frac{1}{x}\right) ", @"\arcsin \left( x^{-1}\right) ")]
    [InlineData(@"\arcsin^{-1} x", @"\sin \left( x\right) ", @"\sin \left( x\right) ")]
    [InlineData(@"\arccos^{-1} x", @"\cos \left( x\right) ", @"\cos \left( x\right) ")]
    [InlineData(@"\arctan^{-1} x", @"\tan \left( x\right) ", @"\tan \left( x\right) ")]
    [InlineData(@"\arccot^{-1} x", @"\cot \left( x\right) ", @"\cot \left( x\right) ")]
    [InlineData(@"\arcsec^{-1} x", @"\frac{1}{\cos \left( x\right) }", @"\cos \left( x\right) ^{-1}")]
    [InlineData(@"\arccsc^{-1} x", @"\frac{1}{\sin \left( x\right) }", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\ln^{-1} x", @"e^x", @"e^x")]
    [InlineData(@"\log^{-1} x", @"10^x", @"10^x")]
    [InlineData(@"\log_3^{-1} x", @"3^x", @"3^x")]
    [InlineData(@"\log^{-1}_{10} x", @"10^x", @"10^x")]
    [InlineData(@"\log_e^{-1} x", @"e^x", @"e^x")]
    [InlineData(@"\ln^{-1} x^2", @"e^{x^2}", @"e^{x^2}")]
    [InlineData(@"\log^{-1} x^2", @"10^{x^2}", @"10^{x^2}")]
    [InlineData(@"\log_{10}^{-1} x^2", @"10^{x^2}", @"10^{x^2}")]
    [InlineData(@"\log_3^{-1} x^2", @"3^{x^2}", @"3^{x^2}")]
    [InlineData(@"\log_e^{-1} x^2", @"e^{x^2}", @"e^{x^2}")]
    [InlineData(@"\ln^{-1} x^{-1}", @"e^{x^{-1}}", @"e^{x^{-1}}")]
    [InlineData(@"\log^{-1} x^{-1}", @"10^{x^{-1}}", @"10^{x^{-1}}")]
    [InlineData(@"\log_{10}^{-1} x^{-1}", @"10^{x^{-1}}", @"10^{x^{-1}}")]
    [InlineData(@"\log_3^{-1} x^{-1}", @"3^{x^{-1}}", @"3^{x^{-1}}")]
    [InlineData(@"\log_e^{-1} x^{-1}", @"e^{x^{-1}}", @"e^{x^{-1}}")]
    [InlineData(@"2\sin^{-1} x", @"2\times \arcsin \left( x\right) ", @"2\times \arcsin \left( x\right) ")]
    [InlineData(@"\sin^{-1} 2x", @"\arcsin \left( 2\times x\right) ", @"\arcsin \left( 2\times x\right) ")]
    [InlineData(@"\sin^{-1} xy", @"\arcsin \left( x\times y\right) ", @"\arcsin \left( x\times y\right) ")]
    [InlineData(@"\cos^{-1} +x", @"\arccos \left( x\right) ", @"\arccos \left( x\right) ")]
    [InlineData(@"\cos^{-1} -x", @"\arccos \left( -x\right) ", @"\arccos \left( -x\right) ")]
    [InlineData(@"\tan^{-1} x\%", @"\arctan \left( \frac{x}{100}\right) ", @"\arctan \left( 0.01\times x\right) ")]
    [InlineData(@"\tan^{-1} x\%^2", @"\arctan \left( \left( \frac{x}{100}\right) ^2\right) ", @"\arctan \left( 0.0001\times x^2\right) ")]
    [InlineData(@"\cot^{-1} x*y", @"\arccot \left( x\right) \times y", @"\arccot \left( x\right) \times y")]
    [InlineData(@"\cot^{-1} x/y", @"\frac{\arccot \left( x\right) }{y}", @"\frac{\arccot \left( x\right) }{y}")]
    [InlineData(@"\cos^{-1} \arccos^{-1} x", @"\arccos \left( \cos \left( x\right) \right) ", @"x")]
    [InlineData(@"\sin^1 x", @"\sin \left( x\right) ^1", @"\sin \left( x\right) ")]
    [InlineData(@"\sin^{+1} x", @"\sin \left( x\right) ^1", @"\sin \left( x\right) ")]
    [InlineData(@"\sin^{+-1} x", @"\sin \left( x\right) ^{-1}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^{-+1} x", @"\sin \left( x\right) ^{-1}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^{--1} x", @"\sin \left( x\right) ^{--1}", @"\sin \left( x\right) ")]
    [InlineData(@"\sin^{-1^2} x", @"\sin \left( x\right) ^{-1^2}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^{-1+3} xy+\cos^{-1+3} yx", @"\sin \left( x\times y\right) ^{-1+3}+\cos \left( y\times x\right) ^{-1+3}", @"1")]
    [InlineData(@"\log^{-a_2} x", @"\log \left( x\right) ^{-a_2}", @"\log \left( x\right) ^{-a_2}")]
    [InlineData(@"\ln^{3-1} x", @"\ln \left( x\right) ^{3-1}", @"\ln \left( x\right) ^2")]
    public void FunctionInverses(string latex, string converted, string result) => Test(latex, converted, result);
    [Theory]
    [InlineData(@"1+(2+3)", @"1+2+3", @"6")]
    [InlineData(@"1+((2+3))", @"1+2+3", @"6")]
    [InlineData(@"2*(3+4)", @"2\times \left( 3+4\right) ", @"14")]
    [InlineData(@"(3+4)*2", @"\left( 3+4\right) \times 2", @"14")]
    [InlineData(@"(5+6)^2", @"\left( 5+6\right) ^2", @"121")]
    [InlineData(@"(5+6)", @"5+6", @"11")]
    [InlineData(@"((5+6))", @"5+6", @"11")]
    [InlineData(@"(5+6)2", @"\left( 5+6\right) \times 2", @"22")]
    [InlineData(@"2(5+6)", @"2\times \left( 5+6\right) ", @"22")]
    [InlineData(@"2(5+6)2", @"2\times \left( 5+6\right) \times 2", @"44")]
    [InlineData(@"(5+6)x", @"\left( 5+6\right) \times x", @"11\times x")]
    [InlineData(@"x(5+6)", @"x\times \left( 5+6\right) ", @"11\times x")]
    [InlineData(@"x(5+6)x", @"x\times \left( 5+6\right) \times x", @"11\times x^2")]
    [InlineData(@"(5+6).2", @"\left( 5+6\right) \times 0.2", @"2.2")]
    [InlineData(@".2(5+6)", @"0.2\times \left( 5+6\right) ", @"2.2")]
    [InlineData(@".2(5+6).2", @"0.2\times \left( 5+6\right) \times 0.2", @"0.44000000000000006")]
    [InlineData(@"(5+6)2.", @"\left( 5+6\right) \times 2", @"22")]
    [InlineData(@"2.(5+6)", @"2\times \left( 5+6\right) ", @"22")]
    [InlineData(@"2.(5+6)2.", @"2\times \left( 5+6\right) \times 2", @"44")]
    [InlineData(@"(5+6)(2)", @"\left( 5+6\right) \times 2", @"22")]
    [InlineData(@"(5+6)(1+1)", @"\left( 5+6\right) \times \left( 1+1\right) ", @"22")]
    [InlineData(@"(5+6)(-(-2))", @"\left( 5+6\right) \times --2", @"22")]
    [InlineData(@"(5+6)(--2)", @"\left( 5+6\right) \times --2", @"22")]
    [InlineData(@"+(1)", @"1", @"1")]
    [InlineData(@"+(1)\%", @"\frac{1}{100}", @"0.01")]
    [InlineData(@"+(-1)", @"-1", @"-1")]
    [InlineData(@"-(+1)", @"-1", @"-1")]
    [InlineData(@"-(-1)", @"--1", @"1")]
    [InlineData(@"--(--1)", @"----1", @"1")]
    [InlineData(@"(2+3)^{(4+5)}", @"\left( 2+3\right) ^{4+5}", @"1953125")]
    [InlineData(@"(2+3)^{((4)+5)}", @"\left( 2+3\right) ^{4+5}", @"1953125")]
    [InlineData(@"2\sin(x)", @"2\times \sin \left( x\right) ", @"2\times \sin \left( x\right) ")]
    [InlineData(@"(2)\sin(x)", @"2\times \sin \left( x\right) ", @"2\times \sin \left( x\right) ")]
    [InlineData(@"\sin(x+1)", @"\sin \left( x+1\right) ", @"\sin \left( 1+x\right) ")]
    [InlineData(@"\sin((x+1))", @"\sin \left( x+1\right) ", @"\sin \left( 1+x\right) ")]
    [InlineData(@"\sin(2(x+1))", @"\sin \left( 2\times \left( x+1\right) \right) ", @"\sin \left( 2\times \left( 1+x\right) \right) ")]
    [InlineData(@"\sin((x+1)+2)", @"\sin \left( x+1+2\right) ", @"\sin \left( 3+x\right) ")]
    [InlineData(@"\sin(x)2", @"\sin \left( x\right) \times 2", @"2\times \sin \left( x\right) ")]
    [InlineData(@"\sin(x)(x+1)", @"\sin \left( x\right) \times \left( x+1\right) ", @"\sin \left( x\right) \times \left( 1+x\right) ")]
    [InlineData(@"\sin(x)(x+1)(x)", @"\sin \left( x\right) \times \left( x+1\right) \times x", @"\sin \left( x\right) \times \left( 1+x\right) \times x")]
    [InlineData(@"\sin(x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin\ (x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin\; (x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin\ \; (x^2)", @"\sin \left( x^2\right) ", @"\sin \left( x^2\right) ")]
    [InlineData(@"\sin^3(x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^3\ (x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^3\; (x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^3\ \; (x)", @"\sin \left( x\right) ^3", @"\sin \left( x\right) ^3")]
    [InlineData(@"\sin^{-1}(x)", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\sin^{-1}\ (x)", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\sin^{-1}\; (x)", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\sin^{-1}\ \; (x)", @"\arcsin \left( x\right) ", @"\arcsin \left( x\right) ")]
    [InlineData(@"\sin(x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin\ (x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin\; (x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin\ \; (x)^2", @"\sin \left( x\right) ^2", @"\sin \left( x\right) ^2")]
    [InlineData(@"\sin(x)^{-1}", @"\sin \left( x\right) ^{-1}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin\ (x)^{-1}", @"\sin \left( x\right) ^{-1}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin\; (x)^{-1}", @"\sin \left( x\right) ^{-1}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin\ \; (x)^{-1}", @"\sin \left( x\right) ^{-1}", @"\sin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^3(x)^{-1}", @"\sin \left( x\right) ^{3\times -1}", @"\sin \left( x\right) ^{-3}")]
    [InlineData(@"\sin^3\ (x)^{-1}", @"\sin \left( x\right) ^{3\times -1}", @"\sin \left( x\right) ^{-3}")]
    [InlineData(@"\sin^3\; (x)^{-1}", @"\sin \left( x\right) ^{3\times -1}", @"\sin \left( x\right) ^{-3}")]
    [InlineData(@"\sin^3\ \; (x)^{-1}", @"\sin \left( x\right) ^{3\times -1}", @"\sin \left( x\right) ^{-3}")]
    [InlineData(@"\sin^{-1}(x)^{-1}", @"\arcsin \left( x\right) ^{-1}", @"\arcsin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^{-1}\ (x)^{-1}", @"\arcsin \left( x\right) ^{-1}", @"\arcsin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^{-1}\; (x)^{-1}", @"\arcsin \left( x\right) ^{-1}", @"\arcsin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^{-1}\ \; (x)^{-1}", @"\arcsin \left( x\right) ^{-1}", @"\arcsin \left( x\right) ^{-1}")]
    [InlineData(@"\sin^a(x)", @"\sin \left( x\right) ^a", @"\sin \left( x\right) ^a")]
    [InlineData(@"\sin^a(x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin^a\ (x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin^a\; (x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin^a\ \; (x)^2", @"\sin \left( x\right) ^{a\times 2}", @"\sin \left( x\right) ^{2\times a}")]
    [InlineData(@"\sin(x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin\ (x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin\; (x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin\ \; (x)^2(x)", @"\sin \left( x\right) ^2\times x", @"\sin \left( x\right) ^2\times x")]
    [InlineData(@"\sin^a(x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    [InlineData(@"\sin^a\ (x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    [InlineData(@"\sin^a\; (x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    [InlineData(@"\sin^a\ \; (x)^2(x)", @"\sin \left( x\right) ^{a\times 2}\times x", @"\sin \left( x\right) ^{2\times a}\times x")]
    public void Parentheses(string latex, string converted, string result) {
      Test(latex, converted, result);
      Test(latex.Replace("(", @"\left(").Replace(")", @"\right)"), converted, result);
    }
    [Theory]
    [InlineData(@"1,2", @"1,2")]
    [InlineData(@"1,2,3", @"1,2,3")]
    [InlineData(@"a,b,c,d", @"a,b,c,d")]
    [InlineData(@"\sqrt2,\sqrt[3]2,\frac34", @"\sqrt{2},2^{\frac{1}{3}},\frac{3}{4}")]
    [InlineData(@"\sin a,\cos b^2,\tan c_3,\cot de,\sec 12f,\csc g+h",
      @"\sin \left( a\right) ,\cos \left( b^2\right) ,\tan \left( c_3\right) ,\cot \left( d\times e\right) ,\frac{1}{\cos \left( 12\times f\right) },\frac{1}{\sin \left( g\right) }+h")]
    public void Comma(string latex, string converted) =>
      Test(latex, converted, null);
    [Theory(Skip = "https://github.com/asc-community/AngouriMath/pull/94")]
    [InlineData(@"\emptyset", @"\emptyset ")]
    [InlineData(@"\mathbb R", @"\emptyset ")] // wip
    [InlineData(@"\mathbb C", @"\emptyset ")] // wip
    [InlineData(@"\{\}", @"\emptyset ")]
    [InlineData(@"\{1\}", @"\left\{ 1\right\} ")]
    [InlineData(@"\{1,2\}", @"\left\{ 1,2\right\} ")]
    [InlineData(@"\{x,y\}", @"\left\{ x,y\right\} ")]
    [InlineData(@"\{\sqrt[3]2,\frac34,\sin^2x\}", @"\left\{ 2^{\frac{1}{3}},\frac{3}{4},\sin \left( x\right) ^2\right\} ")]
    public void Sets(string latex, string converted) {
      Test(latex, converted, null);
      Test(latex.Replace(@"\{", @"\left\{").Replace(@"\}", @"\right\}"), converted, null);
    }
    [Theory]
    [InlineData(@"\emptyset\cup\{2\}", @"\left\{ 2\right\} ")]
    [InlineData(@"\{3,4\}\cap\{4,5\}", @"\left\{ 4\right\} ")]
    [InlineData(@"\{2,3,4\}\setminus\{4\}", @"\left\{ 2,3\right\} ")]
    //[InlineData(@"\{3\}^\complement", @"\left\{ 3\right\} ^\complement")] // wip
    public void SetOperations(string latex, string converted) {
      Test(latex, converted, null);
      Test(latex.Replace(@"\{", @"\left\{").Replace(@"\}", @"\right\}"), converted, null);
    }
    [Theory(Skip = "https://github.com/asc-community/AngouriMath/pull/93")]
    [InlineData(@"(1,2)", @"\left\{ \left( 1,2\right) \right\} ")] // wip
    public void Intervals(string latex, string converted) {
      Test(latex, converted, null);
      Test(latex.Replace("(", @"\left(").Replace(")", @"\right)"), converted, null);
    }
    [Theory]
    [InlineData(@"", "There is nothing to evaluate")]
    [InlineData(@"\ ", "There is nothing to evaluate")]
    [InlineData(@"\;", "There is nothing to evaluate")]
    [InlineData(@"\quad", "There is nothing to evaluate")]
    [InlineData(@"+", "Missing right operand for +")]
    [InlineData(@"-", "Missing right operand for \u2212")]
    [InlineData(@"\times", "Unsupported Unary Operator ×")]
    [InlineData(@"\div", "Unsupported Unary Operator ÷")]
    [InlineData(@"\%", "Missing left operand for %")]
    [InlineData(@"\degree", "Missing left operand for °")]
    [InlineData(@"\dagger", "Unsupported Unary Operator †")]
    [InlineData(@"\times x", "Unsupported Unary Operator ×")]
    [InlineData(@"\div x", "Unsupported Unary Operator ÷")]
    [InlineData(@"\% x", "Missing left operand for %")]
    [InlineData(@"\degree x", "Missing left operand for °")]
    [InlineData(@"x+", "Missing right operand for +")]
    [InlineData(@"x-", "Missing right operand for \u2212")]
    [InlineData(@"x\times", "Missing right operand for ×")]
    [InlineData(@"x\div", "Missing right operand for ÷")]
    [InlineData(@"x\dagger", "Unsupported Binary Operator †")]
    [InlineData(@"1+_21", "Subscripts are unsupported for Binary Operator +")]
    [InlineData(@"-_31", "Subscripts are unsupported for Unary Operator −")]
    [InlineData(@"1\times_41", "Subscripts are unsupported for Binary Operator ×")]
    [InlineData(@"\div_51", "Unsupported Unary Operator ÷")]
    [InlineData(@"1\%_6", "Subscripts are unsupported for Ordinary %")]
    [InlineData(@"1\degree_7", "Subscripts are unsupported for Ordinary °")]
    [InlineData(@"\dagger_8", "Unsupported Unary Operator †")]
    [InlineData(@".", "Invalid number: .")]
    [InlineData(@"1._2", "Subscripts are unsupported for Number 1.")]
    [InlineData(@"..", "Invalid number: ..")]
    [InlineData(@"1..", "Invalid number: 1..")]
    [InlineData(@"..1", "Invalid number: ..1")]
    [InlineData(@"a_+", "Unsupported Unary Operator + in subscript")]
    [InlineData(@"a_|", "Unsupported Ordinary | in subscript")]
    [InlineData(@"a_{1+1}", "Unsupported Binary Operator + in subscript")]
    [InlineData(@"a_{2^3}", "Unsupported exponentiation in subscript")]
    [InlineData(@"a_{a2^3}", "Unsupported exponentiation in subscript")]
    [InlineData(@"a_{a^32}", "Unsupported exponentiation in subscript")]
    [InlineData(@"a_{2_3}", "Unsupported subscript in subscript")]
    [InlineData(@"a_{a2_3}", "Unsupported subscript in subscript")]
    [InlineData(@"a_{a_32}", "Unsupported subscript in subscript")]
    [InlineData(@"\square", "Placeholders should be filled")]
    [InlineData(@"x^\square", "Placeholders should be filled")]
    [InlineData(@"\square^x", "Placeholders should be filled")]
    [InlineData(@"a_\square", "Placeholders should be filled")]
    [InlineData(@"\square_a", "Placeholders should be filled")]
    [InlineData(@"(", "Missing )")]
    [InlineData(@"(_21)", "Subscripts are unsupported for Open (")]
    [InlineData(@"(x", "Missing )")]
    [InlineData(@"((x)", "Missing )")]
    [InlineData(@"(+", "Missing right operand for +")]
    [InlineData(@")", "Missing (")]
    [InlineData(@"(1)_2", "Subscripts are unsupported for Close )")]
    [InlineData(@"x)", "Missing (")]
    [InlineData(@"(x))", "Missing (")]
    [InlineData(@"+)", "Missing right operand for +")]
    [InlineData(@"\left(\right)", "Missing math inside ( )")]
    [InlineData(@"\left(1+\right)", "Missing right operand for +")]
    [InlineData(@"\left(2,3\right)^\square", "Placeholders should be filled")]
    [InlineData(@"(2,3)^\square", "Placeholders should be filled")]
    [InlineData(@"[", "Missing ]")]
    [InlineData(@"[_21)", "Unrecognized bracket pair [ )")]
    [InlineData(@"[x", "Missing ]")]
    [InlineData(@"[x)", "Unrecognized bracket pair [ )")]
    [InlineData(@"[[x)", "Unrecognized bracket pair [ )")]
    [InlineData(@"[+", "Missing right operand for +")]
    [InlineData(@"[x))", "Unrecognized bracket pair [ )")]
    [InlineData(@"\left[\right)", "Unrecognized bracket pair [ )")]
    [InlineData(@"\left[1+\right)", "Missing right operand for +")]
    [InlineData(@"\left[2,3\right)^\square", "Placeholders should be filled")]
    [InlineData(@"[2,3)^\square", "Placeholders should be filled")]
    [InlineData(@"((x]", "Unrecognized bracket pair ( ]")]
    [InlineData(@"(x]", "Unrecognized bracket pair ( ]")]
    [InlineData(@"]", "Missing [")]
    [InlineData(@"]_2", "Subscripts are unsupported for Close ]")]
    [InlineData(@"x]", "Missing [")]
    [InlineData(@"(x]]", "Unrecognized bracket pair ( ]")]
    [InlineData(@"+]", "Missing right operand for +")]
    [InlineData(@"\left(\right]", "Unrecognized bracket pair ( ]")]
    [InlineData(@"\left(1+\right]", "Missing right operand for +")]
    [InlineData(@"\left(2,3\right]^\square", "Placeholders should be filled")]
    [InlineData(@"(2,3]^\square", "Placeholders should be filled")]
    [InlineData(@"[]", "Unrecognized bracket pair [ ]")]
    [InlineData(@"[x]", "Unrecognized bracket pair [ ]")]
    [InlineData(@"[[x]", "Unrecognized bracket pair [ ]")]
    [InlineData(@"[x]]", "Unrecognized bracket pair [ ]")]
    [InlineData(@"\left[\right]", "Unrecognized bracket pair [ ]")]
    [InlineData(@"\left[1+\right]", "Missing right operand for +")]
    [InlineData(@"\left[2,3\right]^\square", "Placeholders should be filled")]
    [InlineData(@"[2,3]^\square", "Placeholders should be filled")]
    [InlineData(@"\{", "Missing }")]
    [InlineData(@"\{_2\}", "Subscripts are unsupported for Open {")]
    [InlineData(@"\{x", "Missing }")]
    [InlineData(@"\{\{x\}", "Missing }")]
    [InlineData(@"\{+", "Missing right operand for +")]
    [InlineData(@"\}", "Missing {")]
    [InlineData(@"\}_2", "Subscripts are unsupported for Close }")]
    [InlineData(@"x\}", "Missing {")]
    [InlineData(@"\{x\}\}", "Missing {")]
    [InlineData(@"+\}", "Missing right operand for +")]
    [InlineData(@"\left\{1+\right\}", "Missing right operand for +")]
    [InlineData(@"\{2,3\}^\square", "Placeholders should be filled")]
    [InlineData(@"\left\{2,3\right\}^\square", "Placeholders should be filled")]
    [InlineData(@"\frac{}{x}", "Missing numerator")]
    [InlineData(@"\frac{x}{}", "Missing denominator")]
    [InlineData(@"\sqrt{}", "Missing radicand")]
    [InlineData(@"\sin", "Missing argument for sin")]
    [InlineData(@"\cos-", "Missing right operand for \u2212")]
    [InlineData(@"\tan\times", "Unsupported Unary Operator ×")]
    [InlineData(@"\cot^(-1)", "Missing )")]
    [InlineData(@"\sec\csc", "Missing argument for csc")]
    [InlineData(@"\arcsin_2x", "Subscripts are unsupported for Large Operator arcsin")]
    [InlineData(@"\operatorname{dab}", "Unsupported Large Operator dab")]
    [InlineData(@",", "Missing left operand for comma")]
    [InlineData(@"1,_22", "Subscripts are unsupported for Punctuation ,")]
    [InlineData(@"1,", "Missing right operand for comma")]
    [InlineData(@",1", "Missing left operand for comma")]
    [InlineData(@",1,2", "Missing left operand for comma")]
    [InlineData(@"1,,2", "Missing left operand for comma")]
    [InlineData(@"1,2,", "Missing right operand for comma")]
    [InlineData(@",,1,2", "Missing left operand for comma")]
    [InlineData(@"1,,2,", "Missing left operand for comma")]
    [InlineData(@"1,2,,", "Missing left operand for comma")]
    [InlineData(@"\arcsin(1,2)", "Comma cannot be argument for arcsin")]
    [InlineData(@"+(3,4]", "Set cannot be right operand for +")]
    [InlineData(@"[5,6)\times", "Set cannot be left operand for ×")]
    [InlineData(@"\frac{[7,8]}{9}", "Set cannot be numerator")]
    [InlineData(@"\sqrt[{[]}]{}", "Unrecognized bracket pair [ ]")]
    [InlineData(@"\sqrt[{[a,b]}]{}", "Set cannot be degree")]
    [InlineData(@"\{\{\}\}", "Set cannot be a set element")]
    [InlineData(@"\cap", "Unsupported Unary Operator ∩")]
    [InlineData(@"\cap1", "Unsupported Unary Operator ∩")]
    [InlineData(@"1\cap", "Entity cannot be left operand for ∩")]
    [InlineData(@"\cup", "Unsupported Unary Operator ∪")]
    [InlineData(@"\cup1", "Unsupported Unary Operator ∪")]
    [InlineData(@"1\cup", "Entity cannot be left operand for ∪")]
    [InlineData(@"\setminus", "Unsupported Unary Operator ∖")]
    [InlineData(@"\setminus1", "Unsupported Unary Operator ∖")]
    [InlineData(@"1\setminus", "Entity cannot be left operand for ∖")]
    [InlineData(@"^\complement", "There is nothing to evaluate")]
    [InlineData(@"1^\complement", "Entity cannot be target of set inversion")]
    [InlineData(@"x^\complement", "Entity cannot be target of set inversion")]
    public void Error(string badLaTeX, string error) =>
      Evaluation.Evaluate(ParseLaTeX(badLaTeX))
      .Match(entity => throw new Xunit.Sdk.XunitException(entity.Latexise()), e => Assert.Equal(error, e));
  }
}
