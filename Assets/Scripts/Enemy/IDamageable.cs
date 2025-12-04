using UnityEngine;

/// <summary>
/// Jednoduché rozhranie pre všetky objekty, ktoré môžu dostať damage.
/// Implementuješ ho na Player, Zombie, atď.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Aplikuj damage na objekt.
    /// </summary>
    /// <param name="amount">Koľko damage objekt dostane.</param>
    void TakeDamage(float amount);
}
