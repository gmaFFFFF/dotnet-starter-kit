using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace gmafffff.СтартовыйНабор.DDD.Инфраструктура.Данные.Тесты;

public class КонтекстТестБд : DbContext {
    public КонтекстТестБд(DbContextOptions<КонтекстТестБд> options) : base(options) { }

    public DbSet<Заказ> Заказы { get; set; } = null!;
    public DbSet<Заказ> Клиенты { get; set; } = null!;
    public DbSet<Товар> Товары { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(КонтекстТестБд).Assembly);
    }
}

public class КлиентEfКонфиг : IEntityTypeConfiguration<Клиент> {
    public void Configure(EntityTypeBuilder<Клиент> builder) {
        builder.ToTable("Клиенты");
        builder.OwnsOne(к => к.Фио,
            фио => {
                фио.Property(фио => фио.Имя).HasMaxLength(32);
                фио.Property(фио => фио.Отчество).HasMaxLength(32);
                фио.Property(фио => фио.Фамилия).HasMaxLength(32);
            });


        builder.OwnsMany(к => к.Emailы,
            e => {
                e.ToTable("КлиентEmails");

                e.Property<int>("Ид");
                e.HasKey("Ид");
                e.WithOwner().HasForeignKey("КлиентИд");

                e.Property(email => email.Значение).HasMaxLength(32);
            });
        builder.Property(сущность => сущность.Ид)
            .HasValueGenerator(typeof(ГенераторГуид))
            .HasColumnType("blob");
    }
}

public class ЗаказEfКонфиг : IEntityTypeConfiguration<Заказ> {
    public void Configure(EntityTypeBuilder<Заказ> builder) {
        builder.OwnsOne(з => з.Реквизиты,
            р => {
                р.Property(р => р.Номер).HasMaxLength(16);
                р.Property(р => р.Дата).HasColumnType("Date");
            });

        builder.OwnsMany(з => з.СтрокиЗаказа,
            сз => {
                сз.ToTable("ЗаказСтроки");

                сз.WithOwner().HasForeignKey("ЗаказИд");

                сз.Property(сз => сз.Цена).HasPrecision(19, 2);
                сз.Property(сз => сз.Цена).HasDefaultValue(0);
                сз.Property(сз => сз.Стоимость)
                    .HasComputedColumnSql("Цена  * Количество");

                сз.Property<Guid>("ТоварИд");
                сз.HasOne(сз => сз.Товар)
                    .WithMany().HasForeignKey("ТоварИд");

                сз.HasKey("ЗаказИд", "ТоварИд");
            });

        builder.Property(сущность => сущность.Ид)
            .HasValueGenerator(typeof(ГенераторГуид))
            .HasColumnType("blob");
    }
}

public class ТоварEfКонфиг : IEntityTypeConfiguration<Товар> {
    public void Configure(EntityTypeBuilder<Товар> builder) {
        builder.ToTable("Товары");

        builder.Property(т => т.Цена).HasPrecision(19, 2);
        builder.Property(т => т.Цена).HasDefaultValue(0);

        builder.Property<int>("ТоварКатегорияИд");
        builder.HasOne(т => т.ТоварКатегория)
            .WithMany().HasForeignKey("ТоварКатегорияИд");
        builder.Navigation(товар => товар.ТоварКатегория).AutoInclude();

        builder.Property<Guid>("ПоставщикИд");
        builder.HasOne(т => т.Поставщик)
            .WithMany().HasForeignKey("ПоставщикИд");

        builder.Property(сущность => сущность.Ид)
            .HasValueGenerator(typeof(ГенераторГуид))
            .HasColumnType("blob");
    }
}

public class ПоставщикEfКонфиг : IEntityTypeConfiguration<Поставщик> {
    public void Configure(EntityTypeBuilder<Поставщик> builder) {
        builder.ToTable("Поставщики");

        builder.OwnsMany(п => п.Emailы, e => {
            e.ToTable("ПоставщикEmails");

            e.Property<int>("Ид");
            e.HasKey("Ид");
            e.WithOwner().HasForeignKey("ПоставщикИд");
        });

        builder.Property(сущность => сущность.Ид)
            .HasValueGenerator(typeof(ГенераторГуид))
            .HasColumnType("blob");
    }
}

public class КонтекстТестБдФабрика : IDesignTimeDbContextFactory<КонтекстТестБд> {
    public КонтекстТестБд CreateDbContext(string[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<КонтекстТестБд>()
            .UseSqlite("Data Source=test.sqlite;");


        return new КонтекстТестБд(optionsBuilder.Options);
    }
}