namespace Abissal.Expedition
{
    /// <summary>
    /// Regras de conclusão da expedição. É uma classe pura, sem depender da Unity,
    /// para que os testes automáticos possam verificá-la isoladamente.
    /// </summary>
    public static class ExpeditionRules
    {
        /// <summary>A vitória só depende da amostra principal, nunca das secundárias.</summary>
        public static bool CanWinExpedition(bool hasMainSample)
        {
            return hasMainSample;
        }

        /// <summary>
        /// Cada amostra vale o mesmo, e a principal conta como uma delas.
        /// Arredonda para baixo, então 100% exige todas as amostras.
        /// </summary>
        public static int CompletionPercent(bool hasMainSample, int secondaryCollected, int secondaryTotal)
        {
            int collected = secondaryCollected + (hasMainSample ? 1 : 0);
            int total = secondaryTotal + 1;
            return collected * 100 / total;
        }
    }
}
