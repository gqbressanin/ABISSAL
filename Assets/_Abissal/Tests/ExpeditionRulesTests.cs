using Abissal.Expedition;
using NUnit.Framework;

namespace Abissal.Tests
{
    /// <summary>Confere as regras de conclusão definidas para a expedição.</summary>
    public class ExpeditionRulesTests
    {
        private const int SecondaryTotal = 6;

        [Test]
        public void SemAmostraPrincipal_NaoPodeVencer()
        {
            Assert.IsFalse(ExpeditionRules.CanWinExpedition(hasMainSample: false));
        }

        [Test]
        public void ComAmostraPrincipal_PodeVencer()
        {
            Assert.IsTrue(ExpeditionRules.CanWinExpedition(hasMainSample: true));
        }

        [Test]
        public void TodasAsSecundariasSemPrincipal_NaoChegamA100()
        {
            int percent = ExpeditionRules.CompletionPercent(false, SecondaryTotal, SecondaryTotal);

            Assert.Less(percent, 100);
        }

        [Test]
        public void PrincipalSemSecundarias_VenceComMenosDe100()
        {
            int percent = ExpeditionRules.CompletionPercent(true, 0, SecondaryTotal);

            Assert.IsTrue(ExpeditionRules.CanWinExpedition(true));
            Assert.AreEqual(14, percent);
        }

        [Test]
        public void TodasAsAmostras_Resultam100()
        {
            Assert.AreEqual(100, ExpeditionRules.CompletionPercent(true, SecondaryTotal, SecondaryTotal));
        }

        [Test]
        public void ExemploDoProjeto_Resulta83()
        {
            Assert.AreEqual(83, ExpeditionRules.CompletionPercent(true, 4, 5));
        }

        [Test]
        public void NenhumaAmostra_Resulta0()
        {
            Assert.AreEqual(0, ExpeditionRules.CompletionPercent(false, 0, SecondaryTotal));
        }

        [Test]
        public void FaltandoUmaAmostra_NuncaArredondaPara100()
        {
            for (int total = 1; total <= 200; total++)
            {
                Assert.Less(ExpeditionRules.CompletionPercent(true, total - 1, total), 100);
            }
        }
    }
}
