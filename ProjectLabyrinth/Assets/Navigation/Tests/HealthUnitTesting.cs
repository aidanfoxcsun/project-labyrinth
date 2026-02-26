using NUnit.Framework;
using UnityEngine;

public class HealthUnitTests
{
    // Test 1: Basic Damage Logic
    [Test]
    public void RecieveDamage_ReducesHitPoints()
    {
        GameObject go = new GameObject();
        go.AddComponent<CircleCollider2D>(); // Health requires a Collider2D component
        Health health = go.AddComponent<Health>();
        health.maxHP = 100;
        health.hitPoints = 100;

        health.RecieveDamage(30);

        Assert.AreEqual(70, health.hitPoints);
    }

    // Test 2: Healing Logic (Clamping)
    [Test]
    public void Heal_DoesNotExceedMaxHP()
    {
        GameObject go = new GameObject();
        go.AddComponent<CircleCollider2D>(); // Health requires a Collider2D component
        Health health = go.AddComponent<Health>();
        health.maxHP = 100;
        health.hitPoints = 90;

        health.Heal(50); // Should cap at 100, not 140

        Assert.AreEqual(100, health.hitPoints);
    }

    // Test 3: Event Triggering (Logic Verification)
    [Test]
    public void RecieveDamage_ReturnsTrue_WhenHealthReachesZero()
    {
        GameObject go = new GameObject();
        go.AddComponent<CircleCollider2D>(); // Health requires a Collider2D component
        Health health = go.AddComponent<Health>();
        health.hitPoints = 10;

        bool isKilled = health.RecieveDamage(15);

        Assert.IsTrue(isKilled);
    }
}
