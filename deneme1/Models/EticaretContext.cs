using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace eticaret.Models;

public partial class EticaretContext : DbContext
{
    public EticaretContext()
    {
    }

    public EticaretContext(DbContextOptions<EticaretContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AltKategori> AltKategoris { get; set; }

    public virtual DbSet<AnaKategori> AnaKategoris { get; set; }

    public virtual DbSet<Kullanici> Kullanicis { get; set; }

    public virtual DbSet<KullaniciBilgileri> KullaniciBilgileris { get; set; }

    public virtual DbSet<Misafir> Misafirs { get; set; }

    public virtual DbSet<MisafirDetay> MisafirDetays { get; set; }

    public virtual DbSet<UrunDetay> UrunDetays { get; set; }

    public virtual DbSet<UrunGorsel> UrunGorsels { get; set; }

    public virtual DbSet<Urunler> Urunlers { get; set; }

    public virtual DbSet<Vergi> Vergis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-9TAOJGA\\SQLEXPRESS;Database=Eticaret;User Id=sa;Password=123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AltKategori>(entity =>
        {
            entity.ToTable("AltKategori");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnaKategoriId).HasColumnName("AnaKategori_id");
            entity.Property(e => e.KategoriAdi)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UstKategoriId).HasColumnName("UstKategori_id");
        });

        modelBuilder.Entity<AnaKategori>(entity =>
        {
            entity.ToTable("AnaKategori");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KategoriAdi)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Kategori_Adi");
        });

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("Kullanici");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Durum).HasColumnName("durum");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<KullaniciBilgileri>(entity =>
        {
            entity.ToTable("Kullanici_bilgileri");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ad)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ad");
            entity.Property(e => e.KullaniciId).HasColumnName("kullanici_id");
            entity.Property(e => e.Mail)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("mail");
            entity.Property(e => e.Soyad)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("soyad");
            entity.Property(e => e.Telefon)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("telefon");
            entity.Property(e => e.Yetki).HasColumnName("yetki");
        });

        modelBuilder.Entity<Misafir>(entity =>
        {
            entity.ToTable("Misafir");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Durum).HasColumnName("durum");
            entity.Property(e => e.Isim)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("isim");
            entity.Property(e => e.Kod)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("kod");
            entity.Property(e => e.Mail)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("mail");
            entity.Property(e => e.Sifre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("sifre");
        });

        modelBuilder.Entity<MisafirDetay>(entity =>
        {
            entity.ToTable("Misafir_Detay");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Adres)
                .HasColumnType("text")
                .HasColumnName("adres");
            entity.Property(e => e.Cinsiyet)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cinsiyet");
            entity.Property(e => e.Il).HasColumnName("il");
            entity.Property(e => e.Ilce).HasColumnName("ilce");
            entity.Property(e => e.Musteriid).HasColumnName("musteriid");
            entity.Property(e => e.Tc)
                .HasColumnType("text")
                .HasColumnName("tc");
            entity.Property(e => e.Telefon)
                .HasColumnType("text")
                .HasColumnName("telefon");
        });

        modelBuilder.Entity<UrunDetay>(entity =>
        {
            entity.ToTable("UrunDetay");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Beden)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("beden");
            entity.Property(e => e.Boy)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("boy");
            entity.Property(e => e.Marka)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("marka");
            entity.Property(e => e.Numara).HasColumnName("numara");
            entity.Property(e => e.Renk)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("renk");
        });

        modelBuilder.Entity<UrunGorsel>(entity =>
        {
            entity.ToTable("UrunGorsel");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ad).IsUnicode(false);
        });

        modelBuilder.Entity<Urunler>(entity =>
        {
            entity.ToTable("Urunler");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Aciklama).HasColumnType("text");
            entity.Property(e => e.Alis).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IndirimliFiyat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("indirimliFiyat");
            entity.Property(e => e.KategoriId).HasColumnName("Kategori_id");
            entity.Property(e => e.Satis).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Stok).HasColumnName("stok");
            entity.Property(e => e.UrunAdi)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.VergiId).HasColumnName("Vergi_id");
        });

        modelBuilder.Entity<Vergi>(entity =>
        {
            entity.ToTable("Vergi");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Adi)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
