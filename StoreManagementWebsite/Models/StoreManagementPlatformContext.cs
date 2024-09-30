﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StoreManagementWebsite.Models;

public partial class StoreManagementPlatformContext : DbContext
{
    public StoreManagementPlatformContext()
    {
    }

    public StoreManagementPlatformContext(DbContextOptions<StoreManagementPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderMessage> OrderMessages { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreOpeningHour> StoreOpeningHours { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
////#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=.;Database=StoreManagementPlatform;Integrated Security=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.CustomerAddressCity).HasMaxLength(10);
            entity.Property(e => e.CustomerAddressDetails).HasMaxLength(50);
            entity.Property(e => e.CustomerAddressDistrict).HasMaxLength(10);
            entity.Property(e => e.CustomerAddressMemo).HasMaxLength(100);
            entity.Property(e => e.CustomerCellPhone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CustomerEinvoiceNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CustomerLocalPhone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CustomerName).HasMaxLength(20);
            entity.Property(e => e.CustomerNickName).HasMaxLength(10);
            entity.Property(e => e.CustomerPassword)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.Property(e => e.FavoriteId).HasColumnName("FavoriteID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorites_Customers");

            entity.HasOne(d => d.Store).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorites_Stores");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.OrderAddressCity).HasMaxLength(10);
            entity.Property(e => e.OrderAddressDetails).HasMaxLength(50);
            entity.Property(e => e.OrderAddressDistrict).HasMaxLength(10);
            entity.Property(e => e.OrderAddressMemo).HasMaxLength(100);
            entity.Property(e => e.OrderEinvoiceNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OrderFinishedTime).HasColumnType("smalldatetime");
            entity.Property(e => e.OrderPhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OrderStoreMemo).HasMaxLength(500);
            entity.Property(e => e.OrderTime).HasColumnType("smalldatetime");
            entity.Property(e => e.OrderUniformInvoiceVia).HasDefaultValue((byte)0);
            entity.Property(e => e.PredictedArrivalTime).HasColumnType("smalldatetime");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Customers");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Stores");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId }).HasName("PK_Order_Details");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("smallmoney");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Details_Orders");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Details_Products");
        });

        modelBuilder.Entity<OrderMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.MessageInformedTime).HasColumnType("smalldatetime");
            entity.Property(e => e.MessageStatus).HasDefaultValue(false);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderMessages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderMessages_Orders");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.ProductName).HasMaxLength(20);
            entity.Property(e => e.ProductUnitPrice).HasColumnType("smallmoney");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Stores");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(20);
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Store).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategories_Stores");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ReviewContent).HasMaxLength(1000);
            entity.Property(e => e.ReviewTime).HasColumnType("smalldatetime");
            entity.Property(e => e.StoreReplyContent).HasMaxLength(1000);
            entity.Property(e => e.StoreReplyTime).HasColumnType("smalldatetime");

            entity.HasOne(d => d.Order).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Orders");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.Property(e => e.ShoppingCartId).HasColumnName("ShoppingCartID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");

            entity.HasOne(d => d.Customer).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCarts_Customers");

            entity.HasOne(d => d.Product).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ShoppingCarts_Products");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.StoreAccountNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StoreAddressCity).HasMaxLength(10);
            entity.Property(e => e.StoreAddressDetails).HasMaxLength(50);
            entity.Property(e => e.StoreAddressDistrict).HasMaxLength(10);
            entity.Property(e => e.StoreAddressMemo).HasMaxLength(500);
            entity.Property(e => e.StoreEndServiceTime).HasColumnType("smalldatetime");
            entity.Property(e => e.StoreImage)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StoreName).HasMaxLength(40);
            entity.Property(e => e.StorePassword)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.StorePhoneNumber)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StoreSetTime).HasColumnType("smalldatetime");
        });

        modelBuilder.Entity<StoreOpeningHour>(entity =>
        {
            entity.HasKey(e => new { e.StoreId, e.MyWeekDay });

            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.MyWeekDay).HasMaxLength(10);
            entity.Property(e => e.StoreClosingTime).HasDefaultValueSql("('')");
            entity.Property(e => e.StoreOpenOrNot).HasDefaultValue(false);
            entity.Property(e => e.StoreOpeningTime).HasDefaultValueSql("('')");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreOpeningHours)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreOpeningHours_Stores");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
