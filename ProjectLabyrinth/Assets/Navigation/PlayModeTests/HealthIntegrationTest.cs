using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

public class HealthIntegrationTest
{
    [UnityTest]
    public IEnumerator Trigger_Collision_ReducesHealth()
    {
        GameObject victim = new GameObject("Victim");
        victim.AddComponent<BoxCollider2D>();
        Health health = victim.AddComponent<Health>();
        health.isPlayer = true;
        health.hitPoints = 10;
        victim.AddComponent<Rigidbody2D>();

        GameObject attacker = new GameObject("Attacker");
        attacker.AddComponent<Hazard>();
        attacker.AddComponent<BoxCollider2D>();
        attacker.AddComponent<Rigidbody2D>();

        attacker.transform.position = victim.transform.position;

        yield return new WaitForFixedUpdate();

        Assert.AreEqual(8, health.hitPoints);
    }
}