using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class DungeonGeneratorTests
{
    // Finds DungeonGenerator at runtime without compile-time reference
    static Type DGType()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType("DungeonGenerator");
            if (t != null) return t;
        }
        return null;
    }

    static object NewDG()
    {
        var t = DGType();
        Assert.NotNull(t, "Could not find type 'DungeonGenerator' at runtime. Is the script compiling?");
        var go = new GameObject("DG_Test");
        return go.AddComponent(t);
    }

    static FieldInfo Field(Type t, string name)
        => t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    static MethodInfo Method(Type t, string name)
        => t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    static void Set(object obj, string fieldName, object value)
    {
        var f = Field(obj.GetType(), fieldName);
        Assert.NotNull(f, $"Field '{fieldName}' not found on DungeonGenerator. Name changed?");
        f.SetValue(obj, value);
    }

    static object Get(object obj, string fieldName)
    {
        var f = Field(obj.GetType(), fieldName);
        Assert.NotNull(f, $"Field '{fieldName}' not found on DungeonGenerator. Name changed?");
        return f.GetValue(obj);
    }

    static void Call(object obj, string methodName, params object[] args)
    {
        var m = Method(obj.GetType(), methodName);
        Assert.NotNull(m, $"Method '{methodName}' not found on DungeonGenerator. Name/signature changed?");
        m.Invoke(obj, args);
    }

    // -----------------------------
    // UNIT TEST: IsAdjacent correct
    // -----------------------------
    [Test]
    public void Unit_IsAdjacent_ManhattanDistance1()
    {
        var dg = NewDG();
        var t = dg.GetType();

        // IsAdjacent(Vector2Int a, Vector2Int b)
        var isAdj = Method(t, "IsAdjacent");
        Assert.NotNull(isAdj, "IsAdjacent method not found. If it's private, that's fine—reflection should still find it unless name changed.");

        bool Adj(Vector2Int a, Vector2Int b) => (bool)isAdj.Invoke(dg, new object[] { a, b });

        Assert.IsTrue(Adj(new Vector2Int(0, 0), new Vector2Int(1, 0)));
        Assert.IsTrue(Adj(new Vector2Int(0, 0), new Vector2Int(0, 1)));
        Assert.IsFalse(Adj(new Vector2Int(0, 0), new Vector2Int(1, 1)));
        Assert.IsFalse(Adj(new Vector2Int(0, 0), new Vector2Int(2, 0)));
    }

    // ---------------------------------------------------
    // REGRESSION TEST: Treasure+Upgrade are always present
    // (This guards against "assigned but never appears"/missing)
    // ---------------------------------------------------
    [Test]
    public void Regression_AssignRoomTypes_AssignsOneTreasureAndOneUpgrade()
    {
        var dg = NewDG();
        var t = dg.GetType();

        // Build a simple connected occupied set (a line)
        var occupied = new HashSet<Vector2Int>();
        for (int i = 0; i < 10; i++) occupied.Add(new Vector2Int(i, 0));

        // These field names MUST match your DungeonGenerator fields.
        // From your earlier code: occupied, roomTypes, occupiedByLargeRooms, largeRoomOrigins, minBossDistance
        Set(dg, "occupied", occupied);

// roomTypes is Dictionary<Vector2Int, RoomType>
var roomTypeEnum = t.GetNestedType("RoomType", BindingFlags.Public | BindingFlags.NonPublic);
Assert.NotNull(roomTypeEnum, "Could not find nested enum 'RoomType' in DungeonGenerator.");

var dictType = typeof(Dictionary<,>).MakeGenericType(typeof(Vector2Int), roomTypeEnum);
var roomTypes = Activator.CreateInstance(dictType);
Set(dg, "roomTypes", roomTypes);


        // large room sets (empty is fine)
        Set(dg, "occupiedByLargeRooms", new HashSet<Vector2Int>());
        Set(dg, "largeRoomOrigins", new HashSet<Vector2Int>());

        // Boss distance so boss always assigns
        Set(dg, "minBossDistance", 1);

        // Call AssignRoomTypes(startPos)
        var start = new Vector2Int(0, 0);
        Call(dg, "AssignRoomTypes", start);

        // Count types in roomTypes dictionary
        // Iterate dictionary entries via reflection
        int treasureCount = 0, upgradeCount = 0;

        var treasureValue = Enum.Parse(roomTypeEnum, "Treasure");
        var upgradeValue  = Enum.Parse(roomTypeEnum, "Upgrade");

        var enumerable = (System.Collections.IEnumerable)Get(dg, "roomTypes");
        foreach (var entry in enumerable)
        {
            var entryType = entry.GetType(); // KeyValuePair<Vector2Int, RoomType>
            var value = entryType.GetProperty("Value")!.GetValue(entry);

            if (Equals(value, treasureValue)) treasureCount++;
            if (Equals(value, upgradeValue))  upgradeCount++;
        }

        Assert.AreEqual(1, treasureCount, "Expected exactly one Treasure room assigned.");
        Assert.AreEqual(1, upgradeCount, "Expected exactly one Upgrade room assigned.");
    }
}
