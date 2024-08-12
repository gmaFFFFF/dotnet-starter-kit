using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace gmafffff.СтартовыйНабор.DDD.Инфраструктура.Данные;

public class ГенераторГуид : ValueGenerator {
    public override bool GeneratesTemporaryValues => false;

    protected override object NextValue(EntityEntry entry) {
        return СледGuid();
    }

    public static Guid СледGuid() {
        // Источник: https://github.com/nhibernate/nhibernate-core/blob/3087d48640eb64f573c0011e9a9b1567afe6adde/src/NHibernate/Id/GuidCombGenerator.cs
        var guidArray = Guid.NewGuid().ToByteArray();

        // Базовая дата - день завершения Курской битвы
        var baseDate = new DateTime(1943, 8, 23);
        var now = DateTime.UtcNow;

        // Get the days and milliseconds which will be used to build the byte string 
        var days = new TimeSpan(now.Ticks - baseDate.Ticks);
        var msecs = now.TimeOfDay;

        // Convert to a byte array 
        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
        var daysArray = BitConverter.GetBytes(days.Days);
        var msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

        // Reverse the bytes to match SQL Servers ordering 
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        // Copy the bytes into the guid 
        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }
}