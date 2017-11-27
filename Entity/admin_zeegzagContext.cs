﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ZeegZag.Data.Entity
{
    public partial class admin_zeegzagContext : DbContext
    {
        public virtual DbSet<BorsaCurrencyT> BorsaCurrencyT { get; set; }
        public virtual DbSet<BorsaT> BorsaT { get; set; }
        public virtual DbSet<CurrencyT> CurrencyT { get; set; }
        public virtual DbSet<HistoryT> HistoryT { get; set; }

        public admin_zeegzagContext(DbContextOptions<admin_zeegzagContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {   modelBuilder.Entity<BorsaCurrencyT>(entity =>
            {
                entity.ToTable("borsa_currency_t");

                entity.HasIndex(e => e.BorsaId)
                    .HasName("FK_borcur_borsaid");

                entity.HasIndex(e => e.FromCurrencyId)
                    .HasName("FK_borcur_curid");

                entity.HasIndex(e => e.ToCurrencyId)
                    .HasName("FK_borcur_tocurid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(20)");

                entity.Property(e => e.AutoGenerated).HasColumnType("bit(1)");

                entity.Property(e => e.BorsaId).HasColumnType("int(11)");

                entity.Property(e => e.Disabled).HasColumnType("bit(1)");

                entity.Property(e => e.FromCurrencyId).HasColumnType("int(11)");

                entity.Property(e => e.High24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.HighHour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.LastUpdate).HasColumnType("datetime");

                entity.Property(e => e.Low24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.LowHour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Open24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.OpenHour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Price).HasColumnType("decimal(16,8)");

                entity.Property(e => e.ToCurrencyId).HasColumnType("int(11)");

                entity.Property(e => e.Volume24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Volume24HourTo).HasColumnType("decimal(16,8)");

                entity.Property(e => e.VolumeHour).HasColumnType("decimal(16,8)");

                entity.HasOne(d => d.Borsa)
                    .WithMany(p => p.BorsaCurrencyT)
                    .HasForeignKey(d => d.BorsaId)
                    .HasConstraintName("FK_borcur_borsaid");

                entity.HasOne(d => d.FromCurrency)
                    .WithMany(p => p.BorsaCurrencyTFromCurrency)
                    .HasForeignKey(d => d.FromCurrencyId)
                    .HasConstraintName("FK_borcur_curid");

                entity.HasOne(d => d.ToCurrency)
                    .WithMany(p => p.BorsaCurrencyTToCurrency)
                    .HasForeignKey(d => d.ToCurrencyId)
                    .HasConstraintName("FK_borcur_tocurid");
            });

            modelBuilder.Entity<BorsaT>(entity =>
            {
                entity.ToTable("borsa_t");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)");

                entity.Property(e => e.ApiName).HasMaxLength(255);

                entity.Property(e => e.IsActive).HasColumnType("bit(1)");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.UseUsd).HasColumnType("bit(1)");
            });

            modelBuilder.Entity<CurrencyT>(entity =>
            {
                entity.ToTable("currency_t");

                entity.Property(e => e.Id)
                    .HasColumnType("int(11)");

                entity.Property(e => e.ImageUrl).HasMaxLength(1024);

                entity.Property(e => e.Alias).HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasMaxLength(8);
            });

            modelBuilder.Entity<HistoryT>(entity =>
            {
                entity.ToTable("history_t");

                entity.HasIndex(e => e.BorsaCurrencyId)
                    .HasName("FK_borcur_borsaid");

                entity.Property(e => e.Id)
                    .HasColumnType("int(20)");

                entity.Property(e => e.BorsaCurrencyId).HasColumnType("int(11)");

                entity.Property(e => e.EntryDate).HasColumnType("datetime");

                entity.Property(e => e.High24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Low24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Open24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Price).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Volume24Hour).HasColumnType("decimal(16,8)");

                entity.Property(e => e.Volume24HourTo).HasColumnType("decimal(16,8)");

                entity.Property(e => e.VolumeHour).HasColumnType("decimal(16,8)");

                entity.HasOne(d => d.BorsaCurrency)
                    .WithMany(p => p.HistoryT)
                    .HasForeignKey(d => d.BorsaCurrencyId)
                    .HasConstraintName("FK_history_bcid");
            });
            
        }
    }
}
