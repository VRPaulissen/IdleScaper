namespace IdleScaper.Persistence.Integrity
{
  	/// <summary>
    /// Integrity layer for detecting tampering/corruption (beyond basic file IO errors).
    /// </summary>
    public interface ISaveIntegrity
    {
        /// <summary>Computes a signature for the given payload.</summary>
        string ComputeSignature(string payload);

        /// <summary>Verifies the payload against a stored signature.</summary>
        bool VerifySignature(string payload, string signature);
    }
}