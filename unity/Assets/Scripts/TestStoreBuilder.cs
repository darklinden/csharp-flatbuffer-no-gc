using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sample;
using Google.FlatBuffers;
using UnityEngine.Profiling;

public class TestStoreBuilder : MonoBehaviour
{
    void Start()
    {
        var builder = Google.FlatBuffers.FlatBufferBuilder.InstanceDefault;
        builder.Clear();

        // Profiler.logFile = "mylog"; //Also supports passing "myLog.raw"
        // Profiler.enableBinaryLog = true;
        Profiler.enableAllocationCallstacks = true;
        Profiler.enabled = true;

        // Optional, if more memory is needed for the buffer
        Profiler.maxUsedMemory = 256 * 1024 * 1024;
    }

    // Update is called once per frame
    void Update()
    {
        // get stored builder 
        // build should be clear after use
        Profiler.BeginSample("DefaultBuilder No GC");
        var builder = Google.FlatBuffers.FlatBufferBuilder.InstanceDefault;
        Profiler.EndSample();

        Profiler.BeginSample("CreateWeapon No GC");
        var weaponOneName = builder.CreateString("Sword");
        short weaponOneDamage = 3;
        var weaponTwoName = builder.CreateString("Axe");
        short weaponTwoDamage = 5;

        // Use the `CreateWeapon()` helper function to create the weapons, since we set every field.
        var sword = Weapon.CreateWeapon(builder, weaponOneName, weaponOneDamage);
        var axe = Weapon.CreateWeapon(builder, weaponTwoName, weaponTwoDamage);
        Profiler.EndSample();

        Profiler.BeginSample("CreateString No GC");
        // Serialize a name for our monster, called "Orc".
        var monsterName = builder.CreateString("Orc");
        Profiler.EndSample();

        Profiler.BeginSample("FlatbufferBuilder StartInventoryVector");
        // Create a `vector` representing the inventory of the Orc. Each number
        // could correspond to an item that can be claimed after he is slain.
        // Note: Since we prepend the bytes, this loop iterates in reverse order.
        Monster.StartInventoryVector(builder, 10);
        for (int i = 9; i >= 0; i--)
        {
            builder.AddByte((byte)i);
        }
        var inv = builder.EndVector();
        Profiler.EndSample();

        Profiler.BeginSample("FlatbufferBuilder CreateWeaponsVector");

        // Pass the `weaps` array into the `CreateWeaponsVector()` method to create a FlatBuffer vector.
        Monster.StartWeaponsVector(builder, 2);
        builder.AddOffset(sword.Value);
        builder.AddOffset(axe.Value);
        var weapons = builder.EndVector();
        Profiler.EndSample();


        Profiler.BeginSample("FlatbufferBuilder CreateMonster");
        Monster.StartPathVector(builder, 2);
        Vec3.CreateVec3(builder, 1.0f, 2.0f, 3.0f);
        Vec3.CreateVec3(builder, 4.0f, 5.0f, 6.0f);
        var path = builder.EndVector();
        Profiler.EndSample();


        Profiler.BeginSample("FlatbufferBuilder CreateMonster");
        // Create our monster using `StartMonster()` and `EndMonster()`.
        Monster.StartMonster(builder);
        Monster.AddPos(builder, Vec3.CreateVec3(builder, 1.0f, 2.0f, 3.0f));
        Monster.AddHp(builder, (short)300);
        Monster.AddName(builder, monsterName);
        Monster.AddInventory(builder, inv);
        Monster.AddColor(builder, Sample.Color.Red);
        Monster.AddWeapons(builder, weapons);
        Monster.AddEquippedType(builder, Equipment.Weapon);
        Monster.AddEquipped(builder, axe.Value); // Axe
        Monster.AddPath(builder, path);
        var orc = Monster.EndMonster(builder);
        Monster.FinishMonsterBuffer(builder, orc);
        Profiler.EndSample();

        Profiler.BeginSample("FlatbufferBuilder Clear");
        // Of type `FlatBuffers.ByteBuffer`.
        // The data in this ByteBuffer does NOT start at 0, but at buf.Position.
        // The end of the data is marked by buf.Length, so the size is
        // buf.Length - buf.Position.
        var buf = builder.DataBuffer;
        Profiler.EndSample();

        Profiler.BeginSample("Read Flatbuffer");
        var monster = Monster.GetRootAsMonster(buf);

        // For C#, unlike most other languages support by FlatBuffers, most values (except for
        // vectors and unions) are available as properties instead of accessor methods.

        Profiler.EndSample();

        Profiler.BeginSample("Read String GC Alloc");
        var name = monster.Name;
        // UnityEngine.Debug.Log(name);
        var secondWeaponName = monster.Weapons(1)?.Name;
        // UnityEngine.Debug.Log(secondWeaponName);
        Profiler.EndSample();

        Profiler.BeginSample("Read Scalar No GC");
        var hp = monster.Hp;
        var mana = monster.Mana;

        var pos = monster.Pos.Value;
        var x = pos.X;
        var y = pos.Y;
        var z = pos.Z;

        int invLength = monster.InventoryLength;
        var thirdItem = monster.Inventory(2);

        int weaponsLength = monster.WeaponsLength;
        var secondWeaponDamage = monster.Weapons(1)?.Damage;

        Profiler.EndSample();

        Profiler.BeginSample("FlatbufferReader Clear");
        builder.Clear();
        Profiler.EndSample();
    }
}
